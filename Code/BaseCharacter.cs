using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseCharacter : GameEntity
{
    public bool IsEnabled = true;

    public float InitialHealth = 100;
    private float currentHealth = 100;
    public float CurrentHealth { get { return currentHealth; } }

    internal Dictionary<Weapon_AmmoType, int> WeaponAmmo;
    bool IsOnGround = true;
    bool IsAlive = true;

    // Use this for initialization
    void Start()
    {
        
    }
    
    void Awake()
    {
        currentHealth = InitialHealth;
    }

    // Update is called once per frame
    public void Update()
    {
        IsAlive = (currentHealth > 0);
    }

    void FixedUpdate()
    {
        IsOnGround = false;
    }

    public virtual bool GetIsAlive()
    {
        return IsAlive;
    }
    
    public virtual void OnHitWeapon(BaseWeapon weapon, BaseCharacter attacker, DamageInfo dmg)
    {
        currentHealth -= dmg.Damage;
    }

    public void DeductAmmo(Weapon_AmmoType type, int amount)
    {
        WeaponAmmo[type] = WeaponAmmo[type] - amount;
    }

    public void AddAmmo(Weapon_AmmoType type, int amount)
    {
        WeaponAmmo[type] = WeaponAmmo[type] + amount;
    }

    public int GetAmmoCount(Weapon_AmmoType type)
    {
        return WeaponAmmo[type];
    }

    protected void OnCollisionStay(Collision coll)
    {
        IsOnGround = true;
    }

    public bool GetIsOnGround() { return IsOnGround; }

    public virtual Vector3 GetWeaponTracerPosition() { return transform.position; }
}
