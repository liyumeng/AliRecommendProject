@echo off
set filename=F:\AliRecommendHomeworkData\3days_8hourspan_unnormal\trainingset_3_8_5.merged
echo Current File:
echo %filename%
pause>nul
python 2sampling.py %filename%.csv
python 3expandfeature.py %filename%.samp.csv
python 4normalize.py %filename%.samp.expand.csv
python 5csv2bin.py %filename%.samp.expand.norm.csv
pause>nul