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
  BT.setTimeout(5000); // also set it for the Bluetooth
  BT.println("[BT INFO] : BT Connected");

  pinMode(BUTTON, INPUT_PULLUP);  
}

void loop() {
  String readText; // Used to store text coming from the ports

  // Check to see if text came from either BT or Serial
  // BT has priority over Serial coms
  if( BT.available() ) {
    readText = BT.readStringUntil('\n'); // If there is text in the buffer it will read until the newline
  } else if ( Serial.available() ) {
    readText = Serial.readStringUntil('\n'); // Same ^^
  }

  // If we recieved something from one of the ports, send out on both
  if( readText.length() ) {
    Serial.print(F("[Serial Loopback] : "));
    Serial.println(readText);
    BT.print(F("[BT Loopback] : "));
    BT.println(readText);
  }

  // Timing for button debounce
  currentTime = millis();
  currentButtonState = digitalRead(BUTTON);

  // If the button was pressed
  if(!currentButtonState && canPress) {
    Serial.print(F("[Serial INFO] : Current Runtime = "));
    Serial.println(currentTime, DEC);
    BT.print(F("[BT INFO] : Current Runtime = "));
    BT.println(currentTime, DEC);
    lastButtonTime = currentTime;
    canPress = false;
  }

  // Do button debouncing
  if(currentButtonState && currentTime > lastButtonTime + 100) {
    canPress = true;
  } 
}
