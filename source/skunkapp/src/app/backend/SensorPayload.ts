export class SensorPayload {
    /**Relative Formaldehyde measurement. There is no unit on this*/
    readonly Formaldehyde?: number;
    
    /**Total Volitile Organic Compounds measured in ppb */
    readonly Voc?: number;

    /**Carbon Dioxide measured in ppm */
    readonly CO2?: number;

    /**The time the measurement was collected */
    readonly TimeStamp?: Date;

    /**Fake data for testing values are from 0-1 */
    readonly DummySensor?: number;
}

