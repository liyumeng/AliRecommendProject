import numpy as np
from sklearn import preprocessing
THETAFILE = ur'F:\AliRecommendHomeworkData\thetas\theta.csv'
FILENAME = ur'c:\data\homework\testset_20141218_3_8.expand.norm.csv'

print '----------------------'
print ur'正在测试',THETAFILE

min_max_scaler = preprocessing.MinMaxScaler()
#读出逻辑回归训练后的theta值
def readTheta():
    with open(THETAFILE,'r') as f:
        theta = map(float,f.readline().rstrip('\n').rstrip(',').split(','))
        return theta

def getXY(raw_data):
    y = raw_data[:,2]
    online = raw_data[:,3]
    x = raw_data[:,3:259]  #3列是给常量留的空间
    result = raw_data[:,:4]
    return x,y,online,result

def test():
    theta = readTheta()

    result = []
    count = 0
    with open(FILENAME,'r') as f:
        f.readline()
        for line in f:
            count+=1
            items = map(float,line.rstrip('\n').rstrip(',').split(','))
            items[3] = 1
            #range(0,81) + range(108,188) + range(215,435)
            y = np.dot(items[3:84] + items[111:191] + items[218:438],theta)
            result.append((items[0],items[1],y))
            if count % 10000 == 0:
                print count

    result.sort(key=lambda x:x[2],reverse=True)
    result = result[:6500]

    print ur'样本总数:',count
    print ur'正在输出...'
    with open('result.csv','w') as f:
        for item in result:
            f.write('%d,%d\n' % (item[0],item[1]))

    print ur'测试结束，输出个数:',6500

test()