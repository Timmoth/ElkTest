#include <ArduinoJson.h>
#include <WiFiMulti.h>
#include "arduino_secrets.h"
#include <HttpRequester.h>

const char* ssid = SECRET_NETWORK_SSID;      // your network SSID
const char* password = SECRET_NETWORK_PASS;  // your network password
const char* elkApiIP = ELKAPI_IP;            // your network password

WiFiMulti WiFiMulti;
HttpRequester* requester;

void setup() {
  Serial.begin(115200);

  WiFi.mode(WIFI_STA);
  WiFiMulti.addAP(ssid, password);
  ConnectToWifi();
  requester = new HttpRequester();
  requester->Setup(String(elkApiIP), 6392);
}

void loop() {
  ConnectToWifi();
  delay(500);

  HttpResponse* postResponse = requester->Post("/api/user", "{\"name\":\"Tim\"}", "application/json");
  if (postResponse == NULL || !postResponse->IsSuccess()) {
    return;
  }

  Serial.println("Response: " + postResponse->Body);


  DynamicJsonDocument postBody(400);
  DeserializationError jsonError = deserializeJson(postBody, postResponse->Body);
  if (jsonError) {
    Serial.println("Deserialize failed: " + String(jsonError.c_str()));
    return;
  }

  int userId = postBody["id"].as<int>();

  HttpResponse* getResponse = requester->Get("/api/user/" + String(userId));
  if (getResponse == NULL || !getResponse->IsSuccess()) {
    return;
  }

  DynamicJsonDocument getBody(400);
  DeserializationError getJsonError = deserializeJson(getBody, getResponse->Body);
  if (getJsonError) {
    Serial.println("Deserialize failed: " + String(getJsonError.c_str()));
    return;
  }

  String name = getBody["name"].as<String>();

  HttpResponse* putResponse = requester->Put("/api/user/" + String(userId), "{\"name\":\"" + name + "1\"}", "application/json");
  if (putResponse == NULL || !putResponse->IsSuccess()) {
    return;
  }

  HttpResponse* deleteResponse = requester->Delete("/api/user/" + String(userId));
  if (deleteResponse == NULL || !deleteResponse->IsSuccess()) {
    return;
  }

  delay(500);
}


bool ConnectToWifi() {
  if (WiFiMulti.run() == WL_CONNECTED) {
    return true;
  }
  // wait for WiFi connection
  Serial.println("Connecting to network: " + String(ssid));
  while ((WiFiMulti.run() != WL_CONNECTED)) {
    Serial.println("connecting...");
  }

  Serial.println("Succesfully connected!");
  return true;
}