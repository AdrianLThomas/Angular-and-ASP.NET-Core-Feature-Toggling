import { Component, OnInit } from '@angular/core';
import { FeaturesService } from "./services/features.service";
import { Features } from "./models/features.model";

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
