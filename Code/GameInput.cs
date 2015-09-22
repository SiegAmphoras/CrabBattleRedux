using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

public class GameInput
{
    static string configsPath = Application.dataPath + "/inputs.cfg";

    static private KeyCode[] validKeyCodes;

    bool capturingInput = false;
    string capturingName = "";
    KeyCode capturedKey;

    InputDefinitions defs;

    public GameInput()
    {
        AssembleValidKeyCodes();

        defs = new InputDefinitions();
        defs.Init();
    }

    private void AssembleValidKeyCodes()
    {
        if (validKeyCodes != null) return;
        validKeyCodes = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
    }

    public void Update()
    {
        if (capturingInput)
        {
            KeyCode k = FetchKey();

            if (k != KeyCode.None)
            {
                if (k == KeyCode.Escape)
                {
                    capturingInput = false;
                    capturedKey = KeyCode.None;
                }
                else
                {
                    capturedKey = k;
                    defs[capturingName] = k;
                    capturingInput = false;

                    SaveInputs();
                }
            }
        }
    }

    KeyCode FetchKey()
    {
        int[] e = (int[])System.Enum.GetValues(typeof(KeyCode));

        foreach(int i in e)
        {
            if (Input.GetKey((KeyCode)i))
            {
                return (KeyCode)i;
            }
        }

        return KeyCode.None;
    }

    public List<InputDef> GetAllInputDefs()
    {
        return defs.GetInputDefs();
    }

    public void StartInputCapture(string inputName)
    {
        capturingInput = true;
        capturingName = inputName;
    }

    public bool ConfigFileExists() { return File.Exists(configsPath); }

    public void LoadInputs()
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

        InputDefinitions c = new InputDefinitions();

        XmlSerializer xml = new XmlSerializer(typeof(InputDefinitions));
        using (TextReader reader = new StreamReader(fs))
        {
            c = (InputDefinitions)xml.Deserialize(reader);
        }

        defs.InputDefs.Clear();
        defs.InputDefs.AddRange(c.InputDefs);

        /*foreach (InputDef def in c.InputDefs)
        {
            defs[def.InputName] = def.InputKey;
        }*/

        c = null;
    }

    public void SaveInputs()
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

        InputDefinitions c = new InputDefinitions();
        c.InputDefs = new List<InputDef>(defs.InputDefs);

        XmlSerializer xml = new XmlSerializer(typeof(InputDefinitions));
        using (TextWriter writer = new StreamWriter(fs))
        {
            xml.Serialize(writer, c);
        }

        c = null;
    }

    public bool IsCapturingInput() { return capturingInput; }

    public string CaptureInputName() { return capturingName; }

    #region Key Overrides
    public bool GetKey(string inputName)
    {
        if (!defs.InputDefs.Exists(i => i.InputName == inputName))
        {
            throw new KeyNotFoundException("InputName " + inputName + " was not found in the InputDefs!");
            return false;
        }

        return Input.GetKey(defs.InputDefs.Find(i => i.InputName == inputName).InputKey);
    }

    public bool GetKeyDown(string inputName)
    {
        if (!defs.InputDefs.Exists(i => i.InputName == inputName))
        {
            throw new KeyNotFoundException("InputName " + inputName + " was not found in the InputDefs!");
            return false;
        }

        return Input.GetKeyDown(defs.InputDefs.Find(i => i.InputName == inputName).InputKey);
    }

    public bool GetKeyUp(string inputName)
    {
        if (!defs.InputDefs.Exists(i => i.InputName == inputName))
        {
            throw new KeyNotFoundException("InputName " + inputName + " was not found in the InputDefs!");
            return false;
        }

        return Input.GetKeyUp(defs.InputDefs.Find(i => i.InputName == inputName).InputKey);
    }
    #endregion
}
