#include "Comm.h"

__global__ static void ImgProcFiveByFiveCompare( int BLOCKID_NUM, int THREADID_NUM, int ImageWidth, int ImageHeight, int StartX, int StartY, int EndX, int EndY, int BrightTH, int DarkTH, int HighValue, int LowValue, int PitchX, int PitchY, unsigned short * AryGrabImg ,unsigned char * AryParticleImg, string * ErrorMessage );
__global__ static void ImgProcCorrectionHotPixel( int BLOCKID_NUM, int THREADID_NUM, int ImageWidth, int ImageHeight, int CmpPitch, unsigned short * AryGrabImg ,unsigned char * AryHotPixelImg, string * ErrorMessage );
__global__ static void ImgProcFourWayHotPixelFilter( int BLOCKID_NUM, int THREADID_NUM, int ImageWidth, int ImageHeight, double BrightTH, double DarkTH, int CmpPitch, unsigned short * AryGrabImg , unsigned char * AryHotPixelImg, string * ErrorMessage );
__global__ static void ImgProcTowWayHotPixelFilter( int BLOCKID_NUM, int THREADID_NUM, int ImageWidth, int ImageHeight, double BrightTH, double DarkTH, int CmpPitch, unsigned short * AryGrabImg , unsigned char * AryHotPixelImg, string * ErrorMessage );

string	ErrMsg;

int BLOCK_NUM;
int THREAD_NUM;

#pragma region --- gpuAssert---
// have erro retrun true, no erro return false
bool gpuAssert(cudaError_t code, bool abort)
{	
	if (code != cudaSuccess) 
   {
      ErrMsg = cudaGetErrorString(code);
      return true;
   }
   return false;
}
#pragma endregion

#pragma region --- InitDevice---
void InitDevice( int tidnumber )
{
	cudaDeviceProp dp;
	
	ErrMsg = "";

	int dev		= 0;
	int DevCnt	= 0;
	if( gpuAssert( cudaGetDeviceCount( &DevCnt )))
		return;

	if( DevCnt >= 1 )
	{
		if( gpuAssert( cudaGetDeviceProperties( &dp, dev )))
			return;
			//ErrMsg = "GetDeviceNameFailed ";
		
		if( gpuAssert( cudaSetDevice( dev )))
			return;
			//ErrMsg = "SetDeviceFailed";		
	}
	else
	{
		ErrMsg = "NoDevice";
	}
		
	THREAD_NUM	= tidnumber;
}
#pragma endregion

#pragma region --- FiveByFiveCompare---
void FiveByFiveCompare( int ImageWidth, int ImageHeight, int StartX, int StartY, int EndX, int EndY, int Bright_TH, int Dark_TH, int High_Value, int Low_Value, int PitchX, int PitchY, unsigned short * AryGrabImg , unsigned char * AryParticleImg )
{
	unsigned short	* AryGrabImgData;
	unsigned char	* AryParticleImgData;

	if( gpuAssert( cudaMalloc( ( void** ) &AryGrabImgData, sizeof( unsigned short ) * ( ImageWidth * ImageHeight ))))
		return;

	if( gpuAssert( cudaMalloc( ( void** ) &AryParticleImgData, sizeof( unsigned char ) * ( ImageWidth * ImageHeight ))))
		return;

	if( gpuAssert( cudaMemcpy(AryGrabImgData, AryGrabImg, sizeof( unsigned short ) *  ( ImageWidth * ImageHeight ),	cudaMemcpyHostToDevice )))
		return;

	if( gpuAssert( cudaMemcpy(AryParticleImgData, AryParticleImg, sizeof( unsigned char ) *  ( ImageWidth * ImageHeight ),	cudaMemcpyHostToDevice )))
		return;

	BLOCK_NUM = ImageWidth / THREAD_NUM + 1;
	ImgProcFiveByFiveCompare<<< BLOCK_NUM, THREAD_NUM, 0 >>>( BLOCK_NUM, THREAD_NUM, ImageWidth, ImageHeight, StartX, StartY, EndX, EndY, Bright_TH, Dark_TH, High_Value, Low_Value, PitchX, PitchY, AryGrabImgData , AryParticleImgData, &ErrMsg );
	if( gpuAssert( cudaPeekAtLastError() ))
		return;

	if( gpuAssert( cudaMemcpy( AryGrabImg, AryGrabImgData, sizeof( unsigned short ) * ( ImageWidth * ImageHeight ), cudaMemcpyDeviceToHost )))
		return;

	if( gpuAssert( cudaMemcpy( AryParticleImg, AryParticleImgData, sizeof( unsigned char ) * ( ImageWidth * ImageHeight ), cudaMemcpyDeviceToHost )))
		return;

	if( gpuAssert( cudaFree(AryGrabImgData )))
		return;

	if( gpuAssert( cudaFree(AryParticleImgData )))
		return;
}
#pragma endregion

#pragma region --- ImgProcFiveByFiveCompare---
__global__ static void ImgProcFiveByFiveCompare( int BLOCKID_NUM, int THREADID_NUM, int ImageWidth, int ImageHeight, int StartX, int StartY, int EndX, int EndY, int BrightTH, int DarkTH, int HighValue, int LowValue, int PitchX, int PitchY, unsigned short * AryGrabImg ,unsigned char * AryParticleImg, string * ErrorMessage )
{
	const int tid = threadIdx.x;
    const int bid = blockIdx.x;
	for( int j =0; j < ImageHeight; j++)
	{
		for( int i = (bid * THREADID_NUM + tid); i < ImageWidth; i += ( BLOCKID_NUM * THREADID_NUM ) )
		{
			double CompareResult;
			double AverageResult;
			if ( ( i >= StartX + 2 * PitchX )  && ( i <= ( EndX - 2 * PitchX ) ) && ( j >= StartY + 2 * PitchY )  && ( j <= ( EndY - 2 * PitchY ) ) )
			{
				// Center
				AverageResult = ( AryGrabImg[ i - 2 * PitchX + ( j - 2 * PitchY ) * ImageWidth ] + AryGrabImg[ i - 1 * PitchX + ( j - 2 * PitchY ) * ImageWidth ] + AryGrabImg[ i + ( j - 2 * PitchY ) * ImageWidth ] + AryGrabImg[ i + 1 * PitchX + ( j - 2 * PitchY ) * ImageWidth ] + AryGrabImg[ i + 2 * PitchX + ( j - 2 * PitchY ) * ImageWidth ] + 
								  AryGrabImg[ i - 2 * PitchX + ( j - 1 * PitchY ) * ImageWidth ] + AryGrabImg[ i - 1 * PitchX + ( j - 1 * PitchY ) * ImageWidth ] + AryGrabImg[ i + ( j - 1 * PitchY ) * ImageWidth ] + AryGrabImg[ i + 1 * PitchX + ( j - 1 * PitchY ) * ImageWidth ] + AryGrabImg[ i + 2 * PitchX + ( j - 1 * PitchY ) * ImageWidth ] + 
								  AryGrabImg[ i - 2 * PitchX + ( j              ) * ImageWidth ] + AryGrabImg[ i - 1 * PitchX + ( j				 ) * ImageWidth ] +														AryGrabImg[ i + 1 * PitchX + ( j			  ) * ImageWidth ] + AryGrabImg[ i + 2 * PitchX + ( j			   ) * ImageWidth ] + 
							      AryGrabImg[ i - 2 * PitchX + ( j + 1 * PitchY ) * ImageWidth ] + AryGrabImg[ i - 1 * PitchX + ( j + 1 * PitchY ) * ImageWidth ] + AryGrabImg[ i + ( j + 1 * PitchY ) * ImageWidth ] + AryGrabImg[ i + 1 * PitchX + ( j + 1 * PitchY ) * ImageWidth ] + AryGrabImg[ i + 2 * PitchX + ( j + 1 * PitchY ) * ImageWidth ] + 
								  AryGrabImg[ i - 2 * PitchX + ( j + 2 * PitchY ) * ImageWidth ] + AryGrabImg[ i - 1 * PitchX + ( j + 2 * PitchY ) * ImageWidth ] + AryGrabImg[ i + ( j + 2 * PitchY ) * ImageWidth ] + AryGrabImg[ i + 1 * PitchX + ( j + 2 * PitchY ) * ImageWidth ] + AryGrabImg[ i + 2 * PitchX + ( j + 2 * PitchY ) * ImageWidth ] 
								) / ( double ) 24.0;
								  
				CompareResult = ( AryGrabImg[ i  + j * ImageWidth ] - AverageResult ) / AverageResult;


				if ( CompareResult < ( DarkTH / ( double )100.0 ) )
				{
					AryParticleImg[  i + j * ImageWidth  ] = LowValue;
				}
				else if ( CompareResult > ( BrightTH / ( double )100.0 ) )
				{
					AryParticleImg[  i + j * ImageWidth  ] = HighValue;
				}
				else
				{
					if( AryParticleImg[  i + j * ImageWidth  ] != LowValue && AryParticleImg[  i + j * ImageWidth  ] != HighValue )
						AryParticleImg[  i + j * ImageWidth  ] = 10;
				}
			}
			else
			{
				AryParticleImg[  i + j * ImageWidth  ] = 5;
			}			
		}
	}
}
#pragma endregion 

#pragma region --- FourWayHotPixelFilter ---
void FourWayHotPixelFilter( int ImageWidth, int ImageHeight, double Bright_TH, double Dark_TH, int CmpPitch, unsigned short * AryGrabImg , unsigned char * AryHotPixelImg )
{
	unsigned short	* AryGrabImgData;
	unsigned char	* AryHotPixelImgData;

	if( gpuAssert( cudaMalloc( ( void** ) &AryGrabImgData, sizeof( unsigned short ) * ( ImageWidth * ImageHeight ))))
		return;

	if( gpuAssert( cudaMalloc( ( void** ) &AryHotPixelImgData, sizeof( unsigned char ) * ( ImageWidth * ImageHeight ))))
		return;

	if( gpuAssert( cudaMemcpy(AryGrabImgData, AryGrabImg, sizeof( unsigned short ) *  ( ImageWidth * ImageHeight ),	cudaMemcpyHostToDevice )))
		return;

	if( gpuAssert( cudaMemcpy(AryHotPixelImgData, AryHotPixelImg, sizeof( unsigned char ) *  ( ImageWidth * ImageHeight ),	cudaMemcpyHostToDevice )))
		return;

	BLOCK_NUM = ImageWidth / THREAD_NUM + 1;
	ImgProcFourWayHotPixelFilter<<< BLOCK_NUM, THREAD_NUM, 0 >>>( BLOCK_NUM, THREAD_NUM, ImageWidth, ImageHeight, Bright_TH, Dark_TH, CmpPitch, AryGrabImgData , AryHotPixelImgData, &ErrMsg );
	ImgProcCorrectionHotPixel<<< BLOCK_NUM, THREAD_NUM, 0 >>>( BLOCK_NUM, THREAD_NUM, ImageWidth, ImageHeight, CmpPitch, AryGrabImgData , AryHotPixelImgData, &ErrMsg );
	if( gpuAssert( cudaPeekAtLastError() ))
		return;

	if( gpuAssert( cudaMemcpy( AryGrabImg, AryGrabImgData, sizeof( unsigned short ) * ( ImageWidth * ImageHeight ), cudaMemcpyDeviceToHost )))
		return;

	if( gpuAssert( cudaMemcpy( AryHotPixelImg, AryHotPixelImgData, sizeof( unsigned char ) * ( ImageWidth * ImageHeight ), cudaMemcpyDeviceToHost )))
		return;

	if( gpuAssert( cudaFree(AryGrabImgData )))
		return;

	if( gpuAssert( cudaFree(AryHotPixelImgData )))
		return;
}
#pragma endregion

#pragma region --- TowWayHotPixelFilter ---
void TowWayHotPixelFilter( int ImageWidth, int ImageHeight, double Bright_TH, double Dark_TH, int CmpPitch, unsigned short * AryGrabImg , unsigned char * AryHotPixelImg )
{
	unsigned short	* AryGrabImgData;
	unsigned char	* AryHotPixelImgData;

	if( gpuAssert( cudaMalloc( ( void** ) &AryGrabImgData, sizeof( unsigned short ) * ( ImageWidth * ImageHeight ))))
		return;

	if( gpuAssert( cudaMalloc( ( void** ) &AryHotPixelImgData, sizeof( unsigned char ) * ( ImageWidth * ImageHeight ))))
		return;

	if( gpuAssert( cudaMemcpy(AryGrabImgData, AryGrabImg, sizeof( unsigned short ) *  ( ImageWidth * ImageHeight ),	cudaMemcpyHostToDevice )))
		return;

	if( gpuAssert( cudaMemcpy(AryHotPixelImgData, AryHotPixelImg, sizeof( unsigned char ) *  ( ImageWidth * ImageHeight ),	cudaMemcpyHostToDevice )))
		return;

	BLOCK_NUM = ImageWidth / THREAD_NUM + 1;
	ImgProcTowWayHotPixelFilter<<< BLOCK_NUM, THREAD_NUM, 0 >>>( BLOCK_NUM, THREAD_NUM, ImageWidth, ImageHeight, Bright_TH, Dark_TH, CmpPitch, AryGrabImgData , AryHotPixelImgData, &ErrMsg );
	ImgProcCorrectionHotPixel<<< BLOCK_NUM, THREAD_NUM, 0 >>>( BLOCK_NUM, THREAD_NUM, ImageWidth, ImageHeight, CmpPitch, AryGrabImgData , AryHotPixelImgData, &ErrMsg );
	if( gpuAssert( cudaPeekAtLastError() ))
		return;

	if( gpuAssert( cudaMemcpy( AryGrabImg, AryGrabImgData, sizeof( unsigned short ) * ( ImageWidth * ImageHeight ), cudaMemcpyDeviceToHost )))
		return;

	if( gpuAssert( cudaMemcpy( AryHotPixelImg, AryHotPixelImgData, sizeof( unsigned char ) * ( ImageWidth * ImageHeight ), cudaMemcpyDeviceToHost )))
		return;

	if( gpuAssert( cudaFree(AryGrabImgData )))
		return;

	if( gpuAssert( cudaFree(AryHotPixelImgData )))
		return;

}
#pragma endregion

#pragma region --- ImgProcCorrectionHotPixel ---
__global__ static void ImgProcCorrectionHotPixel( int BLOCKID_NUM, int THREADID_NUM, int ImageWidth, int ImageHeight, int CmpPitch, unsigned short * AryGrabImg ,unsigned char * AryHotPixelImg, string * ErrorMessage )
{					 
	const int tid = threadIdx.x;
    const int bid = blockIdx.x;
				
	// Fix Hot Pixel Algorithm,³Ì¦h¸É¥ª¥k3Áû( Pitch 2)
	for( int j =0; j < ImageHeight; j++ )
	{		
			for( int i = (bid * THREADID_NUM + tid); i < ImageWidth; i += ( BLOCKID_NUM * THREADID_NUM ) )
			{
				if ( ( i >= ( 6 + CmpPitch) )  && ( i <= (ImageWidth - 6 - 1 - CmpPitch ) ) && ( j >= ( 6 + CmpPitch) ) && ( j <= ( ImageHeight - 6 - 1 - CmpPitch) ) )
				{
					if( AryHotPixelImg[ i+ j * ImageWidth ] != 128 && AryHotPixelImg[ i+ j * ImageWidth ] != 64 )
					{								 
						if ( AryHotPixelImg[ i - 2 + j * ImageWidth ] == 128 && 
							 AryHotPixelImg[ i + 2 + j * ImageWidth ] == 128			)// ½T»{¥ª¥k³£OK
						{
							AryGrabImg[ i + j * ImageWidth ]	 = ( AryGrabImg[ ( i - 2 ) + j * ImageWidth ] + AryGrabImg[ ( i + 2 ) + j * ImageWidth ] ) / 2;
						}
						else if ( AryHotPixelImg[ i+ j * ImageWidth + 2 ] == 128 ) // ½T»{¥kOK
						{
							AryGrabImg[ i + j * ImageWidth ]	 = AryGrabImg[ ( i + 2 ) + j * ImageWidth ] ;
						}
						else if ( AryHotPixelImg[ i+ j * ImageWidth - 2 ] == 128 ) // ½T»{¥ªOK
						{
							AryGrabImg[ i + j * ImageWidth ]	 = AryGrabImg[ ( i - 2 ) + j * ImageWidth ];
						}
						else if ( AryHotPixelImg[ i - 4 + j * ImageWidth ] == 128 && 
							AryHotPixelImg[ i + 4 + j * ImageWidth  ] == 128		) // ½T»{2­¿¥ª¥kOK
						{
							AryGrabImg[ i + j * ImageWidth ]	 = ( AryGrabImg[ ( i - 4 ) + j * ImageWidth ] + AryGrabImg[ ( i + 4 ) + j * ImageWidth ] ) / 2;
						}
						else if ( AryHotPixelImg[ i+ j * ImageWidth + 4 ] == 128 ) // ½T»{2­¿¥kOK
						{
							AryGrabImg[ i + j * ImageWidth ]	 = AryGrabImg[ ( i + 4 ) + j * ImageWidth ];
						}
						else if ( AryHotPixelImg[ i+ j * ImageWidth -4 ] == 128) // ½T»{2­¿¥ªOK
						{
							AryGrabImg[ i + j * ImageWidth ]	 = AryGrabImg[ ( i - 4 ) + j * ImageWidth ];
						}
						else if ( AryHotPixelImg[ i - 6 + j * ImageWidth ] == 128 && 
							AryHotPixelImg[ i + 6 + j * ImageWidth ] == 128		) // ½T»{3­¿¥ª¥kOK
						{
							AryGrabImg[ i + j * ImageWidth ]	 = ( AryGrabImg[ ( i - 6 ) + j * ImageWidth ] + AryGrabImg[ ( i + 6 ) + j * ImageWidth ] ) / 2;
						}
						else if ( AryHotPixelImg[ i+ j * ImageWidth + 6 ] == 128) // ½T»{3­¿¥kOK
						{
							AryGrabImg[ i + j * ImageWidth ]	 = AryGrabImg[ ( i + 6 ) + j * ImageWidth ];
						}
						else if ( AryHotPixelImg[ i+ j * ImageWidth - 6 ] == 128) // ½T»{3­¿¥ªOK
						{
							AryGrabImg[ i + j * ImageWidth ]	 = AryGrabImg[ ( i + 6 ) + j * ImageWidth ];
						}
						else
						{
							//memcpy( ErrorMessage ,"HotPixelFilter::Hot Pixel Over Spec.", COPY_MAX_PATH );
						}						
					}
				}
				else
				{
					AryHotPixelImg[ i + j * ImageWidth ] = 64;
				}
			}
		}
							 
}
#pragma endregion

#pragma region --- ImgProcFourWayHotPixelFilter ---
__global__ static void ImgProcFourWayHotPixelFilter( int BLOCKID_NUM, int THREADID_NUM, int ImageWidth, int ImageHeight, double BrightTH, double DarkTH, int CmpPitch, unsigned short * AryGrabImg ,unsigned char * AryHotPixelImg, string * ErrorMessage )
{
	const int tid = threadIdx.x;
    const int bid = blockIdx.x;
	for( int j =0; j < ImageHeight; j++)
	{
		for( int i = (bid * THREADID_NUM + tid); i < ImageWidth; i += ( BLOCKID_NUM * THREADID_NUM ) )
			{
				double CompareResult[]		= { 0.0, 0.0, 0.0, 0.0 };
				int	DfcDark_CmpResultCnt	= 0;
				int	DfcBright_CmpResultCnt	= 0;						 
				if ( ( i >= CmpPitch )  && ( i <= (ImageWidth - CmpPitch  -1 ) ) && ( j >= CmpPitch )  && ( j <= ( ImageHeight - CmpPitch -1) ) )
				{
					//¤W
					CompareResult[0] = ( (double)AryGrabImg[ i + j * ImageWidth ] / (double)AryGrabImg[ i + ( j - CmpPitch ) * ImageWidth ] );
				
					//¤U
					CompareResult[1] = ( (double)AryGrabImg[ i + j * ImageWidth ] / (double)AryGrabImg[ i + ( j + CmpPitch ) * ImageWidth ] );
					
					//¥ª
					CompareResult[2] = ( (double)AryGrabImg[ i  + j * ImageWidth ] / (double)AryGrabImg[ ( i - CmpPitch ) + j * ImageWidth ] );

					//¥k
					CompareResult[3] = ( (double)AryGrabImg[ i  + j * ImageWidth ] / (double)AryGrabImg[ ( i + CmpPitch ) + j * ImageWidth ] );

					for( int cnt_CmpResult = 0; cnt_CmpResult < ( sizeof( CompareResult ) / sizeof( CompareResult[0] ) ); cnt_CmpResult++ )
					{
						if( CompareResult[ cnt_CmpResult ] < DarkTH )
							DfcDark_CmpResultCnt++;

						if( CompareResult[ cnt_CmpResult ] > BrightTH )
							DfcBright_CmpResultCnt++;
					}

					if ( DfcDark_CmpResultCnt >= 4 )
					{
						AryHotPixelImg[  i + j * ImageWidth  ] = 0;
					}
					else if ( DfcBright_CmpResultCnt >= 4 )
					{
						AryHotPixelImg[  i + j * ImageWidth  ] = 255;
					}
					else
					{
						AryHotPixelImg[i + j * ImageWidth] = 128;
					}
				}
				else
				{
					AryHotPixelImg[  i + j * ImageWidth  ] = 64;
				}			
		}
	}
}
#pragma endregion

#pragma region --- ImgProcTowWayHotPixelFilter ---
__global__ static void ImgProcTowWayHotPixelFilter( int BLOCKID_NUM, int THREADID_NUM, int ImageWidth, int ImageHeight, double BrightTH, double DarkTH, int CmpPitch, unsigned short * AryGrabImg ,unsigned char * AryHotPixelImg, string * ErrorMessage )
{
	const int tid = threadIdx.x;
    const int bid = blockIdx.x;

	for( int j =0; j < ImageHeight; j++)
	{
		for( int i = (bid * THREADID_NUM + tid); i < ImageWidth; i += ( BLOCKID_NUM * THREADID_NUM ) )
		{
			double CompareResultUD[]	= { 0.0, 0.0 };
			double CompareResultLR[]	= { 0.0, 0.0 };

			int	DfcDark_CmpResultCntUD			= 0;
			int	DfcBright_CmpResultCntUD		= 0;						 
			int	DfcDark_CmpResultCntLR			= 0;
			int	DfcBright_CmpResultCntLR		= 0;

			if ( ( i >= CmpPitch )  && ( i <= (ImageWidth - CmpPitch  -1 ) ) && ( j >= CmpPitch )  && ( j <= ( ImageHeight - CmpPitch -1) ) )
			{
				//¤W
				CompareResultUD[0] = ( (double)AryGrabImg[ i + j * ImageWidth ] / (double)AryGrabImg[ i + ( j - CmpPitch ) * ImageWidth ] );
	
				//¤U
				CompareResultUD[1] = ( (double)AryGrabImg[ i + j * ImageWidth ] / (double)AryGrabImg[ i + ( j + CmpPitch ) * ImageWidth ] );

				//¥ª
				CompareResultLR[0] = ( (double)AryGrabImg[ i  + j * ImageWidth ] / (double)AryGrabImg[ ( i - CmpPitch ) + j * ImageWidth ] );

				//¥k
				CompareResultLR[1] = ( (double)AryGrabImg[ i  + j * ImageWidth ] / (double)AryGrabImg[ ( i + CmpPitch ) + j * ImageWidth ] );

				for( int cnt_CmpResult = 0; cnt_CmpResult <  ( sizeof( CompareResultUD ) / sizeof( CompareResultUD[0] ) ); cnt_CmpResult++ )
				{
					if( CompareResultUD[ cnt_CmpResult ] < DarkTH )
						DfcDark_CmpResultCntUD++;

					if( CompareResultUD[ cnt_CmpResult ] > BrightTH )
						DfcBright_CmpResultCntUD++;
				}

				for( int cnt_CmpResult = 0; cnt_CmpResult < ( sizeof( CompareResultLR ) / sizeof( CompareResultLR[0] ) ); cnt_CmpResult++ )
				{
					if( CompareResultLR[ cnt_CmpResult ] < DarkTH )
						DfcDark_CmpResultCntLR++;
							
					if( CompareResultLR[ cnt_CmpResult ] > BrightTH )
						DfcBright_CmpResultCntLR++;
				}

				if ( DfcDark_CmpResultCntLR >= 2 || DfcDark_CmpResultCntUD >= 2 )
				{
					AryHotPixelImg[  i + j * ImageWidth  ] = 0;
				}
				else if ( DfcBright_CmpResultCntLR >= 2 || DfcBright_CmpResultCntUD >= 2 )
				{
					AryHotPixelImg[  i + j * ImageWidth  ] = 255;
				}
				else
				{
					AryHotPixelImg[i + j * ImageWidth] = 128;
				}
			}
			else
			{
				AryHotPixelImg[  i + j * ImageWidth  ] = 64;
			}			
		}
	}
}
#pragma endregion