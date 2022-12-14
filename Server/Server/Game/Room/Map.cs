using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using ServerCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Server.Game
{
	public struct Pos
	{
		public Pos(int y, int x) { Y = y; X = x; }
		public int Y;
		public int X;

		public static bool operator==(Pos lhs, Pos rhs)
		{
			return lhs.Y == rhs.Y && lhs.X == rhs.X;
		}

		public static bool operator!=(Pos lhs, Pos rhs)
		{
			return !(lhs == rhs);
		}

		public override bool Equals(object obj)
		{
			return (Pos)obj == this;
		}

		public override int GetHashCode()
		{
			long value = (Y << 32) | X;
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return base.ToString();
		}
	}

	public struct PQNode : IComparable<PQNode>
	{
		public int F;
		public int G;
		public int Y;
		public int X;

		public int CompareTo(PQNode other)
		{
			if (F == other.F)
				return 0;
			return F < other.F ? 1 : -1;
		}
	}

	public struct Vector3
	{
		public float x;
		public float y;
		public float z;

		public Vector3(float x, float y, float z) { this.x = x; this.y = y;  this.z = z; }

		public static Vector3 up { get { return new Vector3(0, 1, 0); } }
		public static Vector3 down { get { return new Vector3(0, -1, 0); } }
		public static Vector3 left { get { return new Vector3(-1, 0, 0); } }
		public static Vector3 right { get { return new Vector3(1, 0, 0); } }

		public static Vector3 operator +(Vector3 a, Vector3 b)
		{
			return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
		}

		public static Vector3 operator -(Vector3 a, Vector3 b)
		{
			return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
		}

        public static bool operator ==(Vector3 a, Vector3 b)
        {
            return (a.x == b.x) && (a.y == b.y) && (a.z == b.z);
        }

        public static bool operator !=(Vector3 a, Vector3 b)
        {
            return !(a == b);
        }

        public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
        {
            float num = target.x - current.x;
            float num2 = target.y - current.y;
            float num3 = target.z - current.z;
            float num4 = num * num + num2 * num2 + num3 * num3;
            if (num4 == 0f || (maxDistanceDelta >= 0f && num4 <= maxDistanceDelta * maxDistanceDelta))
            {
                return target;
            }

            float num5 = (float)Math.Sqrt(num4);
            return new Vector3(current.x + num / num5 * maxDistanceDelta, current.y + num2 / num5 * maxDistanceDelta, current.z + num3 / num5 * maxDistanceDelta);
        }

        public float magnitude { get { return (float)Math.Sqrt(sqrMagnitude); } }
		public float sqrMagnitude { get { return (x * x + y * y); } }
		public float cellDistFromZero { get { return Math.Abs(x) + Math.Abs(y); } }

        public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            if (t < 0f)
                t = 0f;

            if (t > 1f)
                t = 1f;

            return new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }

        public static float Distance(Vector3 a, Vector3 b)
        {
            float num = a.x - b.x;
            float num2 = a.y - b.y;
            float num3 = a.z - b.z;
            return (float)Math.Sqrt(num * num + num2 * num2 + num3 * num3);
        }
    }

    #region A* PathFinding
    public class Map
	{
        bool[,] _collision;
        GameObject[,] _objects;

        public void LoadMap(string pathPrefix = "../../../../../Common/MapData")
        {
            string text = File.ReadAllText($"{pathPrefix}/Map.txt");
            StringReader reader = new StringReader(text);

            _collision = new bool[50, 50];
            _objects = new GameObject[50, 50];

            for (int y = 0; y < 50; y++)
            {
                string line = reader.ReadLine();
                for (int x = 0; x < 50; x++)
                    _collision[y, x] = (line[x] == '1' ? true : false);
            }
        }

        public bool CanGo(Vector3 cellPos, bool checkObjects = true)
        {
            if (cellPos.x < 0 || cellPos.x >= 50)
                return false;
            if (cellPos.z < 0 || cellPos.z >= 50)
                return false;

            int x = (int)cellPos.x; // ??????
            int y = (int)cellPos.z; // ??????
            return !_collision[x, y] && (!checkObjects || _objects[x, y] == null);
        }

        // U D L R
        int[] _deltaY = new int[] { 1, -1, 0, 0 };
        int[] _deltaX = new int[] { 0, 0, -1, 1 };
        int[] _cost = new int[] { 10, 10, 10, 10 };

        public List<Vector3> FindPath(Vector3 startCellPos, Vector3 destCellPos, bool checkObjects = true, int maxDist = 100)
        {
            List<Pos> path = new List<Pos>();

            // ?????? ?????????
            // F = G + H
            // F = ?????? ?????? (?????? ?????? ??????, ????????? ?????? ?????????)
            // G = ??????????????? ?????? ???????????? ??????????????? ?????? ?????? (?????? ?????? ??????, ????????? ?????? ?????????)
            // H = ??????????????? ????????? ???????????? (?????? ?????? ??????, ??????)

            // (y, x) ?????? ??????????????? ?????? (?????? = closed ??????)
            HashSet<Pos> closeList = new HashSet<Pos>(); // CloseList

            // (y, x) ?????? ?????? ??? ???????????? ???????????????
            // ??????X => MaxValue
            // ??????O => F = G + H
            Dictionary<Pos, int> openList = new Dictionary<Pos, int>(); // OpenList
            Dictionary<Pos, Pos> parent = new Dictionary<Pos, Pos>();

            // ?????????????????? ?????? ????????? ?????????, ?????? ?????? ????????? ????????? ???????????? ?????? ??????
            PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>();

            // CellPos -> ArrayPos
            Pos pos = Cell2Pos(startCellPos);
            Pos dest = Cell2Pos(destCellPos);

            // ????????? ?????? (?????? ??????)
            openList.Add(pos, 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)));

            pq.Push(new PQNode() { F = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)), G = 0, Y = pos.Y, X = pos.X });
            parent.Add(pos, pos);

            while (pq.Count > 0)
            {
                // ?????? ?????? ????????? ?????????
                PQNode pqNode = pq.Pop();
                Pos node = new Pos(pqNode.Y, pqNode.X);
                // ????????? ????????? ?????? ????????? ?????????, ??? ?????? ????????? ????????? ?????? ??????(closed)??? ?????? ??????
                if (closeList.Contains(node))
                    continue;

                // ????????????
                closeList.Add(node);

                // ????????? ??????????????? ?????? ??????
                if (node.Y == dest.Y && node.X == dest.X)
                    break;

                // ???????????? ??? ????????? ??? ?????? ???????????? ???????????? ??????(open)??????
                for (int i = 0; i < _deltaY.Length; i++)
                {
                    Pos next = new Pos(node.Y + _deltaY[i], node.X + _deltaX[i]);

                    // ?????? ?????? ??????
                    if (Math.Abs(pos.Y - next.Y) + Math.Abs(pos.X - next.X) > maxDist)
                        continue;

                    // ?????? ????????? ??????????????? ??????
                    // ????????? ????????? ??? ??? ????????? ??????
                    if (next.Y != dest.Y || next.X != dest.X)
                    {
                        if (CanGo(Pos2Cell(next), checkObjects) == false) // CellPos
                            continue;
                    }

                    // ?????? ????????? ????????? ??????
                    if (closeList.Contains(next))
                        continue;

                    // ?????? ??????
                    int g = 0;// node.G + _cost[i];
                    int h = 10 * ((dest.Y - next.Y) * (dest.Y - next.Y) + (dest.X - next.X) * (dest.X - next.X));
                    // ?????? ???????????? ??? ?????? ??? ?????? ???????????? ??????

                    int value = 0;
                    if (openList.TryGetValue(next, out value) == false)
                        value = Int32.MaxValue;

                    if (value < g + h)
                        continue;

                    // ?????? ??????
                    if (openList.TryAdd(next, g + h) == false)
                        openList[next] = g + h;

                    pq.Push(new PQNode() { F = g + h, G = g, Y = next.Y, X = next.X });

                    if (parent.TryAdd(next, node) == false)
                        parent[next] = node;
                }
            }

            return CalcCellPathFromParent(parent, dest);
        }

        List<Vector3> CalcCellPathFromParent(Dictionary<Pos, Pos> parent, Pos dest)
        {
            List<Vector3> cells = new List<Vector3>();

            if (parent.ContainsKey(dest) == false)
            {
                Pos best = new Pos();
                int bestDist = Int32.MaxValue;

                foreach (Pos pos in parent.Keys)
                {
                    int dist = Math.Abs(dest.X - pos.X) + Math.Abs(dest.Y - pos.Y);
                    // ?????? ????????? ????????? ?????????
                    if (dist < bestDist)
                    {
                        best = pos;
                        bestDist = dist;
                    }
                }

                dest = best;
            }

            {
                Pos pos = dest;
                while (parent[pos] != pos)
                {
                    cells.Add(Pos2Cell(pos));
                    pos = parent[pos];
                }
                cells.Add(Pos2Cell(pos));
                cells.Reverse();
            }

            return cells;
        }

        Pos Cell2Pos(Vector3 cell)
        {
            // CellPos -> ArrayPos
            return new Pos((int)cell.z, (int)cell.x);
        }

        Vector3 Pos2Cell(Pos pos)
        {
            // ArrayPos -> CellPos
            return new Vector3(pos.X, 0f, pos.Y);
        }

#endregion
    }
}
