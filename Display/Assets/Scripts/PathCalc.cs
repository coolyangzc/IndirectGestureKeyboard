using UnityEngine;

public class PathCalc : MonoBehaviour {

    private const int SampleSize = Parameter.SampleSize;
    private const float inf = Parameter.inf;
    
    private const float DTWWindowConst = 0.1f;
    private static int[] DTWL = new int[SampleSize + 1], DTWR = new int[SampleSize + 1];
    private static float[][] dtw = new float[SampleSize + 1][];

    // Use this for initialization
    void Start ()
    {
        InitDTW();
    }

    void InitDTW()
    {
        int w = (int)(SampleSize * DTWWindowConst);
        for (int i = 0; i <= SampleSize; ++i)
        {
            dtw[i] = new float[SampleSize + 1];
            DTWL[i] = Mathf.Max(i - w, 0);
            DTWR[i] = Mathf.Min(i + w, SampleSize);
            for (int j = 0; j <= SampleSize; ++j)
                dtw[i][j] = float.MaxValue;
        }
        dtw[0][0] = 0;
    }

    public static Vector2[] TemporalSampling(Vector2[] stroke)
    {
        float length = 0;
        int count = stroke.Length;
        Vector2[] vector = new Vector2[SampleSize];
        if (count == 1)
        {
            for (int i = 0; i < SampleSize; ++i)
                vector[i] = stroke[0];
            return vector;
        }

        for (int i = 0; i < count - 1; ++i)
            length += Vector2.Distance(stroke[i], stroke[i + 1]);
        float increment = length / (SampleSize - 1);

        Vector2 last = stroke[0];
        float distSoFar = 0;
        int id = 1, vecID = 1;
        vector[0] = stroke[0];
        while (id < count)
        {
            float dist = Vector2.Distance(last, stroke[id]);
            if (distSoFar + dist >= increment)
            {
                float ratio = (increment - distSoFar) / dist;
                last = last + ratio * (stroke[id] - last);
                vector[vecID++] = last;
                distSoFar = 0;
            }
            else
            {
                distSoFar += dist;
                last = stroke[id++];
            }
        }
        for (int i = vecID; i < SampleSize; ++i)
            vector[i] = stroke[count - 1];
        return vector;
    }

    public static Vector2[] Normalize(Vector2[] pts)
    {
        if (pts == null)
            return null;
        float minX = 1f, minY = 1f;
        float maxX = -1f, maxY = -1f;

        Vector2 center = new Vector2(0, 0);
        int size = pts.Length;
        for (int i = 0; i < size; ++i)
        {
            center += pts[i];
            minX = Mathf.Min(minX, pts[i].x);
            maxX = Mathf.Max(maxX, pts[i].x);
            minY = Mathf.Min(minY, pts[i].y);
            maxY = Mathf.Max(maxY, pts[i].y);
        }
        center = center / size;
        float ratio = 1.0f / Mathf.Max(maxX - minX, maxY - minY);
        Vector2[] nPts = new Vector2[size];
        for (int i = 0; i < size; ++i)
            nPts[i] = (pts[i] - center) * ratio;
        return nPts;
    }

    public static float Match(Vector2[] A, Vector2[] B, Parameter.Formula formula, 
                              float threshold = Parameter.inf, bool isShape = false)
    {
        if (A.Length != B.Length || formula == Parameter.Formula.Null)
            return inf;
        /*if (Vector2.Distance(A[0], B[0]) > KeyWidth)
			return 0;*/
        float dis = 0;
        
        switch (formula)
        {
            case (Parameter.Formula.Basic):
                for (int i = 0; i < SampleSize; ++i)
                {
                    dis += Vector2.Distance(A[i], B[i]);
                }
                break;
            case (Parameter.Formula.MinusR):
                for (int i = 0; i < SampleSize; ++i)
                {
                    dis += Mathf.Max(0, Vector2.Distance(A[i], B[i]) - Parameter.radius);
                }
                break;
            case (Parameter.Formula.DTW):
                for (int i = 0; i < SampleSize; ++i)
                {
                    float gap = float.MaxValue;
                    for (int j = DTWL[i]; j < DTWR[i]; ++j)
                    {
                        dtw[i + 1][j + 1] = Vector2.Distance(A[i], B[j]) + Mathf.Min(dtw[i][j], Mathf.Min(dtw[i][j + 1], dtw[i + 1][j]));
                        gap = Mathf.Min(gap, dtw[i+1][j+1]);
                    }
                    if (gap > threshold)
                        return Parameter.inf;
                }
                dis = dtw[SampleSize][SampleSize];
                break;
        }
        return dis / SampleSize / Parameter.keyboardWidth;
        /*if (!isShape)
            return Mathf.Exp(-0.5f * dis * dis / Parameter.radius / Parameter.radius);
        else
            return Mathf.Exp(-0.5f * dis * dis / (Parameter.radiusMul * 0.1f) / (Parameter.radiusMul * 0.1f));*/
    }
}
