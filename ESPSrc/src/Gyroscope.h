#include "Settings.h"
#include "MPU6050_tockn.h"

#ifndef Gyroscope_h
#define Gyroscope_h

MPU6050* _gyroscope_mpu6050;
float _gyroscope_offsetX, _gyroscope_offsetY, _gyroscope_offsetZ;

void inline Gyroscope_Init(TwoWire &w){
    _gyroscope_mpu6050 = new MPU6050(w);
    _gyroscope_mpu6050->begin();
}

void Gyroscope_Calibration(float* x, float* y, float* z){
    if(isnan(*x) || isnan(*y) || isnan(*z)){
        _gyroscope_mpu6050->calcGyroOffsets();
        *x = _gyroscope_mpu6050->getGyroXoffset();
        *y = _gyroscope_mpu6050->getGyroYoffset();
        *z = _gyroscope_mpu6050->getGyroZoffset();
    }else{
        _gyroscope_mpu6050->setGyroOffsets(*x, *y, *z);
    }
}

void inline Gyroscope_Tick(){
    _gyroscope_mpu6050->update();
}

float inline Gyroscope_GetAngleX(){
    return GYROSCOPE_REVERSE_X(_gyroscope_mpu6050->getAngleX() + _gyroscope_offsetX);
}

float inline Gyroscope_GetAngleY(){
    return GYROSCOPE_REVERSE_Y(_gyroscope_mpu6050->getAngleY() + _gyroscope_offsetY);
}

float inline Gyroscope_GetAngleZ(){
    return GYROSCOPE_REVERSE_Z(_gyroscope_mpu6050->getAngleZ() + _gyroscope_offsetZ);
}

void Gyroscope_SetZero(){
    _gyroscope_offsetX = -_gyroscope_mpu6050->getAngleX();
    _gyroscope_offsetY = -_gyroscope_mpu6050->getAngleY();
    _gyroscope_offsetZ = -_gyroscope_mpu6050->getAngleZ();
}

void inline Gyroscope_Reset(){
    _gyroscope_offsetX = _gyroscope_offsetY = _gyroscope_offsetZ = 0;
}

#endif