using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Octrees {
    public class Node {
        static int nextId;
        public readonly int id;
        
        public float f, g, h;
        public Node from;
        
        public List<Edge> edges = new();
        
        public OctreeNode octreeNode;

        public Node(OctreeNode octreeNode) {
            this.id = nextId++;
            this.octreeNode = octreeNode;
        }
        
        public override bool Equals(object obj) => obj is Node other && id == other.id;
        public override int GetHashCode() => id.GetHashCode();
    }

    public class Edge {
        public readonly Node a, b;
        
        public Edge(Node a, Node b) {
            this.a = a;
            this.b = b;
        }

        public override bool Equals(object obj) {
            return obj is Edge other && ((a == other.a && b == other.b) || (a == other.b && b == other.a));
        }
        
        public override int GetHashCode() => a.GetHashCode() ^ b.GetHashCode();
    }
    
    public class Graph {
        public readonly Dictionary<OctreeNode, Node> nodes = new();
        public readonly HashSet<Edge> edges = new();
        
        List<Node> pathList = new();
        
        public int GetPathLength() => pathList.Count;

        public OctreeNode GetPathNode(int index) {
            if (pathList == null) return null;

            if (index < 0 || index >= pathList.Count) {
                Debug.LogError($"Index out of bounds. Path length: {pathList.Count}, Index: {index}"); 
                return null;
            }
            return pathList[index].octreeNode;
        }
        
        const int maxIterations = 10000;

        public bool AStar(OctreeNode startNode, OctreeNode endNode) {
            pathList.Clear();
            Node start = FindNode(startNode);
            Node end = FindNode(endNode);

            if (start == null || end == null) {
                Debug.LogError("Start or End node not found in the graph.");
                return false;
            }
            
            SortedSet<Node> openSet = new(new NodeComparer());
            HashSet<Node> closedSet = new();
            int iterationCount = 0;
            
            start.g = 0;
            start.h = Heuristic(start, end);
            start.f = start.g + start.h;
            start.from = null;
            openSet.Add(start);

            while (openSet.Count > 0) {
                if (++iterationCount > maxIterations) {
                    Debug.LogError("A* exceeded maximum iterations.");
                    return false;
                }
                
                Node current = openSet.First();
                openSet.Remove(current);

                if (current.Equals(end)) {
                    ReconstructPath(current);
                    return true;
                }
                
                closedSet.Add(current);

                foreach (Edge edge in current.edges) {
                    Node neighbor = Equals(edge.a, current) ? edge.b : edge.a;
                    
                    if (closedSet.Contains(neighbor)) continue;
                    
                    float tentative_gScore = current.g + Heuristic(current, neighbor);

                    if (tentative_gScore < neighbor.g || !openSet.Contains(neighbor)) {
                        neighbor.g = tentative_gScore;
                        neighbor.h = Heuristic(neighbor, end);
                        neighbor.f = neighbor.g + neighbor.h;
                        neighbor.from = current;
                        openSet.Add(neighbor);
                    }
                }
            }
            
            return false;
        }

        void ReconstructPath(Node current) {
            while (current != null) {
                pathList.Add(current);
                current = current.from;
            }
            
            pathList.Reverse();
        }
        
        float Heuristic(Node a, Node b) => (a.octreeNode.bounds.center - b.octreeNode.bounds.center).sqrMagnitude;

        public class NodeComparer : IComparer<Node> {
            public int Compare(Node x, Node y) {
                if (x == null || y == null) return 0;
                
                int compare = x.f.CompareTo(y.f);
                if (compare == 0) {
                    return x.id.CompareTo(y.id);
                }
                return compare;
            }
        }

        public void AddNode(OctreeNode octreeNode) {
            if (!nodes.ContainsKey(octreeNode)) {
                nodes.Add(octreeNode, new Node(octreeNode));
            }
        }

        public void AddEdge(OctreeNode a, OctreeNode b) {
            Node nodeA = FindNode(a);
            Node nodeB = FindNode(b);
            
            if (nodeA == null || nodeB == null) return;
            
            var edge = new Edge(nodeA, nodeB);
            if (edges.Add(edge)) {
                nodeA.edges.Add(edge);
                nodeB.edges.Add(edge);
            }
        }

        public void DrawGraph() {
            Gizmos.color = Color.red;
            
            foreach (Edge edge in edges) {
                Gizmos.DrawLine(edge.a.octreeNode.bounds.center, edge.b.octreeNode.bounds.center);
            }
            
            foreach (var node in nodes.Values) {
                Gizmos.DrawWireSphere(node.octreeNode.bounds.center, 0.2f);
            }
        }

        Node FindNode(OctreeNode octreeNode) {
            nodes.TryGetValue(octreeNode, out Node node);
            return node;
        }
    }
}