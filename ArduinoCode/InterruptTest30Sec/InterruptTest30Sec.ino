#define PIN 4
#define INTERRUPT_PERIODS 15 // Interrupt_Period = # * 2 (in sec)

volatile byte interruptCount = 0;
volatile bool statusLED = 0;

void setup() {

  pinMode(PIN, OUTPUT);
  digitalWrite(PIN, LOW);

  cli(); // Stop interrupts from proccessing

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

  sei(); // Enable interrupts

}

void loop() {

  // Nothing in the loop

}

// Timer1 interrupt callback (just pulsing the output of PIN)
ISR(TIMER1_COMPA_vect){
  // Increment the count and check if we hit the # periods
  if( ++interruptCount >= INTERRUPT_PERIODS ) {
    interruptCount = 0;
    statusLED = !statusLED;
    digitalWrite(PIN, statusLED);
  }
}
