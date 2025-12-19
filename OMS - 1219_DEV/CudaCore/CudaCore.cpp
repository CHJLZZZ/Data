// 這是主要 DLL 檔案。

#include "stdafx.h"
#include "CudaCore.h"

using namespace CudaCore;

CudaInterFace::CudaInterFace(){}

CudaInterFace::~CudaInterFace(){}

#pragma region --- InitCudaDevice---
void CudaInterFace::InitCudaDevice( int tidnumber )
{	
	InitDevice( tidnumber );	
}
#pragma endregion

#pragma region --- CudaFiveByFiveCompare---
void CudaInterFace::CudaFiveByFiveCompare( int ImageWidth, int ImageHeight, int StartX, int StartY, int EndX, int EndY, int Bright_TH, int Dark_TH, int High_Value, int Low_Value, int PitchX, int PitchY, unsigned short * AryGrabImg , unsigned char * AryParticleImg )
{
	FiveByFiveCompare( ImageWidth, ImageHeight, StartX, StartY, EndX, EndY, Bright_TH, Dark_TH, High_Value, Low_Value, PitchX, PitchY, AryGrabImg , AryParticleImg );
}
#pragma endregion

#pragma region --- CudaFourWayHotPixelFilter---
void CudaInterFace::CudaFourWayHotPixelFilter( int ImageWidth, int ImageHeight, double Bright_TH, double Dark_TH, int CmpPitch, unsigned short * AryGrabImg , unsigned char * AryHotPixelImg )
{
	FourWayHotPixelFilter( ImageWidth, ImageHeight, Bright_TH, Dark_TH, CmpPitch, AryGrabImg , AryHotPixelImg);
}
#pragma endregion

#pragma region --- CudaFiveByFiveCompare---
void CudaInterFace::CudaTowWayHotPixelFilter( int ImageWidth, int ImageHeight, double Bright_TH, double Dark_TH, int CmpPitch, unsigned short * AryGrabImg , unsigned char * AryHotPixelImg )
{
	TowWayHotPixelFilter( ImageWidth, ImageHeight, Bright_TH, Dark_TH, CmpPitch, AryGrabImg , AryHotPixelImg);
}
#pragma endregion