#ifndef VECTOR2_H_
#define VECTOR2_H_

#include <cmath>

struct Vector2
{
    double x, y;
    Vector2(double x_ = 0, double y_ = 0)
    {
        x_ = x;
        y_ = y;
    }
};

double distance(const Vector2& p, const Vector2& q)
{
    return sqrt((p.x - q.x) * (p.x - q.x) + (p.y - q.y) * (p.y - q.y));
}


#endif // VECTOR2_H_
