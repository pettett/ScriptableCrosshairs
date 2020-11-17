using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Vector2Extension
{
    /// <summary>
    /// Rotate a vector2 about the origin by degrees degrees (not radians)
    /// </summary>
    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
}

public class CrosshairController : MonoBehaviour
{
    public CrosshairTemplate t;
    public static CrosshairController singleton;
    public RectTransform[][] crosshairParts;
    public float spread;
    /// <summary> Quick global color adjuster
    /// This will be the color of elements without overrideColor set to true, and is based off the value in the CrosshairTemplate t
    /// </summary>
    [HideInInspector] public Color color;
    /// <summary> The list of active elements. Stored in a separate array so changes to the crosshair are not
    /// If you want to directly edit the template, use a reference to the CrosshairTemplate class
    /// </summary>
    [HideInInspector] public CrosshairElement[] elements;

    /// <summary>
    /// Set the spread of the controller singleton
    /// </summary>
    public static void SetSpread(float spread)
    {
        singleton.spread = spread;
        //update the layout of the crosshair parts
        singleton.UpdateCrosshairParts();
    }
    /// <summary>
    /// Get the horizontal FOV of the camera
    /// </summary>
    float HorizontalFOV()
    {

        Camera cam = Camera.main;
        var radAngle = cam.fieldOfView * Mathf.Deg2Rad;
        var radHFOV = 2 * Mathf.Atan(Mathf.Tan(radAngle / 2) * cam.aspect);
        return Mathf.Rad2Deg * radHFOV;
    }
    /// <summary>
    /// Create a vector2 based on effectAxis
    /// </summary>
    public static Vector2 MaskFromEffects(EffectAxis e,float enabled=1, float disabled = 0)
    {
        return new Vector2(e.HasFlag(EffectAxis.X) ? enabled : disabled, e.HasFlag(EffectAxis.Y) ? enabled : disabled);
    }
    public void UpdateCrosshairParts()
    {
        //Find the number of canvas units that corralate to 1 degree of change
        float unitsPerDegree = GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta.x / HorizontalFOV();

        for (int i = 0; i < elements.Length; i++)
        {
            //Calculate the desired scale of this element, including current spead
            Vector2 scale = elements[i].size;

            if (elements[i].scaleWithSpread.HasFlag(EffectAxis.X))
                scale.x *= spread * unitsPerDegree;
            if (elements[i].scaleWithSpread.HasFlag(EffectAxis.Y))
                scale.y *= spread * unitsPerDegree;

            Vector2 c = elements[i].offset;


            //make sure to include the spread scaling in the offset
            c += MaskFromEffects(elements[i].offsetFromCenter) * scale.y * 0.5f;


            if (elements[i].offsetWithSpread)
            {
                c += elements[i].spreadOffsetDirection * spread * unitsPerDegree;
            }

            float directionRotation = -elements[i].orbitStartAngle;

            float angleIncrease = elements[i].orbitSize / elements[i].count;
            //calculate the position from center screen
            for (int j = 0; j < elements[i].count; j++)
            {
                directionRotation += angleIncrease;

                crosshairParts[i][j].anchoredPosition = c.Rotate(directionRotation);
                //z rotation is rotation on 2d plane
                crosshairParts[i][j].rotation = Quaternion.Euler(0, 0, directionRotation - elements[i].rotationOffset);

                //if scaling with spread, 1 unit of size represents scaling linearly with spread


                crosshairParts[i][j].sizeDelta = scale;

                //if the element is a default image, set it's color and sprite
                if (t.elements[i].customPrefab == null)
                {
                    crosshairParts[i][j].GetComponent<Image>().color = elements[i].overrideColor ? elements[i].color : color;
                    crosshairParts[i][j].GetComponent<Image>().sprite = elements[i].sprite;
                }

            }
        }
    }

    public T GetComponentInCrosshair<T>() where T : MonoBehaviour
    {
        T t = null;
        for (int i = 0; i < crosshairParts.Length; i++)
        {
            t = GetComponentInCrosshairLayer<T>(i);
            if (t != null) break;
        }
        return t;
    }

    public T GetComponentInCrosshairLayer<T>(int layer) where T : MonoBehaviour
    {
        for (int j = 0; j < crosshairParts[layer].Length; j++)
        {
            if (crosshairParts[layer][j].TryGetComponent(out T comp)) return comp;
        }
        return null;
    }

    private void Awake()
    {
        singleton = this;
    }
    ///<summary>Call this to reset the CrosshairElements array to the values in the crosshair template t</summary>
    public void SyncElementsToTemplate()
    {
        elements = t.elements;
        color = t.color;
    }

    private void Start()
    {
        SyncElementsToTemplate();

        crosshairParts = new RectTransform[t.elements.Length][];

        for (int i = 0; i < t.elements.Length; i++)
        {

            //create empty array for the crosshair parts
            crosshairParts[i] = new RectTransform[t.elements[i].count];

            for (int j = 0; j < t.elements[i].count; j++)
            {
                //create a new crosshair part
                GameObject go;
                if (t.elements[i].customPrefab != null)
                {
                    go = Instantiate(t.elements[i].customPrefab);
                }
                else
                {
                    //Create the crosshair element from scratch
                    go = new GameObject("Crosshair part", typeof(RectTransform), typeof(Image));

                    go.GetComponent<Image>().color = t.elements[i].overrideColor ? t.elements[i].color : t.color;
                    go.GetComponent<Image>().sprite = t.elements[i].sprite;
                }

                var rect = go.GetComponent<RectTransform>();
                rect.SetParent(transform, false);

                crosshairParts[i][j] = rect;
            }
        }
        UpdateCrosshairParts();
    }
}
