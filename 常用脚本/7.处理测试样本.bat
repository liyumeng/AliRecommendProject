@echo off
set filename=F:\AliRecommendHomeworkData\trainsets\trainingset_20141217_3_8_5
echo Current File:
echo %filename%
pause>nul
python 3expandfeature.py %filename%.csv
python 4normalize.py %filename%.expand.csv
python 5csv2bin.py %filename%.expand.norm.csv %filename%.bin
pause>nul