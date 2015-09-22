using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Menu
{
    public string MenuName;
    public bool IsModal = false;

    public bool Enabled { get { return enabled; } set { if (value == false) { OnDisable(); } else { OnEnable(); } enabled = value; } }
    
    bool enabled = false;
    public virtual void OnDrawMenu() { }

    public virtual void OnEnable() { }
    public virtual void OnDisable() { }
}

public class MenuHandler : MonoBehaviour
{
    List<Menu> menus;

    public static MenuHandler instance;

    public int ScreenSafeWidth = 800;
    public int ScreenSafeHeight = 600;

    void Awake()
    {
        menus = new List<Menu>() { new MainMenu(), new PauseMenu(), new EndGameMenu(), new BuyMenu(), new OptionsMenu() };

        instance = this;
    }

    void Update()
    {
        ScreenSafeWidth = Screen.width / 3;
        //ScreenSafeHeight = Screen.height / 2;
    }

    public void DrawMenus()
    {
        foreach (Menu m in menus)
        {
            if (m.Enabled)
            {
                GUI.depth = 100;
                GUI.color = Color.white;
                m.OnDrawMenu();
            }
        }
    }

    public void SetMenuEnabled(string menuName, bool enabled)
    {
        if (!menus.Exists(i => i.MenuName == menuName))
        {
            Debug.LogError("No menu '" + menuName + "' exists! Did you define it?");
            return;
        }

        Menu m = menus.Find(i => i.MenuName == menuName);

        m.Enabled = enabled;
    }

    public void ToggleMenuEnabled(string menuName)
    {
        if (!menus.Exists(i => i.MenuName == menuName))
        {
            Debug.LogError("No menu '" + menuName + "' exists! Did you define it?");
            return;
        }

        Menu m = menus.Find(i => i.MenuName == menuName);
        m.Enabled = !m.Enabled;
    }

    public bool AreMenusActive()
    {
        return (menus.FindAll(i => i.Enabled == true).Count > 0);
    }
}

public class PauseMenu : Menu
{
    Texture solidTexture;
    public PauseMenu()
    {
        MenuName = "PauseMenu";
        Enabled = false;

        solidTexture = Resources.Load<Texture>("textures/solid");
    }

    public override void OnDrawMenu()
    {
        GUI.color = Color.black * 0.5f;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), solidTexture);
        GUI.color = Color.white;

        GUI.Window(0, new Rect((Screen.width / 2) - 200, (Screen.height / 2) - (MenuHandler.instance.ScreenSafeHeight / 2), 400, MenuHandler.instance.ScreenSafeHeight), new GUI.WindowFunction(DrawMainWindow), "", GameHandler.instance.GetStyle("MenuDefault_Window"));

        base.OnDrawMenu();
    }

    void DrawMainWindow(int id)
    {
        if (GUI.Button(new Rect(16, 16, 368, 80), "Resume Game", GameHandler.instance.GetStyle("MenuDefault_Button")))
        {
            MenuHandler.instance.SetMenuEnabled("PauseMenu", false);
        }
        else if (GUI.Button(new Rect(16, 112, 368, 80), "Options", GameHandler.instance.GetStyle("MenuDefault_Button")))
        {
            //MenuHandler.instance.SetMenuEnabled("PauseMenu", false);
            MenuHandler.instance.SetMenuEnabled("OptionsMenu", true);
        }
        else if (GUI.Button(new Rect(16, 208, 368, 80), "Restart Game", GameHandler.instance.GetStyle("MenuDefault_Button")))
        {
            MenuHandler.instance.SetMenuEnabled("PauseMenu", false);
            GameHandler.instance.TransitionToLevel(1, Application.loadedLevelName);
        }
        else if (GUI.Button(new Rect(16, MenuHandler.instance.ScreenSafeHeight - 80 - 16, 368, 80), "Quit Game", GameHandler.instance.GetStyle("MenuDefault_Button")))
        {
            MenuHandler.instance.SetMenuEnabled("PauseMenu", false);
            GameHandler.instance.TransitionToLevel(1, "menu");
        }
    }

    public override void OnEnable()
    {
        Time.timeScale = 0;
        base.OnEnable();
    }

    public override void OnDisable()
    {
        Time.timeScale = 1;
        base.OnDisable();
    }
}

public class MainMenu : Menu
{
    int menuWidth = 300;
    int menuHeight = 500;

    string activeMenu = "main";

    Texture titleCard;

    public MainMenu()
    {
        MenuName = "MainMenu";
        Enabled = false;

        titleCard = Resources.Load<Texture>("textures/titlecard");
    }

    public override void OnDrawMenu()
    {
        GUI.DrawTexture(new Rect(Screen.width / 6, (Screen.height / 4) - 256, 512, 512), titleCard);

        switch(activeMenu)
        {
            case "main":
                GUI.Window(0, new Rect((Screen.width / 3) * 2, (Screen.height / 2) - (menuHeight / 2), menuWidth, menuHeight), new GUI.WindowFunction(DrawMainWindow), "", GameHandler.instance.GetStyle("MenuDefault_Window"));
                break;

            case "credits":
                {
                    int windowX = (Screen.width / 2) + (MenuHandler.instance.ScreenSafeWidth / 2) - menuWidth;

                    GUI.Window(0, new Rect(windowX, (Screen.height / 2) - (menuHeight / 2), menuWidth * 2, menuHeight), new GUI.WindowFunction(DrawCredits), "", GameHandler.instance.GetStyle("MenuDefault_Window"));
                    break;
                }

            case "single":
                {
                    int windowX = (Screen.width / 2) + (MenuHandler.instance.ScreenSafeWidth / 2) - menuWidth;

                    GUI.Window(0, new Rect(windowX, (Screen.height / 2) - (menuHeight / 2), menuWidth * 2, menuHeight), new GUI.WindowFunction(DrawSingleplayerMenu), "Singleplayer", GameHandler.instance.GetStyle("MenuDefault_Window"));
                    break;
                }
        }

        base.OnDrawMenu();
    }

    void DrawMainWindow(int id)
    {
        if (GUI.Button(new Rect(16, 16, menuWidth - 32, 40), "Singleplayer", GameHandler.instance.GetStyle("MenuDefault_Button")))
        {
            activeMenu = "single";
        }
        else if (GUI.Button(new Rect(16, 72, menuWidth - 32, 40), "Multiplayer", GameHandler.instance.GetStyle("MenuDefault_Button_Disabled")))
        {
            
        }
        else if (GUI.Button(new Rect(16, 128, menuWidth - 32, 40), "Options", GameHandler.instance.GetStyle("MenuDefault_Button")))
        {
            //MenuHandler.instance.SetMenuEnabled("MainMenu", false);
            MenuHandler.instance.SetMenuEnabled("OptionsMenu", true);
        }
        else if (GUI.Button(new Rect(16, menuHeight - 32 - 80, menuWidth - 32, 40), "Credits", GameHandler.instance.GetStyle("MenuDefault_Button")))
        {
            activeMenu = "credits";
        }
        else if (GUI.Button(new Rect(16, menuHeight - 16 - 40, menuWidth - 32, 40), "Quit", GameHandler.instance.GetStyle("MenuDefault_Button")))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    int mapIndex = 0;
    Vector2 singleScrollPos;
    float singleVScrollVal;

    void DrawSingleplayerMenu(int id)
    {
        List<MapDefinition> maps = GameHandler.instance.GetMapList();

        //Draw map list
        Rect scrollViewSize = new Rect(32, 48, 180, menuHeight - 128);
        Rect scrollAreaSize = new Rect(0, 0, 180, maps.Count * 28 + (16 * maps.Count));

        singleScrollPos = GUI.BeginScrollView(scrollViewSize, singleScrollPos, scrollAreaSize);

        for(int i = 0; i < maps.Count; i++)
        {
            if (GUI.Button(new Rect(0, (i * 28) + (i * 16), 180, 28), maps[i].MapName, GameHandler.instance.GetStyle("MenuDefault_Button")))
            {
                Singleplayer_SelectMap(i);
            }
        }

        GUI.EndScrollView();

        //Draw map info
        MapDefinition selectedMap = maps[mapIndex];

        GUI.Label(new Rect(menuWidth - 80, 192, 40, 20), "Map Name", GameHandler.instance.GetStyle("MenuDefault_Label2"));
        GUI.Label(new Rect(menuWidth - 60, 208, 80, 20), selectedMap.MapName, GameHandler.instance.GetStyle("MenuDefault_Label"));

        GUI.Label(new Rect(menuWidth - 80, 240, 40, 20), "Description", GameHandler.instance.GetStyle("MenuDefault_Label2"));
        GUI.Label(new Rect(menuWidth - 60, 256, menuWidth + 44, 200), selectedMap.Description, GameHandler.instance.GetStyle("MenuDefault_LabelArea"));

        GUI.DrawTexture(new Rect(menuWidth - 60, 32, menuWidth + 44, 128), selectedMap.mapPreview);


        if (GUI.Button(new Rect((menuWidth * 2) - 80 - 16, menuHeight - 16 - 40, 80, 40), "Start", GameHandler.instance.GetStyle("MenuDefault_Button")))
        {
            GameHandler.instance.TransitionToLevel(1, selectedMap.MapName);
        }

        if(GUI.Button(new Rect(16, menuHeight - 16 - 40, 80, 40), "Back", GameHandler.instance.GetStyle("MenuDefault_Button")))
        {
            activeMenu = "main";
        }
    }

    void Singleplayer_SelectMap(int id)
    {
        mapIndex = id;
    }

    void DrawCredits(int id)
    {
        GUI.Label(new Rect(0, 0, menuWidth * 2, menuHeight), GameHandler.instance.GameInfo.CreditList, GameHandler.instance.GetStyle("MenuDefault_Label_Center"));
        GUI.Label(new Rect(0, menuHeight - 64, menuWidth * 2, 64), "Version " + GameHandler.instance.GameInfo.BuildVersion.ToString(), GameHandler.instance.GetStyle("MenuDefault_Label_Center"));

        if (GUI.Button(new Rect(16, menuHeight - 16 - 40, 80, 40), "Back", GameHandler.instance.GetStyle("MenuDefault_Button")))
        {
            activeMenu = "main";
        }
    }
}

public class EndGameMenu : Menu
{
    int windowWidth = 800;
    int windowHeight = 500;

    public EndGameMenu()
    {
        MenuName = "EndGameMenu";
        Enabled = false;
    }

    public override void OnDrawMenu()
    {
        //GUI.color = Color.black * 0.5f;
        //GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), solidTexture);
        int windowX = (Screen.width / 2) + (MenuHandler.instance.ScreenSafeWidth / 2) - (windowWidth / 2);

        if (windowWidth > MenuHandler.instance.ScreenSafeWidth)
            windowX = (Screen.width / 2) - (windowWidth / 2);

        GUI.Window(0, new Rect(windowX, (Screen.height / 2) - (windowHeight / 2), windowWidth, windowHeight), new GUI.WindowFunction(DrawMainWindow), "Post Game Statistics", GameHandler.instance.GetStyle("MenuDefault_Window"));

        base.OnDrawMenu();
    }

    void DrawMainWindow(int id)
    {
        GUI.Label(new Rect(0, 32, windowWidth, 64), "You failed to protect your Sand Castles :(", GameHandler.instance.GetStyle("MenuDefault_LabelLarge_Center"));

        Rect stats = new Rect(16, 128, windowWidth - 32, 40 * 4);

        DrawStatsHeaders(stats);
        DrawPlayerStats(new Rect(stats.x, stats.y + 32, stats.width, stats.height), GameHandler.instance.GetLocalPlayer());

        if (GUI.Button(new Rect(windowWidth - 16 - 80, windowHeight - 16 - 40, 80, 40), "Retry", GameHandler.instance.GetStyle("MenuDefault_Button")))
        {
            //MenuHandler.instance.SetMenuEnabled("EndGameMenu", false);
            GameHandler.instance.TransitionToLevel(1, Application.loadedLevelName);
        }
        else if (GUI.Button(new Rect(16, windowHeight - 16 - 40, 80, 40), "Quit", GameHandler.instance.GetStyle("MenuDefault_Button")))
        {
            //MenuHandler.instance.SetMenuEnabled("EndGameMenu", false);
            GameHandler.instance.TransitionToLevel(1, "menu");
        }
    }

    void DrawStatsHeaders(Rect area)
    {
        int labelWidth = 80;

        GUI.Label(new Rect(area.x, area.top, labelWidth, 40), "Name", GameHandler.instance.GetStyle("MenuDefault_LabelSmall"));

        GUI.Label(new Rect(area.x + area.width - (labelWidth * 6), area.top, labelWidth, 40), "Kills", GameHandler.instance.GetStyle("MenuDefault_LabelSmall"));
        GUI.Label(new Rect(area.x + area.width - (labelWidth * 5), area.top, labelWidth, 40), "Deaths", GameHandler.instance.GetStyle("MenuDefault_LabelSmall"));
        GUI.Label(new Rect(area.x + area.width - (labelWidth * 4), area.top, labelWidth, 40), "Wave", GameHandler.instance.GetStyle("MenuDefault_LabelSmall"));
        GUI.Label(new Rect(area.x + area.width - (labelWidth * 3), area.top, labelWidth, 40), "Fired", GameHandler.instance.GetStyle("MenuDefault_LabelSmall"));
        GUI.Label(new Rect(area.x + area.width - (labelWidth * 2), area.top, labelWidth, 40), "Hit", GameHandler.instance.GetStyle("MenuDefault_LabelSmall"));
        GUI.Label(new Rect(area.x + area.width - labelWidth, area.top, labelWidth, 40), "Accuracy", GameHandler.instance.GetStyle("MenuDefault_LabelSmall"));
    }

    void DrawPlayerStats(Rect area, PlayerController player)
    {
        PlayerStatistics stats = player.stats;

        int labelWidth = 80;

        GUI.Label(new Rect(area.x, area.top, labelWidth, 40), player.PlayerName, GameHandler.instance.GetStyle("MenuDefault_LabelSmall"));

        GUI.Label(new Rect(area.x + area.width - (labelWidth * 6), area.top, labelWidth, 40), "" + stats.Kills, GameHandler.instance.GetStyle("MenuDefault_LabelSmall"));
        GUI.Label(new Rect(area.x + area.width - (labelWidth * 5), area.top, labelWidth, 40), "" + stats.Deaths, GameHandler.instance.GetStyle("MenuDefault_LabelSmall"));
        GUI.Label(new Rect(area.x + area.width - (labelWidth * 4), area.top, labelWidth, 40), "" + stats.BestWave, GameHandler.instance.GetStyle("MenuDefault_LabelSmall"));
        GUI.Label(new Rect(area.x + area.width - (labelWidth * 3), area.top, labelWidth, 40), "" + stats.ShotsFired, GameHandler.instance.GetStyle("MenuDefault_LabelSmall"));
        GUI.Label(new Rect(area.x + area.width - (labelWidth * 2), area.top, labelWidth, 40), "" + stats.ShotsHit, GameHandler.instance.GetStyle("MenuDefault_LabelSmall"));
        GUI.Label(new Rect(area.x + area.width - labelWidth, area.top, labelWidth, 40), (stats.ShotsHit / stats.ShotsFired).ToString() + "%", GameHandler.instance.GetStyle("MenuDefault_LabelSmall"));
    }
}

public class BuyMenu : Menu
{
    int windowWidth = 800;
    int windowHeight = 600;

    public BuyMenu()
    {
        MenuName = "BuyMenu";
        Enabled = false;
    }

    public override void OnDrawMenu()
    {
        GUI.Window(20, new Rect((Screen.width / 2) - (windowWidth / 2), (Screen.height / 2) - (windowHeight / 2), windowWidth, windowHeight), new GUI.WindowFunction(DrawMainWindow), "", GameHandler.instance.GetStyle("MenuDefault_Window"));

        base.OnDrawMenu();
    }

    float VScrollVal;
    Vector2 scrollPos;
    int buyableIndex = 0;
    PurchaseCategory selectedCategory = PurchaseCategory.Weapons;

    void DrawMainWindow(int id)
    {
        GUI.Label(new Rect(16, 8, 128, 32), "Buy Menu", GameHandler.instance.GetStyle("MenuDefault_Label"));
        DrawCategories(new Rect(16, 48, windowWidth - 32, 48));

        List<BuyableDefinition> buys = GameHandler.instance.GetBuyables().FindAll(i => i.Category == selectedCategory);

        //Draw map list
        Rect scrollViewSize = new Rect(32, 128, 180, windowHeight - 256);
        Rect scrollAreaSize = new Rect(0, 0, 180, buys.Count * 28 + (16 * buys.Count));

        scrollPos = GUI.BeginScrollView(scrollViewSize, scrollPos, scrollAreaSize);

        for (int i = 0; i < buys.Count; i++)
        {
            if (GUI.Button(new Rect(0, (i * 28) + (i * 16), 180, 28), buys[i].Name, GameHandler.instance.GetStyle("MenuDefault_Button")))
            {
                SelectPurchase(i);
            }
        }

        GUI.EndScrollView();

        if (buys.Count > 0)
        {
            BuyableDefinition selectedBuyable = buys[buyableIndex];

            //GUI.Label(new Rect(menuWidth - 80, 192, 40, 20), "Map Name", GameHandler.instance.GetStyle("MenuDefault_Label2"));
            GUI.Label(new Rect((windowWidth / 2) - 128, 128, windowWidth / 2, 20), "Name", GameHandler.instance.GetStyle("MenuDefault_Label2"));
            GUI.Label(new Rect((windowWidth / 2) - 128, 128 + 32, windowWidth / 2, 20), selectedBuyable.Name, GameHandler.instance.GetStyle("MenuDefault_Label"));

            //GUI.Label(new Rect(menuWidth - 80, 240, 40, 20), "Description", GameHandler.instance.GetStyle("MenuDefault_Label2"));
            GUI.Label(new Rect((windowWidth / 2) - 128, 192, windowWidth / 2, 20), "Description", GameHandler.instance.GetStyle("MenuDefault_Label2"));
            GUI.Label(new Rect((windowWidth / 2) - 128, 192 + 32, windowWidth / 2, windowHeight / 2), selectedBuyable.Description, GameHandler.instance.GetStyle("MenuDefault_LabelArea"));

            GUI.Label(new Rect((windowWidth / 2) - 128, windowHeight - 128, windowWidth / 2, 64), "$" + selectedBuyable.Cost, GameHandler.instance.GetStyle("MenuDefault_LabelArea"));
            //GUI.DrawTexture(new Rect(menuWidth - 60, 32, menuWidth + 44, 128), selectedMap.mapPreview);
        }

        if (GUI.Button(new Rect(windowWidth - 128 - 16, windowHeight - 128, 128, 48), "Buy", GameHandler.instance.GetStyle("MenuDefault_Button")))
        {
            GameHandler.instance.PurchaseBuyable(buys[buyableIndex], GameHandler.instance.GetLocalPlayer(), true);
            MenuHandler.instance.SetMenuEnabled(MenuName, false);
        }

        if (GUI.Button(new Rect(16, windowHeight - 32 - 48, 128, 48), "Back", GameHandler.instance.GetStyle("MenuDefault_Button")))
        {
            MenuHandler.instance.SetMenuEnabled(MenuName, false);
        }
    }

    void SelectPurchase(int id)
    {
        buyableIndex = id;
    }

    void DrawCategories(Rect area)
    {
        string[] categories = Enum.GetNames(typeof(PurchaseCategory));

        float buttonWidth = (area.width / categories.Length) - 16;

        for (int i = 0; i < categories.Length; i++)
        {
            string s = categories[i];

            if (GUI.Button(new Rect(area.x + (buttonWidth * i) + (16 * i), area.y, buttonWidth, area.height), s, GameHandler.instance.GetStyle("MenuDefault_Button")))
            {
                selectedCategory = (PurchaseCategory)Enum.Parse(typeof(PurchaseCategory), s);
                buyableIndex = 0;
            }
        }
    }
}

public class OptionsMenu : Menu
{
    int windowWidth = 800;
    int windowHeight = 600;

    Texture solidTexture;

    ComboBox resolutionsBox;
    List<Resolution> resolutions;
    Resolution currentResolution;
    int fullscreenInt;
    bool resolutionFullscreen;

    public string[] qualityLevels;

    int selectedResolution = 0;
    int selectedQualitylevel = 0;

    float audioEffectsLevel;

    string activeSection = "input";

    Vector2 inputScrollPos;
    float inputVScrollVal;

    public OptionsMenu()
    {
        MenuName = "OptionsMenu";
        Enabled = false;

        solidTexture = Resources.Load<Texture>("textures/solid");

        resolutionsBox = new ComboBox();

        IsModal = true;

        FillInfo();
    }

    public void FillInfo()
    {
        resolutions = new List<Resolution>();
        resolutions.AddRange(Screen.resolutions);

        currentResolution = Screen.currentResolution;

        if (resolutions.Exists(i => i.width == currentResolution.width && i.height == currentResolution.height))
        {
            selectedResolution = resolutions.IndexOf(resolutions.Find(i => i.width == Screen.width && i.height == Screen.height));
            currentResolution = resolutions[selectedResolution];
        }

        resolutionFullscreen = Screen.fullScreen;
        fullscreenInt = (resolutionFullscreen) ? 1 : 0;

        qualityLevels = QualitySettings.names;
        selectedQualitylevel = QualitySettings.GetQualityLevel();

        audioEffectsLevel = float.Parse(GameSubsystems.Instance.GetConfigValue<string>("Audio", "EffectsLevel"));
    }

    public override void OnDrawMenu()
    {
        if (GameSubsystems.Instance.Input.IsCapturingInput())
        {
            GUI.color = Color.black * 0.5f;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), solidTexture);

            GUI.Label(new Rect(16, 16, 256, 128), "Press escape to cancel", GameHandler.instance.GetStyle("MenuDefault_Label_Center"));
            GUI.Label(new Rect((Screen.width / 2) - 128, (Screen.height / 2) - 64, 256, 128), "Press a key for " + GameSubsystems.Instance.Input.CaptureInputName(), GameHandler.instance.GetStyle("MenuDefault_LabelLarge_Center"));
        }

        GUI.color = Color.white;

        GUI.Window(20, new Rect((Screen.width / 2) - (windowWidth / 2), (Screen.height / 2) - (windowHeight / 2), windowWidth, windowHeight), new GUI.WindowFunction(DrawMainWindow), "", GameHandler.instance.GetStyle("MenuDefault_Window"));
        
        base.OnDrawMenu();
    }

    public void DrawMainWindow(int id)
    {
        if (IsModal)
        {
            if (GUI.Button(new Rect(windowWidth - 32 - 8, 8, 32, 32), "x", GameHandler.instance.GetStyle("MenuDefault_Button")))
            {
                MenuHandler.instance.SetMenuEnabled(MenuName, false);
            }
        }

        if (GUI.Button(new Rect(8 , 8, windowWidth / 4, 32), "Input", GameHandler.instance.GetStyle("MenuDefault_Button")))
        {
            activeSection = "input";
        }
        else if (GUI.Button(new Rect(24 + (windowWidth / 4), 8, windowWidth / 4, 32), "Audio", GameHandler.instance.GetStyle("MenuDefault_Button")))
        {
            activeSection = "audio";
        }
        else if (GUI.Button(new Rect(40 + ((windowWidth / 4) * 2), 8, windowWidth / 4, 32), "Graphics", GameHandler.instance.GetStyle("MenuDefault_Button")))
        {
            activeSection = "graphics";
        }

        switch (activeSection)
        {
            case "graphics":
                DrawGraphicsSection();
                break;

            case "audio":
                DrawAudioSection();
                break;

            case "input":
                DrawInputSection();
                break;
        }

        if (GUI.Button(new Rect(windowWidth - 128 - 16, windowHeight - 32 - 16, 128, 32), "Apply", GameHandler.instance.GetStyle("MenuDefault_Button")))
        {
            ApplyChanges();
        }
        //resolutionsBox.List(new Rect(0, 0, 200, 40), new GUIContent("Resolution"), resolutions.ToArray(), GameHandler.instance.GetStyle("MenuDefault_Button"), GameHandler.instance.GetStyle("MenuDefault_Window"), GameHandler.instance.GetStyle("MenuDefault_ListButton"));
    }

    public void DrawGraphicsSection()
    {
        GUI.Label(new Rect(16, 64, 128, 32), "Resolution:", GameHandler.instance.GetStyle("MenuDefault_Label"));

        string res = string.Format("{0}x{1}", resolutions[selectedResolution].width, resolutions[selectedResolution].height);
        GUIExtenders.SwitchButtons(new Rect((windowWidth / 2) - 128, 64, 256, 32), res, ref selectedResolution, (resolutions.Count - 1), GameHandler.instance.GetStyle("MenuDefault_Button_Label"), GameHandler.instance.GetStyle("MenuDefault_Button"));
        GUIExtenders.SwitchButtons(new Rect((windowWidth / 2) - 128, 112, 256, 32), resolutionFullscreen.ToString(), ref fullscreenInt, 1, GameHandler.instance.GetStyle("MenuDefault_Button_Label"), GameHandler.instance.GetStyle("MenuDefault_Button"));

        if (fullscreenInt == 1)
            resolutionFullscreen = true;
        else if (fullscreenInt == 0)
            resolutionFullscreen = false;

        GUI.Label(new Rect(16, 256, 128, 32), "Quality Preset:", GameHandler.instance.GetStyle("MenuDefault_Label"));
        GUIExtenders.SwitchButtons(new Rect((windowWidth / 2) - 128, 256, 256, 32), qualityLevels[selectedQualitylevel], ref selectedQualitylevel, (qualityLevels.Length - 1), GameHandler.instance.GetStyle("MenuDefault_Button_Label"), GameHandler.instance.GetStyle("MenuDefault_Button"));
    }

    public void DrawAudioSection()
    {
        GUI.Label(new Rect(16, 64, 128, 32), "SFX Level:", GameHandler.instance.GetStyle("MenuDefault_Label"));
        audioEffectsLevel = GUI.HorizontalSlider(new Rect((windowWidth / 2) - 128, 64, 256, 32), audioEffectsLevel, 0f, 1f);
    }

    public void DrawInputSection()
    {
        GUI.Label(new Rect(16, 64, 128, 32), "Input Key Binds", GameHandler.instance.GetStyle("MenuDefault_Label"));

        List<InputDef> keys = GameSubsystems.Instance.Input.GetAllInputDefs();

        Rect scrollViewSize = new Rect(32, 128, (windowWidth / 3) * 2, (windowHeight / 3) * 1);
        Rect scrollAreaSize = new Rect(0, 0, 256, (keys.Count * 32) + (keys.Count * 16));
        Rect scrollbarSize = new Rect(32 + ((windowWidth / 3) * 2), 128, 32, (windowHeight / 2));
        List<MapDefinition> maps = GameHandler.instance.GetMapList();

        inputScrollPos = GUI.BeginScrollView(scrollViewSize, inputScrollPos, scrollAreaSize);

        for (int i = 0; i < keys.Count; i++)
        {
            string key = keys[i].InputName;

            GUI.Label(new Rect(16, (i * 32) + (i * 16), 256, 32), key, GameHandler.instance.GetStyle("MenuDefault_Label"));

            if (GUI.Button(new Rect(256+16, (i * 32) + (i * 16), 96, 32), keys[i].InputKey.ToString(), GameHandler.instance.GetStyle("MenuDefault_Button")))
            {
                //Singleplayer_SelectMap(i);
                GameSubsystems.Instance.Input.StartInputCapture(keys[i].InputName);
            }
        }

        GUI.EndScrollView();
    }

    public void ApplyChanges()
    {
        ScreenResolution r = ScreenResolution.ToScreenResolution(resolutions[selectedResolution]);
        r.fullscreen = resolutionFullscreen;

        GameSubsystems.Instance.SetConfigValue<ScreenResolution>("Graphics", "Resolution", r);
        GameSubsystems.Instance.SetConfigValue<QualityLevel>("Graphics", "QualityLevel", new QualityLevel() { level = selectedQualitylevel });

        GameSubsystems.Instance.SetConfigValue<string>("Audio", "EffectsLevel", audioEffectsLevel.ToString());

        GameSubsystems.Instance.configs.SaveConfigs();
        GameSubsystems.Instance.LoadSettings();
    }
}