using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct StyleDefinition
{
    public string styleName;
    public GUIStyle style;
}

public class GUIStyleDefinition : ScriptableObject
{
    public List<StyleDefinition> styleDefinitions;
}
