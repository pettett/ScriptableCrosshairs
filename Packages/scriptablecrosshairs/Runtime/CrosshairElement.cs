using UnityEngine;
[System.Flags]
public enum EffectAxis
{
    None = 0,
    X = 1,
    Y = 2

}
[System.Serializable]
public struct CrosshairElement
{
    public Vector2 size;
    public EffectAxis offsetFromCenter;
    [Tooltip("If true, the offset value will be normalized and reprosent the direction the element will move with spread (effected by fov)")]
    public bool offsetWithSpread;
    public EffectAxis scaleWithSpread;
    public Vector2 spreadOffsetDirection;
    public Vector2 offset;
    public bool overrideColor;
    public Color color;
    public int count;
    [Range(0, 360)]
    public float orbitSize;
    [Range(-360, 360)]
    public float orbitStartAngle;
    [Range(0, 360)]
    public float rotationOffset;
    public Sprite sprite;
    public GameObject customPrefab;
}