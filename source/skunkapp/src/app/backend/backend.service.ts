import { Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr";
import { environment } from "../../environments/environment"
import { BehaviorSubject, Observable, Subject, distinctUntilChanged } from 'rxjs';
import { SensorPayload } from './SensorPayload';
import { SensorConfig, SensorsConfig } from '../../environments/SensorsConfig';
import { SensorStatPayload } from './SensorStatPayload';
import { SensorStats } from './SensorStats';

@Injectable({
  providedIn: 'root'
})
export class BackendService {

  readonly sensorsConfig: SensorsConfig = environment.sensors;
  readonly dummySensorConfig: SensorConfig = this.sensorsConfig.DummySensor ?? { id: 'unknown' };

  private _connection: signalR.HubConnection | undefined;
  readonly SensorData$: Observable<SensorPayload>;
  readonly SensorStats$: Observable<SensorStatPayload>;
  private readonly _sensorData: Subject<SensorPayload>;
  private readonly _sensorStats: Subject<SensorStatPayload>;
  private readonly _connected: BehaviorSubject<boolean>;
  readonly connected$: Observable<boolean>;
  get connected(): boolean {
    return this._connected.value;
  }
  private readonly _eth$: BehaviorSubject<number>;
  readonly Eth$: Observable<number>;
  private readonly _ethMax$: BehaviorSubject<number>;
  readonly EthMax$: Observable<number>;
  private readonly _ethAvg$: BehaviorSubject<number>;
  readonly EthAvg$: Observable<number>;
  constructor() {
    this._connected = new BehaviorSubject(false);
    this.connected$ = this._connected.asObservable();
    this._sensorStats = new Subject<SensorStatPayload>();
    this._sensorData = new Subject<SensorPayload>();
    this.SensorData$ = this._sensorData.asObservable();
    this.SensorStats$ = this._sensorStats.asObservable();
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
    });
    this._eth$ = new BehaviorSubject<number>(NaN);
    this.Eth$ = this._eth$
      .asObservable()
      .pipe(
        distinctUntilChanged()
      );
    this._ethMax$ = new BehaviorSubject<number>(NaN);
    this.EthMax$ = this._ethMax$
      .asObservable()
      .pipe(
        distinctUntilChanged()
      );
    this._ethAvg$ = new BehaviorSubject<number>(NaN);
    this.EthAvg$ = this._ethAvg$
      .asObservable()
      .pipe(
        distinctUntilChanged()
      );
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
      let ethValue = this.GetSensorValue(data, this.sensorsConfig.Eth);
      if(typeof ethValue == "number"){
        this._eth$.next(ethValue);
      }
    });
    this._connection?.on('SensorStatsToFrontEnd', async (data: SensorStatsDto[] | undefined) => {
      if (!data || data.length === 0) {
        //todo: log debug
        return;
      }
      let param: SensorStatPayload = {
        Formaldehyde: this.GetSensorStats(data, this.sensorsConfig.Formaldehyde),
        Co2: this.GetSensorStats(data, this.sensorsConfig.Co2),
        VoC: this.GetSensorStats(data, this.sensorsConfig.Voc),
      };
      this._sensorStats.next(param);
      let ethStats = this.GetSensorStats(data, this.sensorsConfig.Eth);
      this._ethAvg$.next(ethStats.Average);
      this._ethMax$.next(ethStats.Max);
    });
  }

  private GetSensorStats(data: SensorStatsDto[],
    sensorConfig: SensorConfig): SensorStats{
      let stats = data.find(d=>d.type === sensorConfig.id) ?? new SensorStatsDto;
      return {
        Type: stats.type ?? sensorConfig.id,
        Max: stats.max ?? NaN,
        Average: stats.average ?? NaN,
      };
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

class SensorStatsDto {
  type: string|undefined;
  max: number|undefined;
  average: number|undefined;
}
