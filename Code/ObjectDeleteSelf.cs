using UnityEngine;
using System.Collections;

public class ObjectDeleteSelf : MonoBehaviour
{
    public float Delay;
    float createTime;

    // Use this for initialization
    void Awake()
    {
        createTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > createTime + Delay)
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}
