import os
import string

words = {}
unigrams = {}
bigrams = {}
N = 10000


def read_corpus():
    corpus_file = "ANC-written-noduplicate+pangram.txt"
    f = open(corpus_file, "r")
    lines = f.readlines()
    for line in lines[:N]:
        data = line.split(' ')
        words[data[0]] = int(data[1])
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
                process(os.path.join(parent, filename))
                cnt += 1
                if cnt % 100 == 0:
                    print("Processed " + str(cnt) + " files")


def save_results():
    f = open('unigrams-written.txt', 'w')
    for word in words:
        if word not in unigrams:
            unigrams[word] = 0
    sorted_unigrams = sorted(unigrams.items(), key=lambda d: d[1], reverse=True)
    for unigram in sorted_unigrams:
        print(unigram[0], unigram[1], file=f)
    f.close()

    f = open('bigrams-written.txt', 'w')
    sorted_bigrams = sorted(bigrams.items(), key=lambda d: d[1], reverse=True)
    for bigram in sorted_bigrams:
        print(bigram[0], bigram[1], file=f)
    f.close()


    #Additive Smoothing
    f = open('bigrams-written-prob.txt', 'w')
    eps = 0.5
    print('eps ' + str(eps), file=f)
    for bigram in sorted_bigrams:
        pair, freq, word = bigram[0], bigram[1], bigram[0].split(' ')[0]
        print(pair, (freq + eps) / (unigrams[word] + N * eps), file=f)
    f.close()

    #Katz Smoothing
    f = open('bigrams-written-katz.txt', 'w')
    cnt = {}
    for bigram in sorted_bigrams:
        pair, freq = bigram[0], bigram[1]
        if freq in cnt:
            cnt[freq] += 1
        else:
            cnt[freq] = 1
    print(len(sorted_bigrams), file=f)
    k = 5
    beta = {}
    for bigram in sorted_bigrams:
        pair, c, [pre, suc] = bigram[0], bigram[1], bigram[0].split(' ')
        if c > k:
            d = 1
        else:
            d = ((c+1)/c * cnt[c+1]/cnt[c] - (k+1)*cnt[k+1]/cnt[1]) / (1 - (k+1)*cnt[k+1] / cnt[1])
        prob = d * c / unigrams[pre]
        print(pair, prob, file = f)
        if pre in beta:
            beta[pre] -= prob
        else:
            beta[pre] = 1 - prob
    for pre in unigrams:
        if pre in beta:
            b = beta[pre]
        else:
            b = 1
        sum = 0
        for suc in unigrams:
            s = pre + ' ' + suc
            if s not in bigrams:
                sum += unigrams[suc]
        print(pre, b/sum, file=f)
    f.close()


read_corpus()
scan_files()
save_results()
