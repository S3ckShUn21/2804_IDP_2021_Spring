int pwm_pin = 9;

void setup() {
  // put your setup code here, to run once:
  TCCR1B = (TCCR1B & 0b11111000) | 0x01; // sets frequency at 31372.55 Hz 
  Serial.begin(9600);
  
}

void loop() {
  // put your main code here, to run repeatedly:
  // delay(64000) or 64000 millis() ~ 1 second

  pinMode(pwm_pin, OUTPUT);
  analogWrite(pwm_pin, 127);
  
}
