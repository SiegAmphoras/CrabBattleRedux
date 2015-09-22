using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.IO;
using System;

public class ConfigEntry
{
    [XmlAttribute("Index")]
    public string Index;
    [XmlAttribute("EntryName")]
    public string Name;
    public System.Object Value;
}

public class ConfigCollection
{
    [XmlArray("ConfigValues")]
    public List<ConfigEntry> configValues;

    public ConfigCollection()
    {
        configValues = new List<ConfigEntry>();
    }
}

public class GameConfigs
{
    static string configsPath = Application.dataPath + "/configs.cfg";
    List<ConfigEntry> configValues;
    List<ConfigEntry> lastConfigValues;

    public GameConfigs()
    {
        configValues = new List<ConfigEntry>();
        lastConfigValues = new List<ConfigEntry>();
    }

    public bool ConfigFileExists() { return File.Exists(configsPath); }

    public void SaveConfigs()
    {
        FileStream fs;

        if (!ConfigFileExists())
        {
            fs = File.Create(configsPath);
        }
        else
        {
            File.WriteAllText(configsPath, string.Empty);
            fs = File.OpenWrite(configsPath);
        }

        ConfigCollection c = new ConfigCollection();
        ConfigEntry[] entries = new ConfigEntry[configValues.Count];
        configValues.CopyTo(entries);
        c.configValues.AddRange(entries);

        XmlSerializer xml = new XmlSerializer(typeof(ConfigCollection), new Type[] {typeof(ScreenResolution), typeof(QualityLevel)});
        using (TextWriter writer = new StreamWriter(fs))
        {
            xml.Serialize(writer, c);
        }

        lastConfigValues = new List<ConfigEntry>();
        lastConfigValues.AddRange(entries);
    }

    public void LoadConfigs()
    {
        FileStream fs;

        if (!ConfigFileExists())
        {
            fs = File.Create(configsPath);
            fs.Close();
            return;
        }
        else
        {
            fs = File.OpenRead(configsPath);
        }
        ConfigCollection c = new ConfigCollection();

        XmlSerializer xml = new XmlSerializer(typeof(ConfigCollection), new Type[] { typeof(ScreenResolution), typeof(QualityLevel) });
        using (TextReader reader = new StreamReader(fs))
        {
            c = (ConfigCollection)xml.Deserialize(reader);
        }

        configValues.AddRange(c.configValues);
        lastConfigValues.AddRange(c.configValues);
        c = null;
    }

    public T GetConfigEntry<T>(string EntryIndex, string EntryName, T DefaultVal) where T : class 
    {
        if (!configValues.Exists(i => i.Index == EntryIndex && i.Name == EntryName))
        {
            configValues.Add(new ConfigEntry() { Index = EntryIndex, Name = EntryName, Value = DefaultVal });
            return DefaultVal;
        }
        else
        {
            ConfigEntry e = configValues.Find(i => i.Index == EntryIndex && i.Name == EntryName);

            return (e.Value as T);
        }
    }

    public void SetConfigEntry<T>(string EntryIndex, string EntryName, T newValue) where T : class 
    {
        if (!configValues.Exists(i => i.Index == EntryIndex && i.Name == EntryName))
        {
            configValues.Add(new ConfigEntry() { Index = EntryIndex, Name = EntryName, Value = newValue });
            return;
        }
        else
        {
            //ConfigEntry e = configValues.Find(i => i.Index == EntryIndex && i.Name == EntryName);
            configValues[configValues.IndexOf(configValues.Find(i => i.Index == EntryIndex && i.Name == EntryName))].Value = newValue;
        }
    }

    public T GetConfigEntry<T>(string EntryIndex, string EntryName) where T : class 
    {
        ConfigEntry e = configValues.Find(i => i.Index == EntryIndex && i.Name == EntryName);

        if (e == null)
            return null;

        return (e.Value as T);
    }
}
