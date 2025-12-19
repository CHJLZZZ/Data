#include "pch.h"

// 這是主要 DLL 檔案。

#include "PreProcCore.h"
using namespace PreProcCore;


ImgPreProcCore::ImgPreProcCore(MIL_ID MilApp, MIL_ID MilSys, int CudaCoreNumber)
{
	try
	{
		this->MilApplication = MilApp;
		this->MilSystem = MilSys;

		this->CudaIF = gcnew CudaInterFace();
		this->CudaIF->InitCudaDevice(CudaCoreNumber);
		if (!String::IsNullOrEmpty(this->CudaIF->GetCudaErrMsg))
			throw gcnew Exception(this->CudaIF->GetCudaErrMsg);
	}
	catch (Exception^ ex)
	{
		throw ex;
	}
	finally
	{

	}
}

ImgPreProcCore::~ImgPreProcCore()
{
	if (this->CudaIF)
	{
		delete this->CudaIF;
		this->CudaIF = nullptr;
	}
}

#pragma region "--- ConvSysStrToWChar ---"
//System::string^ to char*    //2009/11/02 Rick add
const wchar_t* ImgPreProcCore::ConvSysStrToWChar(System::String^ Str) {
	pin_ptr<const wchar_t> wch = PtrToStringChars(Str);
	return wch;
}
#pragma endregion

#pragma region "--- FunCalCalbriationCoef ---"
void ImgPreProcCore::FunCalCalbriationCoef(MIL_ID SrcImg, String^ CoefPath, int GridColumnNumber, int GridRowNumber)
{
	MIL_ID		Mil_FunRtn;
	MIL_ID		CalibrationResult;

	try
	{
		Mil_FunRtn = McalAlloc(this->MilSystem, M_DEFAULT, M_DEFAULT, &CalibrationResult);
		if (Mil_FunRtn == 0)
		{
			throw gcnew Exception("FunCalCalbriationCoef::Allocate CalibrationResult failed.");
		}
		//	Setting calibration object
		McalControl(CalibrationResult, M_FOREGROUND_VALUE, M_FOREGROUND_WHITE);
		McalControl(CalibrationResult, M_TRANSFORM_CACHE, M_ENABLE);

		McalGrid(CalibrationResult,							//	CalibrationId
			SrcImg,										//	SrcImageBufId
			0,											//	GridOffsetX
			0,											//	GridOffsetY
			0,											//	GridOffsetZ
			GridRowNumber,								//	RowNumber
			GridColumnNumber,							//	ColumnNumber
			1,											//	RowSpacing
			1,											//	ColumnSpacing
			M_DEFAULT,									//	Operation
			M_DEFAULT);

		//char		SavePath[DEMURA_MAX_PATH] = "\0";
		//sprintf_s(SavePath, DEMURA_MAX_PATH, "%s", CoefPath);
		McalSave(ConvSysStrToWChar(CoefPath), CalibrationResult, M_DEFAULT);
	}
	catch (Exception^ ex)
	{
		throw ex;
	}
	finally
	{
		if (CalibrationResult) McalFree(CalibrationResult);
	}
}
#pragma endregion

#pragma region "--- FunCalCalbriationImg ---"
void ImgPreProcCore::FunCalCalbriationImg(MIL_ID SrcImg, MIL_ID DstImg, String^ CoefPath)
{
	MIL_ID		CalibrationResult;

	try
	{
		//char		CoefPath[DEMURA_MAX_PATH] = "\0";
		//sprintf_s(CoefPath, DEMURA_MAX_PATH, "%s", CoefPath);
		McalRestore(ConvSysStrToWChar(CoefPath), this->MilSystem, M_DEFAULT, &CalibrationResult);

		//	Grid Translate form Image
		McalTransformImage(SrcImg,								//	SrcImageBufId
			DstImg,								//	DestImageBufId
			CalibrationResult,					//	CalibrationId
			M_BICUBIC | M_OVERSCAN_CLEAR,		//	InterpolationMode
			M_DEFAULT,							//	OperationType
			M_DEFAULT);						//	ControlFlag
	}
	catch (Exception^ ex)
	{
		throw ex;
	}
	finally
	{
		if (CalibrationResult) McalFree(CalibrationResult);
	}
}
#pragma endregion

#pragma region "--- FunCalFFCCoef ---"
void ImgPreProcCore::FunCalFFCCoef(MIL_ID SrcImg, MIL_ID DstImg, String^ CoefPath, MIL_ID OffsetImg, MIL_ID FlatImg, MIL_ID DarkImg)
{
	MIL_ID		Mil_FunRtn;
	MIL_INT		ImageWidth = 0;
	MIL_INT		ImageHeight = 0;
	MIL_ID		mImgCoef = M_NULL;
	MIL_ID		mImgFlatFieldContext = M_NULL;

	try
	{
		MbufInquire(SrcImg, M_SIZE_X, &ImageWidth);
		MbufInquire(SrcImg, M_SIZE_Y, &ImageHeight);

		Mil_FunRtn = MbufAlloc2d(this->MilSystem,
			ImageWidth,
			ImageHeight,
			32 + M_FLOAT,
			M_IMAGE + M_PROC,
			&mImgCoef);
		if (Mil_FunRtn == M_NULL)
		{
			throw gcnew Exception(String::Format("FunCalFFCCoef::Can not allocate mImgCoef."));
		}

		// Allocate the flat field context.
		MimAlloc(this->MilSystem, M_FLAT_FIELD_CONTEXT, M_DEFAULT, M_NULL);

		// Use the automatic gain.
		MimControl(mImgFlatFieldContext, M_GAIN_CONST, M_AUTOMATIC);

		// set the offset image.
		MimControl(mImgFlatFieldContext, M_OFFSET_IMAGE, OffsetImg);

		// set the flat image.
		MimControl(mImgFlatFieldContext, M_FLAT_IMAGE, FlatImg);

		// set the dark image.
		MimControl(mImgFlatFieldContext, M_DARK_IMAGE, DarkImg);

		// Preprocess the flat field context.
		MimFlatField(mImgFlatFieldContext, SrcImg, DstImg, M_PREPROCESS);

		// Coef Image
		MimArith(DstImg, SrcImg, mImgCoef, M_DIV);

		//char		SavePath[DEMURA_MAX_PATH] = "\0";
		//sprintf_s(SavePath, DEMURA_MAX_PATH, "%s", CoefPath);
		MbufSave(ConvSysStrToWChar(CoefPath), mImgCoef);

	}
	catch (Exception^ ex)
	{
		throw ex;
	}
	finally
	{
		if (mImgCoef != M_NULL)
		{
			MbufFree(mImgCoef);
			mImgCoef = M_NULL;
		}

		if (mImgFlatFieldContext != M_NULL)
		{
			MimFree(mImgFlatFieldContext);
			mImgFlatFieldContext = M_NULL;
		}
	}
}
#pragma endregion

#pragma region "--- FunCalFFCImg ---"
void ImgPreProcCore::FunCalFFCImg(MIL_ID SrcImg, MIL_ID DstImg, String^ CoefPath)
{

	MIL_ID		mImgCoef = M_NULL;

	try
	{
		//char		SavePath[DEMURA_MAX_PATH] = "\0";
		//sprintf_s(SavePath, DEMURA_MAX_PATH, "%s", CoefPath);
		MbufRestore(ConvSysStrToWChar(CoefPath), this->MilSystem, &mImgCoef);
		MimArith(SrcImg, mImgCoef, mImgCoef, M_MULT);
		MbufCopy(mImgCoef, DstImg);
	}
	catch (Exception^ ex)
	{
		throw ex;
	}
	finally
	{
		if (mImgCoef != M_NULL)
		{
			MbufFree(mImgCoef);
			mImgCoef = M_NULL;
		}
	}
}
#pragma endregion

#pragma region "--- FunCalHotPixelImg ---"
void ImgPreProcCore::FunCalHotPixelImg(MIL_ID mImg, double HotPixel_Bright_TH, double HotPixel_Dark_TH, int HotPixel_CmpPitch, bool SaveCmpResult)
{
	MIL_ID				Mil_FunRtn;

	MIL_INT				ImageWidth = 0;
	MIL_INT				ImageHeight = 0;
	MIL_INT				ImageBand = 0;
	MIL_INT				ImageSign = 0;
	MIL_INT				ImageDepth = 0;

	MIL_ID				mImgHotPixelBin;

	unsigned short* mArySrc;
	unsigned char* mAryHotPixelBin;

	double				Dark_TH = HotPixel_Dark_TH;
	double				Bright_TH = HotPixel_Bright_TH;
	int					CmpPitch = HotPixel_CmpPitch;

	try
	{
		MbufInquire(mImg, M_SIZE_X, &ImageWidth);
		MbufInquire(mImg, M_SIZE_Y, &ImageHeight);
		MbufInquire(mImg, M_SIZE_BAND, &ImageBand);
		MbufInquire(mImg, M_SIGN, &ImageSign);
		MbufInquire(mImg, M_SIZE_BIT, &ImageDepth);

		mArySrc = (unsigned short*)malloc(sizeof(unsigned short) * (ImageWidth * ImageHeight));
		if (mArySrc == NULL)
		{
			throw gcnew Exception(String::Format("FunCalHotPixelImg::Can not allocate mArySrc."));
		}

		// Allocate HotPixelBin
		mAryHotPixelBin = (unsigned char*)malloc(sizeof(unsigned char) * (ImageWidth * ImageHeight));
		if (mAryHotPixelBin == NULL)
		{
			throw gcnew Exception(String::Format("FunCalHotPixelImg::Can not allocate mAryHotPixelBin."));
		}

		Mil_FunRtn = MbufCreate2d(this->MilSystem,								// MIL System
			ImageWidth,  	   								// Image SizeX
			ImageHeight,  									// Image SizeY
			8 + M_UNSIGNED,									// Image Type
			M_IMAGE + M_PROC,								// Image Attribute
			M_HOST_ADDRESS + M_PITCH,						// Ctrl Flag
			M_DEFAULT,										// Image Pitch
			mAryHotPixelBin,								// Linked Memory Pointer
			&mImgHotPixelBin);								// MIL ID

		MbufClear(mImgHotPixelBin, M_COLOR_BLACK);

		// Put Array to mArySrc
		MbufGet(mImg, mArySrc);

		//4 Way
		this->CudaIF->CudaFourWayHotPixelFilter((int)ImageWidth, (int)ImageHeight, Bright_TH, Dark_TH, CmpPitch, mArySrc, mAryHotPixelBin);
		if (!String::IsNullOrEmpty(this->CudaIF->GetCudaErrMsg))
		{
			throw gcnew Exception(String::Format("FunCalHotPixelImg::{0}", this->CudaIF->GetCudaErrMsg));
		}

		// Get Array to DstImg
		MbufPut(mImg, mArySrc);

		// Save Cmp Result
		if (SaveCmpResult)
		{
			//char		SavePath[DEMURA_MAX_PATH] = "\0";
			//sprintf_s(SavePath, DEMURA_MAX_PATH, "%s", String::Format("D:\\CmpResult-{0:yyyyMMddHHmmss}.tif", DateTime::Now));
			String^ SavePath = String::Format("D:\\CmpResult-{0:yyyyMMddHHmmss}.tif", DateTime::Now);
			MbufSave(ConvSysStrToWChar(SavePath), mImgHotPixelBin);
		}
	}
	catch (Exception^ ex)
	{
		throw ex;
	}
	finally
	{
		if (mImgHotPixelBin != M_NULL)
		{
			MbufFree(mImgHotPixelBin);
			mImgHotPixelBin = M_NULL;
		}
	}

}
#pragma endregion


