using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(CrosshairTemplate))]
public class CrosshairTemplateEditor : Editor
{
    //This doesnt seem to work sometimes
    public override bool RequiresConstantRepaint() => true;

    private SerializedProperty _property;
    private ReorderableList _list;
    float margin = 5;
    Vector2 scale = new Vector2(1, -1);
    private void OnEnable()
    {
        _property = serializedObject.FindProperty("elements");
        _list = new ReorderableList(serializedObject, _property, true, true, true, true)
        {
            drawHeaderCallback = DrawListHeader,
            drawElementCallback = DrawListElement,
        };
        _list.elementHeightCallback += ElementHeight;

    }

    private void DrawListHeader(Rect rect)
    {
        GUI.Label(rect, "Elements");
    }
    private float ElementHeight(int index)
    {
        var item = _property.GetArrayElementAtIndex(index);
        float height = EditorGUIUtility.singleLineHeight * 14 + margin;
        if (!item.FindPropertyRelative("overrideColor").boolValue)
        {
            height -= EditorGUIUtility.singleLineHeight;
        }
        if (!item.FindPropertyRelative("offsetWithSpread").boolValue)
        {
            height -= EditorGUIUtility.singleLineHeight;
        }

        return height;
    }
    private void DrawListElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        var item = _property.GetArrayElementAtIndex(index);
        Rect itemRect = new Rect(rect.x, rect.y + margin * 0.5f, rect.width, EditorGUIUtility.singleLineHeight);


        EditorGUI.PropertyField(itemRect, item.FindPropertyRelative("size"));
        itemRect.y += EditorGUIUtility.singleLineHeight;

        EditorGUI.PropertyField(itemRect, item.FindPropertyRelative("offsetFromCenter"));
        itemRect.y += EditorGUIUtility.singleLineHeight;

        EditorGUI.PropertyField(itemRect, item.FindPropertyRelative("scaleWithSpread"));
        itemRect.y += EditorGUIUtility.singleLineHeight;

        var ows = item.FindPropertyRelative("offsetWithSpread");
        EditorGUI.PropertyField(itemRect, ows);
        itemRect.y += EditorGUIUtility.singleLineHeight;


        if (ows.boolValue)
        {
            EditorGUI.PropertyField(itemRect, item.FindPropertyRelative("spreadOffsetDirection"));
            itemRect.y += EditorGUIUtility.singleLineHeight;
        }

        EditorGUI.PropertyField(itemRect, item.FindPropertyRelative("offset"));
        itemRect.y += EditorGUIUtility.singleLineHeight;

        var oc = item.FindPropertyRelative("overrideColor");
        EditorGUI.PropertyField(itemRect, oc);
        itemRect.y += EditorGUIUtility.singleLineHeight;

        if (oc.boolValue)
        {
            EditorGUI.PropertyField(itemRect, item.FindPropertyRelative("color"));
            itemRect.y += EditorGUIUtility.singleLineHeight;
        }


        EditorGUI.PropertyField(itemRect, item.FindPropertyRelative("count"));
        itemRect.y += EditorGUIUtility.singleLineHeight;

        EditorGUI.PropertyField(itemRect, item.FindPropertyRelative("orbitSize"));
        itemRect.y += EditorGUIUtility.singleLineHeight;

        EditorGUI.PropertyField(itemRect, item.FindPropertyRelative("orbitStartAngle"));
        itemRect.y += EditorGUIUtility.singleLineHeight;

        EditorGUI.PropertyField(itemRect, item.FindPropertyRelative("rotationOffset"));
        itemRect.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(itemRect, item.FindPropertyRelative("sprite"));
        itemRect.y += EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(itemRect, item.FindPropertyRelative("customPrefab"));
        itemRect.y += EditorGUIUtility.singleLineHeight;


    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUILayout.Space();
        _list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();

        //draw on the crosshair in the template
        CrosshairTemplate t = (CrosshairTemplate)target;

        Rect totalRect = GUILayoutUtility.GetAspectRect(1, GUILayout.MaxWidth(300), GUILayout.MaxHeight(300));
        //Get a quick reference to the center
        Vector2 center = totalRect.center;

        foreach (var e in t.elements)
        {
            //draw the surrounding lines only if there are more than 0
            if (e.count != 0)
            {
                float movement = e.orbitStartAngle;

                //Draw the lines
                float angleIncrease = e.orbitSize / e.count;
                Color col = e.overrideColor ? e.color : t.color;
                for (int i = 0; i < e.count; i++)
                {
                    movement += angleIncrease;
                    //GUIUtility.RotateAroundPivot(movement, center);

                    float fakeSpread = (Mathf.Sin(Time.time) + 2);
                    Vector2 size = e.size;

                    if (e.scaleWithSpread.HasFlag(EffectAxis.X))
                        size.x *= fakeSpread * 10;
                    if (e.scaleWithSpread.HasFlag(EffectAxis.Y))
                        size.y *= fakeSpread * 10;


                    //more positive means more down in this viewer
                    //                                      simulate scaling by horizontal fov
                    var mid = center + scale * (e.offset + (e.offsetWithSpread ? e.spreadOffsetDirection * fakeSpread * 10 : Vector2.zero));
                    //apply center offset
                    mid += new Vector2(1, -1) * CrosshairController.MaskFromEffects(e.offsetFromCenter) * size * 0.5f;

                    mid = (mid - center).Rotate(movement) + center;

                    GUIUtility.RotateAroundPivot(e.rotationOffset + movement, mid);


                    if (e.sprite != null)
                    {
                        GUI.color = col;
                        Rect rect = e.sprite.rect;
                        //support drawing sprites that are not the entire texture
                        rect.x /= e.sprite.texture.width;
                        rect.y /= e.sprite.texture.height;
                        rect.width /= e.sprite.texture.width;
                        rect.height /= e.sprite.texture.height;
                        GUI.DrawTextureWithTexCoords(RectAround(mid, size), e.sprite.texture, rect);
                        GUI.color = Color.white;
                    }
                    else
                    {
                        //draw the normal square
                        EditorGUI.DrawRect(RectAround(mid, size), col);
                    }

                    //reset the rotation back to normal
                    GUIUtility.RotateAroundPivot(-e.rotationOffset - movement, mid);


                    //GUIUtility.RotateAroundPivot(-movement, center);
                }
            }
        }
    }
    Rect RectAround(Vector2 center, Vector2 size) => new Rect(center - size * 0.5f, size);
}