using System;
using System.Collections;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float moveSpeed;
    public bool IsMoving { get; private set; }
    public CharacterAnimator Animator => animator;
    public float OffsetY { get; private set; } = 0.3f;
    public Vector2 PreviousTile { get; private set; }

    CharacterAnimator animator;


    void Awake()
    {
        animator = GetComponent<CharacterAnimator>();
        SetPositionAndSnapToTile(transform.position);
    }

    public IEnumerator Move(Vector2 moveVector, Action OnMoveOver = null, bool checkCollisions = true)
    {
        PreviousTile = moveVector * new Vector2(-1, -1);

        animator.MoveX = Mathf.Clamp(moveVector.x, -1f, 1f);
        animator.MoveY = Mathf.Clamp(moveVector.y, -1f, 1f);

        var targetPos = transform.position;
        targetPos.x += moveVector.x;
        targetPos.y += moveVector.y;
        
        var ledge = CheckForLedge(targetPos);
        if (ledge != null)
        {
            if (ledge.TryToJump(this, moveVector))
            {
                yield break;
            }
        }

        if (checkCollisions && !IsPathClear(targetPos)) yield break;

        if(animator.IsSurfing && Physics2D.OverlapCircle(targetPos, 0.3f, GlobalSettings.I.WaterLayer) == null)
        {
            animator.IsSurfing = false;
        }

        IsMoving = true;
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        SetPositionAndSnapToTile(targetPos);
        IsMoving = false;

        OnMoveOver?.Invoke();
    }

    public void HandleUpdate()
    {
        animator.IsMoving = IsMoving;
    }

    bool IsPathClear(Vector3 targetPos)
    {
        var diff = targetPos - transform.position;
        var dir = diff.normalized;

        var layers = GlobalSettings.I.SolidObjectsLayer | GlobalSettings.I.InteractablesLayer | GlobalSettings.I.PlayerLayer;
        if (!animator.IsSurfing)
        {
            layers |= GlobalSettings.I.WaterLayer;
        }

        if (Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1, layers))
        {
            return false;
        }

        return true;
    }

    Ledge CheckForLedge(Vector3 targetPos)
    {
        var collider = Physics2D.OverlapCircle(targetPos, 0.15f, GlobalSettings.I.LedgesLayer);
        return collider?.GetComponent<Ledge>();
    }

    public void LookTowards(Vector3 targetPos)
    {
        var xDiff = Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x);
        var yDiff = Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y);

        if (xDiff == 0 || yDiff == 0)
        {

            animator.MoveX = Mathf.Clamp(xDiff, -1f, 1f);
            animator.MoveY = Mathf.Clamp(yDiff, -1f, 1f);

        }

    }

    public void SetPositionAndSnapToTile(Vector2 pos)
    {
        pos.x = Mathf.Floor(pos.x) + 0.5f;
        pos.y = Mathf.Floor(pos.y) + 0.5f + OffsetY;

        transform.position = pos;
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, GlobalSettings.I.SolidObjectsLayer | GlobalSettings.I.InteractablesLayer) != null)
        {
            return false;
        }
        return true;
    }

}
