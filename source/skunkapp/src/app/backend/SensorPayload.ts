export class SensorPayload {
    readonly Sensors: SensorCollection = {};
}

export class SensorCollection {
    readonly [key: string]: SensorValue;
}

export class SensorValue {
    readonly [key: string]: number;
}