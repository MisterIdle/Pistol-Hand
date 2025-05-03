using UnityEngine;
using System.Collections.Generic;

public class RuleTiteApply : MonoBehaviour
{
    public RuleTileSet SpriteData;
    private SpriteRenderer _spriteRenderer;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        SetCenter();
    }

    public void SetCenter() => _spriteRenderer.sprite = SpriteData.GetOrDefault(SpriteData.CenterSprite);
    public void SetSingle() => _spriteRenderer.sprite = SpriteData.GetOrDefault(SpriteData.SingleSprite);

    public void UpdateSprite(Dictionary<Vector3, bool> neighbors)
    {
        bool up = neighbors[Vector3.up];
        bool down = neighbors[Vector3.down];
        bool left = neighbors[Vector3.left];
        bool right = neighbors[Vector3.right];

        int count = CountConnections(up, down, left, right);

        if (count == 0)
        {
            SetSingle();
            return;
        }

        if (count == 4)
        {
            SetCenter();
            return;
        }

        if (count == 1)
        {
            SetSingleFace(up, down, left, right);
            return;
        }

        if (count == 2)
        {
            if (IsDiagonalPair(up, down, left, right))
            {
                SetCorner(up, down, left, right);
                return;
            }

            if (IsOpposite(up, down, left, right))
            {
                SetOppositeFaces(up, down, left, right);
                return;
            }

            SetFaceMissing(up, down, left, right);
            return;
        }

        if (count == 3)
        {
            SetFaceMissing(up, down, left, right);
            return;
        }
    }

    private int CountConnections(bool up, bool down, bool left, bool right)
    {
        int count = 0;
        if (up) count++;
        if (down) count++;
        if (left) count++;
        if (right) count++;
        return count;
    }

    private bool IsDiagonalPair(bool up, bool down, bool left, bool right)
    {
        return (up && right && !down && !left) ||
               (up && left && !down && !right) ||
               (down && right && !up && !left) ||
               (down && left && !up && !right);
    }

    private bool IsOpposite(bool up, bool down, bool left, bool right)
    {
        return (up && down && !left && !right) || (left && right && !up && !down);
    }

    private void SetSingleFace(bool up, bool down, bool left, bool right)
    {
        if (down) _spriteRenderer.sprite = SpriteData.GetOrDefault(SpriteData.SingleTop);
        else if (up) _spriteRenderer.sprite = SpriteData.GetOrDefault(SpriteData.SingleBottom);
        else if (right) _spriteRenderer.sprite = SpriteData.GetOrDefault(SpriteData.SingleLeft);
        else if (left) _spriteRenderer.sprite = SpriteData.GetOrDefault(SpriteData.SingleRight);
    }

    private void SetOppositeFaces(bool up, bool down, bool left, bool right)
    {
        if (up && down) _spriteRenderer.sprite = SpriteData.GetOrDefault(SpriteData.VerticalOpen);
        else if (left && right) _spriteRenderer.sprite = SpriteData.GetOrDefault(SpriteData.HorizontalOpen);
    }

    private void SetFaceMissing(bool up, bool down, bool left, bool right)
    {
        if (!up) _spriteRenderer.sprite = SpriteData.GetOrDefault(SpriteData.Top);
        else if (!down) _spriteRenderer.sprite = SpriteData.GetOrDefault(SpriteData.Bottom);
        else if (!left) _spriteRenderer.sprite = SpriteData.GetOrDefault(SpriteData.Left);
        else if (!right) _spriteRenderer.sprite = SpriteData.GetOrDefault(SpriteData.Right);
    }

    private void SetCorner(bool up, bool down, bool left, bool right)
    {
        if (up && right) _spriteRenderer.sprite = SpriteData.GetOrDefault(SpriteData.BottomLeftCorner);
        else if (up && left) _spriteRenderer.sprite = SpriteData.GetOrDefault(SpriteData.BottomRightCorner);
        else if (down && right) _spriteRenderer.sprite = SpriteData.GetOrDefault(SpriteData.TopLeftCorner);
        else if (down && left) _spriteRenderer.sprite = SpriteData.GetOrDefault(SpriteData.TopRightCorner);
    }
}
