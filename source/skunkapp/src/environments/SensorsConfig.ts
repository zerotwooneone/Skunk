export type SensorsConfig = {
    readonly [key: string]: SensorConfig | undefined;
    readonly DummySensor?: SensorConfig;
    readonly Formaldehyde: SensorConfig;
    readonly Voc: SensorConfig;
    readonly Co2: SensorConfig;
};

export type NumericConfigValues = { readonly [key: string]: number | undefined };
export type SensorConfig = {
    readonly id: string,
    readonly numbers?: NumericConfigValues,
    readonly MinValue?: number,
    readonly MaxValue?: number,
};
