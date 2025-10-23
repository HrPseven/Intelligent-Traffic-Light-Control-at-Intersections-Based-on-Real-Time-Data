using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Collections;


public class GameData
{
    // public int Density;
    public float Overall_speed;
    public int Duration;
    public int Pedestrians;
    public int Benchmarking;
    public int randomness_seed;
}


[System.Serializable]
public class GameDataFinal
{
    public string Date;
    // public int Density;
    public float Overall_speed;
    public int Duration; // in minutes
    public int Pedestrians;

    public TrafficResult AI; // Leave null if not AI phase
    public TrafficResult TR; // Leave null if not AI phase
}

[System.Serializable]
public class TrafficResult
{
    public float AverageWaitTime;
    public float EmergencyAverageWaitTime;
    public float AverageEmptyGreenLight;
    public float AerageQueueLength;
}

public class InitializerScript : MonoBehaviour
{
    public GameData gameData;
    private float startTime; // Store when the game started
    public GameObject pedestrianspath;
    public WalkPath[] walkPaths;
    private string filePath;
    private float elapsedTime;
    private Results Results;
    public int SpawnWeight;
    public int Overall_density;
    private AI_handler AI_handler;
    private float activeElapsedTime = 0f;

    public int seed = 2025;

    void Awake()
    {
        //Read the values from the JSON file.
        readJsonFile();
        seed = gameData.randomness_seed;
        UnityEngine.Random.InitState(seed);  // Set the global Unity random seed
        Debug.Log($"[RandomSeedController] Seed set to: {seed}");

        // if (gameData.Density == 1) { SpawnWeight = 0; Overall_density = 1; }
        // else if (gameData.Density == 2) { SpawnWeight = 2; Overall_density = 2; }
        // else if (gameData.Density == 2) { SpawnWeight = 0; Overall_density = 2; }
        // else if (gameData.Density == 3) { SpawnWeight = 1; Overall_density = 2; }
        // else if (gameData.Density == 4) { SpawnWeight = 1; Overall_density = 1; }
        // else if (gameData.Density == 5) { SpawnWeight = 2; Overall_density = 2; }

        AI_handler = FindFirstObjectByType<AI_handler>();

        if (gameData.Benchmarking == 0 || gameData.Benchmarking == 1)
        { 
            AI_handler.AI_Handler = true;
        }
        else
            AI_handler.AI_Handler = false;
    }

    void Start()
    {
        Results = FindFirstObjectByType<Results>();
        
        // Set the new value of crowdness.
        SetDensity(Overall_density);

        // Store the start time using real-world time (not affected by Time.timeScale)
        startTime = Time.realtimeSinceStartup;
        Debug.Log("Duration has been set to " + gameData.Duration / 60 + "m.");

        // The Overall speed is setted.
        OverallSpeed();

        // Checks if pedestrians are applied
        if (gameData.Pedestrians == 0)
        {
            Debug.Log("Pedestrians are NOT applied.");
            Pedestrians();
        }
        else
        {
            Debug.Log("Pedestrians are applied.");
        }

        // Populate randomelly.
        if (walkPaths != null && walkPaths.Length > 0)
        {
            foreach (WalkPath walkPath in walkPaths)
            {
                if (walkPath != null)
                {
                    // Apply the logic to each WalkPath
                    if (walkPath.par != null)
                    {
                        DestroyImmediate(walkPath.par);
                    }

                    if (walkPath.walkingPrefabs != null && walkPath.walkingPrefabs.Length > 0 && walkPath.walkingPrefabs[0] != null)
                    {
                        walkPath.SpawnPeople();
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the time that has passed since the start (active time)
        if (Time.timeScale > 0f)
        {
            activeElapsedTime += Time.deltaTime;
        }

        // Check if Benchmarking is 0, and if so, trigger the Duration() method after the set duration
        if ((gameData.Benchmarking == 0 || gameData.Benchmarking == 3) &&
            activeElapsedTime >= gameData.Duration) // convert minutes to seconds
        {
            writeJsonFile();
            Duration();
        }
    }

    // private void OnApplicationQuit()
    // {
    //     writeJsonFile((int)elapsedTime);
    // }

    void SetDensity(int density)
    {
        // Variables for ranges
        float densityIntensity;
        // Set ranges dynamically based on density
        switch (density)
        {
            case 2:
                densityIntensity = 0.03f;
                break;
            default:
                densityIntensity = 0.015f;
                break;
        }
        // Process all PeopleWalkPath objects
        // PeopleWalkPath[] peopleWalkPaths = FindObjectsOfType<PeopleWalkPath>();
        PeopleWalkPath[] peopleWalkPaths = FindObjectsByType<PeopleWalkPath>(FindObjectsSortMode.None);
        foreach (PeopleWalkPath peopleWalkPath in peopleWalkPaths)
        {
            if (peopleWalkPath != null)
            {
                peopleWalkPath.Density = densityIntensity;
                // print(peopleWalkPath.Density);
            }
        }

        // SpawnCar spawnCar = FindObjectOfType<SpawnCar>();
        // SpawnCar spawnCar = FindFirstObjectByType<SpawnCar>();
        // density = spawnCar.OverallDensity;
        // spawnCar.OverallDensity = density;
        Debug.Log("Cars and pedestrians density setted to: " + density);
    }

    void Pedestrians()
    {
        pedestrianspath.SetActive(false);
    }

    void Duration()
    {
        Debug.Log("Time is up. Closing the game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stops play mode in Unity Editor
#endif
    }

    void OverallSpeed()
    {
        Debug.Log("Speed set to " + gameData.Overall_speed + "x");
        Time.timeScale = gameData.Overall_speed;
        // Time.fixedDeltaTime = (gameData.Overall_speed / 100f ) / Time.timeScale;
    }

    void readJsonFile()
    {
        string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "AI-TR traffic system simulation/settings.json");
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            gameData = JsonUtility.FromJson<GameData>(jsonData);
            Debug.Log("The values have been read.");
        }
        else
        {
            Debug.LogError("JSON file not found at path: " + filePath);
        }
    }

    void writeJsonFile()
    {
        string filePath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
            "AI-TR traffic system simulation/TheLatestReturnedData.json"
        );

        float QueueLength = Results.AveStoppedVehicle;
        float AveWait = Results.AverageWait;
        float EmergencyAveWait = Results.AverageWaitService;
        float AveGreen = Results.Green_Light_Utilization;

        DateTime currentDateTime = DateTime.Now;

        GameDataFinal settings;

        // Step 1: Check if file exists and load previous data
        if (File.Exists(filePath))
        {
            string existingJson = File.ReadAllText(filePath);
            settings = JsonUtility.FromJson<GameDataFinal>(existingJson);
        }
        else
        {
            settings = new GameDataFinal();
        }

        // Step 2: Always update global fields
        settings.Date = currentDateTime.ToString();
        // settings.Density = gameData.Density;
        settings.Overall_speed = gameData.Overall_speed;
        settings.Duration = gameData.Duration;
        settings.Pedestrians = gameData.Pedestrians;

        // Step 3: Create result data
        TrafficResult resultData = new TrafficResult
        {
            AverageWaitTime = AveWait,
            EmergencyAverageWaitTime = EmergencyAveWait,
            AverageEmptyGreenLight = AveGreen,
            AerageQueueLength = QueueLength
        };

        // Step 4: Add AI or TR results depending on phase
        if (gameData.Benchmarking == 0)
        {
            settings.AI = resultData;
        }
        else
        {
            settings.TR = resultData;
        }

        // Step 5: Save updated JSON back to file
        string newJson = JsonUtility.ToJson(settings, true);
        File.WriteAllText(filePath, newJson);

        Debug.Log($"Updated results written to: {filePath}");
    }
}