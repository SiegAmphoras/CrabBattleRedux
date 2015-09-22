using UnityEngine;
using System.Collections;

public class AISpawner : MonoBehaviour
{
    public bool IsEnabled = true;
    private GameObject crab;
    private Vector3 location;

    public Vector3 GetSpawnPosition()
    {
        Vector3 rayStart = transform.position;
        Vector3 rayDirection = -transform.up;

        Ray r = new Ray(rayStart, rayDirection);

        Vector3 spawnPosition = rayStart;
        RaycastHit hit;

        if (Physics.Raycast(r, out hit, 1000))
        {
            //Make sure the collider of the crab is above the ground plane, so it doesn't spawn clipped into the object under it
            spawnPosition = hit.point + new Vector3(0, crab.collider.bounds.size.y, 0);
        }

        return spawnPosition;
    }

    /*
        defatult(wave 1 difficutly 2) 30 second wave, 2-10 crabs
        * wave time (30 seconds) + wave Number *(difficulty/2)
        * crab amount (10) + (wave Number*5) (difficulty/2)
        * wave end time ((wave time*2+15) -(wave time)difficulty/2 ie 75-30 for easy(60 seconds after the last crab))
        */
    public void Spawn()
    {
        crab = (GameObject)Instantiate(Resources.Load("AI_Crab"));
        //change this to stop crab stacks
        location = GetSpawnPosition();
        location.x += Random.Range(-5, 5);
        location.z += Random.Range(-5, 5);
        crab.transform.position = location;
    }
}