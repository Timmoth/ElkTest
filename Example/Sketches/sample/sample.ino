void setup() {
  pinMode(20, OUTPUT);
}

bool isPinHigh = false;
void loop() {
  if (isPinHigh) {
    digitalWrite(20, HIGH);
  } else {
    digitalWrite(20, LOW);
  }

isPinHigh = !isPinHigh;
  delay(100);
}