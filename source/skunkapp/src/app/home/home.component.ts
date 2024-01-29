import { Component } from '@angular/core';
import { TestTextComponent } from '../sensors/test-text/test-text.component';
import { BackendService } from '../backend/backend.service';
import { Observable, distinct, filter, map } from 'rxjs';
import { CommonModule } from '@angular/common';
import { environment } from '../../environments/environment';
import { SensorsConfig, SensorConfig } from "../../environments/SensorsConfig";
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { KnobModule } from 'primeng/knob';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'sk-home',
  standalone: true,
  imports: [TestTextComponent, CommonModule, MatProgressBarModule, KnobModule, FormsModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  readonly sensorsConfig: SensorsConfig = environment.sensors;
  readonly dummySensorConfig: SensorConfig = this.sensorsConfig.DummySensor ?? { id: 'unknown' };
  readonly dummySensorValue$: Observable<number | undefined>;
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
    const dummySensorPayload = backend.SensorData$.pipe(
      filter(p => Object.hasOwn(p.Sensors, this.dummySensorConfig.id)),
      map(p => {
        return p.Sensors[this.dummySensorConfig.id];
      }));
    this.dummySensorValue$ = dummySensorPayload.pipe(
      map(p => {
        //incoming value is between 0 and 1
        //output value from between 0 and 100
        if (!Object.hasOwn(p, 'Value -999') ||
          typeof p['Value -999'] !== 'number') {
          return undefined;
        }
        return p['Value -999'] * 100;
      })
    );
    this.formaldehydeSensorValue$ = backend.SensorData$.pipe(
      filter(p => (typeof p.Formaldehyde) == 'number'),
      map(p => {
        //console.warn(`Formaldehyde:${(p.Formaldehyde as number)} max:${this.formaldehydeSensorConfig.MaxValue}`);
        return (p.Formaldehyde as number);
      }),
      distinct());
  }
}
