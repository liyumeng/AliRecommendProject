from sklearn.ensemble import GradientBoostingClassifier
from BinReader import BinReader
import numpy as np
from sklearn.ensemble.gradient_boosting import GradientBoostingRegressor

(data,label,items) = BinReader.readData(ur'F:\AliRecommendHomeworkData\1212新版\train1217.expand.norm.bin') 

X_train = np.array(data)
label = [item[0] for item in label]
y_train = np.array(label)
est = GradientBoostingRegressor(n_estimators=150, learning_rate=0.1,max_depth=3, random_state=0, loss='ls',verbose=1).fit(X_train, y_train)
print 'testing...'

reader = BinReader(ur'F:\AliRecommendHomeworkData\1212新版\test18.expand.norm.bin')
reader.open()
result = [0] * reader.LineCount
for i in xrange(reader.LineCount):
    (x,userid,itemid,label) = reader.readline()
    x[0] = 1
    y = est.predict([x])[0]
    result[i] = (userid,itemid,y)
    if i % 10000 == 0:
        print '%d/%d' % (i,reader.LineCount)
    
result.sort(key=lambda x:x[2],reverse=True)
result = result[:7000]


print ur'正在输出...'
with open('result.csv','w') as f:
    for item in result:
        f.write('%d,%d\n' % (item[0],item[1]))
print ur'阈值：',result[-1][2]
print ur'样本总数:',reader.LineCount