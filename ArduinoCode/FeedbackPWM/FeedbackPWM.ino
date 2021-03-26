int pwm_pin = 9;
double pwm_freq = 127;
double duty_cycle = 50;
int boost_pin = A5; // subject to change
int boost_out;
const int reference = 532; // ((9.2/3.55)/5)*1023
const int error = 5; // (5/1023)*(5 V range)*(3.55 Division factor) is 0.09 V error

void setup() {
  Serial.begin(9600);
  TCCR1B = (TCCR1B & 0b11111000) | 0x01; // sets frequency at 31372.55 Hz 
  pinMode(pwm_pin, OUTPUT);
  
}

void loop() {
  // delay(64000) or 64000 millis() ~ 1 second

  analogWrite(pwm_pin, (int)pwm_freq);
  boost_out = analogRead(boost_pin);

  if ((boost_out < (reference - error)) && (duty_cycle < 60))
  {
    pwm_freq = pwm_freq + 1;
  }
  else if (boost_out > (reference + error) && (duty_cycle > 10))
  {
    pwm_freq = pwm_freq - 1;
  }

  duty_cycle = (pwm_freq/255)*100;
  Serial.print(duty_cycle);
  
}
