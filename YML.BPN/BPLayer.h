#pragma once

class BPNetwork;

/*
神经网络的层对象
使用说明
1. 实例化传入	inputSize	:	输入节点的个数
				outputSize	:	输出节点的个数
2. 调用	SetInput	设置输入节点的值
3. 调用 Train		计算未激活的输出,得到Output
4. 调用 ActiveOutput	激活输出，得到下一层的输入节点NextInput
*/
class BPLayer
{

public:
	BPLayer();
	~BPLayer();

	BPLayer(int inputSize, int outputSize, BPNetwork* net, int dropoutEnabled = 0);

	//1.设置输入
	void SetInput(double* input);

	//2.进行输出处理
	double* Process(int isTesting = 0);
	double* TestingProcess();
	//3.激活本层的输出值
	double* ActiveOutput();

	//4.更新权重向量
	void UpdateWeight();
	void Save(char* filename);
	void Load(char* filename);
	//输入的网络节点
	//需要运行SetInput设置输入值
	double* Input;

	//未激活的本层输出
	//需要运行Process()才能得到该值
	double*	Output;

	//激活后的下一层的输出
	//需要运行ActiveOutput()才能得到该值
	double* NextInput = nullptr;

	//初始为随机权重
	double** Weight;

	//上次权重的更新值
	double** DeltaWeight;
	double** LastDeltaWeight;

	//输出误差，与输出节点数相同
	double* DeltaOutput;

	int InputSize;
	int OutputSize;

	int BatchSize;

	BPNetwork* Net = nullptr;
	double m_dropoutFraction;
	double* Dropout;
	int DropoutEnabled;

private:
	double** m_g2;
	double** m_u2;
	double m_rho;
	double m_epsilon;
	int m_indexInBatch;
};

