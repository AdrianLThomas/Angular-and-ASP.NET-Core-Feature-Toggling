import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';

import { AppComponent } from './app.component';
import { FeaturesComponent } from './features/features.component';
import { FeaturesService } from "./services/features.service";

@NgModule({
  declarations: [
    AppComponent,
    FeaturesComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpModule
  ],
  providers: [FeaturesService],
  bootstrap: [AppComponent]
})
export class AppModule { }
