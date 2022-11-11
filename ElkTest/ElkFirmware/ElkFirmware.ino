#include "ElkRequest.h"

static const int Ok = 200;
static const int NotFound = 404;

void setup() {
  Serial.begin(9600);

  Serial2.setTX(4);
  Serial2.setRX(5);
  Serial2.begin(9600);

  while (!Serial) {
    ;  // wait for serial port to connect. Needed for native USB
  }
}

void loop() {
  ElkRequest* request;

  if (Serial.available() > 0) {
    String input = Serial.readStringUntil('\n');
    request = request->Parse(input);
    if (request != NULL) {
      Accept(request);
    }
  }

  if (Serial2.available() > 0) {
    String input2 = Serial2.readStringUntil('\0');
    Serial.println("[SUT]" + input2);
  }
}

void Accept(ElkRequest* request) {

  // App
  if (request->Name == "AppReset") {
    pinMode(22, OUTPUT);
    digitalWrite(22, 1);
    digitalWrite(22, 0);
    request->Respond(Ok);
    return;
  }
  // Pins
  else if (request->Name == "PinSet") {
    int pin = request->Arguments[0].toInt();
    int value = request->Arguments[1].toInt();
    pinMode(pin, OUTPUT);
    if (pin >= 26 && pin <= 28) {
      analogWrite(pin, value);
    } else {
      digitalWrite(pin, value);
    }
    request->Respond(Ok);
    return;
  } else if (request->Name == "PinGet") {
    int pin = request->Arguments[0].toInt();
    int sampleDuration = request->Arguments[1].toInt();
    int sampleRate = request->Arguments[2].toInt();

    pinMode(pin, INPUT);
    String values = "";
    if (sampleDuration <= 0 || sampleRate <= 0) {
      values = String(read(pin));
    } else {
      values += String(read(pin));
      int sampleCount = sampleDuration / sampleRate;
      for (int i = 1; i < sampleCount; i++) {
        delay(sampleRate);
        values += ";" + String(read(pin));
      }
    }

    request->Respond(Ok, values);
    return;
  }

  // Serial
  else if (request->Name == "SerialSend") {
    String content = request->Arguments[0];
    Serial2.println(content);
    request->Respond(Ok);
    return;
  }
}

int read(int pin) {
  if (pin >= 26 && pin <= 28) {
    return analogRead(pin);
  }
  return (int)digitalRead(pin);
}