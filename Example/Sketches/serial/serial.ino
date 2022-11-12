void setup() {

  // UART0
  Serial1.setTX(0);
  Serial1.setRX(1);
  Serial1.begin(9600);

  // USB
  Serial.begin(9600);
}

void loop() {
  String input = "";
  if (Serial.available() > 0) {
    input = Serial.readStringUntil('\0');
  }

  if (Serial1.available() > 0) {
    input = Serial1.readStringUntil('\0');
  }

  if (input == "") {
    return;
  }

  input.replace("\n", "");
  input.replace("\r", "");

  Serial.println("usb: '" + input + "'");
  Serial1.println("uart: '" + input + "'");
}