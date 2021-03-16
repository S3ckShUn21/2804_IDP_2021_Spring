#define TEMP_READ_PIN A0

#define TEMP_ZERO_DEGREES_OFFSET  500  // 500mV
#define TEMP_MV_PER_C             10   // 10mV / *C

#define VOLTAGE_SCALER 4.8828125f      // 4.8 mV per analogReading

char outputStr[24];

void setup() {
  Serial.begin(9600);
  pinMode(TEMP_READ_PIN, INPUT);
  Serial.println("Reading, Voltage(mV), Temperature(*C)");
}

void loop() {

  // gather the data
  int reading = analogRead(TEMP_READ_PIN);
  float voltage_mV = reading * VOLTAGE_SCALER;
  int temp = translateTemperature( voltage_mV );

  // format the output
  sprintf( outputStr, "%04d, %04d, %d", reading, (int)voltage_mV, temp );
  Serial.println( outputStr );

  delay(100); // data collection @ 10Hz so you can actually read the data
  
}

// Based on the MCP9700E
// Takes a voltage in milivolts and turn it into a temperature reading in *C
int translateTemperature( float voltage ) {
  float deltaV = voltage - (float)TEMP_ZERO_DEGREES_OFFSET; // get the voltage to the zero *C point
  int degreesC = round( deltaV / (float)TEMP_MV_PER_C );    // based on the slope, find the *C
  return degreesC;
}
