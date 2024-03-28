import { Component } from '@angular/core';
import { TestTextComponent } from '../sensors/test-text/test-text.component';
import { BackendService } from '../backend/backend.service';
import { Observable, distinctUntilChanged, filter, map } from 'rxjs';
import { CommonModule } from '@angular/common';
import { environment } from '../../environments/environment';
import { SensorsConfig, SensorConfig } from "../../environments/SensorsConfig";
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { FormsModule } from '@angular/forms';
import { KnobComponent } from '../knob/knob';
import { StatsHack } from './StatsHack';

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
  readonly co2Stats: StatsHack;
  readonly vocStats: StatsHack;
  readonly formStats: StatsHack;
  
  readonly bzMax$: Observable<number>;
  readonly bzAvg$: Observable<number>;
  readonly vocMax$: Observable<number>;
  readonly vocAvg$: Observable<number>;
  readonly co2Max$: Observable<number>;
  readonly co2Avg$: Observable<number>;
  
  constructor(
    private readonly backend: BackendService
  ) {
    this.co2Stats = new StatsHack(400);
    this.vocStats = new StatsHack(400);
    this.formStats = new StatsHack(400);
    /*backend.SensorData$.subscribe(
      p => {
        console.info('sensor data', p);
        return p;
      });*/
    
    this.bzMax$ = this.backend.SensorStats$.pipe(
      filter(s => !!s.Formaldehyde && !Number.isNaN(s.Formaldehyde.Max)),
      map(s=>s.Formaldehyde.Max),
      distinctUntilChanged()
    );
    this.bzAvg$ = this.backend.SensorStats$.pipe(
      filter(s => !!s.Formaldehyde && !Number.isNaN(s.Formaldehyde.Average)),
      map(s => s.Formaldehyde.Average),
      distinctUntilChanged()
    );

    this.vocMax$ = this.backend.SensorStats$.pipe(
      filter(s => !!s.VoC && !Number.isNaN(s.VoC.Max)),
      map(s => s.VoC.Max),
      distinctUntilChanged()
    );
    this.vocAvg$ = this.backend.SensorStats$.pipe(
      filter(s => !!s.VoC && !Number.isNaN(s.VoC.Average)),
      map(s => s.VoC.Average),
      distinctUntilChanged()
    );

    this.co2Max$ = this.backend.SensorStats$.pipe(
      filter(s => !!s.Co2 && !Number.isNaN(s.Co2.Max)),
      map(s => s.Co2.Max),
      distinctUntilChanged()
    );
    this.co2Avg$ = this.backend.SensorStats$.pipe(
      filter(s => !!s.Co2 && !Number.isNaN(s.Co2.Average)),
      map(s => s.Co2.Average),
      distinctUntilChanged()
    );

    this.dummySensorValue$ = backend.SensorData$.pipe(
      filter(p => typeof p.DummySensor == 'number'),
      map(p => {
        return (p.DummySensor as number) * 100;
      }));
    this.formaldehydeSensorValue$ = backend.SensorData$.pipe(
      filter(p => (typeof p.Formaldehyde) == 'number'),
      map(p => {
        const formValue = p.Formaldehyde as number;
        this.formStats.addSample(formValue);
        return formValue;
      }),
      distinctUntilChanged());
    this.voc$ = backend.SensorData$.pipe(
      filter(p => (typeof p.Voc) == 'number'),
      map(p => {
        const newVoc = p.Voc as number;
        this.vocStats.addSample(newVoc);
        return newVoc;
      }),
      distinctUntilChanged());
    this.co2$ = backend.SensorData$.pipe(
      filter(p => (typeof p.CO2) == 'number'),
      map(p => {
        const newCo2 = p.CO2 as number;
        this.co2Stats.addSample(newCo2);
        return newCo2;
      }),
      distinctUntilChanged());
  }
}


