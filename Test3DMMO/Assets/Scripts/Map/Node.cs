using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int _gridX, _gridY;
    public bool _isWall;
    public Vector3 _pos;

    public Node _parentNode;

    public int gCost; // 다음 스퀘어로 가는데 비용
    public int hCost; // 현재 노드에서 도착지까지 비용

    public int FCost { get { return gCost + hCost; } }

    public Node(bool isWall, Vector3 pos, int gridX, int gridY)
    {
        _isWall = isWall;
        _pos = pos;
        _gridX = gridX;
        _gridY = gridY;
    }
}
