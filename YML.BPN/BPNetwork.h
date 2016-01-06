#pragma once
#include"BPLayer.h"
#include "SampleLoader.h"

class BPNetwork
{
public:
	BPNetwork();
	~BPNetwork();

	//这里的layerCount要包含输入层
	BPNetwork(int layerCount, int* unitCountList, double alpha = 0.5, double eta = 0.05);

	//训练样本
	double Train(double* input, double* output);

	//测试
	double* Test(double* input);
	void Test(SampleLoader* loader);
	double FastTest(SampleLoader* loader);
	void Save(char* dirname);
	void Load(char* dirname);
	void Predict(SampleLoader* loader, char* filename);
	//训练直到收敛
	void TrainUntilConvergence(SampleLoader* loader);

	BPLayer** LayerList;

	//这里的输入层实际上是 X序列->W1权重->第一隐层 形成的网络
	//所以这里InputLayer=LayerList[1];因为LayerList[0]实在没什么意义。
	//所以InputLayer->Input就是X序列
	BPLayer* InputLayer;
	//这里的输出层是最后一个隐层->Wn权重->输出层 形成的网络
	//所以这里OutputLayer->Output就是最终的输出
	BPLayer* OutputLayer;

	int LayerCount;	//网络层数

	double Eta = 0.001;		//上一代
	double Alpha;		//本代学习率
	double Error;	//错误率

	long long Times = 0;	//迭代次数
	long long MaxTimes;

};

