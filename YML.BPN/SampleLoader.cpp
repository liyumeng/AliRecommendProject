#include "SampleLoader.h"
#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <iterator>
#include "SampleModel.h"

SampleLoader::SampleLoader() { PositiveCount = 0; }

SampleLoader::SampleLoader(char* filename)
{
	Filename = filename;
	Inputs = NULL;
	Outputs = NULL;
}

SampleLoader::~SampleLoader()
{
	Clear();
}



//快速载入样本 4.078秒，
//目前样本文件中所有数据均为float型
//在本程序里运算时，使用double运算
void SampleLoader::LoadBin()
{
	BatchOpen();
	BatchReadLines(LineCount);
	BatchClose();
}

std::ifstream* m_file = NULL;

void SampleLoader::BatchOpen()
{
	BatchClose();
	m_file = new std::ifstream(Filename, std::ios::binary);
	int lineCount = 0;	//在这里填写样本的文件行数
	int dim = 0;

	m_file->read((char*)&lineCount, 4);
	m_file->read((char*)&dim, 4);

	InputSize = dim - 4;//样本输入的特征维度
	OutputSize = 1;
	LineCount = lineCount;
	RawLineCount = LineCount;
	CurrentLineCount = 0;
}

///返回实际读到的行数
int SampleLoader::BatchReadLines(int count)
{
	float userid, itemid, online, output, value;

	Clear();
	if (CurrentLineCount >= LineCount)
		return 0;

	if (CurrentLineCount + count > LineCount)
	{
		count = LineCount - CurrentLineCount;
	}

	double** inputs = static_cast<double**>(malloc(count*sizeof(double*)));
	double** outputs = static_cast<double**>(malloc(count*sizeof(double*)));

	for (int i = 0; i < count; i++)
	{
		inputs[i] = static_cast<double*>(malloc(InputSize*sizeof(double)));
		outputs[i] = static_cast<double*>(malloc(OutputSize*sizeof(double)));
	}

	for (int i = 0; i < count; i++)
	{
		m_file->read((char*)&userid, 4);
		m_file->read((char*)&itemid, 4);
		m_file->read((char*)&output, 4);
		m_file->read((char*)&online, 4);

		outputs[i][0] = output;
		for (int m = 0; m < InputSize; m++)
		{
			m_file->read((char*)&value, 4);
			inputs[i][m] = value;
		}
		SampleModel model(userid, itemid);
		Items.push_back(model);
	}

	Inputs = inputs;
	Outputs = outputs;
	CurrentLineCount += count;
	return count;
}

void SampleLoader::BatchClose()
{
	if (m_file != NULL)
	{
		m_file->close();
		m_file = NULL;
	}
}

void SampleLoader::Clear()
{
	if (Inputs != NULL)
	{
		for (int i = 0; i < InputSize; i++)
			free(Inputs[i]);
		free(Inputs);
		Inputs = NULL;
	}
	if (Outputs != NULL)
	{
		for (int i = 0; i < OutputSize; i++)
			free(Outputs[i]);
		free(Outputs);
		Outputs = NULL;
	}
}

//将10%的样本作为测试样本进行early stop
void SampleLoader::EnableTestLoader()
{
	TestLoader = new SampleLoader();
	TestLoader->InputSize = InputSize;
	TestLoader->OutputSize = OutputSize;
	TestLoader->LineCount = LineCount / 10;
	LineCount -= TestLoader->LineCount;

	TestLoader->Inputs = &Inputs[LineCount];
	TestLoader->Outputs = &Outputs[LineCount];
	for (int i = LineCount; i < RawLineCount;i++)
	{
		TestLoader->Items.push_back(Items[i]);
	}
	TestLoader->UpdatePositiveCount();
}

void SampleLoader::UpdatePositiveCount()
{
	PositiveCount = 0;
	for (int i = 0; i < LineCount;i++)
	{
		if(Outputs[i][0]>0.5)
		{
			PositiveCount++;
		}
	}
}