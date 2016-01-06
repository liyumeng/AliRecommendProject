import struct
import sys

class BinConverter(object):
    """
    本脚本用于对未归一化的数据进行归一化
    二进制文件格式
    文件前4个字节为文件总行数
    接下来4个字节为每行的维度数
    接下来是数据，数据的每一维是4个字节
    """
    @staticmethod
    def Csv2Bin(src):
        dst = src.rstrip('.csv') + ".bin"
        dstfile = open(dst,'wb')
        #写入文件行数
        dstfile.write(struct.pack('i',0))

        lineCount = 0
        print ur'正在输出...'
        with open(src) as f:
            line = f.readline()
            headers = line.rstrip('\n').rstrip(',').split(',')
            print line
            #每行有多少维（userid,itemid,label开头这3列也算在维度之内，这3个是int型，其余全部double型，第4列是o2o，要辅助用作常数项，所以也用double了）
            length = len(headers)
            dstfile.write(struct.pack('i',length))
    
            for line in f:
                items = line.rstrip('\n').rstrip(',').split(',')
                for i in xrange(3):
                    val = struct.pack('i',int(float(items[i])))
                    dstfile.write(val)

                for i in xrange(3,length):
                    b = struct.pack('d',float(items[i]))
                    dstfile.write(b)

                lineCount+=1
                if lineCount % 1000 == 0:
                    print lineCount

        print ur'总行数：',lineCount
        dstfile.seek(0)
        dstfile.write(struct.pack('i',lineCount))
        dstfile.close()
        print ur'文件保存完毕'
        return dst

if __name__ == '__main__':
    BinConverter.Csv2Bin(ur'c:\data\homework\train15_17.norm.csv');


