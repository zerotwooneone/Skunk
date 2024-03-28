import { SensorsConfig } from "./SensorsConfig";

const sensors: SensorsConfig = {
    Formaldehyde: {
        id: 'BZ',
        MaxValue: 32767 // (2^31) - 1 (short) max value
    },
    Voc: {
        id: 'VOC',
        MaxValue: 20000 //just a guess
    },
    Co2: {
        id: 'CO2',
        MaxValue: 4000
    },
    Eth: {
        id: 'ETH',
        MaxValue: 100
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
