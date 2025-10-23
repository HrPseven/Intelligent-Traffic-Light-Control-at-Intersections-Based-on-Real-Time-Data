using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;



public class Results : MonoBehaviour
{
    private AI_handler AI_Handler;


    public DetectObjectsInArea[] camera_view_SM;
    public DetectObjectsInArea[] beyond_camera_view;
    public DetectObjectsInArea[] laneOpts;
    public DetectObjectsInArea select_empty_cal_penalty = null;

    public int totalPassedCars = 0;

    public float AverageWait = 0;
    private List<float> calAverage = new List<float>();
    public List<int> is_above_limit = new List<int> { 0, 0, 0, 0 };
    private float[] save_start_time = new float[4];

    public float AverageWaitService = 0;
    private List<float> calEmergencyAverage = new List<float>();
    private List<int> waitTimeService = new List<int> { 0, 0, 0, 0 };
    private float[] save_start_time_service = new float[4];

    public float AveStoppedVehicle = 0;
    private List<int> StoppedVehicleSum = new List<int>();

    public float AveGreenDu = 0;
    private List<int> GreenDuSum = new List<int>();
    public float[] AveGreenDuDensity = new float[6];
    private int d = 0;
    private float currentDensity = 0.3f;

    private bool isEmpty = false;
    private float emptyStartTime = -1f;
    public float totalEmptyTime = 0f;
    public float Green_Light_Utilization = 0;
    private List<float> calEmptyGreenAverage = new List<float>();


    public int lastlane1 = -1;
    public float[] num_vehicles = new float[4];
    private bool first_round = true;
    public float[] Line = new float[4];
    public float[] weightandtime = new float[4];

    public int[] ServiceCount = new int[4];
    public bool triger = false;
    public int lastlane2 = -1;
    private SpawnCar spawnCar;
    public int overallspawn = 0;
    public float[] laneSpawn = new float[4];
    public string[] lanedemand = new string[4];
    public float[] greenDu = new float[4];
    public float[] targetTime = new float[4];
    public bool[] Yellowlight = new bool[4];
    public List<bool> Redlight = new List<bool> { true, true, true, true };
    bool minimumDurationmet = false;
    float elapsedMin = 0f;




    void Awake()
    {
        AI_Handler = FindFirstObjectByType<AI_handler>();
        spawnCar = FindFirstObjectByType<SpawnCar>();
        lastlane1 = -1;

        // StartCoroutine(HoldTime());
    }

    public void lanespawn()
    {
        laneSpawn[0] = spawnCar.IndivitualDensityN;
        laneSpawn[1] = spawnCar.IndivitualDensityS;
        laneSpawn[2] = spawnCar.IndivitualDensityW;
        laneSpawn[3] = spawnCar.IndivitualDensityE;  
    }

    void Update()
    {
        // Debug.Log("Red.Count: " + Redlight.Count); 
        if (triger)
        { Main(); }
        
        for (int i = 0; i < 4; i++)
        {
            if (laneOpts[i].sum_vehicle == 0)
            {
                save_start_time[i] = Time.time;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            camera_view_SM[i].StoppedandMovingCars();
            if (camera_view_SM[i].is_service == false)
            {
                save_start_time_service[i] = Time.time;
            }
        }

        if (minimumDurationmet && select_empty_cal_penalty != null)
        {
            if (select_empty_cal_penalty.sum_vehicle == 0)
            {
                if (!isEmpty)
                {
                    emptyStartTime = Time.time;
                    isEmpty = true;
                    // Debug.Log(" Empty ");
                }
                // else if (isEmpty)
                // {
                //     // Still empty → keep accumulating time in Update
                //     totalEmptyTime = Time.time - emptyStartTime;
                // }
            }
            else
            {
                if (isEmpty) // it just stopped being empty
                {
                    totalEmptyTime += Time.time - emptyStartTime;
                    // Debug.Log(" Start ");
                    emptyStartTime = -1f;
                    isEmpty = false;
                }
            }
        }
    }

    public void Main()
    {
        WeightandService();
    }


    public void UpdateTotalPassedCars()
    {
        totalPassedCars++;
    }

    public void results()
    {
        // Getting the number of vehicle for each lane.
        for (int i = 0; i < camera_view_SM.Length; i++)
        {
            // camera_view_SM[i].StoppedandMovingCars();

            int carCount = camera_view_SM[i].carCount;
            int bikeCount = camera_view_SM[i].bikeCount;
            int BusCount = camera_view_SM[i].BusCount;
            int TruckCount = camera_view_SM[i].TruckCount;
            int ServiceCount = camera_view_SM[i].ServiceCount;
            int VanCount = camera_view_SM[i].VanCount;

            // beyond_camera_view[i].StoppedandMovingCars();
            int BeyondSM = beyond_camera_view[i].sum_vehicle;
            int CameraSM = camera_view_SM[i].sum_vehicle;
            int Bsum = BeyondSM - CameraSM;
            // Debug.Log("Bsum: " + BeyondSM + " - " + CameraSM + " = " + Bsum);

            float weight_sum = carCount + ((float)bikeCount * 0.5f) + (ServiceCount * 2) + (VanCount * 2) + (TruckCount * 3) + (BusCount * 4) + Bsum;
            Line[i] = weight_sum;
            // Line[i] = (int)Math.Round(weight_sum, MidpointRounding.AwayFromZero);
            num_vehicles[i] = carCount + bikeCount + BusCount + TruckCount + ServiceCount + VanCount + Bsum;
        }

        // Checking the presence of service vehicle for each lane.
        for (int i = 0; i < camera_view_SM.Length; i++)
        {
            if (i != lastlane1)
            {
                ServiceCount[i] = camera_view_SM[i].ServiceCount;
            }
            else
            { ServiceCount[i] = 0; }
        }

        // Calculating the weight and time relation.
        for (int i = 0; i < is_above_limit.Count; i++)
        {
            if (i != lastlane1)
            {
                if (is_above_limit[i] == 0)
                {
                    weightandtime[i] = Line[i] * 1;
                }
                else
                {
                    weightandtime[i] = Line[i] * is_above_limit[i];
                }
            }
            else
            {
                weightandtime[i] = 0;
            }

            weightandtime[i] = weightandtime[i] * 0.1f;
        }
    }

    IEnumerator mintime()
    {
        while (true)
        {
            if (elapsedMin > 10)
            {
                minimumDurationmet = true;
                // Debug.Log("⛔ minimumDurationmet");
                yield break;
                // if (cancelWait)
                // {
                //     Debug.Log("⛔ Wait cancelled.");
                //     StartCoroutine(Commander(true));
                //     yield break; // Exit the coroutine early
                // }
            }

            yield return null; // Wait one frame
            elapsedMin += Time.deltaTime;
            // Debug.Log("elapsedMin " + elapsedMin);
        }
    }

    public void WeightandService()
    {
        if (first_round)
        {
            for (int i = 0; i < 4; i++)
            {
                save_start_time[i] = Time.time;
            }
        }
        else
        {
            save_start_time[lastlane2] = Time.time;
        }
        first_round = false;


        for (int i = 0; i < 4; i++)
        {
            float elapsed = Time.time - save_start_time[i];
            is_above_limit[i] = (int)elapsed;
        }


        for (int i = 0; i < 4; i++)
        {
            float elapsed = Time.time - targetTime[i];
            if (elapsed >= 0)
            {
                greenDu[i] = 0;
            }
            else
            {
                greenDu[i] = Mathf.Abs((int)elapsed);
            }
        }
    }

    public void empty_green()
    {
        if (isEmpty)
        {
            totalEmptyTime += Time.time - emptyStartTime;
            // Debug.Log(" Start ");
            emptyStartTime = -1f;
            isEmpty = false;
        }

        if (totalEmptyTime != 0) calEmptyGreenAverage.Add(totalEmptyTime);

        float calEmptyGreenAve = 0;
        for (int i = 0; i < calEmptyGreenAverage.Count; i++)
        {
            calEmptyGreenAve = calEmptyGreenAve + calEmptyGreenAverage[i];
        }

        Green_Light_Utilization = Mathf.Round(calEmptyGreenAve / calEmptyGreenAverage.Count);

        // foreach (float a in calEmptyGreenAverage)
        // {
        //     Debug.Log("calEmptyGreenAverage " + a);
        // }
        // Debug.Log("calEmptyGreenAverageC " + calEmptyGreenAverage.Count);



        // if (totalEmptyTime > Green_Light_Utilization)
        // {
        //     Green_Light_Utilization = totalEmptyTime;
        // }
        // Debug.Log(" totalEmptyTime " + totalEmptyTime);
        totalEmptyTime = 0;
        minimumDurationmet = false;
        elapsedMin = 0;
        select_empty_cal_penalty = null;
    }

    public void number_of_vehicles()
    {
        for (int i = 0; i < 4; i++)
        {
            camera_view_SM[i].StoppedandMovingCars();
            num_vehicles[i] = camera_view_SM[i].stopC;
        }
    }

    public void waitTime()
    {

        if (is_above_limit[lastlane2] != 0) calAverage.Add(is_above_limit[lastlane2]);

        float calAve = 0;
        for (int i = 0; i < calAverage.Count; i++)
        {
            calAve = calAve + calAverage[i];
        }

        AverageWait = Mathf.Round(calAve / calAverage.Count);

        // foreach (float a in calAverage)
        // {
        //     Debug.Log("calAverage " + a);
        // }

        // Debug.Log(" calAverageCount " + calAverage.Count);


        for (int i = 0; i < 4; i++)
        {
            float elapsed = Time.time - save_start_time_service[i];
            waitTimeService[i] = (int)elapsed;
        }
        // Debug.Log("waitTimeService = [" + string.Join(", ", waitTimeService) + "]");

        if (waitTimeService[lastlane2] != 0) calEmergencyAverage.Add(waitTimeService[lastlane2]);

        float calEmergencyAve = 0;
        for (int i = 0; i < calEmergencyAverage.Count; i++)
        {
            calEmergencyAve = calEmergencyAve + calEmergencyAverage[i];
        }

        AverageWaitService = Mathf.Round(calEmergencyAve / calEmergencyAverage.Count);

        // foreach (float a in calEmergencyAverage)
        // {
        //     Debug.Log("calEmergencyAverage " + a);
        // }
        // Debug.Log("AverageWaitService " + calEmergencyAverage.Count);

        beyond_camera_view[lastlane2].StoppedandMovingCars();
        int sumofstopped = beyond_camera_view[lastlane2].stopC;
        StoppedVehicleSum.Add(sumofstopped);
        int calStoppedVehicleSum = 0;
        for (int i = 0; i < StoppedVehicleSum.Count; i++)
        {
            calStoppedVehicleSum = calStoppedVehicleSum + StoppedVehicleSum[i];
        }
        // Debug.Log("StoppedVehicleSum: " + StoppedVehicleSum.Count);
        AveStoppedVehicle = Mathf.Round(calStoppedVehicleSum / StoppedVehicleSum.Count);

        // foreach (float a in StoppedVehicleSum)
        // {
        //     Debug.Log("StoppedVehicleSum " + a);
        // }
        // Debug.Log("StoppedVehicleSum.Count: " + StoppedVehicleSum.Count);
        // Debug.Log("AveStoppedVehicle: " + AveStoppedVehicle);


        if (AI_Handler.AI_Handler && AI_Handler.recieved_duration != 0)
        {
            int du = AI_Handler.recieved_duration;
            GreenDuSum.Add(du);
            int calGreenDuSum = 0;
            for (int i = 0; i < GreenDuSum.Count; i++)
            {
                calGreenDuSum = calGreenDuSum + GreenDuSum[i];
            }
            AveGreenDu = Mathf.Round(calGreenDuSum / GreenDuSum.Count);
            Debug.Log("GreenDuSum: " + GreenDuSum.Count);

            if (GreenDuSum.Count == 106) { Time.timeScale = 0f; }

            // foreach (float a in GreenDuSum)
            // {
            //     Debug.Log("GreenDuSum " + a);
            // }
            // Debug.Log("GreenDuSum.Count: " + GreenDuSum.Count);
            // Debug.Log("AveGreenDu: " + AveGreenDu);

            if (currentDensity != spawnCar.OverallDensity)
            {
                AveGreenDuDensity[d] = AveGreenDu;
                d += 1;
                GreenDuSum.Clear();
                if (d == 6) { d = 0; }
            }
            currentDensity = spawnCar.OverallDensity;

            // foreach (float a in AveGreenDuDensity)
            // {
            //     Debug.Log("---AveGreenDuDensity " + a);
            // }
        }



        StartCoroutine(mintime()); 
    }
}