import { SensorStats } from "./SensorStats";

export class SensorStatPayload {
    readonly Formaldehyde: SensorStats = new SensorStats;
    readonly Co2 = new SensorStats;
    readonly VoC = new SensorStats;
}


