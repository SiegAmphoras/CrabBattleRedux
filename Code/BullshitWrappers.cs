using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Xml;
using System;

[Serializable]
public class ScreenResolution
{
    public int width;
    public int height;
    public int refreshRate;
    public bool fullscreen;

    public static Resolution ToResolution(ScreenResolution sr) { return new Resolution() { width = sr.width, height = sr.height, refreshRate = sr.refreshRate }; }
    public static ScreenResolution ToScreenResolution(Resolution r) { return new ScreenResolution() { width = r.width, height = r.height, refreshRate = r.refreshRate }; }
}

[Serializable]
public class QualityLevel
{
    public int level;
}