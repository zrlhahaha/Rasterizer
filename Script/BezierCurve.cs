using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve : MonoBehaviour {

    public Transform[] points;
    public Vector3[] positions;
    public AnimationCurve c;

    public float delta = 0.01f;
    public bool check = false;

    //二次Bezier曲线
    public Vector3 QuadraticBezier(Vector3[] points,float t, int startIndex = 0)
    {
        if (points.Length <startIndex+ 3)
        {
            Debug.Log("QuadraticBezier Error: points count not enough,startIndex = "+ startIndex);
            return Vector3.zero;
        }

        t = Mathf.Clamp01(t);

        Vector3 a = LinerInterpolate(points[startIndex], points[startIndex + 1], t);
        Vector3 b = LinerInterpolate(points[startIndex + 1], points[startIndex + 2], t);

        return LinerInterpolate(a, b, t);
    }


    //三次Bezier曲线
    public Vector3 CubicBezier(Vector3[] points, float t,int startIndex = 0)
    {
        if (points.Length < startIndex+ 4)
        {
            
            Debug.Log("CubicBezier Error: points count not enough,startIndex = "+ startIndex);
            return Vector3.zero;
        }

        t = Mathf.Clamp01(t);

        Vector3 a = LinerInterpolate(points[startIndex], points[startIndex+1], t);
        Vector3 b = LinerInterpolate(points[startIndex+1], points[startIndex+2], t);
        Vector3 c = LinerInterpolate(points[startIndex+2], points[startIndex+3], t);

        Vector3 m = LinerInterpolate(a, b, t);
        Vector3 n = LinerInterpolate(b, c, t);
        return LinerInterpolate(m, n, t);
    }

    public Vector3 LinerInterpolate(Vector3 start,Vector3 end,float t)
    {
        t = Mathf.Clamp01(t);

        return (1 - t) * start + t * end;
    }

    public void CachePosition()
    {
        if (positions == null || positions.Length != points.Length)
        {
            positions = new Vector3[points.Length];
        }
        for (int i = 0; i < points.Length; i++)
        {
            positions[i] = points[i].position;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (points == null || points.Length < 3)
            return;

        CachePosition();

        DrawBezierCurve(0,points.Length-1);
    }
    
    //在范围内连续绘制三次Bezier曲线，多余的点会被忽略
    void DrawBezierCurve(int startIndex,int endIndex)
    {
        int curveCount = (points.Length - 1) / 3;

        for (int i = 0; i < curveCount; i++)
        {
            int start = i * 3;
            Vector3 pre = CubicBezier(positions, 0, start);
            for (float t = delta; t <= 1; t += delta)
            {
                Vector3 next = CubicBezier(positions, t, start);
                Gizmos.DrawLine(pre, next);
                pre = next;
            }
        }
    }

}
