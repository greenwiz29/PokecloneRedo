using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] bool canPassLedges = true;
    [SerializeField] float footAnchorOffset = 0.3f;

    public float moveSpeed;
    public bool IsMoving { get; private set; }
    public CharacterAnimator Animator => animator;
    public float OffsetY { get => footAnchorOffset; set => footAnchorOffset = value; }
    public Vector2 PreviousTile { get; private set; }

    CharacterAnimator animator;

    void Awake()
    {
        animator = GetComponent<CharacterAnimator>();
        SetPositionAndSnapToTile(transform.position);
    }

    public IEnumerator Move(Vector2 moveVector, Action OnMoveOver = null, bool checkCollisions = true, Action<float> onProgress = null)
    {
        PreviousTile = moveVector * new Vector2(-1, -1);

        animator.MoveX = Mathf.Clamp(moveVector.x, -1f, 1f);
        animator.MoveY = Mathf.Clamp(moveVector.y, -1f, 1f);

        var targetPos = transform.position;
        targetPos.x += moveVector.x;
        targetPos.y += moveVector.y;

        var scene = GameController.I.CurrentScene;
        if (scene != null)
        {
            var ledge = scene.GetLedgeAtWorldPos(targetPos);
            if (ledge != null)
            {
                if (canPassLedges && moveVector == (Vector2)ledge.allowedEntryDir)
                {
                    StartCoroutine(Jump(ledge.allowedEntryDir, ledge.jumpDistance));
                    yield break;
                }

                yield break; // blocked
            }

            if (checkCollisions && !IsPathClear(targetPos))
                yield break;

            if (animator.IsSurfing && !scene.GetWaterAtWorldPos(targetPos))
            {
                animator.IsSurfing = false;
            }

            IsMoving = true;

            float totalDist = Vector3.Distance(transform.position, targetPos);
            float traveled = 0f;
            Vector3 lastPos = transform.position;

            while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

                traveled += Vector3.Distance(transform.position, lastPos);
                lastPos = transform.position;

                onProgress?.Invoke(Mathf.Clamp01(traveled / totalDist));
                yield return null;
            }

            SetPositionAndSnapToTile(targetPos);
            IsMoving = false;

            OnMoveOver?.Invoke();
        }
    }

    public void HandleUpdate()
    {
        animator.IsMoving = IsMoving;
    }

    bool IsPathClear(Vector3 targetPos)
    {
        if (GameController.I.CurrentScene.IsSolidAtWorldPos(targetPos))
            return false;

        var diff = targetPos - transform.position;
        var dir = diff.normalized;

        var layers = GlobalSettings.I.InteractablesLayer | GlobalSettings.I.PlayerLayer;
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
        int tileX = Mathf.FloorToInt(pos.x);
        int tileY = Mathf.FloorToInt(pos.y - footAnchorOffset);

        transform.position = new Vector2(tileX + 0.5f, tileY + 0.5f + footAnchorOffset);
    }

    private IEnumerator Jump(Vector2 dir, int distance)
    {
        if (this == GameController.I.Player.Character)
            GameController.I.PauseGame(true);

        animator.IsJumping = true;

        var jumpDest = transform.position + (Vector3)(dir * distance);
        yield return transform
            .DOJump(jumpDest, 0.3f, 1, 0.5f)
            .WaitForCompletion();

        animator.IsJumping = false;

        if (this == GameController.I.Player.Character)
            GameController.I.PauseGame(false);
    }
}
