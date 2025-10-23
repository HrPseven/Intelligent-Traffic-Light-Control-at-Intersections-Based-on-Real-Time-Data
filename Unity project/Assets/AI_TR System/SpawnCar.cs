using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class SpawnCar : MonoBehaviour
{
    public CarWalkPath[] carWalkPathsOne;
    public PeopleWalkPath[] peopleWalkPathsOne;
    public float OverallDensity = 1; 
    // public int SpawnWeight = -1;
    public float IndivitualDensityN, IndivitualDensityS, IndivitualDensityW, IndivitualDensityE;
    private bool One, One1, One2, One3;
    private bool Two, Two1, Two2, Two3;
    private bool Three, Three1, Three2, Three3;
    public LineChecker[] lineChecker;
    private int change_density = 0;
    private InitializerScript InitializerScript;
    private Lane_Demand Lane_Demand;
    private Visuals Visuals;

    void Awake()
    { 
        InitializerScript = FindFirstObjectByType<InitializerScript>();
        Lane_Demand = FindFirstObjectByType<Lane_Demand>();
        Visuals = FindFirstObjectByType<Visuals>();
       
    }


    void Start()
    {
         line_demand();
        
        // OverallDensity = InitializerScript.Overall_density;
        StartCoroutine(density_cotroller(change_density));
        StartCoroutine(DensityController());
        StartCoroutine(DensityControllerPeople());
    }

    void line_demand()
    {
        System.Random rng = new System.Random(InitializerScript.seed); // Use a fixed seed
        float[] possibleValues = { 10f, 4f, 1.6f };
        for (int i = possibleValues.Length - 1; i > 0; i--)
        {
            int randomIndex = rng.Next(0, i + 1);
            (possibleValues[i], possibleValues[randomIndex]) = (possibleValues[randomIndex], possibleValues[i]);
        }
        IndivitualDensityN = possibleValues[0];
        IndivitualDensityS = possibleValues[1];
        IndivitualDensityW = possibleValues[2];

        // Assign the fourth spot randomly from the three values
        IndivitualDensityE = possibleValues[rng.Next(0, possibleValues.Length)];

        // SpawnWeight = InitializerScript.SpawnWeight;
        // if (SpawnWeight == 0) IndivitualDensityE = 1.6f;
        // else if (SpawnWeight == 1) IndivitualDensityE = 4f;
        // else if (SpawnWeight == 2) IndivitualDensityE = 10f;
    }

    public void Generate(int path)
    {
        carWalkPathsOne[path].CarGenerator();
    }

    public void PeopleGenerate(int path)
    {
        peopleWalkPathsOne[path].PeopleGenerator();
    }

    IEnumerator DensityControllerPeople()
    {
        float wait_time = (OverallDensity * 7) + 4;
        
        yield return new WaitForSeconds(wait_time); 
        // switch (OverallDensity)
        // {
        //     case 2:
        //         yield return new WaitForSeconds(4f);

        //         break;
        //     default:
        //         yield return new WaitForSeconds(6f);
        //         break;
        // }

        int randomNumberPeople = UnityEngine.Random.Range(0, peopleWalkPathsOne.Length);
        PeopleGenerate(randomNumberPeople);
        StartCoroutine(DensityControllerPeople());
    }

    IEnumerator density_cotroller(int mode)
    {
        if (mode == 6) { mode = 0;   change_density = 0; }
        // Debug.Log("mode: " + mode);

        int wait_time = 1;

        if (mode == 0) { OverallDensity = 0.3f; wait_time = 120;}
        else if (mode == 1) { OverallDensity = 0.2f; wait_time = 120;}
        else if (mode == 2) { OverallDensity = 0.175f; wait_time = 160;}
        else if (mode == 3) { OverallDensity = 0.15f; wait_time = 160;}
        else if (mode == 4) { OverallDensity = 0.1f; wait_time = 160;}
        else if (mode == 5) { OverallDensity = 0f; wait_time = 240; }
        
        // if (mode == 0) { OverallDensity = 0.15f; wait_time = 120;}
        // else if (mode == 1) { OverallDensity = 0.15f; wait_time = 120;}
        // else if (mode == 2) { OverallDensity = 0.15f; wait_time = 160;}
        // else if (mode == 3) { OverallDensity = 0.15f; wait_time = 160;}
        // else if (mode == 4) { OverallDensity = 0.15f; wait_time = 160;}
        // else if (mode == 5) { OverallDensity = 0.15f; wait_time = 240; }

        Visuals.density_change();
        Lane_Demand.update_laneDemand();

        yield return new WaitForSeconds(wait_time);
        
        InitializerScript.seed += 1;
        UnityEngine.Random.InitState(InitializerScript.seed);
        Debug.Log("seed: " + InitializerScript.seed);
        line_demand();

        change_density += 1;

        StartCoroutine(density_cotroller(change_density));
    }

    IEnumerator DensityController()
    {
        
        yield return new WaitForSeconds(OverallDensity); 
        
        // switch (OverallDensity)
        // {
            // case 0:
            //     yield return new WaitForSeconds(0.5f);
            //     break;
            // case 2:
            //     yield return new WaitForSeconds(0f); 
            //     break;
            // case 3:
            //     yield return new WaitForSeconds(0.01f);
            //     break;
            // default:
            //     yield return new WaitForSeconds(OverallDensity); 
            //     break;
        // }

        Dictionary<int, float> weightedDict = new Dictionary<int, float>
        {
            {0, IndivitualDensityN}, {1, IndivitualDensityN * 2f}, {2, IndivitualDensityN * 1.5f}, {3, IndivitualDensityN}, {4, IndivitualDensityN},
            {5, IndivitualDensityS}, {6, IndivitualDensityS * 2f}, {7, IndivitualDensityS * 1.5f}, {8, IndivitualDensityS}, {9, IndivitualDensityS},
            {10, IndivitualDensityW}, {11, IndivitualDensityW * 2f}, {12, IndivitualDensityW * 1.5f}, {13, IndivitualDensityW}, {14, IndivitualDensityW},
            {15, IndivitualDensityE}, {16, IndivitualDensityE * 2f}, {17, IndivitualDensityE * 1f}, {18, IndivitualDensityE}, {19, IndivitualDensityE},
        };

        int randomNumber = GetWeightedRandom(weightedDict);
        // Debug.Log("randomNumber: " + randomNumber);
        GetRandom(randomNumber);
        StartCoroutine(DensityController());
    }

    void GetRandom(int e)
    {
        if (One == true && Two == true && Three == true)
        {
            One = false;
            Two = false;
            Three = false;
        }
        if (One1 == true && Two1 == true && Three1 == true)
        {
            One1 = false;
            Two1 = false;
            Three1 = false;
        }
        if (One2 == true && Two2 == true && Three2 == true)
        {
            One2 = false;
            Two2 = false;
            Three2 = false;
        }
        if (One3 == true && Two3 == true && Three3 == true)
        {
            One3 = false;
            Two3 = false;
            Three3 = false;
        }

        if (e >= 0 && e <= 4)
        {
            if ((e == 0 || e == 4) && One == false)
            {
                One = true;
                if (lineChecker[0].IsSpawnPointFree)
                {
                    Generate(e); 
                }
            }

            if (e == 1  && Two == false)
            {
                Two = true;
                Generate(e);
            }

            if ((e == 2 || e == 3)  && Three == false)        
            {
                Three = true;
                if (lineChecker[1].IsSpawnPointFree) 
                {
                    Generate(e);
                }
            }
        }

        if (e >= 5 && e <= 9)
        {
            if ((e == 5 || e == 9) && One1 == false)
            {
                One1 = true;
                if (lineChecker[2].IsSpawnPointFree)
                {
                    Generate(e); 
                }
            }

            if (e == 6  && Two1 == false)
            {
                Two1 = true;
                Generate(e);
            }

            if ((e == 7 || e == 8)  && Three1 == false)        
            {
                Three1 = true;
                if (lineChecker[3].IsSpawnPointFree) 
                {
                    Generate(e);
                }
            }
        }

        if (e >= 10 && e <= 14)
        {
            if ((e == 10 || e == 14) && One2 == false)
            {
                One2 = true;
                if (lineChecker[4].IsSpawnPointFree)
                {
                    Generate(e); 
                }
            }

            if (e == 11  && Two2 == false)
            {
                Two2 = true;
                Generate(e);
            }

            if ((e == 12 || e == 13)  && Three2 == false)        
            {
                Three2 = true;
                if (lineChecker[5].IsSpawnPointFree) 
                {
                    Generate(e);
                }
            }
        }

        if (e >= 15 && e <= 19)
        {
            if ((e == 15 || e == 19) && One3 == false)
            {
                One3 = true;
                if (lineChecker[6].IsSpawnPointFree)
                {
                    Generate(e); 
                }
            }

            if (e == 16  && Two3 == false)
            {
                Two3 = true;
                Generate(e);
            }

            if ((e == 17 || e == 18)  && Three3 == false)        
            {
                Three3 = true;
                if (lineChecker[7].IsSpawnPointFree) 
                {
                    Generate(e); 
                }
            }
        }
    }

    int GetWeightedRandom(Dictionary<int, float> weightedNumbers)
    {
        List<int> weightedList = new List<int>();

        foreach (var pair in weightedNumbers)
        {
            int weightCount = Mathf.RoundToInt(pair.Value); // Convert float weight to int
            for (int i = 0; i < weightCount; i++)
            {
                weightedList.Add(pair.Key);
            }
        }

        if (weightedList.Count == 0)
            return 0; // Prevent errors if all weights are 0

        int randomIndex = UnityEngine.Random.Range(0, weightedList.Count);
        return weightedList[randomIndex];
    }
}
