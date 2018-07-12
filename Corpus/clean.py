fd = open('ANC-written-count.txt', 'rb')
outfd_clean = open('ANC-written-clean.txt', 'w')
outfd_nodup = open('ANC-written-noduplicate.txt', 'w')
dict = {}
lines = fd.readlines()
for line in lines[:-1]:
    try:
        line = line.decode('utf-8')
    except UnicodeDecodeError:
        continue
    line_data = line.split('\t')
    word = line_data[0]
    frequency = int(line_data[-1])
    if not word.isalpha():
        continue
    if word in dict:
        dict[word] += frequency
    else:
        dict[word] = frequency
    outfd_clean.write(word + ' ' + str(frequency) + '\n')
sorted_dict = sorted(dict.items(), key=lambda item: item[1], reverse=True)
for word in sorted_dict:
    outfd_nodup.write(word[0] + ' ' + str(word[1]) + '\n')
