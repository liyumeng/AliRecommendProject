from BinReader import BinReader
import numpy as np
class Tester(object):
    """description of class"""


    def __init__(self,filename):
        self._filename = filename
        pass

    def load(self):
        reader = BinReader(self._filename)
        reader.open()
        self.LineCount = reader.LineCount
        self.data = [0] * reader.LineCount
        self.PosiCount = 0
        for i in xrange(reader.LineCount):
            self.data[i] = reader.readline()
            self.data[i][0][0] = 1
            if self.data[i][3] == 1:
                self.PosiCount+=1
        reader.close()

    def mapFeature(self,x):
        x = x[range(0,37)]
        x[0] = 1
        return x

    #返回f值及threshold
    def test(self,theta):
        dim = len(theta)
        print ur'theta维度:',dim
        right = 0
        result = [0] * self.LineCount
        for i in xrange(self.LineCount):
            x = self.mapFeature(np.array(self.data[i][0]))
            y = np.dot(x,theta)
            result[i] = (self.data[i][1],self.data[i][2],y,self.data[i][3])
            
        result.sort(key=lambda x:x[2],reverse=True)
        result = result[:self.PosiCount]

        for item in result:
            right+=item[3]

        f = 1.0 * right / self.PosiCount
        return f,result[-1][2]

    @staticmethod
    def readTheta(filename):
        with open(filename,'r') as f:
            theta = map(float,f.readline().rstrip('\n').rstrip(',').split(','))
        return theta

    @staticmethod
    def fastTest(theta):
        print ur'正在快速测试中...'
        tester = Tester(ur'c:\data\homework\1218t5w.bin')
        tester.load()
        print 'f:%f\nthres:%f' % tester.test(theta)



if __name__ == '__main__':
    tester = Tester(ur'c:\data\homework\1218t5w.bin')
    tester.load()

    theta = Tester.readTheta(ur'F:\AliRecommendHomeworkData\thetas\4.8的theta.txt')

    print '\nf:%f\n\nthres:%f' % tester.test(theta)




