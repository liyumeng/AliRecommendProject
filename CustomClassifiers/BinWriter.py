import struct
import array

class BinWriter(object):
    """description of class"""
    def __init__(self,filename):
        self._filename = filename

    def writeline(self,x,userid,itemid,label):
        u = struct.pack('i',userid)
        i = struct.pack('i',itemid)
        l = struct.pack('i',label)

        self._file.write(u)
        self._file.write(i)
        self._file.write(l)

        x.tofile(self._file)

    def open(self,linecount,dim):
        self._file = open(self._filename,'wb')
        val = struct.pack('i',linecount)
        self._file.write(val)
        val = struct.pack('i',dim)
        self._file.write(val)
        

    def close(self):
        self._file.close()

if __name__ == '__main__':
    reader = BinReader(r'F:\AliRecommendHomeworkData\1000.bin')
    reader.open()
    print reader.readline()

