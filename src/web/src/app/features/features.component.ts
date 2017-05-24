import { Component, OnInit } from '@angular/core';
import { FeaturesService } from "../services/features.service";
import { Observable } from "rxjs/Observable";
import { Features } from "../features/features.model";

@Component({
  selector: 'app-features',
  templateUrl: './features.component.html',
  styleUrls: ['./features.component.css']
})
export class FeaturesComponent implements OnInit {
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
