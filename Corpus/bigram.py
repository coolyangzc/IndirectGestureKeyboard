import os, re, string

words = {}
unigrams = {}
bigrams = {}


def read_corpus():
    corpus_file = "ANC-written-noduplicate+pangram.txt"
    f = open(corpus_file, "r")
    lines = f.readlines()
    for line in lines[:10000]:
        data = line.split(' ')
        words[data[0]] = int(data[1])
    print(len(words))
    f.close()


def process(file_path):
    f = open(file_path, "rb")
    raw = f.read()
    raw = raw.decode('utf-8').lower()
    cur = ''
    data = ['#']
    for ch in raw:
        if ch.isalpha():
            cur += ch
        else:
            if cur != '':
                data.append(cur.lower())
                cur = ''
            if data[-1] != '#' and ch in string.punctuation or ch.isdigit():
                data.append('#')
    for i in range(len(data)):
        if data[i] == '#':
            continue
        if data[i] not in words:
            data[i] = 'OOV'
        else:
            if data[i] in unigrams:
                unigrams[data[i]] += 1
            else:
                unigrams[data[i]] = 1
    for i in range(len(data) - 1):
        if data[i] != '#' and data[i+1] != '#' and data[i] != 'OOV' and data[i+1] != 'OOV':
            s = data[i] + ' ' + data[i+1]
            if s in bigrams:
                bigrams[s] += 1
            else:
                bigrams[s] = 1
    f.close()


def scan_files():
    data_dir = "../../OANC-GrAF/data/"
    cnt = 0
    for parent, dir_names, file_names in os.walk(data_dir):
        for filename in file_names:
            if filename[-4:] == ".txt" and "written_" in parent:
                #print("filename with full path:" + os.path.join(parent, filename))
                #print("Processing " + filename)
                process(os.path.join(parent, filename))
                cnt += 1
                if cnt % 100 == 0:
                    print("Processed " + str(cnt) + " files")


def save_results():
    f = open('unigrams-written.txt', 'w')
    sorted_unigrams = sorted(unigrams.items(), key=lambda d: d[1], reverse=True)
    for unigram in sorted_unigrams:
        print(unigram[0], unigram[1], file=f)
    f.close()

    f = open('bigrams-written.txt', 'w')
    sorted_bigrams = sorted(bigrams.items(), key=lambda d: d[1], reverse=True)
    for bigram in sorted_bigrams:
        print(bigram[0], bigram[1], file=f)
    f.close()

    f = open('bigrams-written-prob.txt', 'w')

    f.close()


read_corpus()
scan_files()
save_results()
