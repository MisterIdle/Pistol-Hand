using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Rule Tile GameObject", menuName = "MapEditor/Rule Tile GameObject", order = 0)]
public class RuleTileSet : ScriptableObject
{
    public Sprite centerSprite;
    public Sprite singleSprite;
    public Sprite defaultSprite;

    [Header("Faces")]
    public Sprite top;
    public Sprite bottom;
    public Sprite left;
    public Sprite right;

    [Header("Single Faces")]
    public Sprite singleTop;
    public Sprite singleBottom;
    public Sprite singleLeft;
    public Sprite singleRight;

    [Header("Corners")]
    public Sprite TopLeftCorner;
    public Sprite TopRightCorner;
    public Sprite BottomLeftCorner;
    public Sprite BottomRightCorner;

    [Header("Opposite Faces")]
    public Sprite verticalOpen;
    public Sprite horizontalOpen;

    public Sprite GetOrDefault(Sprite sprite) => sprite != null ? sprite : defaultSprite;
}
