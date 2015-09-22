using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnHandler
{
    int waveLevel = 0;

    float lastAISpawn;

    private float defaultWaveTimer = 30;
    private float betweenWaveCounter = 30;
    private float defaultWaveCounter = 0;
    private float currentWaveTimer;
    private float currentWaveCounter;

    private int crabSpawn = 0;
    private int crabsRemaining = 0;
    private int crabsKilled = 0;
    private int waveCrabCount = 0;

    private int spawnWaves = 2;
    private int currentSpawnWave = 0;
    public int difficulty = 2;

    bool waveCompleted = true;
    bool waveStarted = false;
    bool isIntermission = false;

    public bool IsIntermission { get { return isIntermission; } }
    public int CurrentWaveNumber { get { return (int)currentWaveCounter; } }
    public int WaveEnemyCount { get { return waveCrabCount; } }
    public int EnemiesKilled { get { return crabsKilled; } }
    public float IntermissionCounter { get { return betweenWaveCounter; } }

    List<int> waveCounts;

    public SpawnHandler()
    {
        //wait 30 seconds? for the game to start/player to get ready\
        currentWaveTimer = defaultWaveTimer;
        currentWaveCounter = defaultWaveCounter;

        waveCounts = new List<int>();
    }

    public void UpdateSpawns()
    {
        if (!waveCompleted)
        {
            if (currentSpawnWave < spawnWaves)
            {
                //there are still more crabs to be spawned in this wave
                currentWaveTimer -= Time.deltaTime;
                if (currentWaveTimer <= 0)
                //time is up, spawn more
                {
                    SpawnAI();
                    currentSpawnWave++;
                    currentWaveTimer = defaultWaveTimer / difficulty;
                }
            }

            isIntermission = false;
        }
        else if (waveCompleted)
        {
            betweenWaveCounter -= Time.deltaTime;
            if (betweenWaveCounter <= 0)
            {
                currentWaveCounter++;
                //Debug.Log(currentWaveCounter);
                betweenWaveCounter = defaultWaveTimer;// / difficulty;
                currentSpawnWave = 0;
                spawnWaves += difficulty;
                OnWaveStart();
                //start spawning next wave
            }
            if (betweenWaveCounter > 0)
            {
                IntermissionUpdate();
            }
        }
    }

    private void IntermissionUpdate()
    {
        //TODO: MP - force all buy menu?
        isIntermission = true;
    }

    private void SpawnAI()
    {
        crabSpawn = waveCounts[currentSpawnWave];//Random.Range(1, 5);
        //spawns between 1 and 5 crabs
        int spawner = Random.Range(0, GameHandler.instance.AISpawnPoints.Count);
        //randomely selects spawn point
        for (int i = 0; i < crabSpawn; i++)
        {
            GameHandler.instance.AISpawnPoints[spawner].Spawn();
        }
    }

    public void OnCrabDeath()
    {
        crabsRemaining--;
        crabsKilled++;

        if (crabsRemaining <= 0)
        {
            OnWaveEnd();
        }
    }

    public void OnWaveEnd()
    {
        waveCompleted = true;
        GameHUD.instance.AppendGameMessage("Wave " + currentWaveCounter + " defeated!");
        waveCrabCount = 0;
        crabsRemaining = 0;
        crabsKilled = 0;
    }

    public void OnWaveStart()
    {
        isIntermission = false;
        waveCompleted = false;
        GameHUD.instance.AppendGameMessage("Wave " + currentWaveCounter + " starting!");

        waveCounts.Clear();
        for (int i = 0; i < spawnWaves; i++)
        {
            int count = Random.Range(1, 5) * difficulty;
            waveCounts.Add(count);
            waveCrabCount += count;
        }

        crabsRemaining = waveCrabCount;

        currentWaveTimer = 0;
    }
}
