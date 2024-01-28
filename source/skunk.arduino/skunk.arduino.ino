int led = LED_BUILTIN; //13 for teensy
int a7Value = 0;

// the setup routine runs once when you press reset:
void setup() {
  Serial.begin(9600); // opens serial port, sets data rate to 9600 bps
  // initialize the digital pin as an output.
  pinMode(led, OUTPUT);
  Serial.println();
  Serial.println("begin");
}

// the loop routine runs over and over again forever:
void loop() {
  a7Value = analogRead(A5);
  Serial.print("BZ:");
  Serial.println(a7Value);
  //digitalWrite(led, HIGH);   // turn the LED on (HIGH is the voltage level)
  //delay(1000);               // wait for a second
  //digitalWrite(led, LOW);    // turn the LED off by making the voltage LOW
  delay(1000);               // wait for a second
}