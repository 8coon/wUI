using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public record WProgressStyle
{
    public Color color = Color.cyan;
    public float borderWidth = 2;
    public float borderPadding = 2;
}

public class WProgress : WWidget
{
    public float value = 0.3f;
    public WProgressStyle style;

    private WColorStyle colorStyle = new WColorStyle();

    protected override void OnInit(Rect rect)
    {
        base.OnInit(rect);
        colorStyle.GetGUIStyle(style.color);
    }

    protected override void OnUpdate(Rect borderRect)
    {
        base.OnUpdate(borderRect);

        var fillerRect = new Rect(borderRect);

        fillerRect.xMin += style.borderWidth + style.borderPadding;
        fillerRect.yMin += style.borderWidth + style.borderPadding;
        fillerRect.xMax -= style.borderWidth + style.borderPadding;
        fillerRect.yMax -= style.borderWidth + style.borderPadding;

        fillerRect.width *= Mathf.Clamp(value, 0, 1);

        // Drawing border
        var guiStyle = colorStyle.GetGUIStyle(style.color);
        RenderBox(borderRect, guiStyle, style.borderWidth);

        // Drawing filler
        GUI.Box(fillerRect, GUIContent.none, guiStyle);
    }
}
