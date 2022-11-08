void setup() {  
  //analog
  pinMode(28, INPUT);
  pinMode(27, OUTPUT);

  //digital
  pinMode(21, INPUT);
  pinMode(20, OUTPUT);
}

void loop() {
  int analogValue = analogRead(28);
  analogWrite(27, analogValue);

  int digitalValue = digitalRead(21);
  digitalWrite(20, digitalValue);
}