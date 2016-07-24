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

vector<string> cmd;
vector<string> words;
vector<double> time, worldX, worldY, relX, relY;

string sentence[PHRASES], mode[PHRASES];
double height[PHRASES], width[PHRASES], heightRatio[PHRASES], widthRatio[PHRASES];

bool same(const std::string& input, const std::string& tar)
{
    if (input.length() > tar.length()+1) return false;
    std::string::const_iterator p = input.begin();
    std::string::const_iterator q = tar.begin();

    while (p != input.end() && q != tar.end() && toupper(*p) == toupper(*q))
        ++p, ++q;
    return (p == input.end()) || (q == tar.end());

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
            break;
        }
        fin >> x >> y >> rx >> ry;
        LinePushBack(s, t, x, y, rx, ry);
        if (same(s, "Began"))
        {
            if (startTime == -1)
                startTime = t;
        }
    }
    getline(fin, sentence[id]);
    getline(fin, sentence[id]);
    fin >> mode[id];
    fin >> widthRatio[id] >> heightRatio[id];
    fin >> height[id] >> width[id];

    cmd.clear(); time.clear();
    worldX.clear(); worldY.clear();
    relX.clear(); relY.clear();


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
            break;
        }
        fin >> x >> y >> rx >> ry;
        LinePushBack(s, t, x, y, rx, ry);
        if (same(s, "Began"))
        {
            if (startTime == -1)
                startTime = t;
        }
    }
    stringstream ss;
    ss << id;
    fstream fout;
    string output = "refine/yzc_" + ss.str() + ".txt";
    fout.open(output.c_str(), fstream::out);
    fout << sentence[id] << endl;
    fout << mode[id] << endl;
    fout << widthRatio[id] << " " << heightRatio[id] << endl;
    fout << height[id] << " " << width[id] << endl;
    rep(i, cmd.size())
    {
        fout << cmd[i] << " " << time[i];
        if (same(cmd[i], "PhraseEnd") || same(cmd[i], "Backspace"))
            fout << endl;
        else
            fout << " " << worldX[i] << " " << worldY[i] << " "
                 << " " << relX[i] <<  " " << relY[i] << endl;
    }
    fin.close();
    fout.close();
}

int main()
{

    string user = "yzc";

    FOR(i, 32, 59)
    {
        stringstream ss;
        ss << i;
        ReadData(i, user + "_" + ss.str() + ".txt");



    }
}
