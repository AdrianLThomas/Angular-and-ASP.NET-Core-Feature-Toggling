import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';

import { AppComponent } from './app.component';
import { FeaturesComponent } from './features/features.component';
import { FeaturesService } from "./services/features.service";
import { ValuesService } from "./services/values.service";
import { ValuesComponent } from './values/values.component';

@NgModule({
  declarations: [
    AppComponent,
    FeaturesComponent,
    ValuesComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpModule
  ],
  providers: [FeaturesService, ValuesService],
  bootstrap: [AppComponent]
})
export class AppModule { }
