import { Component, OnInit } from '@angular/core';
import { FeaturesService, Features } from "./services/features.service";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  public features: Features;

  constructor(private featuresService: FeaturesService) {
    this.features = new Features();
  }

  ngOnInit() {
    this.featuresService.getFeatures().subscribe((returnedFeatures: Features) => {
      this.features = returnedFeatures;
    });
  }
}
