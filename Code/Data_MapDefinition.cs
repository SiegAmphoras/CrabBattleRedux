using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MapDefinition
{
    public Texture mapPreview;
    public string MapName;
    public string SceneName;
    [TextArea()]
    public string Description;
}

public class Data_MapDefinition : ScriptableObject
{
    public List<MapDefinition> mapDefinitions;
}