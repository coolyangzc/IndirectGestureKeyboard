dict_fd = open('ANC-written-noduplicate+pangram.txt', 'rb')


dict = {}
lines = dict_fd.readlines()
for line in lines[:5000]:
    line = line.decode('utf-8')
    word = line.split(' ')[0]
    dict[word] = 1

phrase_fd = open('phrases.txt', 'rb')
outfd = open('pangrams_phrases.txt', 'w')
lines = phrase_fd.readlines()
print('the quick brown fox jumps over the lazy dog', file=outfd)
print('the five boxing wizards jump quickly', file=outfd)
for line in lines:
    line = line.decode('utf-8').strip()
    words = line.lower().split(' ')
    ok = True
    for word in words:
        if word not in dict:
            ok = False
            break
    if ok:
        print(line, file=outfd)
