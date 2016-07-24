#include "Vector2.h"

#include <cmath>
#include <cstdio>
#include <vector>
#include <cstdlib>
#include <sstream>
#include <fstream>
#include <iostream>

#define rep(i,n) for(int i=0; i<n; ++i)
#define FOR(i,a,b) for(int i=a; i<=b; ++i)

using namespace std;

const int PHRASES = 60;
const double eps = 1e-6;
const double inf = 1e10;

double dtw[MAXSAMPLE][MAXSAMPLE];

string sentence[PHRASES], mode[PHRASES], scale[PHRASES];
double height[PHRASES], width[PHRASES], heightRatio[PHRASES], widthRatio[PHRASES], keyboardSize[PHRASES];
double WPM[PHRASES];
Vector2 keyPos[128];

vector<string> cmd;
vector<string> words;
vector<double> time;
vector<Vector2> world, relative;

fstream fout;

int Random(int mo)
{
    return rand() % mo;
}

void InitDTW()
{
    rep(i, MAXSAMPLE)
        rep(j, MAXSAMPLE)
            dtw[i][j] = inf;
    dtw[0][0] = 0;
}

bool same(const std::string& input, const std::string& tar)
{
    if (input.length() > tar.length()+1) return false;
    std::string::const_iterator p = input.begin();
    std::string::const_iterator q = tar.begin();

    while (p != input.end() && q != tar.end() && toupper(*p) == toupper(*q))
        ++p, ++q;
    return (p == input.end()) || (q == tar.end());
}

bool same(const double& x, const double& y)
{
    return (fabs(x-y) < eps);
}

void CalcKeyLayout()
{
    string line1 = "qwertyuiop";
    string line2 = "asdfghjkl";
    string line3 = "zxcvbnm";
    rep(i, line1.length())
    {
        keyPos[line1[i]] = Vector2(-0.45 + i * 0.1, 0.3333);
    }
    rep(i, line2.length())
    {
        keyPos[line2[i]] = Vector2(-0.4 + i * 0.1, 0);
    }
    rep(i, line3.length())
    {
        keyPos[line3[i]] = Vector2(-0.35 + i * 0.1, -0.333);
    }

}

void LinePushBack(string s, double t, double x = 0, double y = 0, double rx = 0, double ry = 0)
{
    cmd.push_back(s);
    time.push_back(t);

    world.push_back(Vector2(x, y));
    relative.push_back(Vector2(rx, ry));
}

void ReadData(int id, string user)
{
    stringstream ss;
    ss << id;
    string fileName = "data/" + user + "_" + ss.str() + ".txt";
    fstream fin;
    fin.open(fileName.c_str(), fstream::in);
    getline(fin, sentence[id]);
    fin >> mode[id];
    fin >> widthRatio[id] >> heightRatio[id];
    fin >> width[id] >> height[id];
    if (width[id] > 2 * height[id])
        scale[id] = "1x1";
    else
        scale[id] = "1x3";

    keyboardSize[id] = widthRatio[id] / 0.8;

    words.clear();
    int alpha = 0;
    string word = "";
    rep(i, sentence[id].length())
        if (sentence[id][i] >= 'a' && sentence[id][i] <= 'z')
        {
            alpha++;
            word += sentence[id][i];
        }
        else
        {
            words.push_back(word);
            word = "";
        }
    if (word.length() > 0)
        words.push_back(word);
    double startTime = -1;
    cmd.clear(); time.clear();
    world.clear(); relative.clear();

    string s;
    double t, x, y, rx, ry;
    while (fin >> s)
    {
        fin >> t;
        if (same(s, "Backspace"))
        {
            startTime = -1;
            cmd.clear(); time.clear();
            world.clear(); relative.clear();
            LinePushBack(s, t);
            continue;
        }
        if (same(s, "PhraseEnd"))
        {
            LinePushBack(s, t);
            continue;
        }
        fin >> x >> y >> rx >> ry;
        LinePushBack(s, t, x, y, rx, ry);
        if (same(s, "Began"))
        {
            if (startTime == -1)
                startTime = t;
        }
    }

    WPM[id] = alpha / (t - startTime) * 12;
    fin.close();
}

void CalcWPM(string fileName)
{
    fstream fout;
    fout.open(fileName.c_str(), fstream::out);
    fout << "user,scale,size,sentence,WPM" << endl;
    rep(i, PHRASES)
    {
        fout<< "1" << ","
            << scale[i] << ","
            << keyboardSize[i] << ","
            << sentence[i] << ","
            << WPM[i] << endl;
    }
    fout.close();
}

void CalcDistance(int id, vector<int>& sampleNums, fstream& fout)
{
    int line = 0;
    double keyWidth = width[id] / 10;
    rep(w, words.size())
    {
        string word = words[w];
        vector<Vector2> pts, rawstroke;
        if (word.length() == 1)
            continue;
        rep(i, word.length())
        {
            int key = word[i];
            pts.push_back(Vector2(keyPos[key].x * width[id], keyPos[key].y * height[id]));
        }
        while (line < cmd.size())
        {
            string s = cmd[line];
            Vector2 p(relative[line].x * width[id], relative[line].y * height[id]);
            line++;
            if (rawstroke.size() == 0 || dist(rawstroke[rawstroke.size()-1], p) > eps)
                rawstroke.push_back(p);
            if (same(s, "Ended"))
                break;
        }
        if (rawstroke.size() <= 1)
            return;
        rep(i, sampleNums.size())
        {
            vector<Vector2> location = temporalSampling(pts, sampleNums[i]);
            vector<Vector2> stroke = temporalSampling(rawstroke, sampleNums[i]);

            double result = match(location, stroke, dtw, Standard) / sampleNums[i];
            fout<< "1" << ","
                << scale[id] << ","
                << keyboardSize[id] << ","
                << word << ","
                << "Standard" << ","
                << sampleNums[i] << ","
                << "pixel" << ","
                << result << endl;
            fout<< "1" << ","
                << scale[id] << ","
                << keyboardSize[id] << ","
                << word << ","
                << "Standard" << ","
                << sampleNums[i] << ","
                << "keyWidth" << ","
                << result / keyWidth << endl;

            result = match(location, stroke, dtw, DTW) / sampleNums[i];

            fout<< "1" << ","
                << scale[id] << ","
                << keyboardSize[id] << ","
                << word << ","
                << "DTW" << ","
                << sampleNums[i] << ","
                << "pixel" << ","
                << result << endl;
            fout<< "1" << ","
                << scale[id] << ","
                << keyboardSize[id] << ","
                << word << ","
                << "DTW" << ","
                << sampleNums[i] << ","
                << "keyWidth" << ","
                << result / keyWidth << endl;
        }
    }
}

int main()
{
    InitDTW();
    CalcKeyLayout();

    string user = "yzc";
    fstream fout;
    fout.open("distance.csv", fstream::out);
    fout << "user,scale,size,word,algorithm,sampleNum,coor,distance" << endl;
    vector<int> sample;
    sample.push_back(16);
    sample.push_back(32);
    sample.push_back(64);
    sample.push_back(128);
    rep(i, 60)
    {
        ReadData(i, user);
        CalcDistance(i, sample, fout);
    }
    //CalcWPM("WPM.csv");

    return 0;
}

