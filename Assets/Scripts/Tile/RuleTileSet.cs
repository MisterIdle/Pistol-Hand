using UnityEngine;

[CreateAssetMenu(fileName = "Rule Tile GameObject", menuName = "MapEditor/Rule Tile GameObject", order = 0)]
public class RuleTileSet : ScriptableObject
{
    public Sprite CenterSprite;
    public Sprite SingleSprite;
    public Sprite DefaultSprite;

    [Header("Faces")]
    public Sprite Top;
    public Sprite Bottom;
    public Sprite Left;
    public Sprite Right;

    [Header("Single Faces")]
    public Sprite SingleTop;
    public Sprite SingleBottom;
    public Sprite SingleLeft;
    public Sprite SingleRight;

    [Header("Corners")]
    public Sprite TopLeftCorner;
    public Sprite TopRightCorner;
    public Sprite BottomLeftCorner;
    public Sprite BottomRightCorner;

    [Header("Opposite Faces")]
    public Sprite VerticalOpen;
    public Sprite HorizontalOpen;

    public Sprite GetOrDefault(Sprite sprite) => sprite != null ? sprite : DefaultSprite;
}
