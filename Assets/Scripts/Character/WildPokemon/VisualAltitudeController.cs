using UnityEngine;

public class VisualAltitudeController
{
    readonly WildPokemonController owner;
    readonly SpriteRenderer sprite;
    readonly VerticalPresenceProfile profile;

    float baseLocalY;
    float currentOffset;
    float targetOffset;
    float nextChangeTime;
    SpriteRenderer shadowRenderer;

    public float CurrentOffset => currentOffset;
    InteractionPlaneBias bias = InteractionPlaneBias.Neutral;


    public VisualAltitudeController(WildPokemonController owner, SpriteRenderer sprite, VerticalPresenceProfile profile)
    {
        this.owner = owner;
        this.sprite = sprite;
        this.profile = profile;
        shadowRenderer = owner.transform.Find("Shadow")?.GetComponent<SpriteRenderer>();
        if (shadowRenderer == null)
        {
            Debug.LogWarning(
                $"{owner.name}: VisualAltitudeController could not find Shadow SpriteRenderer"
            );
        }
        float shadowYOffset = (profile != null && profile.usesGroundFooting) ? -0.5f : 0f;

        shadowRenderer.transform.localPosition = new Vector3(0f, shadowYOffset, 0f);

        baseLocalY = sprite.transform.localPosition.y;

        currentOffset = 0f;
        targetOffset = 0f;
        ScheduleNextChange();
    }

    public void Tick()
    {
        if (profile == null)
            return;

        if (bias == InteractionPlaneBias.Neutral && Time.time >= nextChangeTime)
        {
            PickNewTargetOffset();
            ScheduleNextChange();
        }

        float speed = bias == InteractionPlaneBias.Neutral ? 0.5f : 2.5f;

        currentOffset = Mathf.MoveTowards(currentOffset, targetOffset, Time.deltaTime * speed);

        ApplyVisualOffset();
        UpdateShadow();
    }

    public bool AllowsInteraction()
    {
        if (profile == null)
            return true;

        return Mathf.Abs(currentOffset) <= profile.interactionTolerance;
    }

    public void SetInteractionPlaneBias(InteractionPlaneBias newBias)
    {
        bias = newBias;

        switch (bias)
        {
            case InteractionPlaneBias.Converge:
                targetOffset = 0f;
                break;

            case InteractionPlaneBias.Diverge:
                targetOffset = GetDivergenceOffset();
                break;
        }
    }

    void PickNewTargetOffset()
    {
        targetOffset = Random.Range(profile.offsetRange.x, profile.offsetRange.y);
    }

    void ScheduleNextChange()
    {
        nextChangeTime = Time.time + Random.Range(profile.changeIntervalRange.x, profile.changeIntervalRange.y);
    }

    void ApplyVisualOffset()
    {
        var pos = sprite.transform.localPosition;
        pos.y = baseLocalY + currentOffset;
        sprite.transform.localPosition = pos;
    }

    float GetDivergenceOffset()
    {
        return profile.divergencePreference == DivergencePreference.TowardMaxPositive
            ? profile.offsetRange.y
            : profile.offsetRange.x;
    }

    void UpdateShadow()
    {
        if (shadowRenderer == null || profile == null)
            return;

        float t = Mathf.InverseLerp(
            0f,
            Mathf.Max(Mathf.Abs(profile.offsetRange.x), Mathf.Abs(profile.offsetRange.y)),
            Mathf.Abs(currentOffset)
        );

        float scale = Mathf.Lerp(1f, 0.6f, t);
        float alpha = Mathf.Lerp(0.4f, 0.15f, t);

        shadowRenderer.transform.localScale = new Vector3(scale, scale, 1f);

        var c = shadowRenderer.color;
        c.a = alpha;
        shadowRenderer.color = c;
    }

}

public enum InteractionPlaneBias { Neutral, Converge, Diverge }

public enum DivergencePreference { TowardMaxPositive, TowardMaxNegative }
