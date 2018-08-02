import os
import re
import string

words = {}
unigrams = {}
bigrams = {}
N = 10000
BIGRAM_PAIRS = 1000000


def read_corpus():
    corpus_file = "ANC-written-noduplicate+pangram.txt"
    f = open(corpus_file, 'r')
    lines = f.readlines()
    for line in lines[:N]:
        data = line.split(' ')
        words[data[0]] = int(data[1])
    words['<s>'] = 0
    f.close()


def process(parent, filename):
    file_path = os.path.join(parent, filename)
    f = open(file_path, "rb")
    lines = f.readlines()
    cnt = 0
    for line in lines:
        cnt += 1
        line = line.decode('utf-8').lower()
        data = re.split('[ \t]', line)
        if len(data) != 3:
            continue
        pre, suc, freq = data[0], data[1], int(data[2])
        if pre in words and suc in words:
            s = pre + ' ' + suc
            if s in bigrams:
                bigrams[s] += freq
            else:
                bigrams[s] = freq
        if cnt % 100000 == 0:
            print(filename + ': ' + str(int(cnt / 100000)) + "%")
    f.close()


def scan_files():
    data_dir = "../../LDC2006T13 (Google 1T 5gram language model)/DVD1/data/2gms"
    cnt = 0
    for parent, dir_names, file_names in os.walk(data_dir):
        for filename in file_names:
            if filename[-3:] != '.gz' and filename[-4:] != '.idx':
                print("Processing " + filename)
                process(parent, filename)


def save_results():
    f = open('bigrams-LDC-10k.txt', 'w')
    sorted_bigrams = sorted(bigrams.items(), key=lambda d: d[1], reverse=True)
    for bigram in sorted_bigrams:
        print(bigram[0], bigram[1], file=f)
    f.close()


def calc_bigrams():
    f = open('../../LDC2006T13 (Google 1T 5gram language model)/bigrams-LDC-10k.txt', 'r')
    global bigrams
    bigrams, used_bigrams = {}, {}
    lines = f.readlines()
    done = 0
    for line in lines:
        done += 1
        data = line.split(' ')
        pre, suc, pair, freq = data[0], data[1], data[0] + ' ' + data[1], int(data[2])
        if pre not in unigrams:
            unigrams[pre] = 0
        if suc not in unigrams:
            unigrams[suc] = 0
        unigrams[pre] += freq
        unigrams[suc] += freq
        bigrams[pair] = freq
        if done <= BIGRAM_PAIRS:
            used_bigrams[pair] = freq
        if done % 100000 == 0:
            print('Read', done / 100000, '/', len(lines) / 100000)
    f.close()

    f = open('bigrams-LDC-10k-katz.txt', 'w')
    cnt = {}

    for pair, freq in bigrams.items():
        if freq in cnt:
            cnt[freq] += 1
        else:
            cnt[freq] = 1

    print(BIGRAM_PAIRS, file=f)
    k = 2000
    beta = {}
    done = 0
    for pair, c in used_bigrams.items():
        done += 1
        [pre, suc] = pair.split(' ')
        if c > k:
            d = 1
        else:
            d = ((c + 1) / c * cnt[c+1] / cnt[c] - (k + 1) * cnt[k+1] / cnt[40]) / (
                        1 - (k + 1) * cnt[k+1] / cnt[40])
        prob = d * c / unigrams[pre]
        print("%s %.8g" % (pair, prob), file=f)
        if pre in beta:
            beta[pre] -= prob
        else:
            beta[pre] = 1 - prob
        if done % 100000 == 0:
            print('Bigram', done / 100000, '/', len(used_bigrams) / 100000)
    print('Finish bigram part')
    print(len(unigrams), file=f)
    done = 0
    for pre in unigrams:
        if pre in beta:
            b = beta[pre]
        else:
            b = 1
        tot = 0
        for suc in unigrams:
            if suc == '<s>':
                continue
            s = pre + ' ' + suc
            if s not in used_bigrams:
                tot += unigrams[suc]
        print("%s %d %.8g" % (pre, unigrams[pre], b / tot), file=f)
        done += 1
        if done % 100 == 0:
            print('Unigram', done / 100, '/', len(unigrams) / 100)
    f.close()


read_corpus()

# Clean 2gms for 10K words
# scan_files()
# save_results()

# Katz Smoothing
calc_bigrams()
