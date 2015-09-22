using UnityEngine;
using System.Collections;
using System;

public class GameSubsystems : MonoBehaviour
{
    public GUISkin DefaultSkin;
    public DefaultGameSettings DefaultSettings;
    static GameSubsystems inst;

    public static GameSubsystems Instance
    {
        get
        {
            return inst;
        }
    }

    public GameConfigs configs;

    GameInput input;
    public GameInput Input { get { return input; } }

    void Awake()
    {
        DontDestroyOnLoad(this);
        //Start initializing config classes and low-level subsystems before game actual start
        configs = new GameConfigs();
        configs.LoadConfigs();

        input = new GameInput();
        Input.LoadInputs();

        LoadSettings();

        configs.SaveConfigs();
        Input.SaveInputs();

        inst = this;
    }

    void Start()
    {
        //Once the game starts, throw us to the main menu scene
        Application.LoadLevel("menu");
    }

    void Update()
    {
        Input.Update();
    }

    void OnApplicationQuit()
    {
        configs.SaveConfigs();
    }

    public void LoadSettings()
    {
        LoadGraphicsConfigs();
        LoadPlayerConfigs();
        LoadInputConfigs();
        LoadAudioConfigs();
    }

    void LoadPlayerConfigs()
    {
        configs.GetConfigEntry<string>("Player", "Name", DefaultSettings.playerName);
    }

    void LoadInputConfigs()
    {
        
    }

    void LoadGraphicsConfigs()
    {
        //Fuck unity...
        //Need to typecast their inconsistent structs into classes so it will work with generic type lists
        //This lets us define what the return value should be, and provide a 'default' stand-in value if the ConfigEntry does not exist yet
        ScreenResolution res = configs.GetConfigEntry<ScreenResolution>("Graphics", "Resolution", DefaultSettings.Resolution);
        //int qualityLevel = configs.GetConfigEntry<int>("Graphics", "QualityLevel", 1);
        QualityLevel level = configs.GetConfigEntry<QualityLevel>("Graphics", "QualityLevel", DefaultSettings.QualityLevel);

        Screen.SetResolution(res.width, res.height, res.fullscreen, res.refreshRate);
        QualitySettings.SetQualityLevel(level.level);
    }

    void LoadAudioConfigs()
    {
        float level = float.Parse(configs.GetConfigEntry<string>("Audio", "EffectsLevel", (1f).ToString()));

        AudioManager.SetFXVolume(level);
    }

    public T GetConfigValue<T>(string ConfigIndex, string ConfigName) where T : class
    {
        return configs.GetConfigEntry<T>(ConfigIndex, ConfigName);
    }

    public void SetConfigValue<T>(string ConfigIndex, string ConfigName, T newValue) where T : class
    {
        configs.SetConfigEntry<T>(ConfigIndex, ConfigName, newValue);
    }
}
