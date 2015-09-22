using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct CreatureTier
{
    public int tierLevel;
    public List<BaseAICharacter> Creatures;
}

public class GameHandler : MonoBehaviour
{
    public List<BaseCharacter> SandcastleObjects;
    public List<AISpawner> AISpawnPoints;
    public List<PlayerSpawner> PlayerSpawnPoints;

    List<GameEntity> Entities;

    public List<CreatureTier> creatureTiers;

    public GameObject PlayerObjectTemplate;
    PlayerController localPlayer;

    public GameObject EndgameSpectatePosition;

    public static GameHandler instance { get; protected set; }

    public GUIStyleDefinition StyleDictionary;
    public Data_MapDefinition MapDictionary;
    public Data_PurchaseDefinitions PurchaseDictionary;
    public Data_GameInfo GameInfo;

    public SpawnHandler spawnHandler;
    public NetworkHandler networkHandler;

    public bool RunGameLogic = false;

    Dictionary<string, GUIStyle> styles;
    List<MapDefinition> maps;
    List<BuyableDefinition> buyables;

    bool DebugAI = false;

    //Baseitems should be alive for 30 seconds before being destroyed
    float itemLifetime = 30;

    bool changingLevel = false;
    string nextLevelName;
    float changeLevelTime;
    float fadeAlpha = 0.0f;
    float fadeDelay = 1;
    Texture fadeoutTexture;

    bool GameActive = false;
    bool ShouldStopGame = false;

    public int ObjectsInScene = 0;

    void Awake()
    {
        //GameSettings = Resources.Load<LocalGameSettings>("Main_GameSettings");

        //Screen.SetResolution(GameSettings.screenResolution.width, GameSettings.screenResolution.height, GameSettings.fullscreen);
        //QualitySettings.currentLevel = GameSettings.QualityLevel;
    }

    // Use this for initialization
    void Start()
    {
        networkHandler = new NetworkHandler();

        fadeoutTexture = Resources.Load<Texture>("textures/solid");

        Entities = new List<GameEntity>();

        styles = new Dictionary<string, GUIStyle>();
        maps = new List<MapDefinition>();
        buyables = new List<BuyableDefinition>();

        if (StyleDictionary != null)
            ProcessStyleDictionary();

        if (MapDictionary != null)
            ProcessMapDictionary();

        if (PurchaseDictionary != null)
            ProcessBuyablesDictionary();

        instance = this;

        if (RunGameLogic)
            StartGame();
        else
        {
            //GameHUD.instance.s
            MenuHandler.instance.SetMenuEnabled("MainMenu", true);
        }
    }

    public void StartGame()
    {
        //Beginning game setup
        GameActive = true;

        CreatePlayer(true, 0);

        //Wait();
        //start spawning
        //Spawn();
        spawnHandler = new SpawnHandler();
    }

    public void EndGame()
    {
        //End game process
        localPlayer.IsEnabled = false; //Stop the player from moving
        localPlayer.weaponCam.enabled = false;

        localPlayer.viewCam.transform.position = EndgameSpectatePosition.transform.position;
        localPlayer.viewCam.transform.rotation = EndgameSpectatePosition.transform.rotation;

        MenuHandler.instance.SetMenuEnabled("EndGameMenu", true);
    }

    public PlayerSpawner GetRandomPlayerSpawner() { return PlayerSpawnPoints[Random.Range(0, PlayerSpawnPoints.Count)]; }

    // Update is called once per frame
    void Update()
    {
        if (changingLevel)
        {
            fadeAlpha += Time.deltaTime / fadeDelay;

            if (Time.time > changeLevelTime)
            {
                Application.LoadLevel(nextLevelName);
            }
        }

        //RunGameLogic is set to distinguish whether or not we are in a menu scene (ie, the main menu) or an active level
        if (RunGameLogic)
        {
            if (ShouldStopGame)
                EndGame();

            if (GetAliveSandcastles() <= 0) //If there aren't any sandcastles alive, end the game
            {
                GameActive = false;

                if (!ShouldStopGame)
                    ShouldStopGame = true;
            }
            
            //This will probably change when MP is actually implemented
            if (GetNumAlivePlayers() <= 0) //If there aren't any players alive, end the game
            {
                GameActive = false;

                if (!ShouldStopGame)
                    ShouldStopGame = true;
            }

            if (GameActive)
            {
                spawnHandler.UpdateSpawns();
            }

            //Item and Weapon cleanup
            List<GameEntity> objects = new List<GameEntity>();
            objects.AddRange(GameObject.FindObjectsOfType<GameEntity>());
            ObjectsInScene = objects.Count;

            foreach (GameEntity item in objects)
            {
                if (item.GetComponent<BaseItem>() != null)
                {
                    BaseItem i = item.GetComponent<BaseItem>();

                    float time = Time.time - i.GetTimeCreated();

                    if (time > itemLifetime)
                        GameObject.Destroy(item.gameObject);
                }
                else if (item.GetComponent<BaseWeapon>() != null)
                {
                    BaseWeapon i = item.GetComponent<BaseWeapon>();

                    if (i.GetIsPickedUp())
                        continue;

                    float time = Time.time - i.GetTimeCreated();

                    if (time > itemLifetime)
                        GameObject.Destroy(item.gameObject);
                }
            }
        }
    }

    void OnGUI()
    {
        GUI.skin = GameSubsystems.Instance.DefaultSkin;
        GameHUD.instance.DrawHUD();
        MenuHandler.instance.DrawMenus();
        if (changingLevel)
        {
            GUI.depth = 0;
            GUI.Window(10, new Rect(0, 0, Screen.width, Screen.height), delegate(int id)
            {
                GUI.color = Color.black * fadeAlpha;
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeoutTexture);
            }, "", GUIStyle.none);
        }
    }

    public static BaseCharacter GetNearestSandcastle(Vector3 position)
    {
        if (instance.SandcastleObjects.Count <= 1)
        {
            return instance.SandcastleObjects[0];
        }
        else
        {
            float dist = float.MaxValue;
            BaseCharacter nearest = null;
            foreach (BaseCharacter g in instance.SandcastleObjects)
            {
                if (g.GetIsAlive() && Vector3.Distance(g.transform.position, position) < dist)
                {
                    dist = Vector3.Distance(g.transform.position, position);
                    nearest = g;
                }
            }

            return nearest;
        }
    }

    public static PlayerController GetNearestPlayer(Vector3 position)
    {
        PlayerController[] players = instance.GetAlivePlayers();

        if (players.Length <= 0)
            return null;

        float dist = float.MaxValue;
        PlayerController nearest = null;
        foreach (PlayerController g in players)
        {
            if (g.GetIsAlive() && Vector3.Distance(g.transform.position, position) < dist)
            {
                dist = Vector3.Distance(g.transform.position, position);
                nearest = g;
            }
        }

        return nearest;
    }

    public PlayerController GetLocalPlayer() { return localPlayer; }

    public bool IsLocalPlayer(PlayerController player) { return (player == localPlayer); }

    public void CreatePlayer(bool local, int clientID)
    {
        PlayerSpawner spawn = GetRandomPlayerSpawner();
        PlayerController p = spawn.SpawnPlayer(PlayerObjectTemplate);

        if (local)
        {
            p.PlayerName = GameSubsystems.Instance.GetConfigValue<string>("Player", "Name");//GameSettings.playerName;
            localPlayer = p;
        }

        GameHandler.instance.PurchaseBuyable(buyables.Find(i=>i.Name == "Glock"), p, false);
    }

    public PlayerController[] GetAlivePlayers()
    {
        PlayerController[] ps = GameObject.FindObjectsOfType(typeof(PlayerController)) as PlayerController[];

        List<PlayerController> players = new List<PlayerController>();

        foreach (PlayerController p in ps) { if (p.GetIsAlive()) { players.Add(p); } };

        return players.ToArray();
    }

    public int GetNumAlivePlayers() { return GetAlivePlayers().Length; }
    public int GetAliveSandcastles() { return SandcastleObjects.FindAll(i => i.GetIsAlive()).Count; }

    public static bool IsPlayer(BaseCharacter character)
    {
        return character.GetComponent<PlayerController>() != null;
    }

    public bool IsGameActive() { return GameActive; }

    private void ProcessStyleDictionary()
    {
        foreach (StyleDefinition styleDef in StyleDictionary.styleDefinitions)
        {
            styles.Add(styleDef.styleName, styleDef.style);
        }
    }

    private void ProcessMapDictionary()
    {
        maps.AddRange(MapDictionary.mapDefinitions);
    }

    private void ProcessBuyablesDictionary()
    {
        buyables.AddRange(PurchaseDictionary.purchaseDefinitions);
    }

    public GUIStyle GetStyle(string name)
    {
        if (!styles.ContainsKey(name))
        {
            Debug.Log("didn't find style" + name);
            return GUIStyle.none;
        }

        return styles[name];
    }

    public List<MapDefinition> GetMapList() { return maps; }
    public MapDefinition GetMapDefinition(string name) { return maps.Find(i => i.MapName == name); }
    public List<BuyableDefinition> GetBuyables() { return buyables; }

    public void PurchaseBuyable(BuyableDefinition buyable, PlayerController player, bool deductCash)
    {
        if (deductCash)
        {
            if (player.cashAmount < buyable.Cost)
                return;

            player.DeductCash(buyable.Cost);
        }

        GameObject item = (GameObject)GameObject.Instantiate(buyable.Object, player.transform.position, player.transform.rotation);

        if (buyable.Type == PurchaseType.Weapon)
        {
            BaseWeapon w = item.GetComponent<BaseWeapon>();
            player.ForcePickupWeapon(w);
        }
        else if (buyable.Type == PurchaseType.Item)
        {
            BaseItem i = item.GetComponent<BaseItem>();
            player.ForcePickupItem(i);
        }
    }

    public void TransitionToLevel(float delay, string name)
    {
        fadeDelay = delay;
        changingLevel = true;
        nextLevelName = name;
        changeLevelTime = Time.time + 3f;
    }

    public static RaycastHit[] SortHitObjects(RaycastHit[] hits, GameObject[] filter)
    {
        if (hits.Length < 0)
            return null;

        List<RaycastHit> lhits = new List<RaycastHit>();
        lhits.AddRange(hits);

        lhits.Sort(delegate(RaycastHit p1, RaycastHit p2) { return p2.distance.CompareTo(p1.distance); });
        lhits.Reverse();

        foreach (GameObject g in filter)
        {
            lhits.RemoveAll(i => i.transform.gameObject == g);
        }

        if (hits.Length < 0)
            return null;

        return lhits.ToArray();
    }

    public static RaycastHit GetNearestHitObject(RaycastHit[] hits, GameObject[] filter)
    {
        return SortHitObjects(hits, filter)[0];
    }
}
