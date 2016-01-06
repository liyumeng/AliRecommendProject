#pragma once
#include "SampleModel.h"
#include <vector>

class SampleLoader
{
public:
	SampleLoader();
	SampleLoader(char* filename);
	~SampleLoader();

	void LoadBin();
	void BatchOpen();
	int BatchReadLines(int count);
	void BatchClose();
	void Clear();
	void EnableTestLoader();
	void UpdatePositiveCount();
	char* Filename;
	int InputSize;
	int OutputSize;
	int LineCount;
	int RawLineCount;
	int CurrentLineCount;
	double** Inputs;
	double** Outputs;

	int PositiveCount;

	SampleLoader* TestLoader;
	std::vector<SampleModel> Items;
};

