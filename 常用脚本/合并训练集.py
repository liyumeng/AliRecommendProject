# 本程序用来将FeatureCollection生成的各天的训练集合并到一起

#配置项
#数据路径
dir = r'F:\AliRecommendHomeworkData\3days_8hourspan_unnormal'

#待合并文件的文件名
src = dir + r'\trainingset_2014%d_3_8_5.csv'
#输出文件的文件名
dst = dir + r'\trainingset_3_8_5.merged.csv'

names = range(1121,1131) + range(1201,1209) + range(1215,1217)


print ur'本程序将自动合并每天的训练集，存储路径为：',dir
print ur'输出文件为：',dst
print '--------------------------------------------------'
print ur'按回车键继续...'
raw_input()

dst_f = open(dst,'w')

with open(src % names[0],'r') as f:
    header = f.readline()
    print 'headers:'
    print header
    dst_f.write(header)

for name in names:
    print 'merge file %s' % (src % name)
    with open(src % name,'r') as f:
        f.readline()
        dst_f.write(f.read())
dst_f.close()

print ur'合并成功'
print dst