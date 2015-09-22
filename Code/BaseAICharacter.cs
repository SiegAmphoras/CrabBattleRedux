using UnityEngine;
using System.Collections;

[System.Serializable]
enum Intent
{
    NULL,
    WalkTo,
    Attack,
    Flee,
    Wait,
}

[System.Serializable]
struct AIIntent
{
    public Intent intent;
    public BaseCharacter target;
    public BaseAICharacter owner;
    public Vector3 targetPosition { get { if (target != null) { return target.transform.position; } else return Vector3.zero; } }
    public float intentAssignTime;

    public bool DidCompleteIntent;

    public bool CheckIntentValid()
    {
        //We're supposed to have a target
        //If target is null, we're invalid cause our target doesn't exist anymore
        if (target == null || intent == Intent.Wait)
        {
            DidCompleteIntent = false;
            return false;
        }

        if (intent == (Intent.WalkTo | Intent.Flee)) //WalkTo and Flee are essentially the same thing; walk to a certain point and wait. At least we can check for Intent.Flee and see if our AICharacter is fleeing or not
        {
            //Supposed to walk to our target, we've reached our target, so our intent is completed and thus is not valid
            if (Vector3.Distance(owner.transform.position, targetPosition) < 3)
            {
                DidCompleteIntent = true;
                return false;
            }
        }

        if (intent == Intent.Attack)
        {
            //Supposed to attack our target, make sure our target is of an attackable type
            if (target.GetComponent<BaseCharacter>() != null)
            {
                BaseCharacter targetChar = target.GetComponent<BaseCharacter>();

                //Whatever we're attacking says its still alive, so our intent is still valid
                if (targetChar.GetIsAlive())
                {
                    DidCompleteIntent = false;
                    return true;
                }
                else //Whatever we were attacking is dead, so our intent is invalid
                {
                    DidCompleteIntent = true; //We wanted our target dead and it is, so we'll say we did our job correctly.
                    return false;
                }
            }
            else
            {
                //Target is not a character, we can't attack it...
                return false;
            }
        }

        return true;
    }

    public static bool operator ==(AIIntent a, AIIntent b)
    {
        return (a.intent == b.intent && a.target == b.target && a.targetPosition == b.targetPosition && a.intentAssignTime == b.intentAssignTime);
    }

    public static bool operator !=(AIIntent a, AIIntent b)
    {
        return (a.intent != b.intent || a.target != b.target || a.targetPosition != b.targetPosition || a.intentAssignTime != b.intentAssignTime);
    }

    public static AIIntent Null { get { return new AIIntent() { intent = Intent.NULL, target = null, intentAssignTime = float.MaxValue }; } }

    public float DistanceToTarget(Vector3 position)
    {
        Vector3 closestPos = target.collider.ClosestPointOnBounds(position);

        return Vector3.Distance(position, closestPos);
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }
}

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class BaseAICharacter : BaseCharacter
{
    public float MovementSpeed = 200;
    public float TurningSpeed = 2;

    public float AttackDistance = 2;

    float lastAITick;

    protected Vector3 movementVelocity;

    internal AIIntent currentIntent;

    // Use this for initialization
    void Start()
    {
        lastAITick = Time.fixedTime;
    }

    // Update is called once per frame
    public void FixedUpdate()
    {
        //AI is set to do its routines at 10 frames per second based on game time
        //Keeps it from processing its routines too quickly
        //Should lighten load on ai ticks
        if (Time.fixedTime > lastAITick + 0.1f)
        {
            Tick_AI();
        }

        ProcessIntent();

        transform.position += movementVelocity * Time.deltaTime;
    }

    //Tick_AI
    //Override this function in any child classes and place your AI routines in it
    //Function ticks 10 times per second as to not flood the game with routine updates
    public virtual void Tick_AI()
    {
        if (currentIntent == AIIntent.Null || !currentIntent.CheckIntentValid())
        {
            currentIntent = new AIIntent() { owner = this, intent = Intent.Wait, intentAssignTime = Time.fixedTime };
        }

        lastAITick = Time.fixedTime;
    }

    public virtual void ProcessIntent()
    {
        if (currentIntent.intent == Intent.WalkTo || currentIntent.intent == Intent.Flee || currentIntent.intent == Intent.Attack)
        {
            Vector3 targetPos = new Vector3(currentIntent.targetPosition.x, 0, currentIntent.targetPosition.z);
            Vector3 currentPos = new Vector3(transform.position.x, 0, transform.position.z);

            Quaternion TargetRotation = Quaternion.LookRotation(targetPos - currentPos);

            transform.rotation = Quaternion.Slerp(transform.rotation, TargetRotation, Time.deltaTime*TurningSpeed);

            if (Quaternion.Angle(transform.rotation, TargetRotation) < 10)
            {
                if (currentIntent.intent == Intent.Attack && Vector3.Distance(transform.position, currentIntent.targetPosition) < AttackDistance)
                {
                    //Stop moving
                    Vector3 forwardVel = new Vector3(transform.forward.x, 0, transform.forward.z);
                    movementVelocity *= 0.1f;
                }
                else
                {
                    Vector3 forwardVel = new Vector3(transform.forward.x, 0, transform.forward.z);
                    //rigidbody.AddForce((forwardVel * MovementSpeed) * Time.deltaTime);
                    movementVelocity *= 0.6f;
                    movementVelocity += (forwardVel * (MovementSpeed / 100));
                }
            }
        }
    }

    public override void OnHitWeapon(BaseWeapon weapon, BaseCharacter attacker, DamageInfo dmg)
    {
        //Vector3 dir = (dmg.raycastEnd - dmg.raycastStart).normalized;
        //rigidbody.AddForce((dir * dmg.Damage), ForceMode.Impulse);

        base.OnHitWeapon(weapon, attacker, dmg);
    }

    public Vector3 GetVelocity() { return movementVelocity; }

    public bool CanSeeObject(Vector3 rayStart, Vector3 rayDirection, float MaxDistance, GameObject gameObject)
    {
        Ray r = new Ray(rayStart, rayDirection);
        RaycastHit hit;

        RaycastHit[] hits = Physics.RaycastAll(r, MaxDistance);

        if (hits.Length <= 0)
            return false;

        hit = GameHandler.GetNearestHitObject(hits, new GameObject[] { this.gameObject });

        if (hit.collider.gameObject == gameObject)
            return true;

        return false;
    }
}
