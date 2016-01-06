@echo off
set filename=F:\AliRecommendHomeworkData\434维特征搜索作业数据\trainingset_20141217_3_8_5.csv
echo Current File:
echo %filename%
pause>nul
python 3expandfeature.py %filename%