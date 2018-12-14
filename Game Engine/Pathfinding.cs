using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPI311.GameEngine {
    public class AStarNode {
        public AStarNode Parent { get; set; }
        public Vector3 Position { get; set; }
        public bool Passable { get; set; }
        public bool Closed { get; set; }
        public float Cost { get; set; }
        public float Heuristic { get; set; }
        public int Col { get; set; } // x
        public int Row { get; set; }
        public AStarNode(int col, int row, Vector3 position) {
            Col = col;
            Row = row;
            Position = position;// for 3D space

            Passable = true;
            Cost = 0;
            Heuristic = 0;
            Parent = null;
            Closed = false;
        }
    }
    public class AStarSearch {
        public int Cols { get; set; }
        public int Rows { get; set; }
        public AStarNode[,] Nodes { get; set; }
        public AStarNode Start { get; set; }
        public AStarNode End { get; set; }
        private SortedDictionary<float, List<AStarNode>> openList;

        public AStarSearch(int rows, int cols) {
            openList = new SortedDictionary<float, List<AStarNode>>();
            Rows = rows;
            Cols = cols;
            Nodes = new AStarNode[Rows, Cols];
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    Nodes[r, c] = new AStarNode(c,r, new Vector3(r - 50, 0, c - 50));
        }

        public void Search() {
            #region Initialize grid
            openList.Clear();
            foreach (AStarNode node in Nodes) {
                node.Closed = false;
                node.Cost = Single.MaxValue;
                node.Parent = null;
                node.Heuristic = Vector3.Distance(node.Position, End.Position);
            }
            #endregion

            //The main(most important) part is the Search method.

            AddToOpenList(Start); //Add a start point (null parent)
            while (openList.Count > 0) // openlist is not empty
            {
                AStarNode node = GetBestNode(); // find the best node
                if (node == End)
                    break;
                if (node.Row < Rows - 1)
                    AddToOpenList(Nodes[node.Row + 1, node.Col], node);
                if (node.Row > 0)
                    AddToOpenList(Nodes[node.Row - 1, node.Col], node);
                if (node.Col < Cols - 1)
                    AddToOpenList(Nodes[node.Row, node.Col + 1], node);
                if (node.Col > 0)
                    AddToOpenList(Nodes[node.Row, node.Col - 1], node);
            }
        }

        private void AddToOpenList(AStarNode node, AStarNode parent = null) {
            if (!node.Passable || node.Closed) return;
            if (parent == null) node.Cost = 0;
            else {
                float cost = parent.Cost + 1;
                if (node.Cost > cost) {
                    RemoveFromOpenList(node);
                    node.Cost = cost;
                    node.Parent = parent;
                }
                else
                    return;
            }
            float key = node.Cost + node.Heuristic;
            if (!openList.ContainsKey(key))
                openList[key] = new List<AStarNode>();
            openList[key].Add(node);
        }

        private AStarNode GetBestNode() {
            AStarNode node = openList.ElementAt(0).Value[0];
            openList.ElementAt(0).Value.Remove(node);
            if (openList.ElementAt(0).Value.Count == 0)
                openList.Remove(node.Cost + node.Heuristic);
            node.Closed = true;
            return node;
        }

        private void RemoveFromOpenList(AStarNode node) {
            float key = node.Cost + node.Heuristic;
            if (openList.ContainsKey(key)) {
                openList[key].Remove(node);
                if (openList[key].Count == 0)
                    openList.Remove(key);
            }
        }

    }
}
