#include<SoftwareSerial.h>

#define BUTTON 12

unsigned long lastButtonTime;
unsigned long currentTime;
bool canPress;
bool currentButtonState;

SoftwareSerial BT(2,3); // Setup the BT module as a serial device 
                        // Pin 2 = RX, Pin 3 = TX

void setup() {
  Serial.begin(9600);
  Serial.setTimeout(5000); // Sets the "readStringUntil" timeout to 5 seconds

  BT.begin(9600);
  BT.println("[BT INFO] : BT Connected");

  pinMode(BUTTON, INPUT_PULLUP);  
}

void loop() {
  if( BT.available() ) {
    // If there is text in the buffer it will read until the newline
    String readText = BT.readStringUntil('\n'); 
    Serial.print(F("[Serial Loopback] : "));
    Serial.println(readText);
    BT.print(F("[BT Loopback] : "));
    BT.println(readText);
  }

  currentTime = millis();
  currentButtonState = digitalRead(BUTTON);
  
  if(!currentButtonState && canPress) {
    Serial.print(F("[Serial INFO] : Current Runtime = "));
    Serial.println(currentTime, DEC);
    BT.print(F("[BT INFO] : Current Runtime = "));
    BT.println(currentTime, DEC);
    lastButtonTime = currentTime;
    canPress = false;
  }

  if(currentButtonState && currentTime > lastButtonTime + 100) {
    canPress = true;
  } 
}
