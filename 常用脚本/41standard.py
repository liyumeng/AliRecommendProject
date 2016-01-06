"""
本脚本用于对未归一化的数据进行归一化
"""
import sys
import numpy as np
#配置项
#输入文件路径，需要是未进行归一化的文件
filename = r'F:\AliRecommend2Data\trainsets\1217.sample.expand.csv'
#配置项结束
if len(sys.argv) > 1:
    src = sys.argv[1]
else:
    src = filename

#输出文件
dst = src.rstrip('.csv') + '.norm.csv'

dst_f = open(dst,'w')
with open(src,'r') as f:
    header = f.readline()
    length = len(header.rstrip('\n').rstrip(',').split(','))
    print ur'表头列数量:',length
    aver = [0] * length
    count = 0
    for tmp in f:
        items = [float(item) for item in tmp.rstrip('\n').rstrip(',').split(',')]
        length = len(items)
        for i in xrange(length):
            aver[i]+=items[i]
        count+=1
        if count % 1000 == 0:
            print count

print ur'共有数据%d行' % count


for i in xrange(length):
    aver[i] = 1.0 * aver[i] / count


with open(src,'r') as f:
    header = f.readline()
    length = len(header.rstrip('\n').rstrip(',').split(','))
    print ur'表头列数量:',length
    s = [0] * length
    count = 0
    for tmp in f:
        items = [float(item) for item in tmp.rstrip('\n').rstrip(',').split(',')]
        length = len(items)
        for i in xrange(length):
            s[i]+=(items[i] - aver[i]) * (items[i] - aver[i])
        count+=1
        if count % 1000 == 0:
            print count

for i in xrange(length):
    s[i] = sqrt(1.0 * s[i] / (count - 1))

t = 0
with open(src,'r') as f:
    header = f.readline()
    dst_f.write(header)
    for tmp in f:
        items = [float(item) for item in tmp.rstrip('\n').rstrip(',').split(',')]
        for i in xrange(4,length):
            items[i] = (items[i] - aver[i]) / s[i]
        dst_f.write(','.join(map(str,items)))
        dst_f.write('\n')
        t+=1
        if t % 1000 == 0:
            print t
dst_f.close()
print 'output complete.'
print dst