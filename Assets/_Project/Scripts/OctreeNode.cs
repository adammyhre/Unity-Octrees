using System.Collections.Generic;
using UnityEngine;

namespace Octrees {
    public class OctreeNode {
        public List<OctreeObject> objects = new();
        
        static int nextId;
        public readonly int id;
        
        public Bounds bounds;
        Bounds[] childBounds = new Bounds[8];
        public OctreeNode[] children;
        public bool IsLeaf => children == null;
        
        float minNodeSize;

        public OctreeNode(Bounds bounds, float minNodeSize) {
            id = nextId++;
            
            this.bounds = bounds;
            this.minNodeSize = minNodeSize;
            Vector3 newSize = bounds.size * 0.5f; // halved size
            Vector3 centerOffset = bounds.size * 0.25f; // quarter offset
            Vector3 parentCenter = bounds.center;

            for (int i = 0; i < 8; i++) {
                Vector3 childCenter = parentCenter;
                childCenter.x += centerOffset.x * ((i & 1) == 0 ? -1 : 1);
                childCenter.y += centerOffset.y * ((i & 2) == 0 ? -1 : 1);
                childCenter.z += centerOffset.z * ((i & 4) == 0 ? -1 : 1);
                childBounds[i] = new Bounds(childCenter, newSize);
            }
        }
        
        public void Divide(GameObject obj) => Divide(new OctreeObject(obj));

        void Divide(OctreeObject octreeObject) {
            if (bounds.size.x <= minNodeSize) {
                AddObject(octreeObject);
                return;
            }
            
            children ??= new OctreeNode[8];
            
            bool intersectedChild = false;

            for (int i = 0; i < 8; i++) {
                children[i] ??= new OctreeNode(childBounds[i], minNodeSize);

                if (octreeObject.Intersects(childBounds[i])) {
                    children[i].Divide(octreeObject);
                    intersectedChild = true;
                }
            }

            if (!intersectedChild) {
                AddObject(octreeObject);
            }
        }
        
        void AddObject(OctreeObject octreeObject) => objects.Add(octreeObject);

        public void DrawNode() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            // Optional color in red the objects that intersect with the bounds
            // foreach (OctreeObject obj in objects) {
            //     if (obj.Intersects(bounds)) {
            //         Gizmos.color = Color.red;
            //         Gizmos.DrawCube(bounds.center, bounds.size);
            //     }
            // }
            
            if (children != null) {
                foreach (OctreeNode child in children) {
                    if (child != null) child.DrawNode();
                }
            }
        }
    }
}