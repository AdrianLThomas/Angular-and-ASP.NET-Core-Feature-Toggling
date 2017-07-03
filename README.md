# Feature Toggle demo using Angular (2+) and ASP.NET Core

# Intro
Feature toggling is what it says on the tin, the ability to toggle features off/on. This can be useful for a range of scenarios - perhaps a feature isn't quite yet ready for production yet, or there's some new functionality you would like to trial with some but not all customers. In this blog article we will learn how to feature toggle in an Angular 2+ app and ASP .NET Core Web API. We'll hide components and avoid them from being executed unless enabled, and also protect specific endpoints on the API from being called. There is also a demo with some sample code at the end of this article.

# Getting Started
## Prerequisites
* A code editor (i.e. [Visual Studio Code](https://code.visualstudio.com))
* [npm](https://nodejs.org/en/)
* [Angular CLI](https://www.npmjs.com/package/angular-cli)
* [.NET Core](https://www.microsoft.com/net/download/core)
* [FeatureToggle NuGet package](https://github.com/jason-roberts/FeatureToggle)

## How it all works
In a nutshell: 
* We register what features are available in the API
* Use an MVC filter to determine whether to restrict access to the endpoint (either across actions or the entire controller)
* Expose an endpoint that exposes what features are enabled or disabled
* Make a request to the API from Angular to discover what features are enabled, and simply use `*ngIf` to decide whether that component should be enabled.

# Setup
This article covers typical changes needed to make to an existing solution, not how to set up a new Angular / .NET Core project. Please use the demo at the end of this article as a working reference.

## The API
### Settings file
In our `appSettings.json` file, we configure our features like so:
```
{
  "FeatureToggle":{
    "ValuesFeature": "false",
    "NavigationFeature": "true"
  }
}
```
Our features can then be configured accordingly by adjusting the values in the configuration. Of course we don't need to use this file, and I'll explain in a moment how we can implement our own method of obtaining settings. _Note: The name of the setting should correlate exactly with the class name of our feature._

I'll leave it up to you on how you decide to toggle the switch. This could be deploy time, real time or just manually.

### Settings Feature Provider
We can write our own settings provider so that we can integrate with whichever service we would like (such as an API, database or a config file). 

All that is needed is to implement the IBooleanToggleValueProvider interface, create an instance of it and assign it to a property on the feature at registration. Note: In the previous RC1 version of the Feature Toggle library, there was a bug where we would have needed to write our own Settings Feature Provider, so [here's an example of how this worked if you want to write your own.](https://github.com/AdrianLThomas/Angular-and-ASP.NET-Core-Feature-Toggling/blob/9475e039ac03f54c9dcab69c30373743fba7b210/src/api/Features/Custom/SettingsFeatureProvider.cs).

### Installing the NuGet package
First we need to install the NuGet package. We need to use the RC2 pre-release version (at time of writing) that supports .NET Core. It's a good package to target as it supports many versions of .NET and is frequently updated. 

Run the following to install the package to your project:

`dotnet add package FeatureToggle --version 4.0.0-rc2`

`dotnet restore`

### Making the feature a filter
When using FeatureToggle, we create strongly typed features that inherit the SimpleFeatureToggle class. When creating features, it would be useful for these to also implement the IResourceFilter interface so that we can restrict API access at either a controller or action level. Therefore in this example, we can create a BaseFeature class:
```
public abstract class BaseFeature : SimpleFeatureToggle, IResourceFilter
{
    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        if (!FeatureEnabled)
        {
            StopExecution(context);
        }
    }

    private static void StopExecution(ResourceExecutingContext context)
    {
        context.Result = new ContentResult()
        {
            Content = "Resource unavailable - feature disabled",
            StatusCode = 401
        };
    }
}
```

Our new features should now inherit this class.

#### Feature registration
We need to configure the IOC container with this new feature like so:

```
// FeatureServiceCollectionExtensions.cs
public static void AddFeatures(this IServiceCollection services, IConfigurationRoot configuration)
{
    var provider = new SettingsFeatureProvider(configuration);
    services.AddSingleton(new ValuesFeature() { ToggleValueProvider = provider });});
}

// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddFeatures(Configuration);
}
```

### Associating a feature with controllers / actions
If the feature is disabled, then the request should fail with the specified message and status code. Therefore if we create a new feature:
```
public class ValuesFeature : BaseFeature { }
```

and apply this to a controller or action by using the ServiceFilterAttribute:
```
[ServiceFilter(typeof(ValuesFeature))] //Applied to all actions within controller
public class ValuesController : Controller
{
}   
```
```
[HttpGet]
[ServiceFilter(typeof(ValuesFeature))] //Applied to just this action
public IEnumerable<string> Get()
{
    return new string[] { "value1", "value2" };
}
```

Then we will either execute the action or stop the execution of the action altogether, depending on whether the feature is enabled or disabled respectively.

#### Response from a disabled feature
If the ValuesFeature is turned off, and we make GET request to `http://localhost:4200/api/Values` then we will see the following response:
```
$ curl -i -X GET http://localhost:4200/api/values
HTTP/1.1 401 Unauthorized
X-Powered-By: Express
Access-Control-Allow-Origin: *
connection: close
date: Fri, 26 May 2017 15:33:29 GMT
content-length: 39
content-type: text/plain; charset=utf-8
server: Kestrel

Resource unavailable - feature disabled
```

Note that we get the response message and status code as specified in the BaseFeature class.

### Exposing available features
We want to be able to expose what features are enabled/disabled so that our Angular app can determine what parts of the website should be active (rendered and executed). Therefore we should expose an endpoint that displays all features and their current state.

In this example we inject each feature in to the constructor of the controller and return it from the action. It would be nice to inject a list of IFeatureToggle rather than manually maintaining this list. One option would be to register each feature by the IFeatureToggle interface, which would then provide us with this behaviour. However the issue is that the in order to apply the filter on the controller, we want to use the instance of the filter registered with the IoC container so that the settings get loaded correctly. To do this we use the ServiceFilterAttribute and specify the concrete type, but because we registered the interface rather than the implementation the injection fails. Therefore we have to maintain this list with all available features we want to expose via the API.


```
[Route("api/[controller]")]
public class FeaturesController : Controller
{
    private readonly IEnumerable<IFeatureToggle> _allFeatures;

    public FeaturesController(ValuesFeature valuesFeature, NavigationFeature navigationFeature)
    {
        _allFeatures = new List<IFeatureToggle>()
        {
            valuesFeature,
            navigationFeature
        };
    }
    
    [HttpGet]
    public IDictionary<string, bool> Get()
    {
        var allFeatures = _allFeatures.Select(x => new { Key = x.GetType().Name, Value = x.FeatureEnabled });

        return allFeatures.ToDictionary(k => k.Key, v => v.Value);
    }
}
```

#### Response from the features endpoint
```
$ curl -i -X GET http://localhost:4200/api/features
HTTP/1.1 200 OK
X-Powered-By: Express
Access-Control-Allow-Origin: *
connection: close
date: Fri, 26 May 2017 15:32:58 GMT
content-type: application/json; charset=utf-8
server: Kestrel
transfer-encoding: chunked

{"ValuesFeature":false,"NavigationFeature":true}
```

## The Website
Quite simply, Angular just needs to make the request to the API and use `*ngIf` to bind to the result returned from the API. A features service could be created like so:
```
import { Injectable } from '@angular/core';
import { Http, Response, Headers, RequestOptions, Request, RequestMethod } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import 'rxjs/add/operator/map';
import { Features } from "../models/features.model";

@Injectable()
export class FeaturesService {
    //This should be injected in or replaced by your deployment pipeline.
    private featuresUrl: string = "http://localhost:4200/api/features"; 

    constructor(private http: Http) {
    }

    public getFeatures(): Observable<Features> {
        return this.http
            .get(this.featuresUrl)
            .map(response => response.json() as Features)
            .catch(this.handleError);
    }

    private handleError(error: Response) {
        console.log('error = ' + error);
        return Observable.throw(error.statusText);
    }
}
```

Here is our Features model:
```
export class Features {
  ValuesFeature: boolean;
  NavigationFeature: boolean;
}
```


You can then inject this service in to your component, and bind to the feature in the markup:
```
<app-navigation *ngIf="features.NavigationFeature"></app-navigation>

<h1>Feature Toggling Demo</h1>

<ul *ngIf="features.ValuesFeature">
  <p>The values from the enabled values feature API are:</p>
  <app-values></app-values>
</ul>
```

The nice thing is that since these components don't even get rendered, the components will not execute any code (thus any further API calls they would be making (and failing) will not be made at all).

# Adding new features
So, in a nutshell when adding a new feature the following steps should be followed:
1. Create a new feature class in the API
1. Register the feature with IoC container
1. Add a setting for said feature to AppSettings.json (_remember to name it the same as the class name_)
1. Expose the feature via API endpoint
1. Add to the model in Angular
1. Bind the front end to the state of the feature returned from the API

# Demo
```git clone https://github.com/AdrianLThomas/Angular-and-ASP.NET-Core-Feature-Toggling```

```$ cd "./src/web/"```

```$ npm install```

```$ npm start```

_Note: Ensure ```npm start``` is used rather than ```ng serve```, as we want the proxy to start at the same time._

## API Project
```$ cd "./src/api/"```

```$ dotnet restore```

```$ dotnet run```

# Other Notes
When the Angular app tries to make a request across domains, the browser will block this request for security reasons. Therefore to avoid these CORS (Cross-Origin Resource Sharing) issues for local development, we are using [stories proxy](https://github.com/angular/angular-cli/wiki/stories-proxy). This takes API requests from the port that is serving the Angular website and serves them to the API running on a different port. In a production scenario, it is recommended to either ensure the API is on the same origin, or [set the HTTP response headers accordingly](https://developer.mozilla.org/en-US/docs/Web/HTTP/Access_control_CORS#The_HTTP_response_headers).

# Summary
I hope you've found this post helpful. If you have any feedback or suggestions, please feel free to raise an issue or pull request on GitHub and I'd be happy to take a look.
