#include "Settings.h"
#include "BluetoothSerial.h"

#ifndef Communication_h
#define Communication_h
BluetoothSerial _communication_serialBT;
char _communication_bufferSend[128];
void (*_communication_commandHandler)(uint8_t);

void inline Communication_Init(){
    _communication_serialBT.begin(COMMUNICATION_HOST_NAME "_Gyroscope");
#ifdef COMMUNICATION_HOST_PASSWORD
    _communication_serialBT.setPin(COMMUNICATION_HOST_PASSWORD);
#endif
}

void Communication_Tick(){
  if(_communication_commandHandler != nullptr){
    while(_communication_serialBT.available() > 0){
      _communication_commandHandler(_communication_serialBT.read());
    }
  }
}

void inline Communication_RegisterEventCommand(void (*action)(uint8_t)){
  _communication_commandHandler = action;
}

bool inline Communication_HasClient(){
  return _communication_serialBT.hasClient();
}

void inline Communication_Write(float x, float y, float z){
  int sizeBuff = sprintf(_communication_bufferSend, "S%.2f;%.2f;%.2f\n", x, y, z);
  _communication_serialBT.write((uint8_t*)_communication_bufferSend, sizeBuff);
  #ifdef DEBUG
    Serial.print(F("SendData: "));
    Serial.write(_communication_bufferSend, sizeBuff);
    Serial.println();
  #endif
}

#endif