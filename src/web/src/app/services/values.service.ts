import { Injectable } from '@angular/core';
import { Http, Response, Headers, RequestOptions, Request, RequestMethod } from '@angular/http';
import { Observable } from 'rxjs/Rx';
import 'rxjs/add/operator/map';

@Injectable()
export class ValuesService {
    //This should be injected in or replaced by your deployment pipeline.
    private valuesUrl: string = "http://localhost:4200/api/values"; 

    constructor(private http: Http) {
    }

    public getValues(): Observable<string[]> {
        return this.http
            .get(this.valuesUrl)
            .map(response => response.json())
            .catch(this.handleError);
    }

    private handleError(error: Response) {
        console.log('error = ' + error);
        return Observable.throw(error.statusText);
    }
}
