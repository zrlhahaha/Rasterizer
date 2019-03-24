using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PainterAlgorithm;

public class Test : MonoBehaviour {

    public Plane plane;
    public Transform refPoint;
    public Transform point;
    public BSP bsp;
    public TrianglePainter[] polygonsPainter;

    public TrianglePainter testPlane;
    public Transform p1;
    public Transform p2;
    public Transform p3;
    public Transform p4;

    public Transform p5;
    public Transform p6;

    public Camera cam;
    

    public bool check = false;
	void Start () {
	}
	
	void Update () {

        if (check || Input.GetKeyDown(KeyCode.Space))
        {
            check = false;

            Triangle3D t = testPlane.triangle;

            Camera camera = cam;

            float theta_Y = camera.fieldOfView * 0.5f;
            float theta_X = Mathf.Atan(Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * camera.aspect) * Mathf.Rad2Deg;

            Plane left = new Plane(0, new Vector3(-Mathf.Cos((theta_X) * Mathf.Deg2Rad), 0, -Mathf.Sin((theta_X) * Mathf.Deg2Rad)));
            Plane right = new Plane(0, new Vector3(-Mathf.Cos((180 - theta_X) * Mathf.Deg2Rad), 0, -Mathf.Sin((180 - theta_X) * Mathf.Deg2Rad)));
            Plane near = new Plane(-camera.nearClipPlane, Vector3.back);
            Plane far = new Plane(camera.farClipPlane, Vector3.forward);
            Plane top = new Plane(0, new Vector3(0, -Mathf.Sin((-theta_Y + 90) * Mathf.Deg2Rad), -Mathf.Cos((-theta_Y + 90) * Mathf.Deg2Rad)));
            Plane buttom = new Plane(0, new Vector3(0, -Mathf.Sin((theta_Y - 90) * Mathf.Deg2Rad), -Mathf.Cos((theta_Y - 90) * Mathf.Deg2Rad)));


            List<Triangle3D> ts = Rasterizer.ViewSpaceFrustrumClipping(t,cam);

            print(buttom.Point(p1.position));

            foreach (var go in ts)
            {
                TrianglePainter.CreateTrianglePainter(go);
            }
        }
    }
}


