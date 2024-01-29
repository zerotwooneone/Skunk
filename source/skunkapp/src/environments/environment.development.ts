import { SensorsConfig } from "./SensorsConfig";

const sensors: SensorsConfig = {
    DummySensor: {
        id: 'Dummy Sensor',
        MinValue: 0,
        MaxValue: 1,
    },
    Formaldehyde: {
        id: 'Formaldehyde',
        MaxValue: 32767 // (2^31) - 1 (short) max value
    }
};
export const environment = {
    isDevelopment: true,

    //no trailing slash here
    hostUrl: 'https://localhost:51339',

    //make sure this matches the hub config on the backend exactly
    hubPath: "/FrontendHub",
    sensors: sensors,
};
