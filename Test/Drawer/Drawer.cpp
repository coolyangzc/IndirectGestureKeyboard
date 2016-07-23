#include <cstdio>
#include <fstream>
#include <iostream>
#include "opencv2/opencv.hpp"
#define rep(i,n) for(int i=0; i<n; i++)

const int H = 1280;
const int W = 720;

using namespace cv;
using namespace std;

int Random(int mo)
{
    return rand() % mo;
}

void drawLine(Mat& img, Point u, Point v, int thickness = 5, Scalar* color = NULL)
{
    int lineType = 8;
    if (!color)
        color = new Scalar(0, 255, 255);
    line(img, u, v, *color, thickness, lineType);
    //circle(img, u, 10, Scalar(0, 0, 255));
}

void draw(string outputFileName, bool show = false)
{
    string s, buffer;
    double X, Y;
    vector<double> x, y;
    int cnt = 0;
    while (cin >> s)
    {
        x.clear();
        y.clear();
        buffer = "";
        while (true)
        {
            scanf("%lf%lf", &X, &Y);
            x.push_back(X);
            y.push_back(Y);
            stringstream sX, sY;
            sX << X; sY << Y;
            buffer += s + " " + sX.str() + " " + sY.str() + "\n";
            if (s[0] == 'E')
                break;
            cin >> s;
        }
        Mat img = Mat::zeros(H, W, CV_8UC3);
        rep(i, x.size() - 1)
        {
            drawLine(img, Point(x[i], H - y[i]), Point(x[i+1], H - y[i+1]));
            if (show)
                imshow("Drawer", img);
        }
        stringstream ss;
        ss << cnt++;
        imwrite(outputFileName + ss.str() + ".jpg", img);
        string fileName = outputFileName + ss.str() + ".txt";
        freopen(fileName.c_str(), "w", stdout);
        cout << buffer << endl;
        fclose(stdout);
    }
}

void Merge(Mat& img, fstream& fin, Scalar color)
{
    string s;
    double X, Y;
    vector<double> x, y;
    int cnt = 0;
    while (fin >> s)
    {
        x.clear();
        y.clear();
        while (true)
        {
            fin >> X >> Y;
            x.push_back(X);
            y.push_back(Y);
            if (s[0] == 'E')
                break;
            fin >> s;
        }
        rep(i, x.size() - 1)
        {
            drawLine(img, Point(x[i], H - y[i]), Point(x[i+1], H - y[i+1]), 4, &color);
        }
    }
}

/*void drawFile(string fileName)
{
    freopen(fileName.c_str(), "r", stdin);
    string str, outFileName;
    int cnt = 0;
    while (cin >> str)
    {
        rep(i, 4)
            cin >> str;
        stringstream ss;
        ss << cnt;
        outFileName = ss.str() + str + ".jpg";
        if (cnt < 10)
            outFileName = "0" + outFileName;
        cnt++;
        cout << outFileName << endl;
        rep(i, 6)
            cin >> str;
        draw(outFileName);
    }
}*/

int main()
{
    freopen("data/yuntao.txt", "r", stdin);
    draw("res/yuntao", true);
    string name[] = {"maye", "yixin", "yzc", "yxc", "mzy", "xwj", "yuntao"};
    Mat img = Mat::zeros(H, W, CV_8UC3);
    rep(i, 7)
    {
        Scalar color(Random(255), Random(255), Random(255));
        rep(id, 5)
        {
            stringstream ss;
            ss << id;
            fstream fin;
            string file = "data/merge/" + name[i] + ss.str() + ".txt";

            fin.open(file.c_str(), fstream::in );

            cout << file << endl;
            Merge(img, fin, color);
            fin.close();
        }
    }
    imwrite("Merge.jpg", img);

    waitKey();
    return 0;
}
