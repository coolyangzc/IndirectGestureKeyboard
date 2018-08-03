using UnityEngine;
using UnityEngine.UI;

public class Parameter : MonoBehaviour
{
    public const int SampleSize = 32;
    public const float eps = 1e-6f;
    public const float inf = 1e10f;

    public static bool debugOn = false;
    public static Mode mode = Mode.FixStart;
    public static UserStudy userStudy = UserStudy.Basic;
    public static Formula locationFormula = Formula.DTW, shapeFormula = Formula.Null;
    public static float endOffset = 3.0f, KeyWidth = 0f, radius = 0, radiusMul = 0.20f;

    public Image keyboard;
    public Info info;
    
    //Definitions
    public enum Mode
    {
        Basic = 0,
        FixStart = 1,
        AnyStart = 2,
        End = 3,
    };

    public enum Formula
    {
        Basic = 0,
        MinusR = 1,
        DTW = 2,
        Null = 3,
        End = 4,
    }

    public enum UserStudy
    {
        Basic = 0,
        Train = 1,
        Study1 = 2,
        Study2 = 3,
        End = 4,
    }

    // Use this for initialization
    void Start()
    {
        KeyWidth = keyboard.rectTransform.rect.width * 0.1f;
        ChangeRadius(0);
        ChangeEndOffset(0);
    }

    public void ChangeMode()
    {
        mode = mode + 1;
        if (mode >= Parameter.Mode.End)
            mode = 0;
        info.Log("Mode", mode.ToString());
    }

    public void ChangeLocationFormula()
    {
        locationFormula = locationFormula + 1;
        if (locationFormula >= Parameter.Formula.End)
            locationFormula = 0;
        if (debugOn)
            info.Log("[L]ocation", locationFormula.ToString());
    }

    public void ChangeShapeFormula()
    {
        shapeFormula = shapeFormula + 1;
        if (shapeFormula >= Parameter.Formula.End)
            shapeFormula = 0;
        if (debugOn)
            info.Log("[S]hape", shapeFormula.ToString());
    }

    public void ChangeRadius(float delta)
    {
        if (radiusMul + delta <= eps)
            return;
        radiusMul += delta;
        radius = KeyWidth * radiusMul;
        if (debugOn)
            info.Log("[R]adius", radiusMul.ToString("0.00") + "key");
    }

    public void ChangeEndOffset(float delta)
    {
        if (endOffset + delta <= 0)
            return;
        endOffset += delta;
        if (debugOn)
            info.Log("[E]ndOffset", endOffset.ToString("0.0"));
    }

}
