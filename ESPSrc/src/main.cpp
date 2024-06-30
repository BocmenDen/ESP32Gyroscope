#include "Settings.h"
#include "Communication.h"
#include "Gyroscope.h"
#include <EEPROM.h>

TwoWire Gyroscope_I2C(0);

void LoadCalibration(bool ignoreContainsData = false){
    float values[3] = {NAN, NAN, NAN};
    bool isContainsData = EEPROM.readBool(GYROSCOPE_EEPROM_ADDR);
    if(!ignoreContainsData && isContainsData){
        for (size_t i = 0; i < 3; i++)
            values[i] = EEPROM.read(GYROSCOPE_EEPROM_ADDR + i * sizeof(float) + 1);
    }
    Gyroscope_Calibration(values, values + 1, values + 2);
    for (size_t i = 0; i < 3; i++)
        EEPROM.write(GYROSCOPE_EEPROM_ADDR + i * sizeof(float) + 1, values[i]);
    EEPROM.writeBool(GYROSCOPE_EEPROM_ADDR, true);
    EEPROM.commit();
}

void CommandHandler(uint8_t command){
  #ifdef DEBUG
  Serial.print(F("InputCommand: "));
  Serial.println(command);
  #endif

  switch (command)
  {
  case 1:
    Gyroscope_SetZero();
    break;
  case 2:
    LoadCalibration(true);
    break;
  }
}

void setup() {
  #ifdef DEBUG
  Serial.begin(DEBUG);
  #endif
  EEPROM.begin(128);
  Gyroscope_I2C.begin(GYROSCOPE_SDA, GYROSCOPE_SCL);
  Communication_Init();
  Gyroscope_Init(Gyroscope_I2C);
  LoadCalibration();
  Communication_RegisterEventCommand(CommandHandler);
}

void loop() {
  Communication_Tick();
  if(Communication_HasClient()){
    Gyroscope_Tick();
    Communication_Write(
      Gyroscope_GetAngleX(),
      Gyroscope_GetAngleY(),
      Gyroscope_GetAngleZ()
    );
  }
}