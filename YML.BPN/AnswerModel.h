#pragma once
class AnswerModel
{
public:
	AnswerModel();
	AnswerModel(int userid, int itemid, double value, int y = 0);
	~AnswerModel();

	int UserId;
	int ItemId;
	double Value;
	int Y;
};

