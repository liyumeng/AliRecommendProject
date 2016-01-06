#pragma once
class SampleModel
{
public:
	SampleModel();
	SampleModel(int userid, int itemid);
	~SampleModel();
	int UserId;
	int ItemId;
	double Value;
};

