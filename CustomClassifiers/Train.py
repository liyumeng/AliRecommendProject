import numpy as np
import scipy as sp
from scipy.optimize._minimize import minimize
from sklearn import preprocessing
import random
import shutil
import time
from Tester import Tester
from BinReader import BinReader

filename = ur'F:\AliRecommendHomeworkData\1212新版\train16.expand.csv'
THETAFILE = ur'F:\AliRecommendHomeworkData\thetas\theta.csv'

#整理复制倍数
POSI_DUP = 5
#负例是正理的几倍
NEGA_RATE = 10
pLambda = 0.3

min_max_scaler = preprocessing.MinMaxScaler()

def load(filename):
    with open(filename,'r') as f:
        f.readline()
        return [map(float,item.rstrip('\n').rstrip(',').split(',')) for item in f.xreadlines()]

def writeTheta(theta):
    with open(THETAFILE,'w') as f:
        f.write(','.join([str(item) for item in theta]))

def sigmoid(x):
    return 1.0 / (1 + np.exp(-x))

def predict(theta,x):
    p = sigmoid(np.dot(x ,theta)) >= 0.5
    return p

def mapFeature(x):
    x = x[:,range(0,81) + range(108,188) + range(215,435)]
    x = min_max_scaler.fit_transform(x)
    for item in x:
        item[0] = 1
    return x

def getXY(raw_data):
    y = raw_data[:,2]
    online = raw_data[:,3]
    x = raw_data[:,3:]  #3列是给常量留的空间
    result = raw_data[:,:4]
    return x,y,online,result

def costFunctionReg(theta,x,y,pLambda):
    m = len(y)
    
    h = sigmoid(np.dot(x, theta))
    theta0 = theta[0]
    theta[0] = 0    #正则项不包含theta0

    reg = float(pLambda) / 2 / m * np.inner(theta,theta)
    posi = np.inner(y,np.log(h + 0.00000001))
    nega = np.inner(1 - y,np.log(1 - h + 0.00000001))

    J = -1.0 / m * (posi + nega) + reg

    theta[0] = theta0
    return J

def gradFunctionReg(theta,x,y,pLambda):
    m = len(y)
    
    h = sigmoid(np.dot(x, theta))
    theta0 = theta[0]
    theta[0] = 0    #正则项不包含theta0
    grad = 1.0 / m * np.dot(np.transpose(x) ,h - y) + float(pLambda) / m * theta
    theta[0] = theta0
    return grad

def print_analyse(right_posi_count,predict_posi_count,real_posi_count):
    print '\n',ur'正例总数：%d，预测的正例数：%d，其中对的有：%d' % (real_posi_count,predict_posi_count,right_posi_count)
    precision = float(right_posi_count) / predict_posi_count * 100
    recall = float(right_posi_count) / real_posi_count * 100
    f1 = 2.0 * precision * recall / (precision + recall)
    print 'precision:%.4f%%, recall:%.4f%%, f1:%.4f%%' % (precision,recall,f1)
    return precision,recall,f1

def train():
    print ur'正在载入文件:',filename
    raw_data = load(filename)

    #raw_data = expendX(raw_data) #加上除法特征
    print ur'数据载入成功,len=%d' % len(raw_data)
    posi_dup = POSI_DUP
    raw_data = np.array(raw_data)

    if NEGA_RATE > 0 or posi_dup > 1:
        posi = raw_data[raw_data[:,2] == 1,:]
        nega = raw_data[raw_data[:,2] == 0,:]
        np.random.shuffle(nega)

        raw_data = nega[:NEGA_RATE * posi_dup * len(posi),:]
        for i in range(posi_dup):
            raw_data = np.append(raw_data,posi,axis=0)

    np.random.shuffle(raw_data)

    x,y,online,result = getXY(raw_data)
    print ur'扩充完毕'
    print ur'正例:%d, 负例:%d' % (sum(y),sum(y == 0))
    print ur'在线商品：%d, 在线商品中正例%d' % (sum(online),sum(y[online == 1]))

    x = mapFeature(x)
    print ur'正例扩充%d倍，训练集总大小：%d，维度：%d' % (posi_dup,len(x),x.shape[1])
    
    theta = np.zeros(x.shape[1])

    args = (x,y,pLambda)

    res = minimize(costFunctionReg,theta,jac=gradFunctionReg,args=args,method='BFGS')
    theta = res.x

    writeTheta(theta)

    p = predict(theta,x)

    accuracy = np.mean(p == y) * 100
    print 'Train Accuracy:%f' % accuracy

    print_analyse(sum(p[y == 1]),sum(p),sum(y))

    test(theta)
    pass

def test(theta):
    print '----------------------'
    print ur'正在测试'

    (raw_data,posiCount) = BinReader.readData(r'c:\data\homework\1218t20_3.bin')
    
    x,real_y,online,result = getXY(np.array(raw_data))

    x = mapFeature(x)
    values = sigmoid(np.dot(x ,theta))

    #第3行是真实的结果，第4行是预测的概率
    result[:,3] = values
    
    result = list(result)
    result.sort(key=lambda x:x[3],reverse=True)
    print ur'共获得结果%d' % sum(values > 0.5)
    
    result = result[:1300]
   
    right = sum([item[2] for item in result])
    
    precision,recall,f1 = print_analyse(right,1300,posiCount)
    
    print ur'测试完毕'

if __name__ == '__main__':
    train()
    #theta = Tester.readTheta(ur'F:\AliRecommendHomeworkData\thetas\theta.csv')
    #test(theta)
