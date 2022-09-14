using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Transform _startPos;
    public LayerMask _wallMask;
    public Vector2 _gridWorldSize;
    private float _nodeRadius = 0.5f;
    private float _distanceBetweenNodes;

    Node[,] _nodeArr;
    //public List<Node> _finalPath;

    float _fNodeDiameter;
    int _gridSizeX, _gridSizeY;

    private void Start()
    {
        _fNodeDiameter = _nodeRadius * 2; // 0.5 * 2
        _gridWorldSize.x = 50.0f;
        _gridWorldSize.y = 50.0f;
        _gridSizeX = Mathf.RoundToInt(_gridWorldSize.x / _fNodeDiameter); // 100 / 1 = 100
        _gridSizeY = Mathf.RoundToInt(_gridWorldSize.y / _fNodeDiameter); // 100

        CreateGrid();
    }

    public void CreateGrid()
    {
        _nodeArr = new Node[_gridSizeX, _gridSizeY];
        Vector3 bottomLeft = transform.position - (Vector3.right * _gridWorldSize.x / 2) - (Vector3.forward * _gridWorldSize.y / 2);

        for (int x = 0; x < _gridSizeX; x++)
        {
            for (int y = 0; y < _gridSizeY; y++)
            {
                Vector3 worldPoint = bottomLeft + Vector3.right * (x * _fNodeDiameter + _nodeRadius) + Vector3.forward * (y * _fNodeDiameter + _nodeRadius);
                bool Wall = false;

                if (Physics.CheckSphere(worldPoint, _nodeRadius - 0.1f, _wallMask))
                {
                    Wall = true;
                    Debug.Log($"not wall at {x}, {y}");
                }

                _nodeArr[x, y] = new Node(Wall, worldPoint, x, y);
            }
        }

        GenerateByPath("../Common/MapData");
    }

    private void GenerateByPath(string pathPrefix)
    {
        using (var writer = File.CreateText($"{pathPrefix}/Map.txt"))
        {
            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {
                    Node node = _nodeArr[x, y];
                    if (node._isWall)
                        writer.Write("1");
                    else
                        writer.Write("0");
                }
                writer.WriteLine();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(_gridWorldSize.x, 1, _gridWorldSize.y));//Draw a wire cube with the given dimensions from the Unity inspector

        if (_nodeArr != null)//If the grid is not empty
        {
            foreach (Node n in _nodeArr)//Loop through every node in the grid
            {
                if (!n._isWall)//If the current node is a wall node
                {
                    Gizmos.color = Color.white;//Set the color of the node
                }
                else // 벽인 경우
                {
                    Gizmos.color = Color.yellow;//Set the color of the node
                }

                Gizmos.DrawCube(n._pos, Vector3.one * (_fNodeDiameter - _distanceBetweenNodes));//Draw the node at the position of the node.
            }
        }
    }
}
