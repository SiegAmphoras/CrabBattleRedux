using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class GUIExtenders
{
    public static void SwitchButtons(Rect position, string text, ref int switchIndex, int indexMax, GUIStyle backgroundStyle, GUIStyle buttonStyle)
    {
        GUI.Label(position, text, backgroundStyle);

        if (GUI.Button(new Rect(position.xMin, position.yMin, 32, 32), "<", buttonStyle))
        {
            switchIndex -= 1;

            if (switchIndex < 0)
                switchIndex = indexMax;
        }
        else if (GUI.Button(new Rect(position.xMax - 32, position.yMin, 32, 32), ">", buttonStyle))
        {
            switchIndex += 1;

            if (switchIndex > indexMax)
                switchIndex = 0;
        }
    }
}
