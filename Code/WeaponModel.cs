using UnityEngine;
using System.Collections;

public class WeaponModel : MonoBehaviour
{
    public BaseWeapon parentWeapon;

    // Use this for initialization
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlaySoundParented(string soundName)
    {
        AudioManager.PlaySoundParented(parentWeapon.owner.transform, soundName);
    }
}
