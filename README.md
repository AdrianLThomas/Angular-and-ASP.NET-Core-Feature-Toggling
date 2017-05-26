# Draft! This article is a work in progress...

# Feature Toggle demo using Angular (2+) and ASP.NET Core

# Intro
Feature toggling is what it says on the tin, the ability to toggle features off/on. This can be useful for a range of scenarios - perhaps a feature isn't quite yet ready for production yet, or it is some new functionality you would like to trial with some but not all customers. In this blog article you will learn how to feature toggle in an Angular 2+ app and ASP .NET Core Web API. We'll hide components and avoid them from being executed unless enabled, and also protect specific endpoints on the API from being called. There is also a demo with some sample code at the end of this article.

# Getting Started
## Prerequisites
* A code editor (i.e. [Visual Studio Code](https://code.visualstudio.com))
* [npm](https://nodejs.org/en/)
* [Angular CLI](https://www.npmjs.com/package/angular-cli)
* [.NET Core](https://www.microsoft.com/net/download/core)
* [FeatureToggle nuget package](https://github.com/jason-roberts/FeatureToggle)

## How it all works
In a nutshell: 
* We register what features are available in the API
* Use an MVC filter to determine whether to restrict access to the endpoint (either across actions or the entire controller)
* Expose an endpoint that exposes what features are enabled or disabled
* Make a request to the API from Angular to discover what features are enabled, and simply use `*ngIf` to decide whether that component should be enabled.

# Setup
This article covers typical changes you need to make to your existing solution, not how to set up a new Angular / .NET Core project. Please use the demo at the end of this article as a working reference.

## API
###
In our `appSettings.json` file, we configure our features like so:
```
{
  "FeatureToggle":{
    "ValuesFeature": "false",
    "NavigationFeature": "true"
  }
}
```
This can then be picked up by our features. _Note: The name of the setting should correlate exactly with the class name of our feature._

I'll leave it up to you on how you decide to toggle the switch. This could be deploy time, real time or just manually.

### Settings Feature Provider
Unfortunately one of the issues with the AppSettingsProvider within the FeatureToggle package is that it doesn't correctly.

<TODO! - Issue on github logged>. https://github.com/jason-roberts/FeatureToggle/issues/145


### Installing the nuget package
First we need to install the nuget package. We need to use a pre-release version (at time of writing) that supports .NET core. It's a good package to aim for as it supports many versions of .NET and is frequently updated. There are some issues with it, which we will work around in this article.

Run the following to install the package to your project:

`dotnet add package FeatureToggle --version 4.0.0-rc1`

`dotnet restore`

### Making the feature a filter
When using FeatureToggle, we create strongly typed features that inherit the SimpleFeatureToggle class. When creating features, it would be useful for these to also implement the IResourceFilter interface so that we can restrict API access at either a controller or action level. Therefore in this example, I created a BaseFeature class:
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

### Associating a feature with controllers / actions
If the feature is disabled, then the request will fail with the specified message and status code. Therefore if we create a new feature like so:
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

### Exposing available features
We want to be able to expose what features are enabled/disabled so that our Angular app can determine what parts of the website should be active (rendered and executed). Therefore we should expose an endpoint that displays all features and their current state.

In this example we inject each feature in to the constructor of the controller and return it from the action. It would be nice to inject a list of IFeatureToggle rather than manually maintaining this list. One option would be to register each feature by the IFeatureToggle interface, which would then provide us with this behaviour. However the issue is that the in order to apply the filter on the controller, we want to use the instance of the filter registered with the IoC so that the settings get loaded correctly (due to the aforementioned bug). To do this we use the ServiceFilterAttribute and specify the concrete type, but because we registered the interface rather than the implementation the injection fails. Therefore until the bug has been fixed, we have to live with our own SettingsFeatureProvider and maintain this list.


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

## Website
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

The nice thing is that since these components don't even get rendered, the components will not execute any code (thus any futher API calls they would be making will not be made).

# Adding new features
So if you want to add a new feature, the following areas need updating:
1. Create new feature class in API
1. Register Feature with IoC container
1. Add setting for feature to appSettings.json
1. Expose feature via API endpoint
1. Add to model in Angular
1. Bind front end to state of the feature returned from the API

# Restricting API access
To do

Restricted sample endpoint: http://localhost:4200/api/Values
Get all features (for Angular to call): http://localhost:4200/api/Features
Note: [stories proxy](https://github.com/angular/angular-cli/wiki/stories-proxy) is used in order to avoid CORS issues for local development. This takes API requests from the port that is serving the static content and serves them to the API running on a different port.

# Hiding interface elements
To do
Show network traffic with feature enabled vs disabled. (web requests only made if enabled)

# Gotchas
When registering the feature, ideally it needs to be registered by the interface so that an IEnumerable<IFeatureToggle> can be returned. This is very useful for the Features controller so that the enabled features can be exposed. However the problem with this approach is that in order to use the ServiceFilterAttribute to resolve the feature from the DI container, you must pass in the type of the implementation - which fails, because we registered the interface and passed in the concrete type. Conversely, we could register the concrete version, which would then allow the ServiceFilterAttribute to work, but we cannot inject an IEnumerable<IFeatureToggle>. Therefore we have two places to manage features, one during service registration and one for feature discovery via the API endpoint. 

# Demo
This section is optional - there is a sample demo here, however this article will run through the steps required to integrate this in your application.

```git clone https://github.com/AdrianLThomas/Angular-and-ASP.NET-Core-Feature-Toggling```

```$ cd "./src/web/"```

```$ npm install```

```$ npm start```

Ensure ```npm start``` is used rather than ```ng serve```, as we want the proxy to start at the same time.

## API Project
```$ cd "./src/api/"```

```$ dotnet restore```

```$ dotnet run```