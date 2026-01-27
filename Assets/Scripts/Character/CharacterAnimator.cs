using System.Collections.Generic;
using UnityEngine;

public enum FacingDirection { Left, Right, Up, Down }

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] List<Sprite> walkDownSprites, walkUpSprites, walkLeftSprites, walkRightSprites;
    [SerializeField] List<Sprite> surfDownSprites, surfUpSprites, surfLeftSprites, surfRightSprites;
    [SerializeField] FacingDirection defaultDirection = FacingDirection.Down;

    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool IsMoving { get; set; }
    public bool IsJumping { get; set; }
    public bool IsSurfing { get; set; }
    public FacingDirection DefaultDirection => defaultDirection;
    bool wasMoving;

    //States
    SpriteAnimator walkDownAnim, walkUpAnim, walkLeftAnim, walkRightAnim;
    SpriteAnimator surfDownAnim, surfUpAnim, surfLeftAnim, surfRightAnim;

    SpriteAnimator currentAnim;

    SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        walkDownAnim = new SpriteAnimator(walkDownSprites, spriteRenderer);
        walkUpAnim = new SpriteAnimator(walkUpSprites, spriteRenderer);
        walkLeftAnim = new SpriteAnimator(walkLeftSprites, spriteRenderer);
        walkRightAnim = new SpriteAnimator(walkRightSprites, spriteRenderer);

        surfDownAnim = new SpriteAnimator(surfDownSprites, spriteRenderer);
        surfUpAnim = new SpriteAnimator(surfUpSprites, spriteRenderer);
        surfLeftAnim = new SpriteAnimator(surfLeftSprites, spriteRenderer);
        surfRightAnim = new SpriteAnimator(surfRightSprites, spriteRenderer);
        SetFacingDirection(defaultDirection);

        currentAnim = walkDownAnim;
    }

    void Update()
    {
        var prevAnim = currentAnim;

        if (MoveX == 1)
        {
            currentAnim = IsSurfing ? surfRightAnim : walkRightAnim;
        }
        else if (MoveX == -1)
        {
            currentAnim = IsSurfing ? surfLeftAnim : walkLeftAnim;
        }
        else if (MoveY == 1)
        {
            currentAnim = IsSurfing ? surfUpAnim : walkUpAnim;
        }
        else if (MoveY == -1)
        {
            currentAnim = IsSurfing ? surfDownAnim : walkDownAnim;
        }

        if (currentAnim != prevAnim || IsMoving != wasMoving)
            currentAnim.Start();

        if (IsJumping)
        {
            spriteRenderer.sprite = currentAnim.Frames[^1];
        }
        else if (IsMoving)
            currentAnim.HandleUpdate();
        else
            spriteRenderer.sprite = currentAnim.Frames[0];

        wasMoving = IsMoving;
    }

    public void SetWalkingDownSprites(List<Sprite> sprites)
    {
        walkDownSprites = sprites;
        walkDownAnim = new SpriteAnimator(walkDownSprites, spriteRenderer);
    }

    public void SetWalkingUpSprites(List<Sprite> sprites)
    {
        walkUpSprites = sprites;
        walkUpAnim = new SpriteAnimator(walkUpSprites, spriteRenderer);
    }

    public void SetWalkingLeftSprites(List<Sprite> sprites)
    {
        walkLeftSprites = sprites;
        walkLeftAnim = new SpriteAnimator(walkLeftSprites, spriteRenderer);
    }

    public void SetWalkingRightSprites(List<Sprite> sprites)
    {
        walkRightSprites = sprites;
        walkRightAnim = new SpriteAnimator(walkRightSprites, spriteRenderer);
    }

    public void SetFacingDirection(FacingDirection dir)
    {
        MoveX = 0;
        MoveY = 0;
        
        switch (dir)
        {
            case FacingDirection.Right:
                MoveX = 1;
                break;
            case FacingDirection.Left:
                MoveX = -1;
                break;
            case FacingDirection.Up:
                MoveY = 1;
                break;
            case FacingDirection.Down:
                MoveY = -1;
                break;
            default:
                break;
        }
    }

    public Vector2 GetFacingDirection()
    {
        return new Vector2(MoveX, MoveY);
    }
}
