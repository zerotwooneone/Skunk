import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { SimulatorService } from './simulator/simulator.service';

@Component({
  selector: 'sk-root',
  standalone: true,
  imports: [RouterOutlet, DialogModule, ButtonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'skunkapp';
  visible: boolean = false;
  startSimDisabled: boolean = false;
  stopSimDisabled: boolean = true;

  constructor(private readonly simulator: SimulatorService){}

  showDialog() {
    this.visible = true;
  }
  startSimulator() {
    if(this.startSimDisabled){
      return;
    }
    this.stopSimDisabled = false;
    this.startSimDisabled = true;
    this.simulator.startSim();
  }
  stopSimulator() {
    this.startSimDisabled = false;
    this.stopSimDisabled = true;
  }
}
