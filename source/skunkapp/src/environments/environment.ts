import { SensorsConfig } from "./SensorsConfig";

const sensorsConfig: SensorsConfig = {
    Formaldehyde: {
        id: 'BZ',
        MaxValue: 32767 // (2^31) - 1 (short) max value
    },
    Voc: {
        id: 'VOC',
        MaxValue: 100000 //just a guess
    },
    Co2: {
        id: 'CO2',
        MaxValue: 10000 //just a guess
    }
};
export const environment = {
    isDevelopment: false,

    //no trailing slash here
    hostUrl: 'https://localhost:51339',

    //make sure this matches the hub config on the backend exactly
    hubPath: "/FrontendHub",
    sensors: sensorsConfig,
};
