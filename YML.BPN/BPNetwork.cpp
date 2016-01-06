#include "BPNetwork.h"
#include <ctime>
#include <iostream>
#include <fstream>
#include "AnswerModel.h"
#include <algorithm>

struct Xgreater
{
	bool operator()(const AnswerModel* lx, const AnswerModel* rx) const {
		return lx->Value > rx->Value;
	}
};

BPNetwork::BPNetwork()
{
}


BPNetwork::~BPNetwork()
{
	for (int i = 0; i < LayerCount; i++)
	{
		delete LayerList[i];
	}
	free(LayerList);
}


BPNetwork::BPNetwork(int layerCount, int * unitCountList, double alpha, double eta)
{
	srand(static_cast<unsigned>(time(nullptr)));	//初始化随机数种子

	Alpha = alpha;
	Eta = eta;
	LayerCount = layerCount;
	LayerList = static_cast<BPLayer**>(malloc(layerCount*sizeof(BPLayer*)));
	Times = 0;
	MaxTimes = INT32_MAX;
	//实际层号i从1开始，输入序列X即为第0层的输出NextInput,第0层也不必激活
	//第一层的输入为X序列，
	//W权重为输入层与第一隐藏层之间的权重
	//第一层的Output是第一隐层的未激活值，NextInput为第一隐层的激活值
	//如果为n层网络（这里不计输入层，例如2层网络，则有1个输入层，1个输出层）
	LayerList[0] = new BPLayer(0, unitCountList[0], this);//该层的NextInput为输入变量X
	LayerList[1] = new BPLayer(unitCountList[0], unitCountList[1], this);
	for (int i = 2; i < layerCount; i++)
	{
		LayerList[i] = new BPLayer(unitCountList[i - 1], unitCountList[i], this, 1);
	}
	InputLayer = LayerList[1];//负责激活第一隐层的值
	//其中LayerList[1]->Output为第一隐层未激活值
	//其中LayerList[1]->NextInput为第一隐层激活值
	OutputLayer = LayerList[layerCount - 1];
}

double BPNetwork::Train(double* input, double* output)
{
	double e = 0;//代价函数值

	for (int i = 0; i < LayerList[0]->OutputSize; i++)
	{
		LayerList[0]->NextInput[i] = input[i];
	}

	for (int i = 1; i < LayerCount; i++)
	{
		LayerList[i]->SetInput(LayerList[i - 1]->NextInput);
		LayerList[i]->Process();
		LayerList[i]->ActiveOutput();
	}

	//误差反馈
	for (int i = 0; i < OutputLayer->OutputSize; i++)
	{
		double x = OutputLayer->NextInput[i];	//注意是激活的值
		double y = output[i];
		OutputLayer->DeltaOutput[i] = x*(1 - x)*(x - y);
	}


	for (int c = LayerCount - 2; c > 0; c--)
	{
		BPLayer* curLayer = LayerList[c];
		BPLayer* nextLayer = LayerList[c + 1];

		//计算当前层输出的节点的误差DeltaOutput
		//之前还考虑过要不要计算常量阈值的误差，实际上是不用，也没办法计算，
		//虽然常量有Weight[j][InputSize],但DeltaOutput并没有给常量准备值
		//从意义上来讲，这里计算的是输出的节点与理想值之间的误差，常量并无理想值。
		for (int i = 0; i < curLayer->OutputSize; i++)
		{
			double x = curLayer->NextInput[i];	//注意这里是激活值
			double delta = 0;
			for (int j = 0; j < nextLayer->OutputSize; j++)
			{
				delta += nextLayer->DeltaOutput[j] * nextLayer->Weight[j][i];
			}
			//注意这个节点的误差是要传递到上一层中的（就在下一轮循环中传递的）
			//而实际上这个节点已经失效了，所以它的误差也为零，不应该再传递了，所以乘上了Dropout[i]的值，清零
			curLayer->DeltaOutput[i] = x*(1 - x)*delta;
			if (curLayer->DropoutEnabled)
				curLayer->DeltaOutput[i] *= curLayer->Dropout[i];
		}
	}

	//更新权重矩阵(一定要计算完所有DeltaOutput再一起更新Weight，不然会不收敛)
	for (int i = 1; i < LayerCount; i++)
	{
		LayerList[i]->UpdateWeight();
	}

	//计算代价
	for (int i = 0; i < OutputLayer->OutputSize; i++)
	{
		double x = OutputLayer->NextInput[i];	//注意是激活的值
		double y = output[i];
		e += 0.5*(x - y)*(x - y);
	}

	this->Times++;
	this->Error = e;
	return e;
}

void BPNetwork::TrainUntilConvergence(SampleLoader* loader)
{
	double e = 10000;
	double laste = 0;
	Times = 0;
	int sameCount = 0;
	int count = 0;
	double lastf = FastTest(loader->TestLoader);
	while (e > 1 && Times < this->MaxTimes)
	{
		e = 0;
		for (int i = 0; i < loader->LineCount; i++)
		{
			e += this->Train(loader->Inputs[i], loader->Outputs[i]);
		}
		std::cout << Times << ":" << e << "," << this->Alpha << std::endl;

		this->Alpha *= 0.999;

		if (abs(laste - e) < 0.0005)	//5次连续相同，则跳出循环
		{
			sameCount++;
			if (sameCount > 5) break;
		}
		else
			sameCount = 0;
		laste = e;
		count++;
		if (count % 3 == 0)
		{
			double f = FastTest(loader->TestLoader);
			std::cout << "测试集f值：" << f << std::endl;
			if (lastf - f > 5)
				break;
			this->Save("t");
		}
	}
}

double* BPNetwork::Test(double* input)
{
	for (int i = 0; i < LayerList[0]->OutputSize; i++)
	{
		LayerList[0]->NextInput[i] = input[i];
	}

	for (int i = 1; i < LayerCount; i++)
	{
		LayerList[i]->SetInput(LayerList[i - 1]->NextInput);
		LayerList[i]->Process();
		LayerList[i]->ActiveOutput();
	}
	return OutputLayer->Output;
}

void BPNetwork::Test(SampleLoader* loader)
{
	int positiveAnswerCount = 389;
	int guessPositiveCount = std::min(400, loader->LineCount);
	int rightCount = 0;

	std::vector<AnswerModel*> items;

	for (int i = 0; i < loader->LineCount; i++)
	{
		this->Test(loader->Inputs[i]);
		items.push_back(new AnswerModel(loader->Items[i].UserId, loader->Items[i].ItemId, this->OutputLayer->Output[0], loader->Outputs[i][0]));
		if (i % 10000 == 0)
		{
			std::cout << i << "/" << loader->LineCount << std::endl;
		}
	}

	std::sort(items.begin(), items.end(), Xgreater());


	for (int i = 0; i < guessPositiveCount; i++)
	{
		if (items[i]->Y == 1)
		{
			rightCount++;
		}
	}

	double precision = 1.0*rightCount / positiveAnswerCount;
	double recall = 1.0*rightCount / guessPositiveCount;
	double f = 0;
	if (rightCount > 0)
		f = 2.0 * precision*recall / (precision + recall);
	std::cout << "正确个数:" << rightCount << std::endl;
	std::cout << "预测样本数:" << guessPositiveCount << std::endl;
	std::cout << "p:" << precision << std::endl;
	std::cout << "recall:" << recall << std::endl;
	std::cout << "f:" << f << std::endl;
}

//用在Early Stop时进行测试的
double BPNetwork::FastTest(SampleLoader* loader)
{
	int rightCount = 0;
	int guessRightCount = 0;

	for (int i = 0; i < loader->LineCount; i++)
	{
		this->Test(loader->Inputs[i]);
		if (this->OutputLayer->Output[0] > 0)	//声称是正例
		{
			guessRightCount++;
			if (loader->Outputs[i][0] > 0.5)	//真是正例
			{
				rightCount++;
			}
		}

	}
	return 2.0*rightCount / (guessRightCount + loader->PositiveCount);
}

void BPNetwork::Save(char* dirname)
{
	char tmp[256];
	for (int i = 0; i < LayerCount; i++)
	{
		sprintf(tmp, "%s%d.csv", dirname, i);
		LayerList[i]->Save(tmp);
	}
	std::cout << "权重矩阵输出完毕。" << std::endl;
}

void BPNetwork::Load(char* dirname)
{
	char tmp[256];
	for (int i = 0; i < LayerCount; i++)
	{
		sprintf(tmp, "%s%d.csv", dirname, i);
		LayerList[i]->Load(tmp);
	}
	std::cout << "权重矩阵载入完毕。" << std::endl;
}


//输出预测的样本，到文件中
void BPNetwork::Predict(SampleLoader* loader, char* filename)
{
	std::vector<AnswerModel*> items;

	for (int i = 0; i < loader->LineCount; i++)
	{
		this->Test(loader->Inputs[i]);
		items.push_back(new AnswerModel(loader->Items[i].UserId, loader->Items[i].ItemId, this->OutputLayer->Output[0]));
		//dst << loader->Items[i].UserId << "," << loader->Items[i].ItemId << "," << this->OutputLayer->Output[0] << "," << loader->Outputs[i][0] << std::endl;
		if (i % 10000 == 0)
		{
			std::cout << i << "/" << loader->LineCount << std::endl;
		}
	}

	std::sort(items.begin(), items.end(), Xgreater());

	std::ofstream dst(filename);

	for (int i = 0; i < 6000; i++)
	{
		dst << items[i]->UserId << "," << items[i]->ItemId << "," << items[i]->Value << std::endl;
	}
	dst.close();
	std::cout << "预测结果已输出到:" << filename << std::endl;
}

