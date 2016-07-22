#include <cstdio>
#include <iostream>
#include "opencv2/opencv.hpp"
#define rep(i,n) for(int i=0; i<n; i++)

const int H = 1280;
const int W = 720;

using namespace cv;
using namespace std;

void drawLine(Mat& img, Point u, Point v, int thickness = 5)
{
    int lineType = 8;
    line(img, u, v, Scalar(0,255,255), thickness, lineType);
    circle(img, u, 10, Scalar(0, 0, 255));
}

void draw(string outputFileName, bool show = false)
{
    string s;
    double X, Y;
    vector<double> x, y;
    int cnt = 0;
    while (cin >> s)
    {
        x.clear();
        y.clear();
        while (true)
        {
            scanf("%lf%lf", &X, &Y);
            x.push_back(X);
            y.push_back(Y);
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
    freopen("data/yxc.txt", "r", stdin);
    draw("res/yxc", true);
    waitKey();
    return 0;
}
