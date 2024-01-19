import { Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr";
import { environment } from "../../environments/environment"

@Injectable({
  providedIn: 'root'
})
export class BackendService {

  private _connected: Boolean = false;
  private _connection: signalR.HubConnection | undefined;
  constructor() {
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
      this._connected = false;
      console.warn(`hub connection closed.`, error);
    });
    this._connection.onreconnected(connectionId => {
      console.info('reconnected', connectionId);
      this._connected = true;
    })
  }

  async connect(): Promise<undefined> {
    if (this._connected) {
      console.warn(`already connected`);
      return;
    }
    await this._connection?.start();

    this.registerHandlers();

    this._connected = true;
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
  }

  async ping(): Promise<undefined> {
    await this._connection?.invoke('PingBackend');
  }
}
