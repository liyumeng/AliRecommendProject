import numpy as np
from sklearn import preprocessing
from BinReader import BinReader
THETAFILE = ur'F:\AliRecommendHomeworkData\thetas\theta.csv'
FILENAME = ur'F:\AliRecommendHomeworkData\1212新版\test18.expand.norm.bin'

print '----------------------'
print ur'正在测试',THETAFILE

#读出逻辑回归训练后的theta值
def readTheta():
    with open(THETAFILE,'r') as f:
        theta = map(float,f.readline().rstrip('\n').rstrip(',').split(','))
        return theta


def test():
    theta = readTheta()

    count = 0
    reader = BinReader(FILENAME)
    reader.open()
    result = [0] * reader.LineCount
    for i in xrange(reader.LineCount):
        (x,userid,itemid,label) = reader.readline()
        x[0] = 1
        y = np.dot(x[:37],theta)
        result[i] = (userid,itemid,y)
        if i % 10000 == 0:
            print '%d/%d' % (i,reader.LineCount)
    
    result.sort(key=lambda x:x[2],reverse=True)
    result = result[:6500]

    print ur'样本总数:',count
    print ur'正在输出...'
    with open('result.csv','w') as f:
        for item in result:
            f.write('%d,%d\n' % (item[0],item[1]))

    print ur'测试结束，输出个数:',6500

test()