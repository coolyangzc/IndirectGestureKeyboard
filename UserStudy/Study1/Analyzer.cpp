#include <cstdio>
#include <vector>
#include <cstdlib>
#include <sstream>
#include <fstream>
#include <iostream>

#define rep(i,n) for(int i=0; i<n; ++i)
#define FOR(i,a,b) for(int i=a; i<=b; ++i)


const int PHRASES = 30;
using namespace std;
string sentence[PHRASES], mode[PHRASES];
double height[PHRASES], width[PHRASES], heightRatio[PHRASES], widthRatio[PHRASES];
double WPM[PHRASES];
double keyX[128], keyY[128];

vector<string> cmd;
vector<double> time, worldX, worldY, relX, relY;

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

void CalcKeyLayout()
{
    string line1 = "qwertyuiop";
    string line2 = "asdfghjkl";
    string line3 = "zxcvbnm";
    rep(i, line1.length())
    {
        keyX[line1[i]] = -0.45 + i * 0.1;
        keyY[line1[i]] = 0.375;
    }
    rep(i, line2.length())
    {
        keyX[line2[i]] = -0.4 + i * 0.1;
        keyY[line2[i]] = 0.125;
    }
    rep(i, line3.length())
    {
        keyX[line3[i]] = -0.35 + i * 0.1;
        keyY[line3[i]] = -0.125;
    }

}

void LinePushBack(string s, double t, double x = 0, double y = 0, double rx = 0, double ry = 0)
{
    cmd.push_back(s);
    time.push_back(t);
    worldX.push_back(x);
    worldY.push_back(y);
    relX.push_back(rx);
    relY.push_back(ry);
}

void ReadData(int id, string fileName)
{
    fstream fin;
    fin.open(fileName.c_str(), fstream::in);

    getline(fin, sentence[id]);
    fin >> mode[id];
    fin >> widthRatio[id] >> heightRatio[id];
    fin >> height[id] >> width[id];

    vector<string> words;
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
    worldX.clear(); worldY.clear();
    relX.clear(); relY.clear();
    int wordID = 0;
    string s;
    double t, x, y, rx, ry;
    while (fin >> s)
    {
        fin >> t;
        if (same(s, "Backspace"))
        {
            startTime = -1;
            LinePushBack(s, t);
            continue;
        }
        if (same(s, "PhraseEnd"))
        {
            LinePushBack(s, t);
            continue;
        }
        fin >> x >> y >> rx >> ry;
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
    rep(i, 9)
        fout << WPM[i] << endl;
    fout.close();
}

int main()
{
    CalcKeyLayout();
    //fout.open("firstKey0.txt", fstream::out);
    //fout << "sentence,size,WPM" << endl;
    //fout << "size,key,keyX,keyY,inputX,inputY,offsetX,offsetY" << endl;
    string user = "yzc";
    FOR(i, 0, 9)
    {
        stringstream ss;
        ss << i;
        ReadData(i, "data/" + user + "_" + ss.str() + ".txt");

    }
    CalcWPM("WPM.txt");

    return 0;
}
