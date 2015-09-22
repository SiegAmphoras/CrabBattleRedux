using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class GameHUD : MonoBehaviour
{
    Dictionary<Weapon_AmmoType, Texture> ammoTextures;
    List<KeyValuePair<float, string>> gameMessages;

    Texture castleTexture;
    Texture castleDestroyedTexture;
    Texture solidTexture;
    Texture crosshairTexture;
    Texture ammoTexture;
    Texture healthTexture;
    Texture cashTexture;

    bool showDebug = false;
    bool showUI = true;
    bool lockMouse = true;

    public static GameHUD instance;

    bool IsTouchingWeapon = false;
    bool drawWeaponMessage = false;
    float lastTouchTime;
    string weaponName;

    float gameMsgLifetime = 5;

    float damageAlpha = 0f;

    void Start()
    {
        gameMessages = new List<KeyValuePair<float, string>>(10);

        ammoTextures = new Dictionary<Weapon_AmmoType, Texture>()
        {
            {Weapon_AmmoType.RifleAmmo, Resources.Load<Texture>("textures/ui/rifle_ammo")},
        };

        castleTexture = Resources.Load<Texture>("textures/castle");
        castleDestroyedTexture = Resources.Load<Texture>("textures/castle_dest");
        solidTexture = Resources.Load<Texture>("textures/solid");
        crosshairTexture = Resources.Load<Texture>("textures/crosshair");
        healthTexture = Resources.Load<Texture>("textures/ui/health_icon");
        ammoTexture = Resources.Load<Texture>("textures/ui/ammo_icon");
        cashTexture = Resources.Load<Texture>("textures/ui/cash_icon");

        instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            showUI = !showUI;
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            showDebug = !showDebug;
        }

        if (GameHandler.instance.IsGameActive() && Input.GetKeyDown(KeyCode.Escape)) //Input.GetButtonDown("ShowMenu"))
        {
            MenuHandler.instance.ToggleMenuEnabled("PauseMenu");
        }

        lockMouse = !MenuHandler.instance.AreMenusActive();
        Screen.showCursor = !lockMouse;

        if(lockMouse)
        {
            Rect screen = WindowHelpers.GetWindowRectangle();

            WindowHelpers.SetCursorPosition((int)(screen.x + (screen.width / 2)), (int)(screen.y + (screen.height / 2)));
        }

        try
        {
            foreach(KeyValuePair<float,string> msg in gameMessages)
            {
                if (Time.time > msg.Key + gameMsgLifetime)
                {
                    gameMessages.Remove(msg);
                }
            }
        }
        catch { }
    }

    public void DrawHUD()
    {
        if (GameHandler.instance == null)
            return;

        if (showUI && GameHandler.instance.IsGameActive() && GameHandler.instance.RunGameLogic)
        {
            if (showDebug)
                DrawDebugInfo();

            DrawCastleStatus();
            DrawPlayerInfo();
            DrawWaveInfo();
            DrawMessages();

            if (IsTouchingWeapon)
            {
                if (Time.time > lastTouchTime + 0.1f)
                {
                    IsTouchingWeapon = false;
                    drawWeaponMessage = false;
                }
                else
                    drawWeaponMessage = true;
            }

            if(drawWeaponMessage)
                GUI.Label(new Rect(0, Screen.height - 96, Screen.width, 64), "Press USE to pick up " + weaponName, GameHandler.instance.GetStyle("hud_default"));
        }
    }

    private void DrawWaveInfo()
    {
        if (!GameHandler.instance.spawnHandler.IsIntermission)
        {
            GUI.Label(new Rect(Screen.width / 2 - 64, 32, 128, 32), "Wave: " + GameHandler.instance.spawnHandler.CurrentWaveNumber, GameHandler.instance.GetStyle("hud_default"));
            GUI.Label(new Rect(Screen.width / 2 - 64, 64, 128, 32), GameHandler.instance.spawnHandler.WaveEnemyCount + " | " + GameHandler.instance.spawnHandler.EnemiesKilled, GameHandler.instance.GetStyle("hud_default"));
        }

        if (GameHandler.instance.spawnHandler.IsIntermission)
        {
            TimeSpan time = TimeSpan.FromSeconds(GameHandler.instance.spawnHandler.IntermissionCounter);

            GUI.Label(new Rect(Screen.width / 2 - 64, Screen.height - 96, 128, 32), "INTERMISSION", GameHandler.instance.GetStyle("hud_default"));
            GUI.Label(new Rect(Screen.width / 2 - 64, Screen.height - 64, 128, 32), new DateTime(time.Ticks).ToString(@"mm:ss"), GameHandler.instance.GetStyle("hud_default"));
        }
    }

    private void DrawMessages()
    {
        Rect r = new Rect(32, Screen.height - 96, 500, 32);

        int i = 0;
        List<KeyValuePair<float, string>> msgs = new List<KeyValuePair<float, string>>();
        msgs.AddRange(gameMessages.ToArray());
        msgs.Reverse();

        foreach (KeyValuePair<float, string> msg in msgs)
        {
            Rect lr = new Rect(r.x, r.y - (r.height * i), r.width, r.height);

            GUI.Label(lr, msg.Value, GameHandler.instance.GetStyle("hud_default_left"));
            i++;
        }
    }

    public void DrawPickupMessage(BaseWeapon weapon)
    {
        //GUI.Label(new Rect(0, Screen.height - 96, Screen.width, 64), "Press USE to pick up " + weapon.WeaponName, GameHandler.instance.GetStyle("hud_message"));
        IsTouchingWeapon = true;
        lastTouchTime = Time.time;
        weaponName = weapon.WeaponCleanName;
    }

    public void DrawPlayerInfo()
    {
        PlayerController ply = GameHandler.instance.GetLocalPlayer();

        DrawHealthOverlay();

        //GUI.DrawTexture(new Rect((Screen.width / 2) - 32, (Screen.height / 2) - 32, 64, 64), crosshairTexture);
        DrawCrossHair();

        string health = "" + ply.CurrentHealth;

        string ammo = "";

        if (GameHandler.instance.GetLocalPlayer().CurrentWeapon != null)
            ammo = ply.CurrentWeapon.AmmoCount + "  |  " + ply.GetAmmoCount(ply.CurrentWeapon.AmmoType);

        if (ply.CurrentHealth > 30)
        {
            GUI.Label(new Rect(64, Screen.height - 64, 128, 32), health, GameHandler.instance.GetStyle("hud_default"));
        }
        else
        {
            GUIStyle style = new GUIStyle(GameHandler.instance.GetStyle("hud_default"));
            style.normal.textColor = new Color(1, 0, 0, 1f);
            GUI.Label(new Rect(64, Screen.height - 64, 128, 32), health, style);

            float xOff = Random.Range(-8, 8);
            float yOff = Random.Range(-8, 8);

            style.normal.textColor = new Color(1, 0, 0, 0.75f);
            GUI.Label(new Rect(64 + xOff, Screen.height - 64 + yOff, 128, 32), health, style);
        }


        GUI.color = GameHandler.instance.GetStyle("hud_default").normal.textColor;
        GUI.DrawTexture(new Rect(16, Screen.height - 64, 32, 32), healthTexture);

        if (GameHandler.instance.GetLocalPlayer().CurrentWeapon != null)
            GUI.Label(new Rect(Screen.width - 64 - 128, Screen.height - 64, 128, 32), ammo, GameHandler.instance.GetStyle("hud_default"));

        GUI.color = GameHandler.instance.GetStyle("hud_default").normal.textColor;
        GUI.DrawTexture(new Rect(Screen.width - 16 - 32, Screen.height - 64, 32, 32), ammoTexture);

        GUI.Label(new Rect(Screen.width - 64 - 128, 64, 128, 32), ""+ply.cashAmount, GameHandler.instance.GetStyle("hud_default"));

        GUI.color = GameHandler.instance.GetStyle("hud_default").normal.textColor;
        GUI.DrawTexture(new Rect(Screen.width - 64 - 32 - 128, 64, 32, 32), cashTexture);
    }

    public void DrawHealthOverlay()
    {
        GUI.color = new Color(1, 0, 0, damageAlpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), solidTexture);

        damageAlpha -= 0.1f * Time.deltaTime;

        if (damageAlpha < 0)
            damageAlpha = 0;
    }

    public void DrawCrossHair()
    {
        float screenCenterX = (Screen.width / 2);
        float screenCenterY = (Screen.height / 2);

        float accuracy = 1;

        if(GameHandler.instance.GetLocalPlayer().CurrentWeapon != null)
            accuracy = GameHandler.instance.GetLocalPlayer().CurrentWeapon.currentaccuracy;

        float accuracyDistance = 4 + (4 * Mathf.Pow(accuracy, 2));

        if (accuracyDistance > 32)
            accuracyDistance = 32;

        GUI.color = new Color(1,1,1,0.5f);
        GUI.DrawTexture(new Rect(screenCenterX - 1, screenCenterY - accuracyDistance - 16, 2, 16), solidTexture);
        GUI.DrawTexture(new Rect(screenCenterX - accuracyDistance - 16, screenCenterY - 1, 16, 2), solidTexture);
        GUI.DrawTexture(new Rect(screenCenterX - 1, screenCenterY + accuracyDistance, 2, 16), solidTexture);
        GUI.DrawTexture(new Rect(screenCenterX + accuracyDistance, screenCenterY - 1, 16, 2), solidTexture);
    }

    public void DrawCastleStatus()
    {
        int castles = GameHandler.instance.SandcastleObjects.Count;

        float yOffset = 0;

        for (int i = 0; i < castles; i++)
        {
            yOffset = 16 + (64 * i);

            BaseCharacter c = GameHandler.instance.SandcastleObjects[i];
            float health = c.CurrentHealth / c.InitialHealth;

            float healthBarWidth = 128 * health;

            //Debug.Log(health + " " + healthBarWidth);

            if (health > 0)
            {
                GUI.DrawTexture(new Rect(16, yOffset, 64, 64), castleTexture);
                GUI.color = new Color(128, 0, 0);
                GUI.DrawTexture(new Rect(16 + 64, yOffset + 32 - 4, healthBarWidth, 8), solidTexture);
                GUI.color = Color.white;
            }
            else
            {
                GUI.DrawTexture(new Rect(16, yOffset, 64, 64), castleDestroyedTexture);
            }
        }
    }

    private void DrawDebugInfo()
    {
        GUI.Label(new Rect(16, 16, 128, 32), (1 / Time.deltaTime) + " fps", GameHandler.instance.GetStyle("hud_default_left"));
        GUI.Label(new Rect(16, 48, 128, 32), GameHandler.instance.ObjectsInScene + " objects", GameHandler.instance.GetStyle("hud_default_left"));
    }

    public bool MouseEnabled() { return !lockMouse; }

    public void AppendGameMessage(string msg)
    {
        gameMessages.Add(new KeyValuePair<float, string>(Time.time, msg));
    }

    public void AddDamageHit(float damage) { if (damage < 0.25f) { damage = 0.25f; } damageAlpha += damage; }
}

