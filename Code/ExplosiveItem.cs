using UnityEngine;
using System.Collections;

public class ExplosiveItem : PhysicsBullet
{
    public DamageType DamageType = DamageType.Explosion;
    public float ExplosionDelay = 5;
    public float ExplosionRadius = 20;
    public GameObject ExplosionParticle;

    public bool ExplodeOnCollision = false;

    public string ExplosionSoundName;

    float creationTime = 0;

    // Use this for initialization
    void Start()
    {
        creationTime = Time.fixedTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.fixedTime > creationTime + ExplosionDelay)
        {
            Explode();
            GameObject.DestroyImmediate(this.gameObject);
        }
    }

    public void OnCollisionEnter(Collision col)
    {
        if (ExplodeOnCollision)
        {
            Explode();
            GameObject.DestroyImmediate(this.gameObject);
        }
    }

    public void Explode()
    {
        GameObject particle = GameObject.Instantiate(ExplosionParticle) as GameObject;
        particle.transform.position = this.transform.position;

        Vector3 raycastStart = transform.position;

        //RaycastHit[] collisions = Physics.SphereCastAll(new Ray(raycastStart, Vector3.forward), ExplosionRadius);
        Collider[] col = Physics.OverlapSphere(raycastStart, ExplosionRadius);

        /*foreach (RaycastHit r in collisions)
        {
            GameObject obj = r.transform.gameObject;

            if (obj.GetComponent<BaseCharacter>() != null)
            {
                BaseCharacter ch = obj.GetComponent<BaseCharacter>();

                int damage = (int)(100 * (r.distance / ExplosionRadius));

                ch.OnHitWeapon((OriginWeapon != null) ? OriginWeapon : null, (OriginWeapon != null) ? OriginWeapon.owner : null, new DamageInfo() { type = DamageType, Damage = damage, raycastStart = raycastStart, raycastEnd = r.point, raycastDistance = r.distance }); 
            }
        }*/

        foreach (Collider c in col)
        {
            GameObject obj = c.gameObject;

            if (obj.GetComponent<BaseCharacter>() != null)
            {
                BaseCharacter ch = obj.GetComponent<BaseCharacter>();

                Vector3 hitPos = obj.transform.position;
                float distance = Vector3.Distance(hitPos, raycastStart);

                int damage = (int)(100 * (distance / ExplosionRadius));

                ch.OnHitWeapon((OriginWeapon != null) ? OriginWeapon : null, (OriginWeapon != null) ? OriginWeapon.GetOwner() : null, new DamageInfo() { type = DamageType, Damage = damage, raycastStart = raycastStart, raycastEnd = hitPos, raycastDistance = distance });
            }
        }

        AudioManager.PlaySoundAt(this.transform.position, ExplosionSoundName);
        //AudioSource.PlayClipAtPoint(ExplosionAudioClip, transform.position);
    }
}
