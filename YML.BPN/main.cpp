#include<iostream>

#include "BPNetwork.h"
#include "SampleLoader.h"
#include <ctime>

using namespace std;

void Train()
{
	//载入样本
	//data中第一行表示样本个数n，第二行为输入x的维度，第三行为输出y的维度
	//此后n行，为x及y的值，如1 0 1表示输入x={1,0},输出y={1}
	SampleLoader* loader = new SampleLoader("C:\\data\\small\\norm\\train1217.bin");
	loader->LoadBin();
	loader->EnableTestLoader();
	int inputSize = loader->InputSize;
	cout << "输入维度:" << inputSize << endl;
	cout << "样本个数:" << loader->LineCount << endl;

	//loader->EnableTestLoader();
	//三层网络（1个隐藏层），每层的节点个数
	int unitCount[4] = { inputSize,500,loader->OutputSize };
	BPNetwork* net = new BPNetwork(3, unitCount, 0.4, 0.03);
	//net->Load("t");
	//训练直到收敛
	net->TrainUntilConvergence(loader);
	net->Save("t");
	cout << "共迭代" << net->Times << "次" << endl;
	printf("开始预测...\n");
	net->Test(loader->TestLoader);

	delete net;
}

void Test()
{
	//载入样本
	//data中第一行表示样本个数n，第二行为输入x的维度，第三行为输出y的维度
	//此后n行，为x及y的值，如1 0 1表示输入x={1,0},输出y={1}
	cout << "载入样本中..." << endl;
	SampleLoader* loader = new SampleLoader("C:\\data\\small\\norm\\test1218.bin");
	loader->LoadBin();
	int inputSize = loader->InputSize;
	cout << "输入维度:" << inputSize << endl;

	//三层网络（1个隐藏层），每层的节点个数
	int unitCount[3] = { inputSize,500,loader->OutputSize };
	BPNetwork* net = new BPNetwork(3, unitCount);
	//训练直到收敛
	net->Load("t");
	printf("开始预测...\n");
	//net->Predict(loader, "result.csv");
	net->Test(loader);
}

void TestLoadFile()
{
	clock_t start = clock();
	SampleLoader* loader = new SampleLoader("c:\\data\\small\\train1217.bin");
	loader->LoadBin();
	clock_t end = clock();
	cout << "用时：" << (static_cast<double>(end - start) / CLOCKS_PER_SEC) << "s" << endl;
}

void main()
{
	Train();
	Test();
	getchar();
}