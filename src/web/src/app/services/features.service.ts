import { Injectable } from '@angular/core';
import { Http, Response, Headers, RequestOptions, Request, RequestMethod } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import 'rxjs/add/operator/map';

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

export class Features {
  ValuesFeature: boolean;
  NavigationFeature: boolean;
}