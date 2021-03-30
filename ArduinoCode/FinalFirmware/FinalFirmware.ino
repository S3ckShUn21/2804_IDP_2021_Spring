#include <SoftwareSerial.h>

// Pin Defines
#define TEMP_READ_PIN             A0
#define BOOST_READ_PIN            A5
#define BOOST_CTRL_PIN            11
#define BT_RX                     4
#define BT_TX                     5

// Temperature Defines
#define TEMP_ZERO_DEGREES_OFFSET  500.0f          // 500mV
#define TEMP_MV_PER_C             10.0f           // 10mV / *C
#define VOLTAGE_SCALER            4.8828125f      // 4.8 mV per analogReading
#define NUM_TEMP_SAMPLES          32

// Interrupt Defines
#define INTERRUPT_PERIODS         15              // Total time = # * 2 (in sec)

// Boost Converter Defines
#define BOOST_READING_REF         532             // ( (9.2/3.55) / 5 ) * 1023
#define BOOST_MAX_ERROR           5               // (5/1023) * (5V range) * (3.55 Division factor) = 0.09V
#define BOOST_FREQ                127             // 
#define BOOST_MAX_DUTY_CYCLE      154             // 60% => 60 / 100 * (256 range)         
#define BOOST_MIN_DUTY_CYCLE      26              // 10% => 10 / 100 * (256 range)

// Interrupt Vars
volatile byte interruptCount = 0;
volatile float avgMeasurement = 0;
volatile bool sendData = false;

// Boost Converter Vars
int dutyCycle = 127; // Start @ 50%

// BT Vars
SoftwareSerial BT(BT_RX, BT_TX);


void setup() {
  //
  // IO Setup
  //
  pinMode(TEMP_READ_PIN, INPUT);
  pinMode(BOOST_READ_PIN, INPUT);
  pinMode(BOOST_CTRL_PIN, OUTPUT);

  BT.begin(9600);
  BT.setTimeout(100); // Only wait 0.1 sec if reading data


  //
  // Setup Interrupt
  //
  cli(); // Stop interrupts

  // Init interrupt on timer1 @ 0.5Hz
  TCCR1A = 0; // set entire TCCR1A register to 0
  TCCR1B = 0; // same for TCCR1B
  TCNT1  = 0; //initialize counter value to 0
  
  OCR1A = 31249;  // Compare Match Register Value
                  // This set the frequency to 0.5Hz

  // turn on CTC mode
  TCCR1B |= (1 << WGM12);
  // set 1024 prescaler
  TCCR1B |= (1 << CS12) | (1 << CS10);  
  // enable timer compare interrupt
  TIMSK1 |= (1 << OCIE1A);

  sei(); // Re-enable interrupts


  //
  // Boost Converter Setup
  //
  TCCR2B = (TCCR2B & 0b11111000) | 0x01; // sets Timer2 frequency at 31372.55 Hz
  
  completeTemperatureReading();
}


void loop() {
  
  if( sendData ) {
    sendData = false;
    BT.print( F("[DATA]"));
    BT.println( avgMeasurement );
  }

  updateBoostConverter();
}


// Timer1 interrupt callback
ISR(TIMER1_COMPA_vect){
  // Increment the count and check if we hit the # periods
  if( ++interruptCount >= INTERRUPT_PERIODS ) {
    interruptCount = 0;
    completeTemperatureReading();
  }
}


// Gets a set of readings and averages them
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
// Takes a voltage in milivolts and turns it into a temperature reading in *C 
float translateTemperature( float voltage ) {
  float deltaV = voltage - TEMP_ZERO_DEGREES_OFFSET;    // get the voltage to the zero *C point
  int degreesC = deltaV / TEMP_MV_PER_C;                // based on the slope, find the *C
  return degreesC;
}

void updateBoostConverter(void) {
  int boostOut = analogRead(BOOST_READ_PIN);
  
  if( boostOut < (BOOST_READING_REF - BOOST_MAX_ERROR) && dutyCycle < BOOST_MAX_DUTY_CYCLE ) {
    dutyCycle++; 
  } else if( boostOut > (BOOST_READING_REF + BOOST_MAX_ERROR) && dutyCycle > BOOST_MIN_DUTY_CYCLE) {
    dutyCycle--;
  }

  analogWrite(BOOST_CTRL_PIN, dutyCycle);
}
