#define SaftyCheck 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PainterAlgorithm;


/// <summary>
/// 观察坐标系使用右手坐标系，从左手坐标系变化过来的坐标记得取反，所有在观察坐标系中的操作记得在右手坐标系下进行
/// </summary>
[System.Serializable]
public struct Plane
{
    public float d;
    public Vector3 normal;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="d">原点到平面的距离,原点在平面正方向为正数，在平面负方向为负数</param>
    /// <param name="normal">法向量</param>
    public Plane(float d,Vector3 normal)
    {
        this.d = d;
        this.normal = normal;
    }

    /// <param name="a">平面上的点</param>
    /// <param name="normal">法向量</param>
    public Plane(Vector3 a, Vector3 normal)
    {
        d = -Vector3.Dot(normal, a);
        this.normal = normal;
    }

    /// <param name="a">平面上的点</param>
    /// <param name="b">平面上的点</param>
    /// <param name="c">平面上的点</param>
    public Plane(Vector3 a,Vector3 b,Vector3 c)
    {
        normal = Vector3.Cross(b - a, b - c);
        d = -Vector3.Dot(normal, a);
    }

    public Plane(Triangle3D triangle)
    {
        normal = Vector3.Cross(triangle.b - triangle.a, triangle.b - triangle.c);
        d = -Vector3.Dot(normal, triangle.a);
    }

    public float Point(Vector3 p)
    {
        return Vector3.Dot(normal, p) + d;
    }

}

[System.Serializable]
public struct Triangle3D
{
    public Vector3 a, b, c;
    public Vector3 normal;

    public Triangle3D(Vector3 a, Vector3 b, Vector3 c)
    {
        this.a = a;
        this.b = b;
        this.c = c;

        normal = Vector3.Cross((b - a), (b - c)).normalized;
    }

    public Triangle3D(Vector3 a, Vector3 b, Vector3 c, Vector3 normal)
    {
        this.a = a;
        this.b = b;
        this.c = c;

        this.normal = normal;
    }

    public void ComputeDefaultNormal()
    {
        normal = Vector3.Cross((b - a), (b - c)).normalized;
    }

    public void Negative()
    {
        a = -a;
        b = -b;
        c = -c;
    }

    public Triangle2D To2D()
    {
        return new Triangle2D(a, b, c);
    }
}

[System.Serializable]
public struct Triangle4D
{
    public Vector4 a, b, c;
    public Vector3 normal;

    public Triangle4D(Vector4 a, Vector4 b, Vector4 c)
    {
        this.a = a;
        this.b = b;
        this.c = c;

        normal = Vector3.Cross((b - a), (b - c)).normalized;
    }

    public Triangle4D(Vector4 a, Vector4 b, Vector4 c, Vector3 normal)
    {
        this.a = a;
        this.b = b;
        this.c = c;

        this.normal = normal;
    }

    public void ComputeDefaultNormal()
    {
        normal = Vector3.Cross((b - a), (b - c)).normalized;
    }

    public void Negative()
    {
        a = -a;
        b = -b;
        c = -c;
    }

    public Triangle2D To2D()
    {
        return new Triangle2D(a, b, c);
    }
}

public struct Triangle2D
{
    public Vector2 a,b,c;

    public Triangle2D(Vector2 a,Vector2 b,Vector2 c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
    }
}

public struct TriangleAttribute<T>
{
    public T a, b, c;

    public TriangleAttribute(T a, T b, T c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
    }
}

[System.Serializable]
public class Entity
{
    public MeshFilter meshFilter;
    public Texture2D mainTex;
    public Transform transform;
}

public class Rasterizer : MonoBehaviour
{
    public Transform rendererScreenPivot;
    public Renderer inGameScreen;
    public Renderer target;
    public Texture2D colorBuffer;
    public Entity[] entity;
    public Color color = Color.red;
    public Vector3 lightPos;
    public Color lightColor;
    float[] depthBuffer;

    public float aspect;
    public int pixelWidth;
    public int pixelHeight;


    public Transform v1;
    public Transform v2;
    public Transform v3;
    public Transform v4;
    public Camera cam;

    public TrianglePainter[] trianglePainter;
    public BSP bsp;

    public bool update;
    public bool updateOneFrame = false;
    public bool DrawGizmos = false;
    public bool chekerBoard = false;

    Vector3[] vertexBuffer = new Vector3[4]
    {
                new Vector3(0,0,0),
                new Vector3(2,0,0),
                new Vector3(0,2,0),
                new Vector3(0,0,2),
    };

    int[][] indexBuffer = new int[4][]
    {
                new int[3] {0,1,2 },
                new int[3] {0,1,3 },
                new int[3] {0,2,3 },
                new int[3] {3,1,2 },
    };



    // Use this for initialization
    void Start()
    {

        rendererScreenPivot.localScale = new Vector3(pixelWidth * 0.1f, pixelHeight * 0.1f, 1);

        aspect = (float)pixelWidth / (float)pixelHeight;

        target.material.mainTexture = colorBuffer;
        colorBuffer = new Texture2D(pixelWidth, pixelHeight);
        colorBuffer.filterMode = FilterMode.Point;
        depthBuffer = new float[pixelWidth*pixelHeight];

        inGameScreen.material.mainTexture = colorBuffer;
        

    }
    // Update is called once per frame
    void Update()
    {

        if (update || updateOneFrame || Input.GetKeyDown(KeyCode.E))
        {
            updateOneFrame = false;
            ClearColorBuffer(Color.black);
            ClearDepthBuffer();
            for (int i = 0; i < entity.Length; i++)
            {
                DrawEntity(entity[i]);
            }

            colorBuffer.Apply();
            target.material.mainTexture = colorBuffer;

            return;
        }
    }

    void TestRenderer()
    {
        Helper.display = false;

        List<Triangle3D> ts = bsp.GetSquence(cam.transform.position);
        Helper.DisplayTriangleList(ts,"bsp");

        for (int i = 0; i < ts.Count; i++)
        {
            if (!FaceCulling(ts[i].normal, cam.transform.forward))
            {
                ts.RemoveAt(i);
                i--;
            }
        }

        Helper.DisplayTriangleList(ts, "face culling");

        //观察坐标系，左手坐标系
        m_Transform cam_t = new m_Transform();
        cam_t.CopyFromTransform(cam.transform);
        for (int i = 0; i < ts.Count; i++)
        {
            Triangle3D t = ts[i];
            t.a = cam_t.InverseTransformPoint(t.a);
            t.b = cam_t.InverseTransformPoint(t.b);
            t.c = cam_t.InverseTransformPoint(t.c);
            t.normal = cam_t.InverseTransformDirection(t.normal);
            ts[i] = t;
        }

        int count = ts.Count;
        for (int i = 0; i < count; i++)
        {
            ts.AddRange(ViewSpaceFrustrumClipping(ts[0], cam));
            ts.RemoveAt(0);
        }

        Helper.DisplayTriangleList(ts, "Frustrum Clipping");

        List<Triangle3D> ts_ScreenSpace = new List<Triangle3D>(ts.Count);
        Matrix4x4 p = GetProjectionMatrix(cam);
        //转换到右手坐标
        Matrix4x4 negetive = Matrix4x4.identity;
        negetive.m22 = -1;
        Matrix4x4 total = p * negetive;

        for (int i = 0; i < ts.Count; i++)
        {
            Triangle3D t = ts[i];
            t.a = ClipSpaceToScreenSpace(total * V4Point(t.a), pixelWidth, pixelHeight);
            t.b = ClipSpaceToScreenSpace(total * V4Point(t.b), pixelWidth, pixelHeight);
            t.c = ClipSpaceToScreenSpace(total * V4Point(t.c), pixelWidth, pixelHeight);
            ts_ScreenSpace.Add(t);
        }

        Helper.DisplayTriangleList(ts_ScreenSpace,"Sceen Space");

        for (int i = 0; i < ts_ScreenSpace.Count; i++)
        {
            Triangle3D t = ts_ScreenSpace[i];
            Vector2[] t2 = new Vector2[3];
            t2[0] = t.a;
            t2[1] = t.b;
            t2[2] = t.c;
            DrawTriangleScanLine(colorBuffer, t2);
        }

    }

    void DrawEntity(Entity entity)
    {
        Mesh mesh = entity.meshFilter.mesh;
        Transform objectTransform = entity.transform;
        Texture2D mainTex = entity.mainTex;


        for (int i = 0; i < mesh.triangles.Length; i+=3)
        {
            int a_index = mesh.triangles[i];
            int b_index = mesh.triangles[i+1];
            int c_index = mesh.triangles[i+2];

            TriangleAttribute<Color> colors = new TriangleAttribute<Color>(Color.red,Color.green,Color.blue);
            TriangleAttribute<Vector2> uvs = new TriangleAttribute<Vector2>(mesh.uv[a_index],mesh.uv[b_index],mesh.uv[c_index]);

            Vector4 a = V4Point(mesh.vertices[a_index]);
            Vector4 b = V4Point(mesh.vertices[b_index]);
            Vector4 c = V4Point(mesh.vertices[c_index]);

            m_Transform m_objectTransform = new m_Transform();
            m_objectTransform.CopyFromTransform(objectTransform);

            a = m_objectTransform.TransformPoint(a);
            b = m_objectTransform.TransformPoint(b);
            c = m_objectTransform.TransformPoint(c);

            TriangleAttribute<Vector3> pos_worlds = new TriangleAttribute<Vector3>(a, b, c);
            TriangleAttribute<Vector3> normals = 
                new TriangleAttribute<Vector3>(
                      m_objectTransform.TransformDirection(mesh.normals[a_index]),
                      m_objectTransform.TransformDirection(mesh.normals[b_index]),
                      m_objectTransform.TransformDirection(mesh.normals[c_index]));

            m_Transform cam_t = new m_Transform();
            cam_t.CopyFromTransform(cam.transform);

            a = cam_t.InverseTransformPoint(a);
            b = cam_t.InverseTransformPoint(b);
            c = cam_t.InverseTransformPoint(c);

            Matrix4x4 p = GetProjectionMatrix(cam);
            Matrix4x4 negetive = Matrix4x4.identity;
            negetive.m22 = -1;
            Matrix4x4 total = p * negetive;

            a = ClipSpaceToScreenSpace(total * V4Point(a), pixelWidth, pixelHeight);
            b = ClipSpaceToScreenSpace(total * V4Point(b), pixelWidth, pixelHeight);
            c = ClipSpaceToScreenSpace(total * V4Point(c), pixelWidth, pixelHeight);

            a.z = a.w;
            b.z = b.w;
            c.z = c.w;
            Triangle3D triangle = new Triangle3D(a, b, c);      //z值为摄像机空间z坐标
            DrawTriangle(colorBuffer,triangle, colors, uvs,pos_worlds,normals,mainTex);

        }

    }



    public void ClearColorBuffer(Color color)
    {
        for (int i = 0; i < colorBuffer.width; i++)
        {
            for (int j = 0; j < colorBuffer.height; j++)
            {
                colorBuffer.SetPixel(i, j, color);
            }
        }
        colorBuffer.Apply();
    }

    public void ClearDepthBuffer()
    {
        for (int i = 0; i < depthBuffer.Length; i++)
            depthBuffer[i] = float.MaxValue;
    }

    public void DrawCircle_Bres(Texture2D texture, Vector2 center, float radius)
    {
        Vector2Int _center = Vector2Int.FloorToInt(center);

        int d = (int)(3 - 2 * radius);
        int x = 0;
        int y = (int)(radius);
        while (x <= y)
        {
            texture.SetPixel(_center.x + x, _center.y + y, color);
            texture.SetPixel(_center.x + y, _center.y + x, color);
            texture.SetPixel(_center.x - x, _center.y + y, color);
            texture.SetPixel(_center.x - y, _center.y + x, color);
            texture.SetPixel(_center.x + x, _center.y - y, color);
            texture.SetPixel(_center.x + y, _center.y - x, color);
            texture.SetPixel(_center.x - x, _center.y - y, color);
            texture.SetPixel(_center.x - y, _center.y - x, color);


            if (d >= 0)
            {
                y--;
                d += 4 * (x - y) + 10;
            }
            else
                d += 4 * x + 6;

            x++;
        }

        texture.Apply();
    }

    public void DrawLine_Bres(Texture2D texture, Vector2 start, Vector2 end)
    {
        if (!LineClip_Cohen(ref start, ref end, 0, pixelWidth - 1, pixelHeight - 1, 0))
            return;

        Vector2Int _start = Vector2Int.FloorToInt(start);
        Vector2Int _end = Vector2Int.FloorToInt(end);

        int dy = Mathf.Abs(_end.y - _start.y);
        int dx = Mathf.Abs(_end.x - _start.x);

#if (SaftyCheck)
        if (dx > 1000000 || dy > 10000000)
        {
            Debug.LogWarning("DrawLine_Bres Error");
            return;
        }
#endif

        bool flag = dy > dx;
        if (flag)
        {
            int temp = dy;
            dy = dx;
            dx = temp;

            temp = _start.x;
            _start.x = _start.y;
            _start.y = temp;

            temp = _end.x;
            _end.x = _end.y;
            _end.y = temp;
        }

        int d_Positive = 2 * (dy - dx);
        int d_Negative = 2 * dy;
        int d = 2 * dy - dx;
        int x = _start.x;
        int y = _start.y;
        int step_x = (_end.x - _start.x) > 0 ? 1 : -1;
        int step_y = (_end.y - _start.y) > 0 ? 1 : -1;

        for (int i = 0; i <= dx; i++)
        {
            if (flag)
                texture.SetPixel(y, x, color);
            else
                texture.SetPixel(x, y, color);

            x += step_x;
            if (d >= 0)
            {
                d += d_Positive;
                y += step_y;
            }
            else
                d += d_Negative;
        }

        texture.Apply();
    }

    int EnCode(Vector2 point, float x_Left, float x_Right, float y_Top, float y_Buttom)
    {
        int x = 0;
        int y = 0;
        if (point.x > x_Right)
            x = 2;
        else if (point.x < x_Left)
            x = 1;

        if (point.y > y_Top)
            y = 8;
        else if (point.y < y_Buttom)
            y = 4;

        return x | y;
    }

    //上下左右前后，小端
    int EnCode_ClipSpace(Vector4 point, float epsilon = 1E-6f)
    {
        int res = 0;
        if (point.y - point.w > epsilon)
            res = res | 1;
        else if (point.y + point.w < -epsilon)
            res = res | 2;

        if (point.x - point.w > epsilon)
            res = res | 8;
        else if (point.x + point.w < -epsilon)
            res = res | 4;

        if (point.z - point.w > epsilon)
            res = res | 16;
        else if (point.z + point.w < -epsilon)
            res = res | 32;

        return res;
    }

    //false 完全在框外
    bool LineClip_Cohen(ref Vector2 p1, ref Vector2 p2, float x_Left, float x_Right, float y_Top, float y_Buttom)
    {
        int code1 = code1 = EnCode(p1, x_Left, x_Right, y_Top, y_Buttom);
        int code2 = code2 = EnCode(p2, x_Left, x_Right, y_Top, y_Buttom);



        while (!(code1 == 0 && code2 == 0))
        {


            if ((code1 & code2) != 0)
                return false;

            int code = code1;
            float x, y;
            if (code == 0)
                code = code2;

            if ((code & 1) != 0)
            {
                x = x_Left;
                y = p1.y + (x - p1.x) * (p2.y - p1.y) / (p2.x - p1.x);
            }
            else if ((code & 2) != 0)
            {
                x = x_Right;
                y = p1.y + (x - p1.x) * (p2.y - p1.y) / (p2.x - p1.x);
            }
            else if ((code & 4) != 0)
            {
                y = y_Buttom;
                x = p1.x + (y - p1.y) * (p2.x - p1.x) / (p2.y - p1.y);
            }
            else
            {
                y = y_Top;
                x = p1.x + (y - p1.y) * (p2.x - p1.x) / (p2.y - p1.y);
            }

            if (code == code1)
            {
                p1.x = x;
                p1.y = y;
            }
            else
            {
                p2.x = x;
                p2.y = y;
            }

            code1 = EnCode(p1, x_Left, x_Right, y_Top, y_Buttom);
            code2 = EnCode(p2, x_Left, x_Right, y_Top, y_Buttom);
        }

        return true;
    }

    //clip：裁剪空间  view:观察坐标系左手坐标系
    //在裁剪空间通过w进行EnCode判定是否超出视锥，在观察坐标系进行裁剪
    bool LineClip_3D(ref Vector4 clipSpace_v1, ref Vector4 clipSpace_v2, Vector4 cameraSpace_v1, Vector4 cameraSpace_v2, Matrix4x4 projectionMatrix, Camera camera)
    {
        float theta_Y = camera.fieldOfView * 0.5f;
        float theta_X = Mathf.Atan(Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * camera.aspect) * Mathf.Rad2Deg;

        Plane left = new Plane(0, new Vector3(Mathf.Cos((theta_X) * Mathf.Deg2Rad), 0, Mathf.Sin((theta_X) * Mathf.Deg2Rad)));
        Plane right = new Plane(0, new Vector3(Mathf.Cos((180 - theta_X) * Mathf.Deg2Rad), 0, Mathf.Sin((180 - theta_X) * Mathf.Deg2Rad)));
        Plane near = new Plane(camera.nearClipPlane, Vector3.back);
        Plane far = new Plane(camera.farClipPlane, Vector3.back);
        Plane buttom = new Plane(0, new Vector3(0, Mathf.Sin((-theta_Y + 90) * Mathf.Deg2Rad), Mathf.Cos((-theta_Y + 90) * Mathf.Deg2Rad)));
        Plane top = new Plane(0, new Vector3(0, Mathf.Sin((theta_Y - 90) * Mathf.Deg2Rad), Mathf.Cos((theta_Y - 90) * Mathf.Deg2Rad)));

        int assert = 0;

        while (true)
        {
#if (SaftyCheck)
            if (assert++ == 100)
            {
                Debug.LogError("LineClip_3D over loop");
                Log.LogVector(cameraSpace_v1, "view_1");
                Log.LogVector(cameraSpace_v2, "view_2");
                return false;
            }
#endif


            int code1 = EnCode_ClipSpace(clipSpace_v1);
            int code2 = EnCode_ClipSpace(clipSpace_v2);

            if (code1 == 0 && code2 == 0)
                return true;

            if ((code1 & code2) != 0)
                return false;

            int code = code1;

            if (code == 0)
                code = code2;

            Vector4 point = new Vector4();
            if ((code & 16) != 0)
            {
                point = Intersection(cameraSpace_v1, cameraSpace_v2, far);
            }
            else if ((code & 32) != 0)
            {
                point = Intersection(cameraSpace_v1, cameraSpace_v2, near);
            }
            else if ((code & 1) != 0)
            {
                point = Intersection(cameraSpace_v1, cameraSpace_v2, top);
            }
            else if ((code & 2) != 0)
            {
                point = Intersection(cameraSpace_v1, cameraSpace_v2, buttom);
            }
            else if ((code & 4) != 0)
            {
                point = Intersection(cameraSpace_v1, cameraSpace_v2, left);
            }
            else if ((code & 8) != 0)
            {
                point = Intersection(cameraSpace_v1, cameraSpace_v2, right);
            }
            point.w = 1;

            Vector4 clip = point;
            clip.z = -clip.z;
            clip = projectionMatrix * clip;

            if (code == code1)
            {
                cameraSpace_v1 = point;
                clipSpace_v1 = clip;
            }
            else
            {
                cameraSpace_v2 = point;
                clipSpace_v2 = clip;
            }

        }
    }

    /// <summary>
    /// 在观察坐标系，左手坐标系中进行视锥体裁剪
    /// </summary>
    /// <param name="traingle"></param>
    /// <param name="camera"></param>
    /// <returns></returns>
    public static List<Triangle3D> ViewSpaceFrustrumClipping(Triangle3D traingle,Camera camera)
    {
        float theta_Y = camera.fieldOfView * 0.5f;
        float theta_X = Mathf.Atan(Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * camera.aspect) * Mathf.Rad2Deg;

        Plane left = new Plane(0, new Vector3(Mathf.Cos((theta_X) * Mathf.Deg2Rad), 0, Mathf.Sin((theta_X) * Mathf.Deg2Rad)));
        Plane right = new Plane(0, new Vector3(Mathf.Cos((180 - theta_X) * Mathf.Deg2Rad), 0, Mathf.Sin((180 - theta_X) * Mathf.Deg2Rad)));
        Plane near = new Plane(-camera.nearClipPlane, Vector3.forward);
        Plane far = new Plane(camera.farClipPlane, Vector3.back);
        Plane buttom = new Plane(0, new Vector3(0, Mathf.Sin((-theta_Y + 90) * Mathf.Deg2Rad), Mathf.Cos((-theta_Y + 90) * Mathf.Deg2Rad)));
        Plane top = new Plane(0, new Vector3(0, Mathf.Sin((theta_Y - 90) * Mathf.Deg2Rad), Mathf.Cos((theta_Y - 90) * Mathf.Deg2Rad)));


        List<Triangle3D> ts = new List<Triangle3D>();
        ts.Add(traingle);

        int orignalLength;
        orignalLength = ts.Count;
        for (int i = 0; i < orignalLength; i++)
        {
            ts.AddRange( ClipTrinagle_Frustum(ts[0], near));
            ts.RemoveAt(0);
        }

        orignalLength = ts.Count;
        for (int i = 0; i < orignalLength; i++)
        {
            ts.AddRange(ClipTrinagle_Frustum(ts[0], far));
            ts.RemoveAt(0);
        }

        orignalLength = ts.Count;
        for (int i = 0; i < orignalLength; i++)
        {
            ts.AddRange(ClipTrinagle_Frustum(ts[0], left));
            ts.RemoveAt(0);
        }

        orignalLength = ts.Count;
        for (int i = 0; i < orignalLength; i++)
        {
            ts.AddRange(ClipTrinagle_Frustum(ts[0], right));
            ts.RemoveAt(0);
        }

        orignalLength = ts.Count;
        for (int i = 0; i < orignalLength; i++)
        {
            ts.AddRange(ClipTrinagle_Frustum(ts[0], top));
            ts.RemoveAt(0);
        }

        orignalLength = ts.Count;
        for (int i = 0; i < orignalLength; i++)
        {
            ts.AddRange(ClipTrinagle_Frustum(ts[0], buttom));
            ts.RemoveAt(0);
        }

        return ts;
    }

    public void FillBottomFlatTriangle(Texture2D texture, Vector2 top, Vector2 v1, Vector2 v2)
    {
        float invslope1 = (top.x - v1.x) / (top.y - v1.y);
        float invslope2 = (top.x - v2.x) / (top.y - v2.y);

        for (int i = (int)v1.y; i < top.y - 1; i++)
        {
            DrawLine_Bres(texture, v1, v2);
            v1.x += invslope1;
            v1.y++;
            v2.x += invslope2;
            v2.y++;
        }
    }

    public void FillTopFlatTriangle(Texture2D texture, Vector2 buttom, Vector2 v1, Vector2 v2)
    {
        float invslope1 = (buttom.x - v1.x) / (buttom.y - v1.y);
        float invslope2 = (buttom.x - v2.x) / (buttom.y - v2.y);

        for (int i = (int)v1.y; i > buttom.y - 1; i--)
        {
            DrawLine_Bres(texture, v1, v2);
            v1.x -= invslope1;
            v1.y--;
            v2.x -= invslope2;
            v2.y--;
        }
    }

    public void DrawTriangleScanLine(Texture2D texture, Vector2[] v)
    {
#if (SaftyCheck)

        for (int i = 0; i < 3; i++)
        {
            if (v[i].x == float.NaN || v[i].y == float.NaN)
            {
                print("Given point is invalid");
                return;
            }
        }

#endif


        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2 - i; j++)
            {
                if (v[j].y < v[j + 1].y)
                {
                    Vector2 temp = v[j];
                    v[j] = v[j + 1];
                    v[j + 1] = temp;
                }
            }
        }

        for (int i = 0; i < 3; i++)
        {
            DrawLine_Bres(texture, v[i], v[(i + 1) % 2]);
        }

        Vector2 v_3;
        v_3.y = v[1].y;
        v_3.x = (v_3.y - v[2].y) * (v[0].x - v[2].x) / (v[0].y - v[2].y) + v[2].x;

        FillBottomFlatTriangle(texture, v[0], v[1], v_3);
        FillTopFlatTriangle(texture, v[2], v[1], v_3);
    }

    public void DrawTriangle(Texture2D colorBuffer,Triangle3D t,TriangleAttribute<Color> colors,TriangleAttribute<Vector2> uvs,TriangleAttribute<Vector3> pos_worlds,TriangleAttribute<Vector3> normals,Texture2D mainTex)
    {
        Vector2 min = Vector2.Min(Vector2.Min(t.a, t.b), t.c);
        Vector2 max = Vector2.Max(Vector2.Max(t.a, t.b), t.c);

        Vector2Int min_int = ClampInScreenSpace(pixelWidth, pixelHeight, Vector2Int.FloorToInt(min));
        Vector2Int max_int = ClampInScreenSpace(pixelWidth, pixelHeight, Vector2Int.FloorToInt(max));

        t.a.z = 1 / t.a.z;
        t.b.z = 1 / t.b.z;
        t.c.z = 1 / t.c.z;

        colors.a *= t.a.z;
        colors.b *= t.b.z;
        colors.c *= t.c.z;

        uvs.a *= t.a.z;
        uvs.b *= t.b.z;
        uvs.c *= t.c.z;

        pos_worlds.a *= t.a.z;
        pos_worlds.b *= t.b.z;
        pos_worlds.c *= t.c.z;

        normals.a *= t.a.z;
        normals.b *= t.b.z;
        normals.c *= t.c.z;
        for (int y = min_int.y; y <= max_int.y; y++)
        {
            for (int x = min_int.x; x <= max_int.x; x++)
            {
                Vector2 p = new Vector2(x+0.5f, y+0.5f);
                float ab = EdgeTest(t.a, t.b, p);
                if (ab < 0)
                    continue;

                float bc = EdgeTest(t.b, t.c, p);
                if (bc < 0)
                    continue;

                float ca = EdgeTest(t.c, t.a, p);
                if (ca < 0)
                    continue;

                float area = EdgeTest(t.a, t.b, t.c);
                float w_a = bc / area;
                float w_b = ca / area;
                float w_c = ab / area;

                float z = 1 / (w_a * t.a.z + w_b * t.b.z + w_c * t.c.z);

                if (!DepthTest(x, y, z / cam.farClipPlane))
                    continue;

                Color color =z * ( colors.a  * w_a + colors.b  * w_b + colors.c  * w_c);
                Vector2 uv = z * (uvs.a * w_a + uvs.b * w_b + uvs.c * w_c);
                Vector3 pos_world = z * (pos_worlds.a * w_a + pos_worlds.b * w_b + pos_worlds.c * w_c);
                Vector3 normal = z * (normals.a * w_a + normals.b * w_b + normals.c * w_c);

                Color col;
                if (chekerBoard)
                    col = FragmentShader_ChckerBorad(uv, pos_world, normal, mainTex);
                else
                    col = FragmentShader_BlinnPhong(uv,pos_world, normal, mainTex);

                colorBuffer.SetPixel(x, y, col);
            }
        }
    }

    public Color FragmentShader_BlinnPhong(Vector2 uv,Vector3 pos_world,Vector3 normal,Texture2D mainTex)
    {
        Vector3 normalDir = Vector3.Normalize(normal);
        Vector3 viewDir = Vector3.Normalize(cam.transform.position - pos_world);
        Vector3 lightDir = Vector3.Normalize( lightPos);
        Vector3 halfDir = Vector3.Normalize(lightDir + viewDir);

        Color color = Sample2D(mainTex, uv.x, uv.y);
        Vector4 albedo = (Vector3.Dot(normalDir, lightDir) * 0.5f+ 0.5f)*color;
        Vector4 specular = Mathf.Pow(Mathf.Clamp01(Vector3.Dot(halfDir,normalDir)), 24) * lightColor;
        Vector4 ambient = Vector4.one * 0.2f * color;

        Color col = albedo+  specular + ambient;

        return col;
    }

    public Color FragmentShader_ChckerBorad(Vector2 uv, Vector3 pos_world, Vector3 normal, Texture2D mainTex)
    {
        return CheckerBoard(uv);
    }

    public Color FragmentShader_UV(Vector2 uv, Vector3 pos_world, Vector3 normal, Texture2D mainTex)
    {
        float _x = Mathf.Repeat(uv.x, 1);
        float _y = Mathf.Repeat(uv.y, 1);

        return Color.white * (_x + _y) * 0.5f;
    }

    public Color Sample2D(Texture2D tex,float x,float y)
    {
        if (tex == null)
            return Color.white;
        
        int _x = (int)(Mathf.Repeat(x,1) * (tex.width));
        int _y = (int)(Mathf.Repeat(y,1) * (tex.height));
        return tex.GetPixel(_x, _y);
    }

    //z为归一化的深度
    public bool DepthTest(int x,int y,float z)
    {
        if (depthBuffer[x + y*pixelWidth] < z)
            return false;

        depthBuffer[x + y * pixelWidth] = z;
        return true;
    }

    //返回Cross(toP,toB);
    public float EdgeTest(Vector2 edge_a,Vector2 edge_b,Vector2 p)
    {
        Vector2 toP = p - edge_a;
        Vector2 toB = edge_b - edge_a;

        float cross = toP.x * toB.y - toP.y * toB.x;
        return cross;
    }

    //通过相似三角形裁剪三角形
    public Vector3[] ClipTriangle(Vector3[] triangle, Plane plane)
    {
        Vector3[] res = new Vector3[4];

        for (int i = 0, j = 1, n = 0; i < 3; i++, j++)
        {
            if (j == 3)
                j = 0;

            float di = Vector3.Dot(triangle[i], plane.normal) + plane.d;

            float dj = Vector3.Dot(triangle[j], plane.normal) + plane.d;


            if (di <= 0)
            {
                res[n++] = triangle[i];

                if (dj > 0)
                {
                    res[n++] = Vector3.Lerp(triangle[i], triangle[j], di / (di - dj));
                }
            }
            else
            {
                if (dj <= 0)
                {
                    res[n++] = Vector3.Lerp(triangle[i], triangle[j], di / (di - dj));
                }
            }


        }
        return res;
    }

    //通过平面公式和直线公式求交，裁剪三角形
    public static List<Triangle3D> ClipTriangle(Triangle3D triangle, Plane plane)
    {
        List<Triangle3D> result = new List<Triangle3D>();
        Vector3 a = triangle.a;
        Vector3 b = triangle.b;
        Vector3 c = triangle.c;

        float fa = Helper.SnapToZero(plane.Point(a));
        float fb = Helper.SnapToZero(plane.Point(b));
        float fc = Helper.SnapToZero(plane.Point(c));

        if ((fa >= 0 && fb >= 0 && fc >= 0) || (fa <= 0 && fb <= 0 && fc <= 0))
        {
            result.Add(triangle);
            return result;
        }

        if (fa * fc > 0)
        {
            Vector3 temp = b;
            b = c;
            c = temp;
        }
        else if (fb * fc > 0)
        {
            Vector3 temp = a;
            a = c;
            c = temp;
        }

        Vector3 A = Intersection(a, c, plane);
        Vector3 B = Intersection(b, c, plane);

        result.Add(new Triangle3D(a, b, A, triangle.normal));
        result.Add(new Triangle3D(b, A, B, triangle.normal));
        result.Add(new Triangle3D(A, B, c, triangle.normal));

        return result;
    }

    //用于将三角形于视锥体平面裁剪
    //平面法向量需要朝向视锥体内部，返回存在于视锥体内的三角形
    public static List<Triangle3D> ClipTrinagle_Frustum(Triangle3D triangle,Plane frustumPlane)
    {
        List<Triangle3D> result = new List<Triangle3D>();
        Vector3 a = triangle.a;
        Vector3 b = triangle.b;
        Vector3 c = triangle.c;

        float fa = Helper.SnapToZero(frustumPlane.Point(a));
        float fb = Helper.SnapToZero(frustumPlane.Point(b));
        float fc = Helper.SnapToZero(frustumPlane.Point(c));

        if ((fa <= 0 && fb <= 0 && fc <= 0))
        {
            return result;
        }
        else if (fa >= 0 && fb >= 0 && fc >= 0)
        {
            result.Add(triangle);
            return result;
        }

        if (fa * fc > 0)
        {
            Vector3 tv = b;
            b = c;
            c = tv;

            float tf = fb;
            fb = fc;
            fc = tf;
        }
        else if (fb * fc > 0)
        {
            Vector3 tv = a;
            a = c;
            c = tv;

            float tf = fa;
            fa = fc;
            fc = tf;

        }

        Vector3 A = Intersection(a, c, frustumPlane);
        Vector3 B = Intersection(b, c, frustumPlane);

        if (fa > 0)
        {
            result.Add(new Triangle3D(a, b, A, triangle.normal));
            result.Add(new Triangle3D(b, A, B, triangle.normal));
        }
        else
        {
            result.Add(new Triangle3D(A, B, c, triangle.normal));
        }

        return result;

    }


    //fov为degree
    public Matrix4x4 GetProjectionMatrix(Camera camera)
    {
        float cot = 1 / Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float far = camera.farClipPlane;
        float near = camera.nearClipPlane;

        return new Matrix4x4
            (
            new Vector4(cot / camera.aspect, 0, 0, 0),
            new Vector4(0, cot, 0, 0),
            new Vector4(0, 0, -(far + near) / (far - near), -1),
            new Vector4(0, 0, -2 * near * far / (far - near), 0)
            );
    }

    //齐次除法并映射到屏幕坐标 vec.w为摄像机坐标系z值取负
    public Vector4 ClipSpaceToScreenSpace(Vector4 vec, int pixelWidth, int pixelHeight)
    {
        return new Vector4(vec.x * pixelWidth / (2 * vec.w) + pixelWidth * 0.5f
            , vec.y * pixelHeight / (2 * vec.w) + pixelHeight * 0.5f,vec.z,vec.w);
    }

    //平面法线要朝向原点，否则会计算出错
    //法线同侧为正，不同侧为负
    public static Vector3 Intersection(Vector3 v1, Vector3 v2, Plane plane)
    {
        Vector3 dir = v2 - v1;
        float t = -(Vector3.Dot(plane.normal, v1) + plane.d) / Vector3.Dot(plane.normal, dir);
        return v1 + t * dir;
    }

    public Vector2 FlatVector3(Vector3 vec)
    {
        return new Vector2(vec.x, vec.y);
    }


    public static bool FaceCulling(Vector3 normal,Vector3 viewDirection)
    {
        float go = Vector3.Dot(normal, viewDirection);
        return ((go) <0);
    }

    public static Vector4 V4Point(Vector3 vec)
    {
        Vector4 res = vec;
        res.w = 1;
        return res;
    }

    public static Vector2Int ClampInScreenSpace(int pixelWidth,int pixelHeight,Vector2Int pos)
    {
        int x = Mathf.Clamp(pos.x, 0, pixelWidth - 1);
        int y = Mathf.Clamp(pos.y, 0, pixelHeight - 1);
        return new Vector2Int(x, y);
    }

    public Color CheckerBoard(Vector2 uv)
    {
        float checkerBoard = ((uv.x * 5 % 1.0 > 0.5) ^ (uv.y * 5 % 1.0) < 0.5) ? 1 : 0;
        return Color.white * checkerBoard;
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        if (!DrawGizmos)
            return;
        Gizmos.DrawLine(v1.position, v2.position);
        Gizmos.DrawLine(v2.position, v3.position);
        Gizmos.DrawLine(v3.position, v1.position);

    }
}
