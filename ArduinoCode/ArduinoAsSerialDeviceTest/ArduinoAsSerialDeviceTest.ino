#define BUTTON          A2
#define GROUND_PIN      A0

#define DEBOUNCE_PERIOD 250

unsigned long lastButtonTime;
unsigned long currentTime;
bool canPress;
bool currentButtonState;

void setup() {
  Serial.begin(9600);
  Serial.setTimeout(5000); // Sets the "readStringUntil" timeout to 5 seconds

  pinMode(BUTTON, INPUT_PULLUP);

  pinMode(GROUND_PIN, OUTPUT);
  digitalWrite(GROUND_PIN, LOW);

  // seed the random number generation
  randomSeed(analogRead(A4));
}

void loop() {
  if( Serial.available() ) {
    // If there is text in the buffer it will read until the newline
    String readText = Serial.readStringUntil('\n'); 
    Serial.print(F("[Loopback] : "));
    Serial.println(readText);
  }

  currentTime = millis();
  currentButtonState = digitalRead(BUTTON);
  
  if(!currentButtonState && canPress) {
    
    char format[40] = "[INFO] : Current Runtime = %lu";
    char strBuffer[40];
    sprintf(strBuffer, format, currentTime);
    Serial.println(strBuffer);
    delay(200);
    float data = 24 + (random(100) / 10.0f) - 5.0;
    Serial.print("[DATA]");
    Serial.println(data);
    
    lastButtonTime = currentTime;
    canPress = false;
  }

  if(currentButtonState && currentTime > lastButtonTime + DEBOUNCE_PERIOD) {
    canPress = true;
  } 
}
