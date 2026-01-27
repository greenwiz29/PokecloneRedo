using System.Collections;
using UnityEngine;

public class VisualAltitudeController
{
    readonly WildPokemonController owner;
    readonly SpriteRenderer sprite;
    readonly VerticalPresenceProfile profile;

    float baseLocalY;
    float currentOffset;
    float neutralPhase;

    SpriteRenderer shadowRenderer;
    Coroutine altitudeRoutine;
    bool overrideActive;
    public bool IsMoving { get; set; }

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
    }

    public void Tick()
    {
        ApplyVisualOffset();
        UpdateShadow();
    }

    public void OnMoveProgress(float t01)
    {
        if (overrideActive)
            return;

        if (bias != InteractionPlaneBias.Neutral)
            return;

        if (profile.neutralMode == NeutralAltitudeMode.PerMoveCycle)
        {
            currentOffset = EvaluatePerMoveOffset(t01);
        }
        else // Continuous
        {
            neutralPhase += Time.deltaTime * profile.neutralBobSpeed;
            currentOffset = EvaluateContinuousOffset(neutralPhase);
        }
    }

    float EvaluatePerMoveOffset(float t)
    {
        float shaped = profile.perMoveCurve.Evaluate(t);

        return Mathf.Lerp(
            profile.offsetRange.x,
            profile.offsetRange.y,
            shaped
        );
    }

    float EvaluateContinuousOffset(float phase)
    {
        float mid = (profile.offsetRange.x + profile.offsetRange.y) * 0.5f;
        float amp = (profile.offsetRange.y - profile.offsetRange.x) * 0.5f;

        return mid + Mathf.Sin(phase) * amp;
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

        if (bias == InteractionPlaneBias.Neutral)
        {
            overrideActive = false;
            return;
        }

        float target = bias == InteractionPlaneBias.Converge
            ? 0f
            : GetDivergenceOffset();

        StartTransition(target, 0.25f);
    }

    public void StartTransition(float newTarget, float duration)
    {
        if (altitudeRoutine != null)
            owner.StopCoroutine(altitudeRoutine);

        altitudeRoutine = owner.StartCoroutine(AltitudeTransition(newTarget, duration));
    }

    IEnumerator AltitudeTransition(float newTarget, float duration)
    {
        overrideActive = true;

        float start = currentOffset;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            currentOffset = Mathf.Lerp(start, newTarget, t);

            ApplyVisualOffset();
            UpdateShadow();

            yield return null;
        }

        currentOffset = newTarget;
        overrideActive = false;
        altitudeRoutine = null;
    }

    public void ReanchorBaseHeight()
    {
        baseLocalY = sprite.transform.localPosition.y - currentOffset;
    }

    void ApplyVisualOffset()
    {
        if (!float.IsFinite(currentOffset))
        {
            Debug.LogWarning($"{owner.name}: currentOffset was NaN, resetting");
            currentOffset = 0f;
        }

        var pos = sprite.transform.localPosition;
        pos.y = baseLocalY + currentOffset;
        sprite.transform.localPosition = pos;
    }

    public float GetDivergenceOffset()
    {
        return profile.divergencePreference == DivergencePreference.TowardMaxPositive
            ? profile.offsetRange.y
            : profile.offsetRange.x;
    }

    void UpdateShadow()
    {
        if (shadowRenderer == null || profile == null)
            return;

        float maxOffset = Mathf.Max(Mathf.Abs(profile.offsetRange.x), Mathf.Abs(profile.offsetRange.y));

        if (maxOffset <= Mathf.Epsilon)
            return; // no altitude → no shadow scaling

        float t = Mathf.InverseLerp(0f, maxOffset, Mathf.Abs(currentOffset));


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
