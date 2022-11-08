#include "ElkRequest.h"

static const int Ok = 200;
static const int NotFound = 404;

void setup() {
  Serial.begin(9600);
}

void loop() {
  ElkRequest* request;
  while (Serial.available() == 0) {}
  String input = Serial.readStringUntil('\n');
  request = request->Parse(input);
  if (request != NULL) {
    Accept(request);
  }
}

void Accept(ElkRequest* request) {
  if (request->Name == "AppReset") {
    pinMode(22, OUTPUT);
    digitalWrite(22, 1);
    digitalWrite(22, 0);
    request->Respond(Ok);
    return;
  } else if (request->Name == "PinSet") {
    int pin = request->Arguments[0].toInt();
    int value = request->Arguments[1].toInt();
    pinMode(pin, OUTPUT);
    if(pin >= 26 && pin <= 28){
      analogWrite(pin, value);
    }else{
      digitalWrite(pin, value);
    }
    request->Respond(Ok);
    return;
  } else if (request->Name == "PinGet") {
    int pin = request->Arguments[0].toInt();
    pinMode(pin, INPUT);
    int value = 0;
    if(pin >= 26 && pin <= 28){
      value = analogRead(pin);
    }else{
      value = digitalRead(pin);
    }
    request->Respond(Ok, String(value));
    return;
  }
}