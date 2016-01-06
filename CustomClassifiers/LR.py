import numpy as np
import scipy as sp
from scipy.optimize._minimize import minimize
from sklearn import preprocessing
import random
import shutil
import time

DAYS = 3
HOURSPAN = 8

RESULTCOUNT = 400
MAINDIR = r'F:\AliRecommend2Data'
DIR = MAINDIR + r'\%ddays_%dhourspan_unnormal' % (DAYS,HOURSPAN)
LOGDIR = MAINDIR + r'\logs'
#文件名1126就表示预测的那天是11月26号，使用的数据是23、24、25三天的数据
TRAININGFILE = DIR + r'\trainingset_2014%d' + r'_%d_%d_5.csv' % (DAYS,HOURSPAN)
MERGEDTRAININGFILE = DIR + r'\trainingset_merged.csv'
#训练出theta后直接用某个文件进行测试就可以了
TESTFILE = DIR + r'\testset_2014%d' + r'_%d_%d.csv' % (DAYS,HOURSPAN)
THETAFILE = r'F:\AliRecommendHomeworkData\thetas\theta.txt'

ANSWREFILE = DIR + r'\testset_20141219' + r'_%d_%d.csv' % (DAYS,HOURSPAN)

PREDICTFILE = MAINDIR + r'\predicts\2014%d.csv'
RIGHTANSWERFILE = MAINDIR + r'\answers\2014%d.csv'
RESULTFILE = DIR + r'\result.csv'
RIHGTANSWER = MAINDIR + r'\answers\count.csv'


#整理复制倍数
POSI_DUP = 0
#负例是正理的几倍
NEGA_RATE = 0
pLambda = 0.3

def load(filename):
    with open(filename,'r') as f:
        f.readline()
        return [map(float,item.rstrip('\n').rstrip(',').split(',')) for item in f.xreadlines()]

def load_result(filename):
    with open(filename,'r') as f:
        f.readline()
        return [item.rstrip('\n') for item in f.xreadlines()]

min_max_scaler = preprocessing.MinMaxScaler()
standard_scaler = preprocessing.StandardScaler()

#ui 在4~39，u在44~79，i在111~146，c在151~186，uc在218~253
ui_range = range(4,40)
u_range = range(44,79)
i_range = range(111,146)
c_range = range(151,186)
uc_range = range(218,253)

useritem_f = range(1,41)
user_f = range(41,99)
item_f = range(99,139)
category_f = range(139,179)
usercategory_f = range(179,218)

def mapFeature(x):
    #x = x[:,range(0,81) + range(108,188) + range(215,255)]
    x = min_max_scaler.fit_transform(x)
    #x = standard_scaler.fit_transform(x)

    for item in x:
        item[0] = 1
        print item
        raw_input()
    return x

def readTheta():
    with open(THETAFILE,'r') as f:
        theta = map(float,f.readline().rstrip('\n').rstrip(',').split(','))
        return theta

def writeTheta(theta):
    with open(THETAFILE,'w') as f:
        f.write(','.join([str(item) for item in theta]))

def sigmoid(x):
    return 1.0 / (1 + np.exp(-x))

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

def predict(theta,x):
    p = sigmoid(np.dot(x ,theta)) >= 0.5
    return p

def getXY(raw_data):
    y = raw_data[:,2]
    online = raw_data[:,3]
    x = raw_data[:,3:]  #3列是给常量留的空间
    result = raw_data[:,:4]
    return x,y,online,result

#4~49,44~79 ..142~177,182~217
def divide(a,b):
    if b > 0 or b < 0:
        return 1.0 * a / b
    else:
        return 0

def expendX(raw_data):
    for i in range(len(raw_data)):
        tail = []
        for a,b in zip(ui_range,u_range):#ui 在4~39，u在44~79，i在111~146，c在151~186，uc在218~253
            tail.append(divide(raw_data[i][a],raw_data[i][b]))
        for a,b in zip(uc_range,c_range):
            tail.append(divide(raw_data[i][a],raw_data[i][b]))
        for a,b in zip(ui_range,uc_range):
            tail.append(divide(raw_data[i][a],raw_data[i][b]))
        for a,b in zip(i_range,c_range):
            tail.append(divide(raw_data[i][a],raw_data[i][b]))
        for a,b in zip(uc_range,u_range):
            tail.append(divide(raw_data[i][a],raw_data[i][b]))

        raw_data[i] = raw_data[i] + tail
    return raw_data

def minimize2(costFunctionReg,theta,jac=gradFunctionReg,**kwargs):
    rate = 0.1
    lastJ = 0
    args = kwargs['args']
    for i in range(40000):
        J = costFunctionReg(theta,args[0],args[1],args[2])
        grad = gradFunctionReg(theta,args[0],args[1],args[2])
        theta = theta - rate * grad
        if i % 100 == 0:
            print '\r',i,J,
        if np.abs(J - lastJ) < 10E-6:
            break
        lastJ = J
    return theta


def read(file,num):
    t = file.readline()
    i = 1
    item = []
    while t:
        item.append(map(float,t.rstrip('\n').rstrip(',').split(',')))
        if i >= num:
            break
        t = file.readline()
        i = i + 1
    return item

def print_analyse(right_posi_count,predict_posi_count,real_posi_count):
    print '\n',ur'正例总数：%d，预测的正例数：%d，其中对的有：%d' % (real_posi_count,predict_posi_count,right_posi_count)
    precision = float(right_posi_count) / predict_posi_count * 100
    recall = float(right_posi_count) / real_posi_count * 100
    f1 = 2.0 * precision * recall / (precision + recall)
    print 'precision:%.4f%%, recall:%.4f%%, f1:%.4f%%' % (precision,recall,f1)
    return precision,recall,f1

def getRightAnswer(date):
    dict = {}
    with open(RIHGTANSWER,'r') as f:
        f.readline()
        answers = [item.rstrip('\r\n').split(',') for item in f.readlines()]
        for item in answers:
            dict[item[0]] = int(item[1])
    key = '2014%d' % date
    return dict.get(key,0)


def train(date=0,theta=0):
    if date == 0:
        filename = MERGEDTRAININGFILE
        filename = DIR + r'\trainingset_3_8_5.merged.csv'
        filename = ur'F:\AliRecommendHomeworkData\1000.csv'
    else:
        filename = TRAININGFILE % date
    raw_data = load(filename)
    #raw_data = expendX(raw_data) #加上除法特征
    print 'data loaded,len=%d' % len(raw_data)
    posi_dup = POSI_DUP
    raw_data = np.array(raw_data)

    if NEGA_RATE > 0 or posi_dup > 1:
        posi = raw_data[raw_data[:,2] == 1,:]
        nega = raw_data[raw_data[:,2] == 0,:]
        np.random.shuffle(nega)

        raw_data = nega[:NEGA_RATE * posi_dup * len(posi),:]
        for i in range(posi_dup):
            raw_data = np.append(raw_data,posi,axis=0)

    #np.random.shuffle(raw_data)

    x,y,online,result = getXY(raw_data)
    print ur'扩充完毕'
    print ur'正例:%d, 负例:%d' % (sum(y),sum(y == 0))
    print ur'在线商品：%d, 在线商品中正例%d' % (sum(online),sum(y[online == 1]))

    x = mapFeature(x)
    print ur'正例扩充%d倍，训练集总大小：%d，维度：%d' % (posi_dup,len(x),x.shape[1])
    if theta == 0:
        theta = np.zeros(x.shape[1])
    else:
        theta = readTheta()


    args = (x,y,pLambda)

    res = minimize(costFunctionReg,theta,jac=gradFunctionReg,args=args,method='BFGS')
    theta = res.x

    p = predict(theta,x)
    writeTheta(theta)
    with open(MAINDIR + "\\thetas\\%d.txt" % date,'w') as f:
        f.write(','.join([str(item) for item in theta]))

    accuracy = np.mean(p == y) * 100
    print 'Train Accuracy:%f' % accuracy

    print_analyse(sum(p[y == 1]),sum(p),sum(y))

    pass

def test(date):
    print '----------------------'
    print ur'正在测试%d' % date
    theta = readTheta()
    step = 1000000

    real_posi_count = 0
    predict_posi_count = 0
    right_posi_count = 0

    #raw_data = load(TESTFILE % date)
    raw_data = load(ur'F:\AliRecommendHomeworkData\434维特征搜索作业数据\testset_20141218_3_8.expand.csv')
    #raw_data = expendX(raw_data)
    x,real_y,online,result = getXY(np.array(raw_data))

    x = mapFeature(x)
    values = sigmoid(np.dot(x ,theta))

    #第3行是真实的结果，第4行是预测的概率
    result[:,3] = values
    
    result = list(result)
    result.sort(key=lambda x:x[3],reverse=True)
    print ur'共获得结果%d' % sum(values > 0.5)
    #last_right_result = load_result(RIGHTANSWERFILE % (date - 1))
    
    result = [item for item in result if '%d,%d' % (int(item[0]),int(item[1]))]
    result = result[:6500]
    real_posi_count = getRightAnswer(date)
    predict_posi_count = len(result)
    right_posi_count = sum([item[2] for item in result])
    
    precision,recall,f1 = print_analyse(right_posi_count,predict_posi_count,real_posi_count)
    writePredict(result,date)
    print 'test finished.'
    return precision,recall,f1,right_posi_count



def answer():
    with open(THETAFILE,'r') as f:
        theta = map(float,f.readline().rstrip('\n').rstrip(',').split(','))

    raw_data = load(ANSWREFILE)
    raw_data = expendX(raw_data)
    x,real_y,online,result = getXY(np.array(raw_data))

    x = mapFeature(x)
    values = sigmoid(np.dot(x ,theta))

    #第3行无用，第4行是预测的概率
    result[:,3] = values
    
    print ur'共获得结果%d' % sum(values > 0.5)

    result = list(result)
    result.sort(key=lambda x:x[3],reverse=True)
    result = result[:RESULTCOUNT]

    write_answer(result)
    

total_predict = []
total_right_predict = []

def writePredict(result,date):
    with open(PREDICTFILE % date,'w') as f:
        f.write('userid,itemid\n')
        for item in result:
            f.write('%d,%d\n' % (int(item[0]),int(item[1])))

def train_all():
    for i in range(1121,1131) + range(1201,1209) + range(1216,1219):
        train(i)
def main():
    train(0)
    test(0)
main()

