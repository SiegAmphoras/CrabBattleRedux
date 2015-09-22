using UnityEngine;
using System.Collections;

public class BaseItem : GameEntity
{
    public string ItemName;
    public string ItemCleanName;

    public bool Permanent = false;
    float createTime;

    public string PickupSoundName;

    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// OnPickup is called when a player collides with this BaseItem
    /// 
    /// Override this function to write a custom 'effect' to be applied to the player
    /// </summary>
    /// <param name="player">The player who picked this item up</param>
    public virtual void OnPickup(PlayerController player)
    {
        AudioManager.PlaySoundAt(transform.position, PickupSoundName);
        GameObject.Destroy(this.gameObject);
    }
}
