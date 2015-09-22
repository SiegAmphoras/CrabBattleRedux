using UnityEngine;
using System.Collections;

public class AICrabbotCharacter : BaseAICharacter
{
    public BaseWeapon Weapon;
    public GameObject deathParticle;
    public GameObject teleportParticle;
    //AudioSource audioSrc;
    public Animator modelAnimator;

    public float walkMovementSpeed = 200;
    public float rollMovementSpeed = 500;

    bool IsRolling = false;
    bool Contracting = false;

    public float contractDelay = 0.2f;
    float contractTime;

    float lastDeployTime;
    public float DeployDelay = 0.5f;

    int hitCount = 0;
    float lastHitTime;

    // Use this for initialization
    void Start()
    {
        //audioSrc = this.gameObject.AddComponent<AudioSource>();
        //audioSrc.maxDistance = 100;

        GameObject.Instantiate(teleportParticle, transform.position, transform.rotation);
        AudioManager.PlaySoundAt(transform.position, "Teleport");
    }

    // Update is called once per frame
    new public void FixedUpdate()
    {
        base.FixedUpdate();

        //modelAnimator.SetBool("IsAttacking", CrabClaw.IsFiring());

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

        if (hitCount > 0)
        {
            if (Time.time > lastHitTime + 0.5)
            {
                hitCount = 0;
            }
        }

        if (IsRolling)
            MovementSpeed = rollMovementSpeed;
        else
            MovementSpeed = walkMovementSpeed;

        modelAnimator.SetBool("IsRolling", IsRolling);
    }

    //We calculate *how* we do what we want to do here
    public override void ProcessIntent()
    {
        //If we want to attack our target
        if (currentIntent.intent == Intent.Attack)
        {
            if (IsRolling)
            {
                Deploy();
            }

            if (Time.time > lastDeployTime + DeployDelay)
            {
                Quaternion TargetRotation = Quaternion.LookRotation(currentIntent.targetPosition - transform.position);

                //If we're close enough to our target to hit him
                if (Vector3.Distance(transform.position, currentIntent.targetPosition) < AttackDistance && Quaternion.Angle(transform.rotation, TargetRotation) < 10)
                {
                    if (CanSeeObject(transform.position, transform.forward, AttackDistance, currentIntent.target.gameObject))
                    {
                        //Debug.Log("Wants to fire!");
                        //If our 'weapon' is ready to attack
                        if (Weapon.CanFire() && Weapon.magazineCurrentCount > 0)
                        {
                            //Hit our target
                            Weapon.Fire();
                            modelAnimator.Play("fire", 0, 0);
                        }
                        else
                        {
                            Weapon.UnFire();
                        }
                    }
                    else
                    {
                        currentIntent = new AIIntent() { intent = Intent.WalkTo, owner = this, intentAssignTime = Time.fixedTime, target = currentIntent.target };
                    }
                }
            }
        }
        else if (currentIntent.intent == Intent.Flee)
        {
            if (!IsRolling)
            {
                if (!Contracting)
                {
                    Contracting = true;
                    contractTime = Time.time;
                    modelAnimator.SetTrigger("Contract");
                }
                else
                {
                    if (Time.time > contractTime + contractDelay)
                    {
                        IsRolling = true;
                        Contracting = false;
                    }
                }
            }
        }
        else if (currentIntent.intent == Intent.WalkTo)
        {
            if (CanSeeObject(transform.position, transform.position - currentIntent.target.transform.position, AttackDistance, currentIntent.target.gameObject))
            {
                currentIntent = new AIIntent() { intent = Intent.Attack, owner = this, intentAssignTime = Time.fixedTime, target = GameHandler.GetNearestPlayer(transform.position) };
            }
        }

        base.ProcessIntent();
    }

    private void Deploy()
    {
        IsRolling = false;
        Contracting = false;
        modelAnimator.SetTrigger("Deploy");

        movementVelocity = Vector3.zero;

        lastDeployTime = Time.time;
    }

    //We calculate what we want to do here
    public override void Tick_AI()
    {
        base.Tick_AI();

        //If our current intent has become invalid (usually if our target has died or doesn't exist anymore)
        if (!currentIntent.CheckIntentValid())
        {
            if (GameHandler.instance.GetNumAlivePlayers() > 0)
            {
                currentIntent = new AIIntent() { intent = Intent.Attack, owner = this, intentAssignTime = Time.fixedTime, target = GameHandler.GetNearestPlayer(transform.position) };
            }
            else
            {
                currentIntent = new AIIntent() { intent = Intent.Wait, owner = this, intentAssignTime = Time.fixedTime };
            }
        }
    }

    public override void OnHitWeapon(BaseWeapon weapon, BaseCharacter attacker, DamageInfo dmg)
    {
        //Test to see if the person who just shot us is closer than our current target (usually the sandcastle)
        if (GameHandler.IsPlayer(attacker))
        {
            hitCount++;
            lastHitTime = Time.time;

            if (hitCount > 5)
            {
                GameObject g = new GameObject("AITarget");

                g.transform.position = new Vector3(gameObject.transform.position.x + Random.Range(-20, 20), 0, gameObject.transform.position.z + Random.Range(-20, 20));

                AINullTarget target = g.AddComponent<AINullTarget>();

                currentIntent = new AIIntent() { intent = Intent.Flee, owner = this, intentAssignTime = Time.fixedTime, target = target };
            }

            /*if (attacker != currentIntent.target)
            {
                if (Vector3.Distance(transform.position, attacker.transform.position) < Vector3.Distance(transform.position, currentIntent.targetPosition))
                {
                    //If the attacker is close, switch to him as the target and attack him/her
                    currentIntent = new AIIntent() { owner = this, target = attacker, intent = Intent.Attack, intentAssignTime = Time.fixedTime };
                }
            }*/
        }

        base.OnHitWeapon(weapon, attacker, dmg);

        if (this.CurrentHealth <= 0)
        {
            if (GameHandler.IsPlayer(attacker))
            {
                (attacker as PlayerController).RewardForKill(600, this);
            }
        }
    }
}
