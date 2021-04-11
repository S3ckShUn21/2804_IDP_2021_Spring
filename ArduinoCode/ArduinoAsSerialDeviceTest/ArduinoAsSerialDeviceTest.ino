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
    Serial.print(F("[INFO] : Current Runtime = "));
    Serial.println(currentTime, DEC);
    lastButtonTime = currentTime;
    canPress = false;
  }

  if(currentButtonState && currentTime > lastButtonTime + DEBOUNCE_PERIOD) {
    canPress = true;
  } 
}
