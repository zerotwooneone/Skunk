import { Injectable } from '@angular/core';
import { BackendService } from '../backend/backend.service';
import { BehaviorSubject, Subject } from 'rxjs';
import { SensorPayload } from '../backend/SensorPayload';

@Injectable({
  providedIn: 'root'
})
export class SimulatorService {
  subject?: Subject<SensorPayload>;
  private _simActive: boolean = false;
  get simActive(): boolean {
    return this._simActive;
  }

  constructor(private readonly backendService: BackendService) { }

  startSim(): void {
    this._simActive = true;

    //todo: improve this terrible hack
    const bes = this.backendService as any;
    this.subject = bes._sensorData;

    const connected:BehaviorSubject<boolean> = bes._connected;
    connected.next(true);
  }

  stopSim(): void {
    this._simActive = false;

    //todo: improve this terrible hack
    const bes = this.backendService as any;

    const connected: BehaviorSubject<boolean> = bes._connected;
    connected.next(false);
  }
}
