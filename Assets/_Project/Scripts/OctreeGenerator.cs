using UnityEngine;

namespace Octrees {
    public class OctreeGenerator : MonoBehaviour {
        public GameObject[] objects;
        public float minNodeSize = 1f;
        public Octree ot;
        
        public readonly Graph waypoints = new();
        
        void Awake() => ot = new Octree(objects, minNodeSize, waypoints);

        void OnDrawGizmos() {
            if (!Application.isPlaying) return;
            
            Gizmos.color = Color.green;
            // Gizmos.DrawWireCube(ot.bounds.center, ot.bounds.size);
            
            ot.root.DrawNode();
            // ot.graph.DrawGraph();
        }
    }
}
