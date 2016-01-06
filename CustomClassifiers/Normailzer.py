from BinReader import BinReader
import array
from BinWriter import BinWriter
from BinConverter import BinConverter
class Normailzer(object):
    """description of class"""
    def __init__(self,filename):
        self.filename = filename
        self.reader = BinReader(filename)

    def getMaxMin(self):
        self.reader.open()
        dim = self.reader.XDim - 1  #去除常数项
        max = [0] * dim
        min = [0] * dim
        for k in xrange(self.reader.LineCount):
            (x,userid,itemid,label) = self.reader.readline()
            for i in xrange(dim):
                if x[i + 1] > max[i]:
                    max[i] = x[i + 1]
                if x[i + 1] < min[i]:
                    min[i] = x[i + 1]
            if k % 10000 == 0:
                print '%d/%d' % (k,self.reader.LineCount)
        self.reader.close()
        return (max,min)
    
    ##调用该函数，实现归一化
    def transform(self):
        (max,min) = self.getMaxMin()
        self.reader.open()
        dim = self.reader.XDim - 1  #去除常数项
        dis = [0] * dim
        for i in xrange(dim):
            dis[i] = max[i] - min[i]

        self.writer = BinWriter(self.filename.rstrip('.bin') + '.norm.bin')
        self.writer.open(self.reader.LineCount,self.reader.Dim)

        for k in xrange(self.reader.LineCount):
            (x,userid,itemid,label) = self.reader.readline()
            for i in xrange(dim):
                x[i + 1] = (x[i + 1] - min[i]) / dis[i]
            self.writer.writeline(x,userid,itemid,label)

            if k % 10000 == 0:
                print '%d/%d' % (k,self.reader.LineCount)

        self.writer.close()

if __name__ == '__main__':
    src = ur'F:\AliRecommend2Data\github数据集\raw\train1217.samp.csv'
    dst = BinConverter.Csv2Bin(src)
    tester = Normailzer(dst)
    tester.transform()
    print ur'运行结束'




