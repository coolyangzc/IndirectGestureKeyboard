#ifndef VECTOR2_H_
#define VECTOR2_H_

#include <cmath>
#include <vector>
#include <iostream>
#include <algorithm>

#define rep(i,n) for(int i=0; i<n; ++i)
#define FOR(i,a,b) for(int i=a; i<=b; ++i)

using namespace std;

const int MAXSAMPLE = 200 + 1;

struct Vector2
{
    double x, y;
    Vector2(double x_ = 0, double y_ = 0)
    {
        x = x_;
        y = y_;
    }
    Vector2 operator +(const Vector2 &b)
    {
        return Vector2(x + b.x, y + b.y);
    }
    Vector2 operator -(const Vector2 &b)
    {
        return Vector2(x - b.x, y - b.y);
    }
    Vector2 operator *(const double &r)
    {
        return Vector2(x*r, y*r);
    }
    Vector2 operator /(const double &r)
    {
        return Vector2(x/r, y/r);
    }
};

double dist(const Vector2& p, const Vector2& q)
{
    return sqrt((p.x - q.x) * (p.x - q.x) + (p.y - q.y) * (p.y - q.y));
}

vector<Vector2> temporalSampling(vector<Vector2> stroke, int num)
{
    int cnt = stroke.size();
    if (cnt == 1)
        return stroke;
    vector<Vector2> vec(num);
    double length = 0;
    rep(i, cnt-1)
        length += dist(stroke[i], stroke[i+1]);
    double increment = length / (num - 1);
    Vector2 last = stroke[0];
    double distSoFar = 0;
    int id = 1, vecID = 1;
    vec[0] = stroke[0];

    while (id < cnt)
    {
        double d = dist(last, stroke[id]);
        if (distSoFar + d >= increment)
        {
            double Ratio = (increment - distSoFar) / d;
            last = last + (stroke[id] - last) * Ratio;
            vec[vecID++] = last;
            distSoFar = 0;
        }
        else
        {
            distSoFar += d;
            last = stroke[id++];
        }
    }
    for (int i = vecID; i < num; ++i)
        vec[i] = stroke[cnt - 1];
    return vec;
}

enum Formula
{
    Standard = 0,
    DTW = 1,
};

double match(const vector<Vector2>& A, vector<Vector2>& B, double dtw[MAXSAMPLE][MAXSAMPLE], Formula formula)
{
    if (A.size() != B.size())
        return -1;
    double dis = 0;
    int num = A.size();
    switch(formula)
    {
    case (Standard):
        rep(i, num)
            dis += dist(A[i], B[i]);
        break;

    case (DTW):
        int w = max(num / 0.1, 2.0);
        rep(i, num)
            FOR(j, max(0, i - w), min(i + w, num - 1))
            {
                dtw[i+1][j+1] = dist(A[i], B[j]) + min(dtw[i][j], min(dtw[i][j+1], dtw[i+1][j]));
            }
        dis = dtw[num][num];
        break;
    }
    return dis;
}


#endif // VECTOR2_H_
