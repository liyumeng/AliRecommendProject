import pybrain
from pybrain.tools.shortcuts import buildNetwork
from pybrain.supervised.trainers.backprop import BackpropTrainer
from pybrain.datasets.supervised import SupervisedDataSet
from BinReader import BinReader
from pybrain.utilities import percentError
from pybrain.datasets.classification import ClassificationDataSet
from pybrain.structure.networks.feedforward import FeedForwardNetwork
from pybrain.structure.modules.sigmoidlayer import SigmoidLayer
from pybrain.structure.modules.linearlayer import LinearLayer
from pybrain.structure.connections.full import FullConnection
from pybrain.tools.xml.networkwriter import NetworkWriter

dim = 381
n = FeedForwardNetwork()
inLayer = LinearLayer(dim)
hiddenLayer = SigmoidLayer(100)
outLayer = LinearLayer(1)

n.addInputModule(inLayer)
n.addModule(hiddenLayer)
n.addOutputModule(outLayer)

in_to_hidden = FullConnection(inLayer,hiddenLayer)
hidden_to_out = FullConnection(hiddenLayer,outLayer)

n.addConnection(in_to_hidden)
n.addConnection(hidden_to_out)

n.sortModules()


print 'build set'

alldata = ClassificationDataSet(dim, 1, nb_classes=2)

(data,label,items) = BinReader.readData(ur'F:\AliRecommendHomeworkData\1212新版\train15_17.expand.samp.norm.bin') 
#(train,label,data) = BinReader.readData(r'C:\data\small\norm\train1217.bin')
for i in range(len(data)):
    alldata.addSample(data[i],label[i])

tstdata, trndata = alldata.splitWithProportion(0.25)

trainer = BackpropTrainer(n,trndata,momentum=0.1,verbose=True,weightdecay=0.01)

print 'start'
#trainer.trainEpochs(1)
trainer.trainUntilConvergence(maxEpochs=2)
trnresult = percentError(trainer.testOnClassData(),trndata['class'])

tstresult = percentError(trainer.testOnClassData(dataset=tstdata), tstdata['class'])

print "epoch: %4d" % trainer.totalepochs, \
        "  train error: %5.2f%%" % trnresult, \
        "  test error: %5.2f%%" % tstresult

print 'get result'

#trainer.trainUntilConvergence()
#out = ClassificationDataSet(37,1)
#(test,label,items) = BinReader.readData(r'C:\data\homework\1218t5w.bin')
##(test,label,data) = BinReader.readData(r'C:\data\small\norm\test1218.bin')
#for i in range(len(test)):
#    temp = [0]
#    out.addSample(test[i],temp)
#    #print temp,label[i]
#out = n.activateOnDataset(out)

#outresult = percentError(trainer.testOnClassData(dataset=out),
#tstdata['class'])
#print " out error: %5.4f%%" % outresult
#f = open(r'd:\rrr.csv','w')
#for i in range(len(test)):
#    f.write('%d,%d,%f\n' % (items[i][0],items[i][1],out[i]))
#f.close()
#pass
NetworkWriter.writeToFile(n, 'filename.xml')

reader = BinReader(ur'F:\AliRecommendHomeworkData\1212新版\test18.expand.norm.bin')
reader.open()
result = [0] * reader.LineCount
for i in xrange(reader.LineCount):
    (x,userid,itemid,label) = reader.readline()
    x[0] = 1
    y = n.activate(x)[0]
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