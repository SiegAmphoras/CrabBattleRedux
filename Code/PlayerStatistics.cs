using UnityEngine;
using System.Collections;

public class PlayerStatistics
{
    public int Kills = 0;
    public int Deaths = 0;
    public int BestWave = 0;
    public int ShotsFired = 0;
    public int ShotsHit = 0;
    public float Accuracy { get { return (ShotsHit / ShotsFired) * 1f; } }
}
