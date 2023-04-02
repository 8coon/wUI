using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public record WSnap
{
    public bool top;
    public bool right;
    public bool bottom;
    public bool left;
}

[System.Serializable]
public record WPadding
{
    public float top = 0;
    public float right = 0;
    public float bottom = 0;
    public float left = 0;
}

[System.Serializable]
public record WSize
{
    public float height = 40;
    public float width = 200;
}

public class WColorStyle
{
    private Color cachedColor = Color.clear;
    private GUIStyle guiStyle;
    private Texture2D texture;

    public GUIStyle GetGUIStyle(Color color)
    {
        if(!cachedColor.Equals(color) || guiStyle == null)
        {
            texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();

            guiStyle = new GUIStyle();
            guiStyle.normal.background = texture;

            cachedColor = color;
        }

        return guiStyle;
    }
}

public class WWidget : MonoBehaviour
{
    public WSnap snap;
    public WPadding padding;
    public WSize size;

    public static void RenderBox(Rect rect, GUIStyle borderStyle, float borderWidth)
    {
        //  top
        GUI.Box(new Rect(
            rect.xMin,
            rect.yMin,
            rect.width,
            borderWidth
        ), GUIContent.none, borderStyle);

        //  right
        GUI.Box(new Rect(
            rect.xMax - borderWidth,
            rect.yMin,
            borderWidth,
            rect.height
        ), GUIContent.none, borderStyle);

        //  bottom
        GUI.Box(new Rect(
            rect.xMin,
            rect.yMax - borderWidth,
            rect.width,
            borderWidth
        ), GUIContent.none, borderStyle);

        //  left
        GUI.Box(new Rect(
            rect.xMin,
            rect.yMin,
            borderWidth,
            rect.height
        ), GUIContent.none, borderStyle);
    }

    protected Rect GetRect()
    {
        var rect = new Rect(
            Screen.width / 2 - size.width / 2,
            Screen.height / 2 - size.height / 2,
            size.width,
            size.height
        );

        if (snap.left && snap.right)
        {
            rect.xMin = padding.left;
            rect.xMax = Screen.width - padding.right;
        }
        else if (snap.left)
        {
            rect.xMin = padding.left;
            rect.xMax = padding.left + size.width;
        }
        else if (snap.right)
        {
            rect.xMin = Screen.width - padding.right - size.width;
            rect.xMax = Screen.width - padding.right;
        }

        if (snap.top && snap.bottom)
        {
            rect.yMin = padding.top;
            rect.yMax = Screen.height - padding.bottom;
        } else if (snap.top)
        {
            rect.yMin = padding.top;
            rect.yMax = padding.top + size.height;
        } else if (snap.bottom)
        {
            rect.yMin = Screen.height - padding.bottom - size.height;
            rect.yMax = Screen.height - padding.bottom;
        }

        return rect;
    }

    protected virtual void OnInit(Rect rect) {}
    protected virtual void OnUpdate(Rect rect) {}

    void Start()
    {
        OnInit(GetRect());
    }

    void OnGUI()
    {
        OnUpdate(GetRect());
    }
}
