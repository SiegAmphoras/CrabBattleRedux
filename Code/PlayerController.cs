using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class PlayerController : BaseCharacter
{
    public string PlayerName;

    public float MovementSpeed = 400;

    public GameObject view;
    public Camera viewCam;
    public Camera weaponCam;

    public int cashAmount = 0;

    int selectedWeapon = 0;

    Vector3 movementVelocity;

    BaseWeapon[] Weapons = new BaseWeapon[] { null, null };
    public BaseWeapon CurrentWeapon { get { return Weapons[selectedWeapon]; } }

    public PlayerStatistics stats;

    Vector3 viewRotationDelta;
    Quaternion viewRotationVelocity;

    // Use this for initialization
    void Start()
    {
        WeaponAmmo = new Dictionary<Weapon_AmmoType, int>()
        {
            {Weapon_AmmoType.RifleAmmo, 140},
            {Weapon_AmmoType.PistolAmmo, 100},
            {Weapon_AmmoType.GrenadeAmmo, 10},
        };

        stats = new PlayerStatistics();

        viewRotationVelocity = new Quaternion();
    }

    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!GetIsAlive())
        {
            if(CurrentWeapon != null)
                this.DropCurrentWeapon();
            //GameObject.Destroy(this);
        }

        if (!IsEnabled)
        {
            return;
        }

        //Enable input if our mouse isn't active according to the hud, otherwise don't take input cause we're in a menu or something...
        if (!GameHUD.instance.MouseEnabled() && GetIsAlive())
        {
            if (Weapons[selectedWeapon] != null)
            {
                Weapons[selectedWeapon].transform.localPosition = Vector3.zero;
                Weapons[selectedWeapon].transform.localRotation = Quaternion.Euler(Vector3.zero);

                if (GameSubsystems.Instance.Input.GetKey("Fire"))
                {
                    Weapons[selectedWeapon].Fire();
                }
                else if (GameSubsystems.Instance.Input.GetKeyUp("Fire"))
                    Weapons[selectedWeapon].UnFire();

                if (GameSubsystems.Instance.Input.GetKeyDown("Reload"))
                {
                    Weapons[selectedWeapon].Reload();
                }
            }

            if (GameSubsystems.Instance.Input.GetKeyDown("Switch Weapon"))
            {
                if (selectedWeapon == 0 && Weapons[1] != null)
                    SwitchToWeapon(1);
                else if (selectedWeapon == 1 && Weapons[0] != null)
                    SwitchToWeapon(0);
            }

            if (GameSubsystems.Instance.Input.GetKeyDown("Buy Menu"))
            {
                MenuHandler.instance.SetMenuEnabled("BuyMenu", true);
            }

            float yawDelta = Input.GetAxis("Mouse X");
            float pitchDelta = Input.GetAxis("Mouse Y");

            viewRotationDelta = new Vector3(pitchDelta, yawDelta, 0);

            Vector3 eulerRot = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(eulerRot + new Vector3(0, yawDelta, 0));
            view.transform.localRotation = Quaternion.Euler(view.transform.localRotation.eulerAngles + new Vector3(-pitchDelta, 0, 0));
            weaponCam.transform.localRotation = view.transform.localRotation;

            float forwardMove = Input.GetAxis("Vertical");
            float sideMove = Input.GetAxis("Horizontal");

            Vector3 inputVelocity = ((transform.forward * forwardMove) + (transform.right * sideMove)).normalized;

            if (GameSubsystems.Instance.Input.GetKeyDown("Jump") && GetIsOnGround())
            {
                rigidbody.AddForce(Vector3.up * 5f, ForceMode.Impulse);
            }

            if (GetIsOnGround())
            {
                movementVelocity += (inputVelocity * (MovementSpeed / 100));
            }
            else
            {
                //movementVelocity.y += Physics.gravity.y * Time.fixedDeltaTime;
            }
        }

        viewCam.transform.localRotation = viewRotationVelocity;
        viewRotationVelocity = Quaternion.RotateTowards(viewRotationVelocity, Quaternion.identity, 50 * Time.deltaTime);

        //Do wave count statistics
        if (GameHandler.instance.spawnHandler.CurrentWaveNumber > stats.BestWave)
            stats.BestWave = GameHandler.instance.spawnHandler.CurrentWaveNumber;

        if (GetIsOnGround())
            movementVelocity *= 0.6f;

        transform.position += movementVelocity * Time.deltaTime;

        base.Update();
    }

    void OnPickupWeapon(BaseWeapon weapon)
    {
        if (Weapons[0] == null || Weapons[1] == null)
        {
            if (Weapons[0] == null)
            {
                //Debug.Log("Picking up into slot 0");
                PickupWeapon(weapon, 0);
                SwitchToWeapon(0);
            }
            else if (Weapons[1] == null)
            {
                //Debug.Log("Picking up into slot 1");
                PickupWeapon(weapon, 1);
                SwitchToWeapon(1);
            }
        }
        /*else
        {
            Debug.Log("Swapping weapon");
            DropCurrentWeapon();
            PickupWeapon(weapon, selectedWeapon);
        }*/
    }

    void PickupWeapon(BaseWeapon weapon, int slot)
    {
        weapon.OnPickup(this);
        Weapons[slot] = weapon;

        SwitchToWeapon(slot);
    }

    void DropCurrentWeapon()
    {
        Weapons[selectedWeapon].transform.parent = null;
        Weapons[selectedWeapon].Drop();
    }

    void OnCollisionEnter(Collision col)
    {
        if (!GetIsAlive())
            return;

        //Debug.Log("Shit is happening");

        GameObject obj = col.gameObject;

        //Debug.Log(obj);
        GameObject weaponObj = null;
        BaseWeapon weapon = null;

        if (ObjectHeirarchyHasComponent<BaseWeapon>(obj, out weapon, out weaponObj))
        {
            //Debug.Log("Is weapon");
            //PickupWeapon(weapon, selectedWeapon);
            //OnPickupWeapon(weapon);
            if (Weapons[selectedWeapon] == null)
                OnPickupWeapon(weapon);
            else if (HasWeapon(weapon))
            {
                weapon.TakeAmmo(this);
                GameHUD.instance.AppendGameMessage("Picked up " + weapon.WeaponCleanName + " ammo");
                AudioManager.PlaySoundAt(transform.position, "Ammo_Grab");
            }
        }
        else if(obj.GetComponent<BaseItem>() != null)
        {
            BaseItem item = obj.GetComponent<BaseItem>();
            item.OnPickup(this);
            GameHUD.instance.AppendGameMessage("Picked up " + item.ItemCleanName);
        }
    }

    void OnCollisionStay(Collision col)
    {
        base.OnCollisionStay(col);

        GameObject obj = col.gameObject;

        //Debug.Log(obj);
        GameObject weaponObj = null;
        BaseWeapon weapon = null;

        if (ObjectHeirarchyHasComponent<BaseWeapon>(obj, out weapon, out weaponObj))
        {
            if (!HasWeapon(weapon))
            {
                GameHUD.instance.DrawPickupMessage(weapon);

                if (GameSubsystems.Instance.Input.GetKeyDown("Use"))
                {
                    if (!HasOpenWeaponSlot())
                    {
                        DropCurrentWeapon();
                        PickupWeapon(weapon, selectedWeapon);
                    }
                    else
                    {
                        PickupWeapon(weapon, GetOpenWeaponSlot());
                    }
                }
            }
        }
    }

    public bool ObjectHeirarchyHasComponent<T>(GameObject obj, out T component, out GameObject componentObj)  where T : Component
    {
        bool res = false;
        GameObject curObj = obj;

        component = default(T);
        componentObj = null;

        while (res == false)
        {
            if (curObj.GetComponent(typeof(T)) != null)
            {
                res = true;
                component = curObj.GetComponent<T>();
                componentObj = curObj;
                break;
            }
            else if (curObj.transform.parent != null)
            {
                GameObject parent = curObj.transform.parent.gameObject;
                curObj = parent;
                continue;
            }
            else if (curObj.transform.parent == null && curObj.GetComponent<T>() == null)
            {
                res = false;
                break;
            }
        }

        return res;
    }

    public override void OnHitWeapon(BaseWeapon weapon, BaseCharacter attacker, DamageInfo dmg)
    {
        //Vector3 dir = (dmg.raycastEnd - dmg.raycastStart).normalized;
        //rigidbody.AddForce((dir * dmg.Damage), ForceMode.Impulse);
        if (GameHandler.instance.IsLocalPlayer(this))
        {
            GameHUD.instance.AddDamageHit(dmg.Damage / 100);
            ViewPunch(new Vector3(UnityEngine.Random.Range(-15, 15), UnityEngine.Random.Range(-15, 15), 0));
        }

        base.OnHitWeapon(weapon, attacker, dmg);
    }

    public void SwitchToWeapon(int newWeaponSlot)
    {
        if (newWeaponSlot == selectedWeapon)
            return;

        Weapons[selectedWeapon].active = false;
        Weapons[newWeaponSlot].active = true;
        selectedWeapon = newWeaponSlot;
    }

    public void RewardForKill(int rewardAmount, BaseCharacter victim)
    {
        cashAmount += rewardAmount;
        stats.Kills++;
    }

    public void DeductCash(int amount)
    {
        cashAmount -= amount;
    }

    public bool HasWeapon(BaseWeapon weapon)
    {
        if (Weapons[0] == null && Weapons[1] == null)
            return false;

        if (Weapons[0] != null)
        {
            if (Weapons[0].WeaponName == weapon.WeaponName)
                return true;
        }

        if (Weapons[1] != null)
        {
            if (Weapons[1].WeaponName == weapon.WeaponName)
                return true;
        }

        return false;
    }

    public bool HasOpenWeaponSlot()
    {
        return Weapons[0] == null || Weapons[1] == null;
    }

    public int GetOpenWeaponSlot()
    {
        if (Weapons[0] == null)
            return 0;

        if (Weapons[1] == null)
            return 1;

        return -1;
    }

    public void ForcePickupItem(BaseItem item)
    {
        item.OnPickup(this);
        GameHUD.instance.AppendGameMessage("Picked up " + item.ItemName);
    }

    public void ForcePickupWeapon(BaseWeapon weapon)
    {
        if (HasWeapon(weapon))
        {
            weapon.TakeAmmo(this);
            GameHUD.instance.AppendGameMessage("Picked up " + weapon.WeaponName + " ammo");
            AudioManager.PlaySoundAt(transform.position, "Ammo_Grab");
            return;
        }

        if (!HasOpenWeaponSlot())
        {
            DropCurrentWeapon();
            PickupWeapon(weapon, selectedWeapon);
        }
        else
        {
            PickupWeapon(weapon, GetOpenWeaponSlot());
        }
    }

    public override Vector3 GetWeaponTracerPosition()
    {
        return viewCam.transform.position;
    }

    public Vector3 GetViewRotationDelta()
    {
        return viewRotationDelta;
    }

    public void ViewPunch(Vector3 angle)
    {
        viewRotationVelocity = Quaternion.Euler(viewRotationVelocity.eulerAngles + angle);
    }
}
