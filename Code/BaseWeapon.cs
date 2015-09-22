using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Weapon_AmmoType
{
    RifleAmmo,
    PistolAmmo,
    GrenadeAmmo,

}

public enum DamageType
{
    Bullet,
    Slash,
    Explosion,
}

public struct DamageInfo
{
    public int Damage;
    public DamageType type;

    public Vector3 raycastStart;
    public Vector3 raycastEnd;
    public float raycastDistance;
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class BaseWeapon : GameEntity
{
    public string WeaponName;
    public string WeaponCleanName;

    internal BaseCharacter owner;

    [Header("Models")]
    public GameObject WeaponViewModel;
    public GameObject WeaponWorldModel;

    [Header("Weapon Information")]
    //TODO: Move these to a global environment dictionary or something; we shouldn't have to set these for each individual weapon (besides maybe the weapon tracer)
    public GameObject bulletHitParticle;
    public GameObject muzzleFlashParticle;
    public GameObject bulletTracerParticle;

    internal Animator weaponAnimator;

    public bool StartsPickedUp = false;
    internal bool IsPickedUp = false;

    public bool UsesModels = true;

    [Header("Weapon Firing Information")]

    public bool Semiauto = false;

    public float FireDelay;
    public float WeaponEffectiveDistance = 700;

    public DamageType DamageType;
    public int DamageAmount = 1;

    public int ShotsPerCycle = 1;

    public bool RampAccuracy = false;
    public float BaseAccuracy = 2;
    internal float currentaccuracy = 0;

    [Header("Weapon Magazine Information")]

    public bool ConsumesAmmo = true;
    public Weapon_AmmoType AmmoType;

    public bool ConsumesMagazineAmmo = true;

    public bool UsesMagazines = true;
    public bool AutoReloadWhenEmpty = false;
    public int MagazineCapacity = 10;
    internal int magazineCurrentCount;
    public int AmmoCount { get { return magazineCurrentCount; } }

    bool ammoTaken = false;

    bool reloading = false;
    float reloadTime;
    public float ReloadDelay = 3;

    bool firing = false;
    internal bool canFire = true;
    float lastFireTime;
    float lastAccCorrection;

    bool ownerIsPlayer = false;

    Vector3 viewmodelOffset;

    // Use this for initialization
    public void Start()
    {
        owner = GetComponentInParent<BaseCharacter>();

        if (owner != null && owner.GetComponent<PlayerController>() != null && UsesModels)
        {
            weaponAnimator = WeaponViewModel.GetComponent<Animator>();
        }

        magazineCurrentCount = MagazineCapacity;

        if (StartsPickedUp)
            IsPickedUp = true;
    }

    // Update is called once per frame
    public void Update()
    {
        if (owner != null)
            ownerIsPlayer = GameHandler.IsPlayer(owner);

        if (IsPickedUp)
        {
            if (UsesModels == true)
            {
                collider.enabled = false;
                rigidbody.detectCollisions = false;
                rigidbody.isKinematic = true;
            }

            //WeaponModel.layer = LayerMask.NameToLayer("WeaponViewModel");
            //SetLayerRecursively(gameObject, LayerMask.NameToLayer("WeaponViewModel"));
            //WeaponModel.rigidbody.active = false;
            if (UsesModels == true)
            {
                WeaponWorldModel.SetActive(false);
                WeaponViewModel.SetActive(true);

                CalcViewmodelPosition();
            }
        }
        else
        {
            if (UsesModels == true)
            {
                collider.enabled = true;
                rigidbody.detectCollisions = true;
                rigidbody.isKinematic = false;
            }

            //WeaponModel.layer = LayerMask.NameToLayer("Default");
            //SetLayerRecursively(gameObject, LayerMask.NameToLayer("Default"));
            //WeaponModel.rigidbody.active = true;
            if (UsesModels == true)
            {
                WeaponWorldModel.SetActive(true);
                WeaponViewModel.SetActive(false);
            }
        }

        if (firing)
        {
            if (!Semiauto)
            {
                if (Time.time > lastFireTime + FireDelay)
                {
                    firing = false;
                    if (UsesModels)
                    {
                        weaponAnimator.SetBool("Firing", false);
                    }
                }
            }

            if (Time.time > lastFireTime + 0.05f)
            {
                if (muzzleFlashParticle != null)
                    muzzleFlashParticle.SetActive(false);
            }
        }
        else
        {
            if (AutoReloadWhenEmpty && magazineCurrentCount <= 0)
            {
                if (Time.time > lastFireTime + 1.5f)
                {
                    //lastFireTime = Time.fixedTime - 1.5f;

                    Reload();
                }
            }

            if (RampAccuracy)
            {
                if (Time.time > lastAccCorrection + 0.5f)
                {
                    if (currentaccuracy > BaseAccuracy)
                        currentaccuracy -= (Time.deltaTime * 10);

                    if (currentaccuracy < BaseAccuracy)
                        currentaccuracy = BaseAccuracy;
                }
            }
        }

        if (reloading)
        {
            if (Time.time > reloadTime + ReloadDelay)
            {
                reloading = false;
                RefreshMagazine();
            }
        }
    }

    public virtual void Reload()
    {
        if (reloading || !canFire)
            return;

        //Setup for animation and delay before refreshing magazine
        if (!UsesMagazines)
            return;

        //Not enough ammo to reload, so return
        if (ConsumesAmmo && owner.GetAmmoCount(AmmoType) <= 0)
            return;

        //audioSource.PlayOneShot(reloadClip);
        reloading = true;

        if (UsesModels)
        {
            if (weaponAnimator != null)
                weaponAnimator.SetTrigger("Reload");
        }

        reloadTime = Time.fixedTime;
    }

    private void RefreshMagazine()
    {
        if (!ConsumesAmmo)
        {
            magazineCurrentCount = MagazineCapacity;
            return;
        }

        //Do actual ammo deduction here
        int ammoDifference = MagazineCapacity - magazineCurrentCount;
        int availableAmmo = owner.GetAmmoCount(AmmoType);

        if (availableAmmo >= ammoDifference)
        {
            owner.DeductAmmo(AmmoType, ammoDifference);
            magazineCurrentCount = MagazineCapacity;
        }
        else
        {
            owner.DeductAmmo(AmmoType, availableAmmo);
            magazineCurrentCount += availableAmmo;
        }
    }

    public virtual void Fire()
    {
        if (firing || !canFire)
            return;

        //Debug.Log("Fired!");
        //Used to interrupt reloads by attempting to fire
        //Use this for shotguns, and probably nothing else
        /*if (reloading)
        {
            reloading = false;
            return;
        }*/

        if (UsesMagazines && reloading)
            return;

        if (UsesMagazines && magazineCurrentCount <= 0)
        {
            if(Time.time > lastFireTime + 1.5f)
            {
                lastFireTime = Time.time;
                AudioManager.PlaySoundParented(owner.transform, WeaponName + "_DryFire");
                //Reload();
            }

            return;
        }

        lastFireTime = Time.time;
        //audioSource.PlayOneShot(fireClip);
        AudioManager.PlaySoundParented(owner.transform, WeaponName + "_Fire");

        firing = true;
        //Debug.Log("Fired!");

        if (UsesModels)
        {
            if (weaponAnimator != null)
            {
                /*weaponAnimator.SetTrigger("Fire");

                if(weaponAnimator.GetCurrentAnimatorStateInfo(0).IsName("fire"))
                    weaponAnimator.ForceStateNormalizedTime(0f);*/
                weaponAnimator.Play("fire", 0, 0);
            }

            if (muzzleFlashParticle != null)
                muzzleFlashParticle.SetActive(true);
        }

        FireBullet();
    }

    public virtual void FireBullet()
    {
        if (ConsumesMagazineAmmo)
        {
            magazineCurrentCount--;
        }

        for (int i = 0; i < ShotsPerCycle; i++)
        {
            Quaternion rayRotation = CalcuateShotSpread(owner.transform);
            Vector3 weaponPosition = owner.transform.position;

            Vector3 rayStart = owner.transform.position;
            Vector3 rayDirection = rayRotation * Vector3.forward;

            if (ownerIsPlayer)
            {
                rayRotation = CalcuateShotSpread((owner as PlayerController).viewCam.transform);
                rayStart = (owner as PlayerController).viewCam.transform.position;
                rayDirection = rayRotation * Vector3.forward;

                if (UsesModels && muzzleFlashParticle != null)
                {
                    weaponPosition = muzzleFlashParticle.transform.position;
                }
            }

            if (bulletTracerParticle != null)
                GameObject.Instantiate(bulletTracerParticle, owner.GetWeaponTracerPosition(), rayRotation);

            if (GameHandler.IsPlayer(owner))
            {
                (owner as PlayerController).stats.ShotsFired++;
            }

            Ray r = new Ray(rayStart, rayDirection);
            RaycastHit hit;

            RaycastHit[] hits = Physics.RaycastAll(r, WeaponEffectiveDistance);

            if (hits.Length <= 0)
                continue;

            hit = GameHandler.GetNearestHitObject(hits, new GameObject[] { owner.gameObject });

            GameObject hitObject = hit.transform.gameObject;
            PlayerController player = hitObject.GetComponent<PlayerController>();

            //Debug.Log("Hit object " + hitObject.name);

            Debug.DrawLine(rayDirection, hit.point, Color.red);
            Debug.DrawLine(hit.point, hit.point + (hit.normal * 5), Color.yellow);

            if (hitObject.GetComponent<BaseCharacter>() != null)
            {
                //Hit character
                BaseCharacter c = hitObject.GetComponent<BaseCharacter>();
                //Debug.Log("Hit Character " + hitObject.name);
                if (hitObject.GetComponent<BaseAICharacter>() != null)
                {
                    if (GameHandler.IsPlayer(owner))
                    {
                        (owner as PlayerController).stats.ShotsHit++;
                    }
                }

                c.OnHitWeapon(this, this.owner, new DamageInfo() { Damage = DamageAmount, type = DamageType });
            }
            else
            {
                //If isn't player object
                if (bulletHitParticle != null)
                    GameObject.Instantiate(bulletHitParticle, hit.point, Quaternion.LookRotation(Vector3.up, hit.normal));
                /*Debug.DrawLine(rayDirection, hit.point, Color.red);
                Debug.DrawLine(hit.point, hit.point + (hit.normal * 5), Color.yellow);*/
            }
        }

        if (RampAccuracy)
        {
            currentaccuracy += 1.5f;

            if (currentaccuracy > 5)
                currentaccuracy = 5;
        }
    }

    public Quaternion CalcuateShotSpread(Transform transform)
    {
        Quaternion newDir;
        Vector3 dirEuler = transform.rotation.eulerAngles;

        Vector2 rSpread = new Vector2(Random.Range(0, -currentaccuracy), Random.Range(-currentaccuracy, currentaccuracy));

        //Debug.Log(rSpread);

        //newDir = transform.forward + (rSpread.x * transform.right) + (-rSpread.y * transform.up);
        newDir = Quaternion.Euler(dirEuler.x + rSpread.x, dirEuler.y - rSpread.y, 0);

        return newDir;
    }

    public virtual void UnFire()
    {
        if (magazineCurrentCount <= 0)
        {
            lastFireTime = Time.fixedTime - 1.5f;

            /*if (AutoReloadWhenEmpty)
                Reload();*/
        }

        if (!Semiauto)
            return;

        firing = false;
    }

    public virtual void OnPickup(BaseCharacter character)
    {
        if (character.gameObject.GetComponent<PlayerController>() != null)
        {
            this.gameObject.transform.parent = (character as PlayerController).weaponCam.transform;
        }
        else
            this.gameObject.transform.parent = character.transform;


        this.owner = character;
        this.IsPickedUp = true;
        this.gameObject.transform.localPosition = Vector3.zero;
        this.gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);

        //TODO: Set world model not visible
        //TODO: Set view model visible

        if (UsesModels)
        {
            if (WeaponViewModel != null)
            {
                weaponAnimator = WeaponViewModel.GetComponent<Animator>();
            }
        }
    }

    public virtual void OnSwitchAway()
    {
        canFire = false;

        if (UsesModels)
        {
            if (weaponAnimator != null)
                weaponAnimator.SetTrigger("SwitchAway");
        }
    }

    public virtual void OnSwitchTo()
    {
        if (UsesModels)
        {
            if (weaponAnimator != null)
                weaponAnimator.SetTrigger("SwitchTo");
        }

        canFire = true;
    }

    public virtual void Drop()
    {
        collider.enabled = true;
        rigidbody.detectCollisions = true;
        rigidbody.isKinematic = false;
        rigidbody.AddForce((transform.forward * 5) + (Vector3.up * 5), ForceMode.Impulse);
        IsPickedUp = false;
        owner = null;
        ownerIsPlayer = false;
    }

    public void OnCollisionEnter(Collision col)
    {
        /*GameObject obj = col.gameObject;

        Debug.Log(obj);

        if (obj.GetComponent<PlayerController>() != null)
        {
            Debug.Log("Shit is happening");
        }*/
    }

    void SetLayerRecursively( GameObject obj, int newLayer  )
    {
        if (obj == null)
            return;

        obj.layer = newLayer;
   
        foreach(Transform child in obj.transform)
        {
            if (child == null)
                continue;

            SetLayerRecursively(child.gameObject, newLayer );
        }
    }

    public bool CanFire()
    {
        return !firing && canFire;
    }

    public bool IsFiring()
    {
        return firing;
    }

    public virtual void TakeAmmo(PlayerController player)
    {
        if (ammoTaken)
            return;

        player.AddAmmo(AmmoType, MagazineCapacity);
        ammoTaken = true;
    }

    public void CalcViewmodelPosition()
    {
        Vector3 rotationDelta = (owner as PlayerController).GetViewRotationDelta();

        viewmodelOffset += new Vector3(rotationDelta.y * 0.001f, -rotationDelta.x * 0.001f, 0);

        viewmodelOffset.x = Mathf.Clamp(viewmodelOffset.x, -0.1f, 0.1f);
        viewmodelOffset.y = Mathf.Clamp(viewmodelOffset.y, -0.1f, 0.1f);

        viewmodelOffset = Vector3.Lerp(viewmodelOffset, Vector3.zero, Time.deltaTime * 4);

        WeaponViewModel.transform.localPosition = viewmodelOffset;
    }

    public BaseCharacter GetOwner() { return owner; }

    public bool GetIsPickedUp() { return IsPickedUp; }
}
