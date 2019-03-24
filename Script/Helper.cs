using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Log
{
    public static void LogVector(Vector3 vec,string name = "")
    {
        Debug.Log(string.Format("LOG name:x{0},y{1},z{2}", vec.x, vec.y, vec.z));
    }

    public static void LogVector(Vector4 vec, string name = "")
    {
        Debug.Log(string.Format("LOG name:x{0},y{1},z{2},w{3}", vec.x, vec.y, vec.z,vec.w));
    }

}

public static class Helper
{

    public static bool display = true;

    public static Vector3 SnapToZero( Vector3 vec,float epslion = 1E-6f)
    {
        if (Mathf.Abs(vec.x) < epslion)
            vec.x = 0;

        if (Mathf.Abs(vec.y) < epslion)
            vec.y = 0;

        if (Mathf.Abs(vec.z) < epslion)
            vec.z = 0;

        return vec;
    }

    public static float SnapToZero(float f, float epslion = 1E-6f)
    {
        if (Mathf.Abs(f) < epslion)
            f = 0;

        return f;
    }

    public static Color RandomColor()
    {
        return new Color(Random.value, Random.value, Random.value);
    }

    public static void DisplayTriangleList(List<Triangle3D> ts,string prefix = "TrianglePainters")
    {
        if (!display)
            return;

        GameObject parent = new GameObject();
        parent.name = prefix;

        for (int i = 0; i < ts.Count; i++)
        {
            TrianglePainter tp = TrianglePainter.CreateTrianglePainter(ts[i], prefix + " " + i.ToString());
            tp.transform.SetParent(parent.transform);
        }
    }

    
}

