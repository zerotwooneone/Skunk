import { Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr";
import { environment } from "../../environments/environment"
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { SensorPayload } from './SensorPayload';
import { SensorConfig, SensorsConfig } from '../../environments/SensorsConfig';

@Injectable({
  providedIn: 'root'
})
export class BackendService {

  readonly sensorsConfig: SensorsConfig = environment.sensors;
  readonly dummySensorConfig: SensorConfig = this.sensorsConfig.DummySensor ?? { id: 'unknown' };

  private _connection: signalR.HubConnection | undefined;
  readonly SensorData$: Observable<SensorPayload>;
  private readonly _sensorData: Subject<SensorPayload>;
  private readonly _connected: BehaviorSubject<boolean>;
  readonly connected$: Observable<boolean>;
  get connected(): boolean {
    return this._connected.value;
  }
  constructor() {
    this._connected = new BehaviorSubject(false);
    this.connected$ = this._connected.asObservable();
    this._sensorData = new Subject<SensorPayload>();
    this.SensorData$ = this._sensorData.asObservable();
    const defaultBuilder = new signalR.HubConnectionBuilder()
      .withAutomaticReconnect()
      .withKeepAliveInterval(5000)
      .withUrl(`${environment.hostUrl}${environment.hubPath}`);
    const timeoutBuilder = environment.isDevelopment
      ? defaultBuilder
      : defaultBuilder.withServerTimeout(30000);

    const builder = timeoutBuilder;
    this._connection = builder.build();
    this._connection.onclose(error => {
      this._connected.next(false);
      console.warn(`hub connection closed.`, error);
    });
    this._connection.onreconnected(connectionId => {
      console.info('reconnected', connectionId);
      this._connected.next(true);
    })
  }

  async connect(): Promise<boolean> {
    if (this.connected) {
      console.warn(`already connected`);
      return this.connected;
    }
    if (!this._connection) {
      return false;
    }
    try {
      await this._connection.start();
    } catch (exception) {
      console.error("error connecting", exception);
      return false;
    }

    try {
      this.registerHandlers();
    } catch (exception) {
      console.error("error registering handlers", exception);
    }
    this._connected.next(true);
    return this.connected;
  }

  private registerHandlers(): void {
    this._connection?.on('PingFrontEnd', async () => {
      try {
        await this._connection?.invoke('PongBackend');
      } catch (e) {
        console.error("error sending pong to backend", e);
      }
    });

    this._connection?.on('PongFrontEnd', async () => {
      console.info('got pong from backend');
    });

    this._connection?.on('SensorDataToFrontend', async (data: SensorPayloadDto | undefined) => {
      if (!data) {
        //todo: log debug
        return;
      }
      const timeStampUnixMs = data.sensors?.["timeStamp"]?.["value"];
      const timeStamp = !!timeStampUnixMs
        ? new Date(timeStampUnixMs)
        : undefined;
      const payload: SensorPayload = {
        Formaldehyde: this.GetSensorValue(data, this.sensorsConfig.Formaldehyde),
        Voc: this.GetSensorValue(data, this.sensorsConfig.Voc),
        CO2: this.GetSensorValue(data, this.sensorsConfig.Co2), 
        DummySensor: this.GetSensorValue(data, this.sensorsConfig.DummySensor, "Value -999"),
        TimeStamp: timeStamp
      };
      this._sensorData.next(payload);
    });
  }

  private GetSensorValue(
    data: SensorPayloadDto, 
    sensorConfig?: SensorConfig,
    valueKey: string = "value"): number|undefined {
    
    return !!sensorConfig
      ? data.sensors?.[sensorConfig.id]?.[valueKey]
      : undefined;
  }

  async ping(): Promise<boolean> {
    if (!this.connected) {
      return false;
    }
    if (!this._connection) {
      return false;
    }
    try {
      await this._connection?.invoke('PingBackend');
    } catch (exception) {
      console.error('problem with ping', exception);
      return false;
    }
    return true;
  }
}

class SensorPayloadDto {
  readonly sensors?: SensorCollectionDto;
}

class SensorCollectionDto {
  [key: string]: SensorValueDto | undefined;
}

class SensorValueDto {
  [key: string]: number | undefined;
}
