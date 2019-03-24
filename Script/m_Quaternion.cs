using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct m_Quaternion{

    private float _w;
    private float _x;
    private float _y;
    private float _z;
    private Matrix4x4 _rotateMatrix;

    public float w { get { return _w; } set { _w = value; dirty = true; } }
    public float x { get { return _x; } set { _x = value; dirty = true; } }
    public float y { get { return _y; } set { _y = value; dirty = true; } }
    public float z { get { return _z; } set { _z = value; dirty = true; } }
    public Matrix4x4 rotateMatrix
    {
        get
        {
            if (dirty)
            {
                dirty = false;
                UpdateRotateMatrix();
            }
            return _rotateMatrix;
        }
        private set
        {
            _rotateMatrix = value;
        }
    }

    bool dirty;

    static public m_Quaternion AngleAxis( float angle, Vector3 axis)
    {
        m_Quaternion rot = new m_Quaternion();

        rot.w = Mathf.Cos(angle * 0.5f*Mathf.Deg2Rad);
        float _sin= Mathf.Sin(angle * 0.5f*Mathf.Deg2Rad);
        rot.x = _sin * axis.x;
        rot.y = _sin * axis.y;
        rot.z = _sin * axis.z;

        return rot;
    }

    public Vector3 RotateVec(Vector4 vec)
    {
        return rotateMatrix * vec;
    }

    public void UpdateRotateMatrix()
    {
        rotateMatrix = new Matrix4x4
    (
      new Vector4(1 - 2 * y * y - 2 * z * z, 2 * x * y + 2 * w * z, 2 * x * z - 2 * w * y, 0)
    , new Vector4(2 * x * y - 2 * w * z, 1 - 2 * x * x - 2 * z * z, 2 * w * x + 2 * y * z, 0)
    , new Vector4(2 * w * y + 2 * x * z, 2 * y * z - 2 * w * x, 1 - 2 * x * x - 2 * y * y, 0)
    , new Vector4(0, 0, 0, 1)
    );
    }

    public static m_Quaternion GetCopyFromQuaternion(Quaternion q)
    {
        m_Quaternion res = new m_Quaternion();
        res.x = q.x;
        res.y = q.y;
        res.z = q.z;
        res.w = q.w;

        return res;
    }


}
