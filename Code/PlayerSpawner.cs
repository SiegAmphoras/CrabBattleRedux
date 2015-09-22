using UnityEngine;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
    public bool IsEnabled = true;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public PlayerController SpawnPlayer(GameObject template)
    {
        Vector3 pos = GetSpawnPosition();

        PlayerController p = (GameObject.Instantiate(template, pos, Quaternion.identity) as GameObject).GetComponent<PlayerController>();

        return p;
    }

    public Vector3 GetSpawnPosition()
    {
        Vector3 rayStart = transform.position;
        Vector3 rayDirection = -transform.up;

        Ray r = new Ray(rayStart, rayDirection);

        Vector3 spawnPosition = rayStart;
        RaycastHit hit;

        if (Physics.Raycast(r, out hit, 1000))
        {
            spawnPosition = hit.point;
        }

        return spawnPosition;
    }
}
