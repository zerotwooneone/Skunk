import { Component } from '@angular/core';
import { TestTextComponent } from '../sensors/test-text/test-text.component';
import { BackendService } from '../backend/backend.service';
import { Observable, distinct, filter, map } from 'rxjs';
import { CommonModule } from '@angular/common';
import { environment } from '../../environments/environment';
import { SensorsConfig, SensorConfig } from "../../environments/SensorsConfig";
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { FormsModule } from '@angular/forms';
import { KnobComponent } from '../knob/knob';

@Component({
  selector: 'sk-home',
  standalone: true,
  imports: [TestTextComponent, CommonModule, MatProgressBarModule, KnobComponent, FormsModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  readonly sensorsConfig: SensorsConfig = environment.sensors;
  readonly dummySensorConfig: SensorConfig = this.sensorsConfig.DummySensor ?? { id: 'unknown' };
  readonly vocConfig: SensorConfig = environment.sensors.Voc ?? { id: 'unknown' };
  readonly co2Config: SensorConfig = environment.sensors.Co2 ?? { id: 'unknown' };
  readonly dummySensorValue$: Observable<number | undefined>;
  readonly voc$: Observable<number | undefined>;
  readonly co2$: Observable<number | undefined>;
  readonly formaldehydeSensorConfig: SensorConfig = this.sensorsConfig.Formaldehyde;
  readonly formaldehydeSensorValue$: Observable<number>;
  constructor(
    private readonly backend: BackendService
  ) {
    /*backend.SensorData$.subscribe(
      p => {
        console.info('sensor data', p);
        return p;
      });*/
    this.dummySensorValue$ = backend.SensorData$.pipe(
      filter(p => typeof p.DummySensor == 'number'),
      map(p => {
        return (p.DummySensor as number) * 100;
      }));
    this.formaldehydeSensorValue$ = backend.SensorData$.pipe(
      filter(p => (typeof p.Formaldehyde) == 'number'),
      map(p => {
        //console.warn(`Formaldehyde:${(p.Formaldehyde as number)} max:${this.formaldehydeSensorConfig.MaxValue}`);
        return (p.Formaldehyde as number);
      }),
      distinct());
    this.voc$ = backend.SensorData$.pipe(
      filter(p => (typeof p.Voc) == 'number'),
      map(p => {
        //console.warn(`Formaldehyde:${(p.Formaldehyde as number)} max:${this.formaldehydeSensorConfig.MaxValue}`);
        return (p.Voc as number);
      }),
      distinct());
    this.co2$ = backend.SensorData$.pipe(
      filter(p => (typeof p.CO2) == 'number'),
      map(p => {
        //console.warn(`Formaldehyde:${(p.Formaldehyde as number)} max:${this.formaldehydeSensorConfig.MaxValue}`);
        return (p.CO2 as number);
      }),
      distinct());
  }
}
