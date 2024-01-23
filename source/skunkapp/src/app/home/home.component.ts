import { Component } from '@angular/core';
import { TestTextComponent } from '../sensors/test-text/test-text.component';
import { BackendService } from '../backend/backend.service';
import { Observable, map } from 'rxjs';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'sk-home',
  standalone: true,
  imports: [TestTextComponent, CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  testValue: Observable<number>;
  constructor(
    private readonly backend: BackendService
  ) {
    this.testValue = backend.SensorData$.pipe(
      map(p => {
        console.info('sensor data', p);
        return p;
      }),
      map(p => {
        if (!Object.hasOwn(p.Sensors, 'Dummy Sensor') ||
          !Object.hasOwn(p.Sensors['Dummy Sensor'], 'Value -999')) {
          return 0;
        }
        return p.Sensors['Dummy Sensor']['Value -999'];
      }));
  }
}
