from BinReader import BinReader
from BinWriter import BinWriter

TOPN = 198000

reader = BinReader(ur'F:\AliRecommendHomeworkData\1212新版\test18.expand.norm.bin')
reader.open()

writer = BinWriter(reader._filename.rstrip('.bin') + '.top.bin')
writer.open(TOPN,reader.Dim)

with open('an.csv') as f:
    items = set(f.readlines())


posi = 0
for i in range(reader.LineCount):
    (x,userid,itemid,label) = reader.readline()

    if i < 800000:
        continue
    if '%d,%d\n' % (userid,itemid) in items:
        label = 1
        posi+=1
    else:
        label = 0
    writer.writeline(x,userid,itemid,label)
   
print ur'正例个数:',posi
writer.close()
reader.close()
