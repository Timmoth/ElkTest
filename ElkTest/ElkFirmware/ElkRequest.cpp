#include "ElkRequest.h"

ElkRequest::Elkkequest() {
}

void ElkRequest::Init(int id, String name, String* arguments, int argumentCount) {
  Id = id;
  Name = name;
  Arguments = arguments;
  ArgumentCount = argumentCount;
}

ElkRequest* ElkRequest::Parse(String input) {
  int id;
  String name;
  String* arguments = new String[0];
  int argumentCount = 0;

  for (int i = 0; i <= 2; i++) {
    int delimeterIndex = input.indexOf(":");
    String inputComponent = "";
    if (delimeterIndex == -1) {
      if (i < 2) {
        Serial.println("AppLog:Could not parse request;" + input);
        // Not a valid Elk request
        return NULL;
      }

      // requests with no arguments are allowed
    } else {
      // Get Input
      inputComponent = input.substring(0, delimeterIndex);
      input = input.substring(delimeterIndex + 1, input.length());
    }

    switch (i) {
      case 0:
        id = inputComponent.toInt();
        break;
      case 1:
        name = inputComponent;
        break;
      case 2:
        if (input.length() == 0) {
          break;
        }

        int argDelimeterIndex = input.indexOf(";");

        if (argDelimeterIndex == -1) {
          argumentCount = 1;
          arguments = new String[1]{
            input
          };
          break;
        }

        argumentCount = 1;
        for (int j = 0; j < input.length(); j++) {
          if (input.charAt(j) == ';') {
            argumentCount++;
          }
        }

        arguments = new String[argumentCount];
        for (int j = 0; j < argumentCount; j++) {
          argDelimeterIndex = input.indexOf(";");
          if (argDelimeterIndex > -1) {
            String inputComponent = input.substring(0, argDelimeterIndex);
            arguments[j] = inputComponent;
            input = input.substring(argDelimeterIndex + 1, input.length());
          } else {
            arguments[j] = input;
          }
        }

        break;
    }
  }

  ElkRequest* request = new ElkRequest();
  request->Init(id, name, arguments, argumentCount);
  return request;
}

void ElkRequest::Respond(int status, String body){
  Serial.println(String(Id) + ":" + String(status) + ":" + body);
}

void ElkRequest::Respond(int status){
    Serial.println(String(Id) + ":" + String(status) + ":");
}