using UnityEngine;
using System.Collections;

public class PhysicsWeapon : BaseWeapon
{
    [Header("Physics Weapon Information")]
    public PhysicsBullet BulletObject;
    public float ThrowForce = 200;

    public bool TreatAsAmmo = false;
    public bool FireOnRelease = false;


    public override void FireBullet()
    {
        if (!FireOnRelease)
            FirePhysBullet();
    }

    public override void UnFire()
    {
        if (FireOnRelease)
        {
            if (UsesMagazines && magazineCurrentCount > 0)
            {
                weaponAnimator.SetTrigger("Unfire");
                FirePhysBullet();
            }
        }

        base.UnFire();
    }

    public void FirePhysBullet()
    {
        if (ConsumesMagazineAmmo)
        {
            magazineCurrentCount--;
        }

        for (int i = 0; i < ShotsPerCycle; i++)
        {
            Vector3 origin = owner.transform.position;

            if (GameHandler.IsPlayer(owner))
                origin = (owner as PlayerController).viewCam.transform.position;

            Vector3 spawnPos = origin + (owner.transform.forward * 0.75f);

            PhysicsBullet bullet = (PhysicsBullet)GameObject.Instantiate(BulletObject, spawnPos, owner.transform.rotation);
            Quaternion fwdRot = CalcuateShotSpread(owner.transform);

            if (GameHandler.IsPlayer(owner))
            {
                fwdRot = CalcuateShotSpread((owner as PlayerController).viewCam.transform);
            }

            if (RampAccuracy)
            {
                currentaccuracy += 1;
            }

            bullet.OriginWeapon = this;

            bullet.rigidbody.AddForce(((fwdRot * Vector3.forward) * ThrowForce) + (Vector3.up * ThrowForce / 4), ForceMode.Impulse);
        }
    }

    public override void TakeAmmo(PlayerController player)
    {
        if (TreatAsAmmo)
        {
            base.TakeAmmo(player);
            GameObject.Destroy(this.gameObject);
        }
    }
}
