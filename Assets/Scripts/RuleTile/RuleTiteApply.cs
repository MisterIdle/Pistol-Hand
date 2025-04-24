using UnityEngine;
using System.Collections.Generic;

public class RuleTiteApply : MonoBehaviour
{
    public RuleTileSet spriteData;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetCenter();
    }

    public void SetCenter() => spriteRenderer.sprite = spriteData.GetOrDefault(spriteData.centerSprite);
    public void SetSingle() => spriteRenderer.sprite = spriteData.GetOrDefault(spriteData.singleSprite);

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
        if (down) spriteRenderer.sprite = spriteData.GetOrDefault(spriteData.singleTop);
        else if (up) spriteRenderer.sprite = spriteData.GetOrDefault(spriteData.singleBottom);
        else if (right) spriteRenderer.sprite = spriteData.GetOrDefault(spriteData.singleLeft);
        else if (left) spriteRenderer.sprite = spriteData.GetOrDefault(spriteData.singleRight);
    }

    private void SetOppositeFaces(bool up, bool down, bool left, bool right)
    {
        if (up && down) spriteRenderer.sprite = spriteData.GetOrDefault(spriteData.verticalOpen);
        else if (left && right) spriteRenderer.sprite = spriteData.GetOrDefault(spriteData.horizontalOpen);
    }

    private void SetFaceMissing(bool up, bool down, bool left, bool right)
    {
        if (!up) spriteRenderer.sprite = spriteData.GetOrDefault(spriteData.top);
        else if (!down) spriteRenderer.sprite = spriteData.GetOrDefault(spriteData.bottom);
        else if (!left) spriteRenderer.sprite = spriteData.GetOrDefault(spriteData.left);
        else if (!right) spriteRenderer.sprite = spriteData.GetOrDefault(spriteData.right);
    }

    private void SetCorner(bool up, bool down, bool left, bool right)
    {
        if (up && right) spriteRenderer.sprite = spriteData.GetOrDefault(spriteData.BottomLeftCorner);
        else if (up && left) spriteRenderer.sprite = spriteData.GetOrDefault(spriteData.BottomRightCorner);
        else if (down && right) spriteRenderer.sprite = spriteData.GetOrDefault(spriteData.TopLeftCorner);
        else if (down && left) spriteRenderer.sprite = spriteData.GetOrDefault(spriteData.TopRightCorner);
    }
}
