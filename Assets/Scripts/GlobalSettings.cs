using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GlobalSettings : MonoSingleton<GlobalSettings>
{
    [SerializeField] Color defaultFontColor, highlightedTextColor, highlightedImageColor;
    [SerializeField] LayerMask solidObjectsLayer, interactablesLayer, grassLayer, playerLayer, fovLayer, portalLayer, triggersLayer, ledgesLayer, waterLayer;
    [SerializeField] Color psnColor, brnColor, slpColor, parColor, frzColor;
    [SerializeField] float sellFactor = 0.85f;

    Dictionary<ConditionID, Color> statusColors;

    public Color DefaultFontColor => defaultFontColor;
    public Color HighlightedTextColor => highlightedTextColor;
    public Color HighlightedImageColor => highlightedImageColor;
    public LayerMask SolidObjectsLayer => solidObjectsLayer;
    public LayerMask InteractablesLayer => interactablesLayer;
    public LayerMask GrassLayer => grassLayer;
    public LayerMask PlayerLayer => playerLayer;
    public LayerMask FoVLayer => fovLayer;
    public LayerMask PortalLayer => portalLayer;
    public LayerMask TriggersLayer => triggersLayer;
    public LayerMask LedgesLayer => ledgesLayer;
    public LayerMask WaterLayer => waterLayer;
    public LayerMask TriggerableLayers => grassLayer | fovLayer | portalLayer | triggersLayer | waterLayer;
    public LayerMask CollisionLayers => solidObjectsLayer | interactablesLayer | playerLayer | waterLayer;

	public float SellFactor { get => sellFactor; set => sellFactor = value; }
    public Color Transparent = new Color(1, 1, 1, 0);

	public Color GetStatusColor(ConditionID condition) => statusColors[condition];

    void Awake()
    {
        base.Awake();
        
        statusColors = new Dictionary<ConditionID, Color>(){
            {ConditionID.psn, psnColor},
            {ConditionID.brn, brnColor},
            {ConditionID.par, parColor},
            {ConditionID.slp, slpColor},
            {ConditionID.frz, frzColor}
        };
    }

    public void DebugDrawBox(Vector2 point, Vector2 size, float angle, Color color, float duration)
    {

        var orientation = Quaternion.Euler(0, 0, angle);

        // Basis vectors, half the size in each direction from the center.
        Vector2 right = orientation * Vector2.right * size.x / 2f;
        Vector2 up = orientation * Vector2.up * size.y / 2f;

        // Four box corners.
        var topLeft = point + up - right;
        var topRight = point + up + right;
        var bottomRight = point - up + right;
        var bottomLeft = point - up - right;

        // Now we've reduced the problem to drawing lines.
        Debug.DrawLine(topLeft, topRight, color, duration);
        Debug.DrawLine(topRight, bottomRight, color, duration);
        Debug.DrawLine(bottomRight, bottomLeft, color, duration);
        Debug.DrawLine(bottomLeft, topLeft, color, duration);
    }
}
