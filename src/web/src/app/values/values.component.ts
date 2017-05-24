import { Component, OnInit } from '@angular/core';
import { ValuesService } from "app/services/values.service";

@Component({
  selector: 'app-values',
  templateUrl: './values.component.html',
  styleUrls: ['./values.component.css']
})
export class ValuesComponent implements OnInit {
  public values: string[];

  constructor(private valuesService: ValuesService) { 
  }

  ngOnInit() {
     this.valuesService.getValues().subscribe((values: string[]) => {
      this.values = values;
    });
  }
}
