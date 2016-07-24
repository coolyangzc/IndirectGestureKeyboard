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

string sentence[PHRASES], mode[PHRASES], scale[PHRASES];
double height[PHRASES], width[PHRASES], heightRatio[PHRASES], widthRatio[PHRASES], keyboardSize[PHRASES];
double WPM[PHRASES];
double keyX[128], keyY[128];

vector<string> cmd;
vector<string> words;
vector<double> time;
vector<Vector2> world, relative;

fstream fout;

int Random(int mo)
{
    return rand() % mo;
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
        keyX[line1[i]] = -0.45 + i * 0.1;
        keyY[line1[i]] = 0.333;
    }
    rep(i, line2.length())
    {
        keyX[line2[i]] = -0.4 + i * 0.1;
        keyY[line2[i]] = 0;
    }
    rep(i, line3.length())
    {
        keyX[line3[i]] = -0.35 + i * 0.1;
        keyY[line3[i]] = -0.333;
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

void CalcDistance(int id, fstream& fout)
{

}

int main()
{
    CalcKeyLayout();

    string user = "yzc";
    fstream fout;
    fout.open("distance.csv", fstream::out);
    fout << "user,scale,size,word,algorithm,sampleNum,distance" << endl;

    rep(i, 60)
    {
        ReadData(i, user);
        CalcDistance(i, fout);
    }
    CalcWPM("WPM.csv");

    return 0;
}
