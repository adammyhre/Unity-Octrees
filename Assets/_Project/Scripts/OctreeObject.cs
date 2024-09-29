using UnityEngine;

namespace Octrees {
    public class OctreeObject {
        public Bounds bounds;

        public OctreeObject(GameObject obj) {
            bounds = obj.GetComponent<Collider>().bounds;
        }
        
        public bool Intersects(Bounds boundsToCheck) => bounds.Intersects(boundsToCheck);
    }
}