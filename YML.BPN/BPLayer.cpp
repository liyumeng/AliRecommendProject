#include "BPLayer.h"
#include <cmath>
#include "BPNetwork.h"
#include <fstream>


BPLayer::BPLayer()
{
}


BPLayer::~BPLayer()
{
	free(Input);
	free(Dropout);
	free(Output);
	free(NextInput);
	free(DeltaOutput);
	for (int i = 0; i < OutputSize; i++)
	{
		free(Weight[i]);
		free(DeltaWeight[i]);
		free(m_g2[i]);
		free(m_u2[i]);
	}
	free(Weight);
	free(DeltaWeight);
	free(m_g2);
	free(m_u2);
}



BPLayer::BPLayer(int inputSize, int outputSize, BPNetwork* net, int dropoutEnabled)
{
	m_rho = 0.95;
	m_epsilon = 1e-6;
	m_dropoutFraction = 0.5;
	m_indexInBatch = 0;
	DropoutEnabled = dropoutEnabled;

	InputSize = inputSize;
	OutputSize = outputSize;
	BatchSize = 32;
	Net = net;

	//留出一个常量空间，最后一个元素是常量
	Input = static_cast<double*>(malloc((inputSize + 1)*sizeof(double)));

	Dropout = static_cast<double*>(malloc((inputSize + 1)*sizeof(double)));

	Output = static_cast<double*>(malloc(outputSize*sizeof(double)));

	NextInput = static_cast<double*>(malloc(outputSize*sizeof(double)));

	DeltaOutput = static_cast<double*>(malloc(outputSize*sizeof(double)));

	//Weight[j][last]为常数项，即阈值
	Weight = static_cast<double**>(malloc((outputSize)*sizeof(double*)));
	DeltaWeight = static_cast<double**>(malloc((outputSize)*sizeof(double*)));
	LastDeltaWeight = static_cast<double**>(malloc((outputSize)*sizeof(double*)));


	m_g2 = static_cast<double**>(malloc((outputSize)*sizeof(double*)));
	m_u2 = static_cast<double**>(malloc((outputSize)*sizeof(double*)));


	for (int j = 0; j < outputSize; j++)
	{
		//输入项应该多一个常数项
		Weight[j] = static_cast<double*>(malloc((inputSize + 1)*sizeof(double)));
		DeltaWeight[j] = static_cast<double*>(malloc((inputSize + 1)*sizeof(double)));
		LastDeltaWeight[j] = static_cast<double*>(malloc((inputSize + 1)*sizeof(double)));
		m_g2[j] = static_cast<double*>(malloc((inputSize + 1)*sizeof(double)));
		m_u2[j] = static_cast<double*>(malloc((inputSize + 1)*sizeof(double)));

		for (int i = 0; i <= inputSize; i++)
		{
			Weight[j][i] = 1.0*rand() / RAND_MAX - 0.5;	//产生-0.5到0.5的随机数
			DeltaWeight[j][i] = 0;
			LastDeltaWeight[j][i] = 0;
			m_g2[j][i] = 0;
			m_u2[j][i] = 0;
		}
	}
}

double* BPLayer::Process(int isTesting)
{
	//dropout 以dropoutFraction的概率随机清零
	if (DropoutEnabled)
	{
		for (int i = 0; i <= InputSize; i++)
		{
			if (1.0*rand() / RAND_MAX < m_dropoutFraction)
			{
				Dropout[i] = 0;
				Input[i] = 0;
			}
			else
			{
				Dropout[i] = 1;
			}
		}
	}


	for (int j = 0; j < OutputSize; j++)
	{
		Output[j] = 0;
		for (int i = 0; i <= InputSize; i++)
		{
			Output[j] += Input[i] * Weight[j][i];
		}
	}
	return Output;
}


double* BPLayer::TestingProcess()
{
	for (int j = 0; j < OutputSize; j++)
	{
		Output[j] = 0;
		for (int i = 0; i <= InputSize; i++)
		{
			Output[j] += Input[i] * Weight[j][i];
		}
		//隐藏层节点输出衰减1-dropoutFraction
		if (DropoutEnabled)
			Output[j] *= (1 - m_dropoutFraction);	
	}
	return Output;
}

double* BPLayer::ActiveOutput()
{
	for (int j = 0; j < OutputSize; j++)
	{
		NextInput[j] = 1.0 / (1 + exp(-Output[j]));
	}
	return NextInput;
}

void BPLayer::SetInput(double * input)
{
	for (int i = 0; i < InputSize; i++)
	{
		Input[i] = input[i];
	}
	Input[InputSize] = 1;
}

void BPLayer::UpdateWeight()
{
	double lastDelta;
	double delta;
	m_indexInBatch++;
	Input[InputSize] = 1;
	//对同一批的数据，更新梯度取平均
	for (int j = 0; j < OutputSize; j++)
	{
		for (int i = 0; i <= InputSize; i++)
		{
			DeltaWeight[j][i] += DeltaOutput[j] * Input[i];
		}
	}

	if (m_indexInBatch >= BatchSize)
	{
		for (int j = 0; j < OutputSize; j++)
		{
			for (int i = 0; i <= InputSize; i++)
			{
				/*
				lastDelta = LastDeltaWeight[j][i];
				delta = DeltaWeight[j][i] / m_indexInBatch;

				delta = Net->Alpha *delta + Net->Eta* lastDelta;
				LastDeltaWeight[j][i] = delta;
				DeltaWeight[j][i] = 0;

				Weight[j][i] -= delta;
				*/

				double g = DeltaWeight[j][i] / m_indexInBatch;
				m_g2[j][i] = 0.95*m_g2[j][i] + 0.05*g*g;
				double update = sqrt(m_u2[j][i] + m_epsilon) / sqrt(m_g2[j][i] + m_epsilon)*(-g);
				m_u2[j][i] = 0.95*m_u2[j][i] + 0.05*update * update;
				Weight[j][i] += update;
				DeltaWeight[j][i] = 0;

			}
		}
		m_indexInBatch = 0;
	}

}

void BPLayer::Save(char* filename)
{
	std::ofstream dst(filename);

	for (int j = 0; j < this->OutputSize; j++)
	{
		//输入项应该多一个常数项
		for (int i = 0; i <= this->InputSize; i++)
		{
			dst << Weight[j][i] << ",";
		}
		dst << "\n";
	}
	dst.close();
}

void BPLayer::Load(char* filename)
{
	std::fstream src(filename);
	char c;
	for (int j = 0; j < this->OutputSize; j++)
	{
		//输入项应该多一个常数项
		for (int i = 0; i <= this->InputSize; i++)
		{
			src >> Weight[j][i] >> c;
		}
	}
	src.close();
}
