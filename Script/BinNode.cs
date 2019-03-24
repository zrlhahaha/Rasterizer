using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BinNode<T>
{
    public T data;
    public BinNode<T> left;
    public BinNode<T> right;

    public BinNode(T data)
    {
        this.data = data;
    }

    private BinNode()
    {
    }

}

