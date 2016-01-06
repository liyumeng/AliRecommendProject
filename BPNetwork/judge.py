
with open(r'c:\data\1218answer.csv') as f:
    answers=set(f.readlines())
    
with open('result.csv') as f:
    items=f.readlines()

count=len([item for item in items if item in answers])

print count
raw_input()

    