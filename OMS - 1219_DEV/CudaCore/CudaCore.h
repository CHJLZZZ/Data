// CudaCore.h

#pragma once

#include "Comm.h"
#include <time.h>


namespace CudaCore {

	using namespace System;

	public ref class CudaInterFace
	{
		// TODO: 在此加入這個類別的方法。
	public:
		CudaInterFace();
		~CudaInterFace();

		void	InitCudaDevice( int tidnumber );
	
		void	CudaFiveByFiveCompare( int ImageWidth, int ImageHeight, int StartX, int StartY, int EndX, int EndY, int Bright_TH, int Dark_TH, int High_Value, int Low_Value, int PitchX, int PitchY, unsigned short * AryGrabImg , unsigned char * AryParticleImg );

		void	CudaFourWayHotPixelFilter( int ImageWidth, int ImageHeight, double Bright_TH, double Dark_TH, int CmpPitch, unsigned short * AryGrabImg , unsigned char * AryHotPixelImg );
	
		void	CudaTowWayHotPixelFilter( int ImageWidth, int ImageHeight, double Bright_TH, double Dark_TH, int CmpPitch, unsigned short * AryGrabImg , unsigned char * AryHotPixelImg );
		
		property String ^ GetCudaErrMsg
		{
			String ^ get()
			{
				return gcnew String(ErrMsg.c_str());
			} 
		}
	};
}
