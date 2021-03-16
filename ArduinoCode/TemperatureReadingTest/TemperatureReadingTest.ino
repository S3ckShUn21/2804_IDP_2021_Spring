#define TEMP_READ_PIN A0

#define TEMP_ZERO_DEGREES_OFFSET  500  // 500mV
#define TEMP_MV_PER_C             10   // 10mV / *C

char outputStr[32];

void setup() {
  Serial.begin(9600);
  pinMode(TEMP_READ_PIN, INPUT);
}

void loop() {

  // gather the data
  int reading = analogRead(TEMP_READ_PIN);
  float voltage_mV = reading * (5.0f/1024.0f);       // (5v / 1024setps) is the multiplier per step
  int temp = translateTemperature( voltage_mV );

  // format the output
  sprintf( outputStr, "%04d, %4.2f, %d", reading, voltage_mV, temp );
  Serial.println( outputStr );

  delay(100); // data collection @ 10Hz so you can actually read the data
  
}

// Based around the MCP 9700E
// Takes a voltage in milivolts and turn it into a temperature reading in *C
int translateTemperature( float voltage ) {
  float deltaV = voltage - (float)TEMP_ZERO_DEGREES_OFFSET; // get the voltage to the zero *C point
  int degreesC = round( deltaV / (float)TEMP_MV_PER_C );    // based on the slope, find the *C
  return degreesC;
}
