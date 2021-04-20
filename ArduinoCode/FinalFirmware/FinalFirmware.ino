#include <avr/sleep.h>
#include <avr/power.h>

#include <SoftwareSerial.h>

// Pin Defines
#define TEMP_READ_PIN             A0
#define BOOST_READ_PIN            A5
#define BOOST_CTRL_PIN            11
#define BT_RX                     4
#define BT_TX                     5

// Temperature Defines
#define TEMP_ZERO_DEGREES_OFFSET  540.0f          // 540mV for our specific temp sensor
#define TEMP_MV_PER_C             10.0f           // 10mV / *C
#define VOLTAGE_SCALER            4.8828125f      // 4.8 mV per analogReading
#define NUM_TEMP_SAMPLES          32

// Interrupt Defines
#define INTERRUPT_PERIODS         15              // Total time = # * 2 (in sec)

// Boost Converter Defines
#define BOOST_READING_9V          519             // ( (9.0/3.55) / 5 ) * 1024
#define BOOST_READING_8_5_V       490             // ( (8.5/3.55) / 5 ) * 1024
#define BOOST_READING_REF         532             // ( (9.2/3.55) / 5 ) * 1024 ~~~~ +- some error in the resistor vals
#define BOOST_MAX_ERROR           4               // (3/1023) * (5V range) * (3.55 Division factor) = 0.052V
#define BOOST_MAX_DUTY_CYCLE      154             // 60% => 60 / 100 * (256 range)
#define BOOST_AVG_DUTY_CYCLE      141             // 55% => 55 / 100 * (256 range)
#define BOOST_MIN_DUTY_CYCLE      128             // 50% => 50 / 100 * (256 range)

// Interrupt Vars
volatile byte interruptCount = 0;
volatile float avgMeasurement = 0;
volatile bool sleepEnabled = false;

// Boost Converter Vars
int dutyCycle = 127; // Start @ 50%

// BT Vars
SoftwareSerial BT(BT_RX, BT_TX);


void setup() {
  //
  // IO Setup
  //
  Serial.begin(9600);
  pinMode(TEMP_READ_PIN, INPUT);
  pinMode(BOOST_READ_PIN, INPUT);
  pinMode(BOOST_CTRL_PIN, OUTPUT);
  
  pinMode(LED_BUILTIN, OUTPUT);
  digitalWrite(LED_BUILTIN, LOW);

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
  if( sleepEnabled ) {
    BT.println("SLEEP MODE");
    digitalWrite(LED_BUILTIN, LOW);
    GoToSleep();
    ExitSleep();
  } else {
    digitalWrite(LED_BUILTIN, HIGH);
    updateBoostConverter();
  }
}

////////////////////////////////////////////////////////
//
//  Interrupt callback
//
////////////////////////////////////////////////////////

ISR(TIMER1_COMPA_vect){
  
  // Increment the count and check if we hit the # periods
  if( ++interruptCount >= INTERRUPT_PERIODS ) {
    interruptCount = 0;
    // Send a measurement
    completeTemperatureReading();
    BT.print( F("[DATA]"));
    BT.println( avgMeasurement );
  }

  // Every 2 seconds we will check the battery voltage
  // to see if we need to turn on the boost converter
  checkVoltage();
}

////////////////////////////////////////////////////////
//
//  Temperature Functions
//
////////////////////////////////////////////////////////

// Gets a set of readings and averages them
void completeTemperatureReading(void) {
  float measurementBuffer[NUM_TEMP_SAMPLES];
  avgMeasurement = 0;
  
  //
  // Grab NUM_TEMP_SAMPLES readings
  //
  for( byte i = 0; i < NUM_TEMP_SAMPLES; i++ ) {
    float reading = analogRead(TEMP_READ_PIN);
    float translatedTemp = translateTemperature( reading * VOLTAGE_SCALER );
    measurementBuffer[i] = translatedTemp;
    delayMicroseconds(100); // hopefully this gives the ACD time to chill ??
  }

  //
  // Remove the top and bottom two outlier samples
  //
  byte firstH = 0;          // i of the top sample
  byte secondH = 0;         // i of the 2nd highest sample
  byte firstL = 0;          // i of the bottom sample
  byte secondL = 0;         // i of the 2nd smallest sample
  
  for( byte i = 0; i < NUM_TEMP_SAMPLES; i++ ) {
    float var = measurementBuffer[i];
    if( var > measurementBuffer[firstH] ) { // High Check
      secondH = firstH;
      firstH = i;
    }
    if( var < measurementBuffer[firstL] ) { // Low Check
      secondL = firstL;
      firstL = i;
    }
  }
  measurementBuffer[firstH] = 0;
  measurementBuffer[secondH] = 0;
  measurementBuffer[firstL] = 0;
  measurementBuffer[secondL] = 0;
  
  //
  // Average the filtered data set
  //
  for( byte i = 0; i < NUM_TEMP_SAMPLES; i++ ) {
    avgMeasurement += measurementBuffer[i];
  }
  avgMeasurement /= (NUM_TEMP_SAMPLES - 4); // Sub 4 bc we filter the top/bottom 2 outliers
}


// Based on the MCP9700E
// Takes a voltage in milivolts and turns it into a temperature reading in *C 
float translateTemperature( float voltage ) {
  float deltaV = voltage - TEMP_ZERO_DEGREES_OFFSET;      // get the voltage to the zero *C point
  float degreesC = deltaV / TEMP_MV_PER_C;                // based on the slope, find the *C
  return degreesC;
}

////////////////////////////////////////////////////////
//
//  Boost Converter Functions
//
////////////////////////////////////////////////////////

void updateBoostConverter(void) {
  int boostOut = 0;
  for( int i = 0; i < 16; i++ ) {
    boostOut += analogRead(BOOST_READ_PIN);  
  }
  boostOut >>= 4; // divide by 16
  
  if( boostOut < (BOOST_READING_REF - BOOST_MAX_ERROR) ) {
    dutyCycle = BOOST_MAX_DUTY_CYCLE; 
  } else if( boostOut > (BOOST_READING_REF + BOOST_MAX_ERROR) ) {
    dutyCycle = BOOST_MIN_DUTY_CYCLE;
  } else {
    dutyCycle = BOOST_AVG_DUTY_CYCLE;
  }
  
  //Serial.print(dutyCycle);
  //Serial.print(", ");
  //Serial.println(boostOut);
  analogWrite(BOOST_CTRL_PIN, dutyCycle);
}

// If the voltage is high enough we will stay in sleep mode
// otherwise we will turn on the boost converter
void checkVoltage(void) {
  unsigned int reading = analogRead(BOOST_READ_PIN);
  sleepEnabled = (reading > BOOST_READING_8_5_V);
}

////////////////////////////////////////////////////////
//
//  Sleep Functions
//
////////////////////////////////////////////////////////

void GoToSleep()
{
  set_sleep_mode(SLEEP_MODE_IDLE);
  sleep_enable();

  // Turn off the boost converter
  analogWrite(BOOST_CTRL_PIN, 0);

  // Disable certain timers and other peripherals
  power_timer0_disable();
  power_timer2_disable();
  power_spi_disable();
  power_adc_disable();
  power_twi_disable();

  sleep_mode();
}

void ExitSleep()
{
  // Exit sleep and restore power to peripherals
  sleep_disable();
  power_all_enable();
}
