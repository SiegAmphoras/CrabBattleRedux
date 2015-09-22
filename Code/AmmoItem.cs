using UnityEngine;
using System.Collections;

public class AmmoItem : BaseItem
{
    public Weapon_AmmoType AmmoType;
    public int AmmoAmount;

    public override void OnPickup(PlayerController player)
    {
        player.AddAmmo(AmmoType, AmmoAmount);
        
        base.OnPickup(player);
    }
}
