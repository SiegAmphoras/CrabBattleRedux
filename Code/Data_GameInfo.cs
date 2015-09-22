using UnityEngine;
using System.Collections;

[System.Serializable]
public class Version
{
    public int Major;
    public int Minor;
    public int Build;
    public int Revision;

    public Version()
    {
        Major = 0;
        Minor = 0;
        Build = 0;
        Revision = 0;
    }

    public void Set(string version)
    {
        string[] s = version.Split('.');

        Major = int.Parse(s[0]);
        Minor = int.Parse(s[1]);
        Build = int.Parse(s[2]);
        Revision = int.Parse(s[3]);
    }

    public void IncrementRevision()
    {
        Revision++;

        if (Revision > 999)
        {
            Build++;
            Revision = 0;
        }
    }

    public override string ToString()
    {
        return string.Format("{0}.{1}.{2}.{3}", Major, Minor, Build, Revision);
    }
}

[System.Serializable]
public class Data_GameInfo : ScriptableObject
{
    public Version BuildVersion;

    [TextArea()]
    public string CreditList;
}
