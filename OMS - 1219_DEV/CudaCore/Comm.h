#include <cuda.h>
#include <cuda_runtime.h>
#include <string>  
using namespace std;

//#define BLOCK_NUM		60
//#define THREAD_NUM		192
#define COPY_MAX_PATH	255

extern int BLOCK_NUM;
extern int THREAD_NUM;

extern	string	ErrMsg;

void	InitDevice( int tidnumber );

void	FiveByFiveCompare( int ImageWidth, int ImageHeight,int StartX, int StartY,int EndX, int EndY, int Bright_TH, int Dark_TH, int High_Value, int Low_Value, int PitchX, int PitchY, unsigned short * AryGrabImg ,unsigned char * AryParticleImg );

void	FourWayHotPixelFilter( int ImageWidth, int ImageHeight, double Bright_TH, double Dark_TH, int CmpPitch, unsigned short * AryGrabImg ,unsigned char * AryHotPixelImg );

void	TowWayHotPixelFilter( int ImageWidth, int ImageHeight, double Bright_TH, double Dark_TH, int CmpPitch, unsigned short * AryGrabImg ,unsigned char * AryHotPixelImg );

bool	gpuAssert(cudaError_t code, bool abort=true);