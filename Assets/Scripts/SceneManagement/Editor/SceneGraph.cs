#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

public class SceneGraph
{
    public class Node
    {
        public SceneDetails Scene;
        public List<Edge> Edges = new();
        public Rect Rect;
        public Vector2 WorldPos; // from SceneTrigger.transform.position
    }

    public class Edge
    {
        public Node From;
        public Node To;
        public EdgeType Type;
        public LocationPortal Portal;
        public LocationPortal TargetPortal; // optional, may be null if mismatch

        // portal world positions for visualization
        public Vector2 FromPos;
        public Vector2 ToPos;
    }

    public enum EdgeType
    {
        WorldAdjacency,     // SceneTrigger overlap
        InteriorPortal,     // LocationPortal
        PortalMismatch      // Broken interior portal
    }


    public Dictionary<string, Node> Nodes = new();
}
#endif
