using UnityEngine;
using System.Collections;

public class GameEntity : MonoBehaviour
{
    float timeCreated;

    // Use this for initialization
    void Start()
    {
        timeCreated = Time.time;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public float GetTimeCreated() { return timeCreated; }
}
