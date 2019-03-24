using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PainterAlgorithm;

public class TrianglePainter : MonoBehaviour {

    public Vector3[] pos;
    public Color color;
    public Triangle3D triangle;
	// Use this for initialization
	void Start () {
        CacheTriangle();
	}
	
	// Update is called once per frame
	void Update () {
	}

    //void Cache()
    //{
    //    if (transform.childCount < 3)
    //        return;

    //    if (mesh == null)
    //        mesh = new Mesh();

    //    if ( mesh.vertices.Length != transform.childCount)
    //        mesh.vertices = new Vector3[transform.childCount];

    //    int cnt = 0;
    //    foreach (Transform go in transform)
    //    {
    //        if (cnt == 3)
    //            return;

    //        mesh.vertices[cnt++] = go.position;
    //    }

    //    Plane p = new Plane(mesh.vertices[0],mesh.vertices[1],mesh.vertices[2]);
    //    mesh.SetVertices()

    //}
    public static TrianglePainter CreateTrianglePainter(Triangle3D triangle,string name = "TrianglePainter")
    {
        GameObject go = new GameObject(name);
        go.transform.position = (triangle.a + triangle.b + triangle.c) / 3;

        TrianglePainter tp = go.AddComponent<TrianglePainter>();
        tp.color = Helper.RandomColor(); 
        GameObject p1 = new GameObject("p1");
        GameObject p2 = new GameObject("p2");
        GameObject p3 = new GameObject("p3");

        p1.transform.position = triangle.a;
        p2.transform.position = triangle.b;
        p3.transform.position = triangle.c;

        p1.transform.SetParent(go.transform);
        p2.transform.SetParent(go.transform);
        p3.transform.SetParent(go.transform);


        return tp;
    }

    void CacheTriangle()
    {
        if (pos == null || transform.childCount != pos.Length)
            pos = new Vector3[transform.childCount];

        int i = 0;
        foreach (Transform item in transform)
        {
            pos[i++] = item.position;
        }

        triangle = new Triangle3D(pos[0], pos[1], pos[2]);
    }

    //在这里更新数据会和内部序列化冲突，暂时在start中更新数据，
    //TODO:在自定义编辑器中修改值
    private void OnDrawGizmos()
    {
        if (transform.childCount < 3)
            return;

        Gizmos.color = color;

        CacheTriangle();

        triangle = new Triangle3D(pos[0], pos[1], pos[2]);

        Gizmos.DrawLine(triangle.a , triangle.b);
        Gizmos.DrawLine(triangle.b, triangle.c);
        Gizmos.DrawLine(triangle.c, triangle.a);

        Vector3 mid = (pos[0] + pos[1] + pos[2]) / 3;

        Gizmos.DrawLine(mid, mid + triangle.normal.normalized * 0.2f);
    }
}


