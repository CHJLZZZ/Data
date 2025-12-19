// PreProcCore.h

#pragma once
#include <stdlib.h>
#include <omp.h>
#include "mil.h"
#include <vcclr.h>

using namespace System;
using namespace CudaCore;

namespace PreProcCore {


#define DEMURA_MAX_PATH	255

	public ref class ImgPreProcCore
	{
		// TODO:  在此加入這個類別的方法。
	public:
		ImgPreProcCore(MIL_ID MilApp, MIL_ID MilSys, int CudaCoreNumber);

	protected:
		~ImgPreProcCore();

	public:
		MIL_ID					MilApplication;
		MIL_ID					MilSystem;
		CudaInterFace^ CudaIF;

		const wchar_t* ConvSysStrToWChar(System::String^ Str);
		void	FunCalCalbriationCoef(MIL_ID SrcImg, String^ CoefPath, int GridColumnNumber, int GridRowNumber);
		void	FunCalCalbriationImg(MIL_ID SrcImg, MIL_ID DstImg, String^ CoefPath);
		void	FunCalFFCCoef(MIL_ID SrcImg, MIL_ID DstImg, String^ CoefPath, MIL_ID OffsetImg, MIL_ID FlatImg, MIL_ID DarkImg);
		void	FunCalFFCImg(MIL_ID SrcImg, MIL_ID DstImg, String^ CoefPath);
		void	FunCalHotPixelImg(MIL_ID mImg, double Bright_TH, double Dark_TH, int CmpPitch, bool SaveCmpResult);

	};
}
