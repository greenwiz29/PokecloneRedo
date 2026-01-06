#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneGraphVisualizer : EditorWindow
{
    SceneGraph graph;
    Vector2 pan;
    float zoom = 1f;
    SceneTrigger[] triggers;

    [MenuItem("Tools/Scene Graph")]
    static void Open()
    {
        GetWindow<SceneGraphVisualizer>("Scene Graph");
    }

    void OnEnable()
    {
        triggers = FindObjectsByType<SceneTrigger>(FindObjectsSortMode.None);

        // center graph initially
        pan = position.size / 2f;

        Rebuild();
    }

    void OnGUI()
    {
        HandleInput();

        if (GUILayout.Button("Rebuild Graph"))
            Rebuild();

        if (graph == null)
            return;

        if (UnityEngine.Event.current.type == EventType.Repaint)
        {
            DrawGrid();

            triggers.Select(t => t.SceneDetails).ToList().ForEach(d => DrawSceneBounds(d));

            DrawEdges();
            DrawNodes();
        }
    }

    void Rebuild()
    {
        graph = BuildSceneGraph();
        Repaint();
    }

    // ---------------- Drawing ----------------

    void DrawNodes()
    {
        foreach (var node in graph.Nodes.Values)
        {
            bool isLoaded = SceneManager.GetSceneByName(node.Scene.SceneName).isLoaded;

            Vector2 graphPos = WorldToGraph(node.WorldPos);

            // Label style (must be created BEFORE measuring size)
            var labelStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Max(8, Mathf.RoundToInt(8 * zoom)),
                clipping = TextClipping.Overflow
            };

            string sceneName = node.Scene.SceneName;

            // Measure text size
            Vector2 textSize = labelStyle.CalcSize(new GUIContent(sceneName));

            // Padding around text (scaled with zoom)
            float paddingX = 3f * zoom;
            float paddingY = 2f * zoom;

            float width = textSize.x + paddingX * 2f;
            float height = textSize.y + paddingY * 2f;

            Rect rect = new Rect(
                graphPos.x - width / 2f,
                graphPos.y - height / 2f,
                width,
                height
            );

            node.Rect = rect;

            // Background
            Color prevColor = GUI.color;

            GUI.color = isLoaded
                ? new Color(0.4f, 0.8f, 1f, 0.35f)   // loaded: cool cyan-blue
                : new Color(0.8f, 0.8f, 0.8f, 0.25f); // unloaded: neutral gray

            EditorGUI.DrawRect(rect, GUI.color);

            GUI.color = prevColor;


            // Outline
            Handles.color = isLoaded ? Color.cyan : Color.white;
            Handles.DrawLine(rect.min, new Vector2(rect.xMax, rect.yMin));
            Handles.DrawLine(new Vector2(rect.xMax, rect.yMin), rect.max);
            Handles.DrawLine(rect.max, new Vector2(rect.xMin, rect.yMax));
            Handles.DrawLine(new Vector2(rect.xMin, rect.yMax), rect.min);

            // Label
            GUI.Label(rect, sceneName, labelStyle);

            // Click selection
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                Selection.activeObject = node.Scene.gameObject;
                Event.current.Use();
            }
        }
    }

    void DrawEdges()
    {
        Handles.BeginGUI();

        foreach (var node in graph.Nodes.Values)
        {
            foreach (var edge in node.Edges)
            {
                Vector2 from = WorldToGraph(edge.FromPos);
                Vector2 to = WorldToGraph(edge.ToPos);

                // Curved lines for portals
                if (edge.Type == SceneGraph.EdgeType.InteriorPortal || edge.Type == SceneGraph.EdgeType.PortalMismatch)
                {
                    Vector2 tangentOffset = new Vector2(0, Mathf.Abs(to.y - from.y) * 0.3f + 20);
                    Handles.DrawBezier(from, to, from + tangentOffset, to - tangentOffset, Handles.color, null, 2f);
                }
                else
                {
                    Handles.color = Color.green;
                    Handles.DrawLine(from, to);
                }

                if (edge.Portal != null && IsMouseNearLine(from, to))
                    DrawPortalTooltip(edge);
            }
        }

        Handles.EndGUI();
    }

    void DrawGrid()
    {
        Handles.color = new Color(1, 1, 1, 0.05f);

        for (int i = 0; i < position.width; i += 50)
            Handles.DrawLine(new Vector3(i, 0), new Vector3(i, position.height));

        for (int j = 0; j < position.height; j += 50)
            Handles.DrawLine(new Vector3(0, j), new Vector3(position.width, j));
    }

    // ---------------- Input ----------------

    void HandleInput()
    {
        var e = Event.current;

        // Pan with middle mouse drag
        if (e.type == EventType.MouseDrag && e.button == 2)
        {
            pan += e.delta;
            Repaint();
        }

        // Zoom with scroll wheel
        if (e.type == EventType.ScrollWheel)
        {
            float oldZoom = zoom;
            float zoomDelta = -e.delta.y * 0.05f;
            float newZoom = Mathf.Clamp(zoom + zoomDelta, 0.25f, 5f);

            Vector2 mouse = e.mousePosition;
            Vector2 offset = mouse - pan;

            // adjust pan so the point under cursor stays fixed
            pan -= offset * (newZoom / oldZoom - 1f);

            zoom = newZoom;
            e.Use();
            Repaint();
        }

    }

    void DrawSceneBounds(SceneDetails details)
    {
        var col = details.GetComponent<BoxCollider2D>();
        if (!col) return;

        // Scale by zoom
        Vector2 size = col.size * zoom;

        // Center in graph space (flip Y)
        Vector2 center = WorldToGraph(col.bounds.center);

        // Draw the rectangle
        Vector3 topLeft = center + new Vector2(-size.x / 2, -size.y / 2);
        Rect rect = new Rect(topLeft, size);

        // Use EditorGUI.DrawRect for simple wireframe
        Color prevColor = GUI.color;
        GUI.color = new Color(0, 1, 1, 0.25f); // semi-transparent cyan
        EditorGUI.DrawRect(rect, GUI.color);
        GUI.color = prevColor;

        // Outline
        Handles.color = Color.cyan;
        Handles.DrawLine(rect.min, new Vector2(rect.xMax, rect.yMin));
        Handles.DrawLine(new Vector2(rect.xMax, rect.yMin), rect.max);
        Handles.DrawLine(rect.max, new Vector2(rect.xMin, rect.yMax));
        Handles.DrawLine(new Vector2(rect.xMin, rect.yMax), rect.min);
    }

    bool IsMouseNearLine(Vector2 a, Vector2 b, float threshold = 6f)
    {
        var mouse = Event.current.mousePosition;
        float distance = HandleUtility.DistancePointLine(mouse, a, b);
        return distance < threshold;
    }

    void DrawPortalTooltip(SceneGraph.Edge edge)
    {
        var portal = edge.Portal;

        string status =
            edge.Type == SceneGraph.EdgeType.PortalMismatch
                ? "❌ Connection mismatch"
                : "✅ Connected";

        string lockInfo =
            portal.LockCondition != null
                ? $"Locked: {portal.LockCondition.name}"
                : "Unlocked";

        string text =
            $@"PORTAL
            From: {edge.From.Scene.SceneName}
            To: {portal.TargetSceneName}
            EntryPoint: {portal.EntryPointId}
            Direction: {portal.Direction}
            {lockInfo}
            {status}";

        var size = EditorStyles.helpBox.CalcSize(new GUIContent(text));
        var rect = new Rect(
            Event.current.mousePosition + new Vector2(15, 15),
            size
        );

        GUI.Box(rect, text, EditorStyles.helpBox);
    }

    SceneGraph BuildSceneGraph()
    {
        var graph = new SceneGraph();

        foreach (var trigger in triggers)
        {
            var details = trigger.SceneDetails;
            if (!details) continue;

            graph.Nodes[details.SceneName] = new SceneGraph.Node
            {
                Scene = details,
                WorldPos = details.gameObject.transform.position
            };
        }

        BuildWorldAdjacency(graph);

        var portalLookup = LoadAllPortals(graph);
        BuildPortalEdges(graph, portalLookup);

        return graph;
    }

    void BuildWorldAdjacency(SceneGraph graph)
    {
        foreach (var node in graph.Nodes.Values)
        {
            foreach (var connected in node.Scene.ConnectedScenes)
            {
                if (connected == null)
                    continue;

                if (!graph.Nodes.TryGetValue(connected.SceneName, out var target))
                    continue;

                node.Edges.Add(new SceneGraph.Edge
                {
                    From = node,
                    To = target,
                    Type = SceneGraph.EdgeType.WorldAdjacency
                });
            }
        }
    }

    void BuildPortalEdges(SceneGraph graph, Dictionary<string, List<PortalInfo>> portalLookup)
    {
        // Flatten all target portals for lookup by (sceneName, entryPointId)
        var targetPortalLookup = new Dictionary<(string sceneName, string entryId), PortalInfo>();
        foreach (var list in portalLookup.Values)
        {
            foreach (var portal in list)
            {
                if (string.IsNullOrEmpty(portal.EntryPointId)) continue;
                targetPortalLookup[(portal.SceneName, portal.EntryPointId)] = portal;
            }
        }

        foreach (var node in graph.Nodes.Values)
        {
            if (!portalLookup.TryGetValue(node.Scene.SceneName, out var portals)) continue;

            foreach (var portalInfo in portals)
            {
                // Try to find a matching portal in the target scene
                targetPortalLookup.TryGetValue((portalInfo.TargetSceneName, portalInfo.EntryPointId), out var targetPortal);

                // Get target node if it exists
                graph.Nodes.TryGetValue(portalInfo.TargetSceneName, out var targetNode);

                SceneGraph.EdgeType edgeType;
                Vector3 toPos;

                if (targetNode == null || targetPortal.Equals(default(PortalInfo)))
                {
                    edgeType = SceneGraph.EdgeType.PortalMismatch;
                    toPos = targetNode != null ? targetNode.WorldPos : portalInfo.Position + Vector3.right * 50;
                }
                else
                {
                    edgeType = node.Scene.ConnectedScenes.Contains(targetNode.Scene)
                        ? SceneGraph.EdgeType.InteriorPortal
                        : SceneGraph.EdgeType.PortalMismatch;

                    toPos = targetPortal.Position;
                }

                // Skip if we've already drawn this portal pair
                if (string.Compare(node.Scene.SceneName, portalInfo.TargetSceneName) > 0)
                    continue;

                node.Edges.Add(new SceneGraph.Edge
                {
                    From = node,
                    To = targetNode,
                    Type = edgeType,
                    Portal = null,        // optional, GameObject not stored
                    TargetPortal = null,  // optional
                    FromPos = portalInfo.Position,
                    ToPos = toPos
                });
            }
        }
    }

    Dictionary<string, List<PortalInfo>> LoadAllPortals(SceneGraph graph)
    {
        var result = new Dictionary<string, List<PortalInfo>>();

        foreach (var node in graph.Nodes.Values)
        {
            string sceneName = node.Scene.SceneName;
            string path = $"Assets/Scenes/{sceneName}.unity";

            if (!System.IO.File.Exists(path)) continue;
            if (sceneName == "MainMenu") continue;

            // Open scene additively
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            // Gather all portal info
            var portals = Object.FindObjectsByType<LocationPortal>(FindObjectsSortMode.None);
            var portalInfos = new List<PortalInfo>();

            foreach (var portal in portals)
            {
                portalInfos.Add(new PortalInfo
                {
                    SceneName = sceneName,
                    EntryPointId = portal.EntryPointId,
                    TargetSceneName = portal.TargetSceneName,
                    Position = portal.transform.position,
                    LockConditionName = portal.LockCondition != null ? portal.LockCondition.name : null
                });
            }

            result[sceneName] = portalInfos;

            // Close scene immediately — no portal GameObjects are stored
            EditorSceneManager.CloseScene(scene, true);
        }

        return result;
    }

    Vector2 WorldToGraph(Vector2 world)
    {
        // flip Y
        Vector2 flipped = new Vector2(world.x, -world.y);

        // scale by zoom, then offset by pan
        return flipped * zoom + pan;
    }

}

public struct PortalInfo
{
    public string SceneName;          // source scene
    public string EntryPointId;
    public string TargetSceneName;
    public Vector3 Position;
    public string LockConditionName;  // optional, for tooltip
}

#endif