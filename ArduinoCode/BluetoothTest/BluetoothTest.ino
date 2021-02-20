#include<SoftwareSerial.h>

// Cole added a comment

int answer = 0;
SoftwareSerial bluetooth(2, 3);

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  bluetooth.begin(9600);
  
}

void loop() {
  // put your main code here, to run repeatedly:

  Serial.println("Would you like to send data? Enter '1' for yes or else for no.: ");
  Serial.println("\n");
while (Serial.available() == 0) {
    // Wait for input
  }
  answer = Serial.parseInt();

  if (answer == 1)
  {
    // send data
    bluetooth.println("Data recieved. ");
    Serial.println("Data sent. ");
    answer = 0;
  }
  
}