#define BUTTON 12

unsigned long lastButtonTime;
unsigned long currentTime;
bool canPress;
bool currentButtonState;

void setup() {
  Serial.begin(9600);
  Serial.setTimeout(5000); // Sets the "readStringUntil" timeout to 5 seconds

  pinMode(BUTTON, INPUT_PULLUP);  
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

  if(currentButtonState && currentTime > lastButtonTime + 100) {
    canPress = true;
  } 
}
