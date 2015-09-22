using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;

[Serializable]
public class InputDef
{
    public string InputName;
    public KeyCode InputKey;
}

public class InputDefinitions
{
    [XmlArray("InputConfigs")]
    private List<InputDef> inputDict = new List<InputDef>()
    {
        new InputDef() {InputName = "Forward", InputKey = KeyCode.W},
        new InputDef() {InputName = "Backward", InputKey = KeyCode.S},
        new InputDef() {InputName = "Strafe Left", InputKey = KeyCode.A},
        new InputDef() {InputName = "Strafe Right", InputKey = KeyCode.D},
        new InputDef() {InputName = "Jump", InputKey = KeyCode.Space},

        new InputDef() {InputName = "Use", InputKey = KeyCode.E},
        new InputDef() {InputName = "Buy Menu", InputKey = KeyCode.B},

        new InputDef() {InputName = "Switch Weapon", InputKey = KeyCode.Q},
        new InputDef()  {InputName = "Reload", InputKey = KeyCode.R},
        new InputDef()  {InputName = "Fire", InputKey = KeyCode.Mouse0},
    };

    public List<InputDef> InputDefs;

    public void Init()
    {
        InputDefs = new List<InputDef>();
        InputDefs.AddRange(inputDict);
    }

    public KeyCode this[string inputName]
    {
        get
        {
            if (inputDict.Exists(i => i.InputName == inputName))
            {
                return inputDict.Find(i => i.InputName == inputName).InputKey;
            }
            else
                return KeyCode.None;
        }

        set
        {
            if (inputDict.Exists(i => i.InputName == inputName))
            {
                inputDict.Find(i => i.InputName == inputName).InputKey = value;
            }
            else
                throw new KeyNotFoundException("Attempted to set KeyCode for key "+inputName+". The key does not exist!");
        }
    }

    public List<InputDef> GetInputDefs()
    {
        return inputDict;
    }
}
