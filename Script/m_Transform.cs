using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class m_Transform {

    public Matrix4x4 translate;
    public Matrix4x4 scale;
    public Matrix4x4 worldMatrix;
    public Matrix4x4 inverseWorldMatrix;
    public m_Quaternion q;
    bool dirty = true;
    bool inverseDirty = true;

    public m_Transform()
    {
        translate.m00 = 1;
        translate.m11 = 1;
        translate.m22 = 1;
        translate.m33 = 1;

        scale.m00 = 1;
        scale.m11 = 1;
        scale.m22 = 1;
        scale.m33 = 1;
    }

    public void SetPosition(Vector3 pos)
    {
        translate.m03 = pos.x;
        translate.m13 = pos.y;
        translate.m23 = pos.z;

        dirty = true;
        inverseDirty = true;

    }

    public void SetRotate(m_Quaternion rot)
    {
        q = rot;
        dirty = true;
        inverseDirty = true;

    }

    public void SetScale(Vector3 newScale)
    {
        scale.m00 = newScale.x;
        scale.m11 = newScale.y;
        scale.m22 = newScale.z;
        dirty = true;
        inverseDirty = true;

    }

    public Matrix4x4 GetWorldMatrix()
    {
        if (dirty)
        {
            worldMatrix = translate*  q.rotateMatrix * scale ;
            dirty = false;
        }

        return worldMatrix;
    }

    public Matrix4x4 GetInversWorldMatrix()
    {
        if (inverseDirty)
        {
            inverseWorldMatrix = q.rotateMatrix.inverse;
            Vector3 t = new Vector3(translate.m03, translate.m13, translate.m23);
            t = -(q.rotateMatrix.inverse * t);
            inverseWorldMatrix.m03 = t.x;
            inverseWorldMatrix.m13 = t.y;
            inverseWorldMatrix.m23 = t.z;

            inverseDirty = false;
        }
        return inverseWorldMatrix;
    }

    /// <summary>
    /// 局部坐标转换到世界坐标
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public Vector4 TransformPoint(Vector4 vec)
    {
        return GetWorldMatrix() * vec;
    }

    public Vector4 TransformPoint(Vector3 vec)
    {
        Vector4 v4 = vec;
        v4.w = 1;
        return GetWorldMatrix() * v4;
    }

    public Vector4 TransformDirection(Vector4 dir)
    {
        return GetWorldMatrix() * dir;
    }

    public Vector3 TransformDirection(Vector3 dir)
    {
        return GetWorldMatrix() * dir;
    }



    /// <summary>
    /// 世界坐标转换到局部坐标
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public Vector4 InverseTransformPoint(Vector4 vec)
    {
        return GetInversWorldMatrix() * vec;
    }

    public Vector4 InverseTransformPoint(Vector3 vec)
    {
        Vector4 v4 = vec;
        v4.w = 1;
        return GetInversWorldMatrix() * v4;
    }

    public Vector4 InverseTransformDirection(Vector4 dir)
    {
        return GetInversWorldMatrix() * dir;
    }

    public Vector3 InverseTransformDirection(Vector3 dir)
    {
        return GetInversWorldMatrix() * dir;
    }




    public void CopyFromTransform(Transform target)
    {
        SetPosition(target.position);
        SetRotate(m_Quaternion.GetCopyFromQuaternion(target.rotation));
        SetScale(target.localScale);
    }
}
