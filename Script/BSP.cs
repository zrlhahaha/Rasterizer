using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PainterAlgorithm
{

    [System.Serializable]
    public class BSP
    {
        public BinNode<List<Triangle3D>> head;

        public BSP(Triangle3D[] polygons)
        {
            head = new BinNode<List<Triangle3D>>(new List<Triangle3D>());
            head.data.AddRange(polygons);
        }

        private BSP()
        {

        }

        public void Build()
        {
            BuildRecursivly(head);
        }

        void BuildRecursivly(BinNode<List<Triangle3D>> node)
        {

            if (node.data.Count <= 1)
                return;

            Triangle3D main = node.data[0];
            Plane plane = new Plane(main.a, main.normal);

            for (int i = 1; i < node.data.Count; i++)
            {
                Triangle3D go = node.data[i];

                float fa = Helper.SnapToZero(plane.Point(go.a));
                float fb = Helper.SnapToZero(plane.Point(go.b));
                float fc = Helper.SnapToZero(plane.Point(go.c));

                if (fa >= 0 && fb >= 0 && fc >= 0)
                {
                    if (node.right == null)
                    {
                        node.right = new BinNode<List<Triangle3D>>(new List<Triangle3D>());
                        node.right.data = new List<Triangle3D>();
                    }

                    node.right.data.Add(go);
                }
                else if (fa <= 0 && fb <= 0 && fc <= 0)
                {
                    if (node.left == null)
                    {
                        node.left = new BinNode<List<Triangle3D>>(new List<Triangle3D>());
                        node.left.data = new List<Triangle3D>();
                    }

                    node.left.data.Add(go);
                }
                else
                {
                    List<Triangle3D> ts = Rasterizer.ClipTriangle(go, plane);
                    node.data.AddRange(ts);
                    node.data.RemoveAt(i);
                    i--;
                }
            }

            node.data.RemoveRange(1, node.data.Count - 1);

            if (node.left != null)
                BuildRecursivly(node.left);

            if (node.right != null)
                BuildRecursivly(node.right);

        }

        public List<Triangle3D> GetSquence(Vector3 viewPoint)
        {
            List<Triangle3D> ts = new List<Triangle3D>();
            GetSquenceRecursivly(head, ts,viewPoint);
            return ts;
        }

        int test = 0;
        void GetSquenceRecursivly(BinNode<List<Triangle3D>> node,List<Triangle3D> sequence, Vector3 view)
        {
            if (node.data.Count != 1)
            {
                Debug.Log("BSP tree not right");
                return;
            }

            Triangle3D t = node.data[0];

            float d = new Plane(t).Point(view);

            BinNode<List<Triangle3D>> first;
            BinNode<List<Triangle3D>> last;

            if (d >= 0)
            {
                first = node.left;
                last = node.right;
            }
            else
            {
                first = node.right;
                last = node.left;
            }

            if (first != null)
                GetSquenceRecursivly(first, sequence,view);

            sequence.Add(t);

            if (last != null)
                GetSquenceRecursivly(last, sequence, view);
        }       
    }
}
