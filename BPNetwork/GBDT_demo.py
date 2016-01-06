from sklearn.ensemble import GradientBoostingClassifier
from BinReader import BinReader
import numpy as np
from sklearn.ensemble.gradient_boosting import GradientBoostingRegressor


(data,label,items) = BinReader.readData(ur'C:\data\medium\norm\train1217.bin') 

X_train = np.array(data)
label = [item[0] for item in label]
y_train = np.array(label)
est = GradientBoostingRegressor(n_estimators=300, learning_rate=0.1,max_depth=5, random_state=0, loss='ls',verbose=1).fit(X_train, y_train)
print 'testing...'


pass

reader = BinReader(ur'C:\data\test1218.bin')
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
result = result[:400]


print ur'正在输出...'
with open('result1.csv','w') as f:
    for item in result:
        f.write('%d,%d\n' % (item[0],item[1]))
print ur'阈值：',result[-1][2]
print ur'样本总数:',reader.LineCount