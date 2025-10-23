using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Linq;
using UnityEngine.PlayerLoop;
using System.Collections;




public class Visuals : MonoBehaviour
{
    private Results Results;
    private AI_handler AI_handler;
    private InitializerScript Initializer;
    private SpawnCar SpawnCar;
    private TextController TextController;

    public TMP_Text WaitTimeV0;
    public TMP_Text WaitTimeV1;
    public TMP_Text WaitTimeV2;
    public TMP_Text WaitTimeV3;
    public Button[] WaitTimeB = new Button[4];
    public TMP_Text EmergencyPresenceV0;
    public TMP_Text EmergencyPresenceV1;
    public TMP_Text EmergencyPresenceV2;
    public TMP_Text EmergencyPresenceV3;
    public Button[] EmergencyPresenceB = new Button[4];
    public TMP_Text WeightV0;
    public TMP_Text WeightV1;
    public TMP_Text WeightV2;
    public TMP_Text WeightV3;

    public Button[] WeightB = new Button[4];

    public TMP_Text LaneID;
    public TMP_Text Pweight;
    public TMP_Text LaneWaitTime;
    public TMP_Text OverallDensity;
    public TMP_Text GreenDuration;
    public Button GreenDurationB;


    // public TMP_Text AveGreenDuV;
    public TMP_Text QueueLengthV;
    public TMP_Text AverageWaitV;
    public TMP_Text EmergencyAverageWaitV;
    public TMP_Text GreenLightUtilizationV;

    public TMP_Text OverallSpawnV;
    // public TMP_Text SpawnWeightV;
    public TMP_Text Overall_speedV;
    public TMP_Text BenchmarkingV;
    public TMP_Text Duration;
    public TMP_Text DurationV;
    public TMP_Text PedestriansV;
    

    void Awake()
    {
        SpawnCar = FindFirstObjectByType<SpawnCar>();
        foreach (TMP_Text t in GetComponentsInChildren<TMP_Text>())
        {
            if (t.transform.parent.name == "WaitTimeV0") WaitTimeV0 = t;
            if (t.transform.parent.name == "WaitTimeV1") WaitTimeV1 = t;
            if (t.transform.parent.name == "WaitTimeV2") WaitTimeV2 = t;
            if (t.transform.parent.name == "WaitTimeV3") WaitTimeV3 = t;

            if (t.transform.parent.name == "EmergencyPresenceV0") EmergencyPresenceV0 = t;
            if (t.transform.parent.name == "EmergencyPresenceV1") EmergencyPresenceV1 = t;
            if (t.transform.parent.name == "EmergencyPresenceV2") EmergencyPresenceV2 = t;
            if (t.transform.parent.name == "EmergencyPresenceV3") EmergencyPresenceV3 = t;

            if (t.transform.parent.name == "WeightV0") WeightV0 = t;
            if (t.transform.parent.name == "WeightV1") WeightV1 = t;
            if (t.transform.parent.name == "WeightV2") WeightV2 = t;
            if (t.transform.parent.name == "WeightV3") WeightV3 = t;

            if (t.transform.parent.name == "LaneID") LaneID = t;
            if (t.transform.parent.name == "Pweight") Pweight = t;
            if (t.transform.parent.name == "LaneWaitTime") LaneWaitTime = t;
            if (t.transform.parent.name == "OverallDensity") OverallDensity = t;
            if (t.transform.parent.name == "GreenDuration") GreenDuration = t;

            // if (t.name == "AveGreenDuV") AveGreenDuV = t;
            if (t.name == "QueueLengthV") QueueLengthV = t;
            if (t.name == "AverageWaitV") AverageWaitV = t;
            if (t.name == "EmergencyAverageWaitV") EmergencyAverageWaitV = t;
            if (t.name == "GreenLightUtilizationV") GreenLightUtilizationV = t;

            if (t.name == "OverallSpawnV") OverallSpawnV = t;
            // if (t.name == "SpawnWeightV") SpawnWeightV = t;
            if (t.name == "Overall_speedV") Overall_speedV = t;
            if (t.name == "BenchmarkingV") BenchmarkingV = t;
            if (t.name == "Duration") Duration = t;
            if (t.name == "DurationV") DurationV = t;
            if (t.name == "PedestriansV") PedestriansV = t;
        }

        foreach (Button b in GetComponentsInChildren<Button>())
        {
            if (b.name == "WaitTimeV0") WaitTimeB[0] = b;
            if (b.name == "WaitTimeV1") WaitTimeB[1] = b;
            if (b.name == "WaitTimeV2") WaitTimeB[2] = b;
            if (b.name == "WaitTimeV3") WaitTimeB[3] = b;

            if (b.name == "EmergencyPresenceV0") EmergencyPresenceB[0] = b;
            if (b.name == "EmergencyPresenceV1") EmergencyPresenceB[1] = b;
            if (b.name == "EmergencyPresenceV2") EmergencyPresenceB[2] = b;
            if (b.name == "EmergencyPresenceV3") EmergencyPresenceB[3] = b;

            if (b.name == "WeightV0") WeightB[0] = b;
            if (b.name == "WeightV1") WeightB[1] = b;
            if (b.name == "WeightV2") WeightB[2] = b;
            if (b.name == "WeightV3") WeightB[3] = b;

            if (b.name == "GreenDuration") GreenDurationB = b;
        }
    }

    void Start()
    {
        Results = FindFirstObjectByType<Results>();
        AI_handler = FindFirstObjectByType<AI_handler>();
        Initializer = FindFirstObjectByType<InitializerScript>();
        TextController = FindFirstObjectByType<TextController>();
        

        if (AI_handler.AI_Handler)
        {
            GameObject Fixed_traditional_duration = GameObject.Find("Fixed_traditional_duration");
            GameObject clock_vise_lane_selection = GameObject.Find("clock_vise_lane_selection");

            if (Fixed_traditional_duration != null)
            {
                clock_vise_lane_selection.gameObject.SetActive(false);
                Fixed_traditional_duration.gameObject.SetActive(false);
            }
        }
        else
        {
            GameObject AI_Lane = GameObject.Find("AI_Lane");
            TMP_Text waitTime = GameObject.Find("WaitTime")?.GetComponent<TMP_Text>();
            // GameObject LaneRow = GameObject.Find("LaneRow");
            // GameObject Table = GameObject.Find("Table");

            GameObject laneID1 = GameObject.Find("LaneID");
            GameObject Pweight1 = GameObject.Find("Pweight");
            GameObject LaneWaitTime1 = GameObject.Find("LaneWaitTime");
            GameObject OverallDensity1 = GameObject.Find("OverallDensity");
            GameObject AI_Duration = GameObject.Find("AI_Duration");
            GameObject DataTitle = GameObject.Find("DataTitle");
            GameObject LanesData = GameObject.Find("LanesData");

            if (Pweight1 != null)
            {
                waitTime.text = $"Wait Time:";
                AI_Lane.gameObject.SetActive(false);
                // LaneRow.gameObject.SetActive(false);
                // Table.gameObject.SetActive(false);

                laneID1.gameObject.SetActive(false);
                Pweight1.gameObject.SetActive(false);
                LaneWaitTime1.gameObject.SetActive(false);
                OverallDensity1.gameObject.SetActive(false);
                AI_Duration.gameObject.SetActive(false);
                DataTitle.gameObject.SetActive(false);
                LanesData.gameObject.SetActive(false);
            }
        }

        // if(Initializer.gameData.SpawnWeight == 0) SpawnWeightV.text = "Low";
        // else if(Initializer.gameData.SpawnWeight == 1) SpawnWeightV.text = "Medium";
        // else if(Initializer.gameData.SpawnWeight == 2) SpawnWeightV.text = "High"; 

        Overall_speedV.text = $"{Initializer.gameData.Overall_speed}x";

        Duration.text = $"Duration({Initializer.gameData.Duration / 60}m):";

        if (Initializer.gameData.Benchmarking == 0 || Initializer.gameData.Benchmarking == 3)
        {
            Duration.text = $"Duration ({Initializer.gameData.Duration / 60}m):";
            BenchmarkingV.text = "Yes";
            if (Initializer.gameData.Benchmarking == 0)
            {
                DurationV.text = $"{(Initializer.gameData.Duration / 60) / 2}m for AI"; 
            }
            else
            {
                DurationV.text = $"{(Initializer.gameData.Duration / 60) / 2}m for TR";
            }
        }
        else
        {
            Duration.text = "Duration:";
            BenchmarkingV.text = "No"; DurationV.text = "No limitation";
        }

        if(Initializer.gameData.Pedestrians == 0) PedestriansV.text = "Not Applied";
        else PedestriansV.text = "Applied";
    }

    public void visuals()
    {
        Results.results();
        Results.waitTime();

        if (AI_handler.AI_Handler)
        {
            for (int i = 0; i < 4; i++)
            {
                if (Results.is_above_limit[i] >= AI_handler.TimeLimitation)
                {
                    WaitTimeB[i].image.color = new Color(1f, 0.3820645f, 0f, 1f);
                    // Debug.Log("🟢" + WaitTimeB[i]);
                }
                else
                {
                    // Debug.Log("🔴" + WaitTimeB[i]);
                    WaitTimeB[i].image.color = Color.white;
                }

                if (Results.ServiceCount[i] > 0)
                {
                    EmergencyPresenceB[i].image.color = new Color(1f, 0.5f, 0f, 1f); // Orange (RGBA)
                }
                else
                {
                    EmergencyPresenceB[i].image.color = Color.white;
                }
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (WeightB[i].image.color == Color.green) WeightB[i].image.color = Color.red;
        }

        WeightB[Results.lastlane2].image.color = Color.green;

        WaitTimeV0.text = $"{Results.is_above_limit[0]}";
        WaitTimeV1.text = $"{Results.is_above_limit[1]}";
        WaitTimeV2.text = $"{Results.is_above_limit[2]}";
        WaitTimeV3.text = $"{Results.is_above_limit[3]}";

        EmergencyPresenceV0.text = $"{Results.ServiceCount[0]}";
        EmergencyPresenceV1.text = $"{Results.ServiceCount[1]}";
        EmergencyPresenceV2.text = $"{Results.ServiceCount[2]}";
        EmergencyPresenceV3.text = $"{Results.ServiceCount[3]}";

        WeightV0.text = $"{Results.weightandtime[0].ToString("F1")}";
        WeightV1.text = $"{Results.weightandtime[1].ToString("F1")}";
        WeightV2.text = $"{Results.weightandtime[2].ToString("F1")}";
        WeightV3.text = $"{Results.weightandtime[3].ToString("F1")}";

        StartCoroutine(GreenDurationText());


        QueueLengthV.text = $"{Results.AveStoppedVehicle}";
        // AveGreenDuV.text = $"{Results.AveGreenDuDensity}";
        AverageWaitV.text = $"{Results.AverageWait}";
        EmergencyAverageWaitV.text = $"{Results.AverageWaitService}";
        GreenLightUtilizationV.text = $"{Results.Green_Light_Utilization}";
    }

    IEnumerator GreenDurationText()
    {
        yield return null; 

        if (Initializer.gameData.Benchmarking == 2 || Initializer.gameData.Benchmarking == 3)
        {
            GreenDuration.text = "30";
        }
        else
        {
            LaneID.text = $"{AI_handler.lastlane}";
            Pweight.text = $"{AI_handler.duration_weights[2]}";
            LaneWaitTime.text = $"{AI_handler.duration_weights[3]}";

            // if (AI_handler.duration_weights[1] == 0) { laneDensity.text = "L";}
            // else if (AI_handler.duration_weights[1] == 0.5) { laneDensity.text = "M";}
            // else if (AI_handler.duration_weights[1] == 1) { laneDensity.text = "H";}

            if (SpawnCar.OverallDensity == 0.3f) { OverallDensity.text = "15:00"; }
            else if (SpawnCar.OverallDensity == 0.2f) { OverallDensity.text = "16:00"; }
            else if (SpawnCar.OverallDensity == 0.175f) { OverallDensity.text = "17:00"; }
            else if (SpawnCar.OverallDensity == 0.15f) { OverallDensity.text = "18:00"; }
            else if (SpawnCar.OverallDensity == 0.1f) { OverallDensity.text = "19:00"; }
            else if (SpawnCar.OverallDensity == 0) { OverallDensity.text = "20:00"; }

            GreenDuration.text = $"{AI_handler.recieved_duration}";
        }
    }

    public void density_change()
    {
        if (SpawnCar.OverallDensity == 0.3f) OverallSpawnV.text = "Very Low";
        else if (SpawnCar.OverallDensity == 0.2f) OverallSpawnV.text = "Low";
        else if (SpawnCar.OverallDensity == 0.175f) OverallSpawnV.text = "Moderate";
        else if (SpawnCar.OverallDensity == 0.15f) OverallSpawnV.text = "High";
        else if (SpawnCar.OverallDensity == 0.1f) OverallSpawnV.text = "Very High";
        else if (SpawnCar.OverallDensity == 0) OverallSpawnV.text = "Max";

        // TextController.update_lanedemand();
    }

    public void refVisuals()
    {
        // QueueLengthV.text = $"{Results.totalPassedCars}";

        for (int i = 0; i < 4; i++)
        {
            Image img = GreenDurationB.image;
            if (Results.greenDu[i] != 0)
            {
                img.fillCenter = false;
                GreenDurationB.image.color = Color.green;
                // GreenDuration.text = $"{Results.greenDu[i]}";
            }
            else if (Results.Yellowlight[i])
            {
                img.fillCenter = true;
                GreenDurationB.image.color = Color.yellow;
                GreenDuration.text = $"";
            }
            else if (Results.Redlight[i])
            {
                img.fillCenter = true;
                GreenDurationB.image.color = Color.red;
                GreenDuration.text = $"";
            }
        }
    }

    void Update()
    {
        refVisuals();
    }
}
