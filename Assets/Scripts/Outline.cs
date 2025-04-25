using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Outline", 14)]
public class Outline : BaseMeshEffect
{
    public Color effectColor = Color.black;
    public float thickness = 1f;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || thickness <= 0)
            return;

        List<UIVertex> originalVerts = new List<UIVertex>();
        vh.GetUIVertexStream(originalVerts);

        int count = originalVerts.Count;
        List<UIVertex> newVerts = new List<UIVertex>();

        Vector2[] directions = new Vector2[]
        {
            new Vector2(-1, 0),
            new Vector2(1, 0),
            new Vector2(0, -1),
            new Vector2(0, 1)
        };

        foreach (var dir in directions)
        {
            for (int i = 0; i < count; i++)
            {
                UIVertex vt = originalVerts[i];
                vt.position += (Vector3)(dir * thickness);
                vt.color = effectColor;
                newVerts.Add(vt);
            }
        }

        newVerts.AddRange(originalVerts);

        vh.Clear();
        vh.AddUIVertexTriangleStream(newVerts);
    }
}
