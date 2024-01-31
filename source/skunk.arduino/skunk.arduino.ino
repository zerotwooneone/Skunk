#include <Wire.h>
#include "SGP30.h"

int led = LED_BUILTIN; //13 for teensy
SGP30 SGP;//create an object of the SGP30 class
const float relativeHumidityPct = 45;
const float tempC = 26;
bool ledOn = false;


/* return absolute humidity [mg/m^3] with approximation formula
* @param temperature [°C]
* @param humidity [%RH]
*/
uint32_t getAbsoluteHumidity(float temperature, float humidity) {
    // approximation formula from Sensirion SGP30 Driver Integration chapter 3.15
    const float absoluteHumidity = 216.7f * ((humidity / 100.0f) * 6.112f * exp((17.62f * temperature) / (243.12f + temperature)) / (273.15f + temperature)); // [g/m^3]
    const uint32_t absoluteHumidityScaled = static_cast<uint32_t>(1000.0f * absoluteHumidity); // [mg/m^3]
    return absoluteHumidityScaled;
}

// the setup routine runs once when you press reset:
void setup() {
  Serial.begin(9600); // opens serial port, sets data rate to 9600 bps

  while (!Serial) { delay(10); } // Wait for serial console to open!

  Wire.begin();  

  // initialize the digital pin as an output.
  pinMode(led, OUTPUT);
  Serial.println();
  Serial.println("begin");

  Serial.print("SGP:");
  Serial.println(SGP.begin());
  Serial.print("SGP.measureTest:");
  Serial.println(SGP.measureTest());

  Serial.print("SGP.setRelHumidity:");
  Serial.println(SGP.setRelHumidity(tempC, relativeHumidityPct), HEX);

  SGP.GenericReset(); //this may reset ALL I2C that support this on the bus
}

// the loop routine runs over and over again forever:
void loop() {
  if(SGP.measure(true)){     //  returns false if no measurement is made 
    Serial.print("VOC:");
    Serial.println(SGP.getTVOC());
    Serial.print("CO2:");
    Serial.println(SGP.getCO2());
    Serial.print("H2:");
    Serial.println(SGP.getH2());
    Serial.print("ETH:");
    Serial.println(SGP.getEthanol());
  } 
  Serial.print("BZ:");
  Serial.println(analogRead(A1));
  digitalWrite(led, ledOn);
  ledOn = !ledOn;
  delay(1000);               // wait for a second  
}