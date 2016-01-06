#include "AnswerModel.h"



AnswerModel::AnswerModel()
{
}
AnswerModel::AnswerModel(int userid, int itemid, double value, int y)
{
	UserId = userid;
	ItemId = itemid;
	Value = value;
	Y = y;
}


AnswerModel::~AnswerModel()
{
}

