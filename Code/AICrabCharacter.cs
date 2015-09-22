using UnityEngine;
using System.Collections;

public class AICrabCharacter : BaseAICharacter
{
    BaseWeapon CrabClaw;
    public AudioClip AttackSound;
    public GameObject deathParticle;
    public GameObject teleportParticle;
    //AudioSource audioSrc;
    public Animator modelAnimator;

    public float dropProbabilty = 0.1f;
    public GameObject[] ItemDrop;

    // Use this for initialization
    void Start()
    {
        //audioSrc = this.gameObject.AddComponent<AudioSource>();
        //audioSrc.maxDistance = 100;

        //Create a weapon that doesn't use a model or gameobject
        CrabClaw = new BaseWeapon()
        {
            WeaponName = "Weapon_CrabClaw",
            owner = this, //Set our owner, since we don't have a gameobject to set this
            UsesModels = false, //Don't use a model, because we'll use the character's model
            bulletHitParticle = null, //No bullethit particles...
            FireDelay = 1.5f, //Half a second delay between attacks
            IsPickedUp = true, //Need to tell the weapon that we're starting as being picked up
            WeaponEffectiveDistance = 5, //Attack distance
            //fireClip = AttackSound, //Attack sound
            //audioSource = audioSrc, //Set the audio source of the weapon to use the character's audio source
            UsesMagazines = false, //Set this up to be a melee weapon
            ConsumesAmmo = false,
            DamageAmount = 5,
            DamageType = DamageType.Slash,
        };

        GameObject.Instantiate(teleportParticle, transform.position, transform.rotation);
        AudioManager.PlaySoundAt(transform.position, "Teleport");
    }

    // Update is called once per frame
    new public void FixedUpdate()
    {
        base.FixedUpdate();

        modelAnimator.SetBool("IsAttacking", CrabClaw.IsFiring());
        //Debug.Log(GetVelocity());
        //Debug.Log(this.movementVelocity.normalized);
        //Debug.Log(this.movementVelocity.normalized.magnitude);
        modelAnimator.SetFloat("FwdVelocity", movementVelocity.normalized.magnitude);
    }

    public void Update()
    {
        if (CurrentHealth <= 0)
        {
            GameObject.Instantiate(deathParticle, transform.position, transform.rotation);
            GameObject.Destroy(gameObject);
        }

        if (!GameHandler.instance.IsGameActive())
        {
            GameObject.Instantiate(teleportParticle, transform.position, transform.rotation);
            GameObject.Destroy(gameObject);
        }

        CrabClaw.UsesMagazines = false;
        CrabClaw.UsesModels = false;
        CrabClaw.canFire = true;
        CrabClaw.Update(); //The crabclaw weapon is normally bound to a gameObject. Since it is not in this case, we need to call update manually.
    }

    //We calculate *how* we do what we want to do here
    public override void ProcessIntent()
    {
        //If we want to attack our target
        if (currentIntent.intent == Intent.Attack)
        {
            //If we're close enough to our target to hit him
            if (Vector3.Distance(transform.position, currentIntent.targetPosition) < AttackDistance)
            {
                //Debug.Log("Wants to fire!");
                //If our 'weapon' is ready to attack
                if (CrabClaw.CanFire())
                {
                    //Hit our target
                    CrabClaw.Fire();
                }
            }
        }

        base.ProcessIntent();
    }

    //We calculate what we want to do here
    public override void Tick_AI()
    {
        base.Tick_AI();

        //If our current intent has become invalid (usually if our target has died or doesn't exist anymore)
        if (!currentIntent.CheckIntentValid())
        {
            //Find the nearest castle
            BaseCharacter nearestCastle = GameHandler.GetNearestSandcastle(transform.position);
            //Set our intent to walk to that castle
            //TODO: This should be an Attack intent
            if (nearestCastle == null)
            {
                currentIntent = new AIIntent() { intent = Intent.Wait, intentAssignTime = Time.fixedTime, target = null };
            }
            else
            {
                currentIntent = new AIIntent() { intent = Intent.Attack, intentAssignTime = Time.fixedTime, target = nearestCastle };
            }
        }
    }

    public override void OnHitWeapon(BaseWeapon weapon, BaseCharacter attacker, DamageInfo dmg)
    {
        //Test to see if the person who just shot us is closer than our current target (usually the sandcastle)
        if (GameHandler.IsPlayer(attacker))
        {
            if (Vector3.Distance(transform.position, attacker.transform.position) < Vector3.Distance(transform.position, currentIntent.targetPosition))
            {
                //If the attacker is close, switch to him as the target and attack him/her
                currentIntent = new AIIntent() { owner = this, target = attacker, intent = Intent.Attack, intentAssignTime = Time.fixedTime };
            }
        }

        base.OnHitWeapon(weapon, attacker, dmg);

        if (this.CurrentHealth <= 0)
        {
            if (GameHandler.IsPlayer(attacker))
            {
                (attacker as PlayerController).RewardForKill(150, this);
                GameHandler.instance.spawnHandler.OnCrabDeath();

                //GameObject g = ItemDrop[Random.Range(0, ItemDrop.Length)];

                float r = Random.RandomRange(0.0f, 1.0f);

                if (r < dropProbabilty)
                {
                    GameObject g = (GameObject)GameObject.Instantiate(ItemDrop[Random.Range(0, ItemDrop.Length)], transform.position, transform.rotation);
                    g.rigidbody.AddForce(Vector3.up * 5, ForceMode.Impulse);
                }
            }
        }
    }
}
