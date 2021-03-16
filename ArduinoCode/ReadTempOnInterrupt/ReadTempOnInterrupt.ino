// Pin Defines
#define TEMP_READ_PIN             A0
// Temperature Defines
#define TEMP_ZERO_DEGREES_OFFSET  500.0f          // 500mV
#define TEMP_MV_PER_C             10.0f           // 10mV / *C
#define VOLTAGE_SCALER            4.8828125f      // 4.8 mV per analogReading
#define NUM_TEMP_SAMPLES          32 
// Interrupt Defines
#define INTERRUPT_PERIODS         15              // Total time = # * 2 (in sec)


volatile byte interruptCount = 0;
volatile float avgMeasurement = 0;
volatile bool sendData = false;


void setup() {
  // Basic setup
  Serial.begin(9600);
  pinMode(TEMP_READ_PIN, INPUT);

  // Setup Interrupt
  cli(); // Stop interrupts

  // Init interrupt on timer1 @ 0.5Hz
  TCCR1A = 0; // set entire TCCR1A register to 0
  TCCR1B = 0; // same for TCCR1B
  TCNT1  = 0; //initialize counter value to 0
  
  OCR1A = 31249;  // Compare Match Register Value
                  // This set the frequency to 0.5Hz

  // turn on CTC mode
  TCCR1B |= (1 << WGM12);
  // Set CS10 and CS12 bits for 1024 prescaler
  TCCR1B |= (1 << CS12) | (1 << CS10);  
  // enable timer compare interrupt
  TIMSK1 |= (1 << OCIE1A);

  sei(); // Re-enable interrupts

  //Serial.println("Setup complete");
  completeTemperatureReading();
}


void loop() {
  if( sendData ) {
    sendData = false;
    Serial.println( avgMeasurement );
  }
}


// Timer1 interrupt callback
ISR(TIMER1_COMPA_vect){
  // Increment the count and check if we hit the # periods
  if( ++interruptCount >= INTERRUPT_PERIODS ) {
    interruptCount = 0;
    completeTemperatureReading();
  }
}


void completeTemperatureReading(void) {
  avgMeasurement = 0;
  // Grab 4 readings
  for( byte i = 0; i < NUM_TEMP_SAMPLES; i++ ) {
    uint16_t reading = analogRead(TEMP_READ_PIN);
    avgMeasurement += translateTemperature( reading * VOLTAGE_SCALER ); 
  }
  avgMeasurement /=  NUM_TEMP_SAMPLES;
  sendData = true;
}


// Based on the MCP9700E
// Takes a voltage in milivolts and turns
// it into a temperature reading in *C 
float translateTemperature( float voltage ) {
  float deltaV = voltage - TEMP_ZERO_DEGREES_OFFSET;    // get the voltage to the zero *C point
  int degreesC = deltaV / TEMP_MV_PER_C;                // based on the slope, find the *C
  return degreesC;
}
