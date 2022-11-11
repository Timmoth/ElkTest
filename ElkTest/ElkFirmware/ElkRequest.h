#ifndef ElkRequest_h
#define ElkRequest_h
#include <Arduino.h>

class ElkRequest {
public:
  void Init(int id, String name, String* arguments, int argumentCount);
  int Id;
  String Name;
  String* Arguments;
  int ArgumentCount;
  ElkRequest* Parse(String input);
  void Respond(int status, String body);
  void Respond(int status);
private:
};

#endif