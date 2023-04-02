using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;

public record WDialogCharacter
{
    public string name = "";
}

public record WDialogOption
{
    public string text = "";
    public string label = "";
}

public record WDialogScreen
{
    public string label = "";
    public WDialogCharacter character = new();
    public List<string> lines = new();
    public List<WDialogOption> options = new();

    public string GetText()
    {
        return string.Join("\n", lines);
    }
}

[System.Serializable]
public record WDialogStyle
{
    public Font font;
    public FontStyle fontStyle = FontStyle.Normal;
    public int fontSize = 24;

    public float borderWidth = 2;
    public float padding = 8;

    public Color backColor = new Color(0.2f, 0.2f, 0.2f, 0.64f);
    public Color borderColor = Color.black;
    public Color textColor = Color.white;
}

public class WDialogParser
{
    public List<WDialogScreen> screens = new();

    private WDialogScreen screen = new WDialogScreen();

    private void NewScreen()
    {
        screens.Add(screen);
        screen = new WDialogScreen();
    }

    private void TryNewScreen()
    {
        if ((screen.GetText().Length + screen.options.Count) > 0)
        {
            NewScreen();
        }
    }

    private bool TryParseLabel(string line)
    {
        if (Regex.IsMatch(line, "^\\w+:$"))
        {
            TryNewScreen();

            screen.label = line.Replace(":", "");

            return true;
        }

        return false;
    }

    private bool TryParseCharacterName(string line)
    {
        var gteIndex = line.IndexOf('>');

        if (gteIndex > -1)
        {
            TryNewScreen();

            screen.character.name = line.Substring(0, gteIndex).Trim();
            var text = line.Substring(gteIndex + 1).Trim();

            if (text.Length > 0)
            {
                screen.lines.Add(text);
            }

            return true;
        }

        return false;
    }

    private bool TryParseOption(string line)
    {
        if (Regex.IsMatch(line, "^\\[\\w+\\]:"))
        {
            var dotsIndex = line.IndexOf(':');
            var option = new WDialogOption();

            option.label = line.Substring(1, dotsIndex - 1);
            option.text = line.Substring(dotsIndex + 1).Trim();

            screen.options.Add(option);

            return true;
        }

        return false;
    }

    private bool TryParseEnd(string line)
    {
        if (line.StartsWith("end."))
        {
            TryNewScreen();

            screen.label = "end";

            NewScreen();

            return true;
        }

        return false;
    }

    private bool TryParseText(string line)
    {
        line = line.Trim();

        if (line.Length > 0)
        {
            TryNewScreen();

            if (screen.lines.Count == 0)
            {
                screen.lines.Add(line);
            }
            else
            {
                screen.lines[screen.lines.Count - 1] += " " + line;
            }
        }
        
        return true;
    }

    public void Parse(string source)
    {
        var lines = Regex.Split(source, "\r\n|\r|\n");

        screens = new();
        screen = new WDialogScreen();

        foreach (var line in lines)
        {
            if (!(
                TryParseLabel(line) ||
                TryParseCharacterName(line) ||
                TryParseOption(line) ||
                TryParseEnd(line) ||
                TryParseText(line)
            ))
            {
                throw new System.Exception("Failed to parse dialog");
            }
        }
    }
}

public class WDialog : WWidget
{
    public TextAsset asset;
    public WDialogStyle style;

    public UnityEvent onShow;
    public UnityEvent onLabelReached;
    public UnityEvent onHide;

    private WColorStyle backColorStyle = new WColorStyle();
    private WColorStyle borderColorStyle = new WColorStyle();
    private GUIStyle textStyle = new GUIStyle();

    private WDialogParser parser = new WDialogParser();
    private int currentScreen = -1;

    private float charTimeout = 0.05f;
    private float lastCharTs = 0;
    private float lastSkipTs = 0;
    private string currentText = "";
    private string targetText = "";

    public WDialogScreen GetCurrentScreen()
    {
        if (currentScreen == -1 || currentScreen >= parser.screens.Count)
        {
            return null;
        }

        return parser.screens[currentScreen];
    }

    public string GetCurrentLabel()
    {
        var screen = GetCurrentScreen();

        if (screen != null)
        {
            return screen.label;
        }

        return "";
    }

    public bool IsVisible()
    {
        return currentScreen > -1;
    }

    private int FindScreenIndex(string label)
    {
        if (label == "")
        {
            return -1;
        }

        for (int i = 0; i < parser.screens.Count; i++)
        {
            if (parser.screens[i].label == label)
            {
                return i;
            }
        }

        return -1;
    }

    private void GotoScreen(int index)
    {
        if (currentScreen == index || index >= parser.screens.Count || index == -1)
        {
            return;
        }

        if (parser.screens[index].label == "end")
        {
            currentScreen = index;
            Hide();
            return;
        }

        if (currentScreen == -1)
        {
            currentScreen = index;
            onShow.Invoke();
        }
        else
        {
            currentScreen = index;
        
            if (GetCurrentLabel() != "")
            {
                onLabelReached.Invoke();
            }
        }

        var screen = GetCurrentScreen();

        if (screen != null)
        {
            currentText = "";
            targetText = screen.GetText();
        }

        Debug.Log("[WDialog] Goto screen " + currentScreen);
    }

    public void Show(string label)
    {
        GotoScreen(FindScreenIndex(label));
    }

    public void Show()
    {
        GotoScreen(0);
    }

    public void Hide()
    {
        currentScreen = -1;
        onHide.Invoke();
    }

    protected override void OnInit(Rect rect)
    {
        base.OnInit(rect);

        if (asset)
        {
            parser.Parse(asset.text);
        }
    }

    private bool TrySkip()
    {
        if (
            Time.realtimeSinceStartup >= lastSkipTs + .2f &&
            (Input.GetKeyUp("enter") || Input.GetKeyUp("space"))
        )
        {
            lastSkipTs = Time.realtimeSinceStartup;

            if (currentText.Length < targetText.Length)
            {
                currentText = targetText;
            }
            else
            {
                GotoScreen(currentScreen + 1);
            }

            return true;
        }

        return false;
    }

    protected override void OnUpdate(Rect rect)
    {
        base.OnUpdate(rect);

        var screen = GetCurrentScreen();

        if (screen == null)
        {
            return;
        }

        var backStyle = backColorStyle.GetGUIStyle(style.backColor);
        var borderStyle = borderColorStyle.GetGUIStyle(style.borderColor);

        textStyle.font = style.font;
        textStyle.fontSize = style.fontSize;
        textStyle.fontStyle = style.fontStyle;
        textStyle.normal.textColor = style.textColor;
        textStyle.wordWrap = true;

        GUI.Box(rect, GUIContent.none, backStyle);

        var characterRect = new Rect(rect.x, rect.y, rect.height, rect.height);
        RenderBox(characterRect, borderStyle, style.borderWidth);

        var textRect = new Rect(
            rect.x + rect.height - style.borderWidth,
            rect.y,
            rect.width - rect.height + style.borderWidth,
            rect.height
        );

        RenderBox(textRect, borderStyle, style.borderWidth);

        textRect.xMin += style.padding + style.borderWidth;
        textRect.yMin += style.padding + style.borderWidth;
        textRect.xMax -= style.padding + style.borderWidth;
        textRect.yMax -= style.padding + style.borderWidth;

        textStyle.fixedWidth = textRect.width;

        if (
            !TrySkip() &&
            currentText.Length < targetText.Length &&
            Time.realtimeSinceStartup >= lastCharTs + charTimeout
        )
        {
            lastCharTs = Time.realtimeSinceStartup;
            currentText += targetText.Substring(currentText.Length, 1);
        }

        var textContent = new GUIContent(currentText);
        //var textSize = guiStyle.CalcHeight(textContent, size.width);

        //Debug.Log(textSize);

        GUI.Label(textRect, textContent, textStyle);
    }
}
