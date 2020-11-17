using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Crosshair Template", menuName = "Crosshair Template", order = 0)]
public class CrosshairTemplate : ScriptableObject
{
    public Color color;

    [HideInInspector] public CrosshairElement[] elements = new CrosshairElement[0];

    private void OnValidate()
    {
        for (int i = 0; i < elements.Length; i++)
        {
            ClampAbove(ref elements[i].count, 1);
            //All these variables should be > 0
            ClampAbove0(ref elements[i].size);


            if (elements[i].spreadOffsetDirection.sqrMagnitude == 0)
            {
                elements[i].spreadOffsetDirection = new Vector2(0, 1);
            }
            elements[i].spreadOffsetDirection.Normalize();

        }
    }

    void ClampAbove0(ref float value)
    {
        value = value > 0 ? value : 0;
    }
    void ClampAbove0(ref int value) => ClampAbove(ref value, 0);
    void ClampAbove0(ref Vector2 value)
    {
        ClampAbove0(ref value.x);
        ClampAbove0(ref value.y);
    }

    void ClampAbove(ref int value, int above)
    {
        value = value > above ? value : above;
    }
}
