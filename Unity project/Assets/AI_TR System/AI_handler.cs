using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using System.Linq;
using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;
using Newtonsoft.Json;

using System.Net.Sockets;
using System.Text;
using System;
using UnityEngine.Assertions.Must;
using UnityEditor;
using Unity.VisualScripting.FullSerializer;
using System.Data.Common;

public class AI_handler : MonoBehaviour
{
    private TLSystem TL;
    private TrafficController TrafficController;
    public DetectObjectsInArea[] beyond_camera_view;
    public DetectObjectsInArea[] camera_view_SM;
    public DetectObjectsInArea[] laneOpts;
    private DetectObjectsInArea select_empty_cal_penalty = null;
    private float[] Line = new float[4];
    public int[] num_vehicle = new int[4];
    
    private int carCount;
    private int bikeCount;
    private int VanCount;
    private int BusCount;
    private int[] ServiceCount = new int[4];
    private int TruckCount;
    private int Bsum;

    public bool AI_Handler;
    public int lastlane = -1;
    private int previous_lane = -1 ;
    private int two_previous_lane = -1 ;

    private float[] save_start_time = new float[4];
    public List<int> is_above_limit = new List<int> { 0, 0, 0, 0 };
    public int recieved_duration;
    private int last_weight;
    private float last_reward;
    private bool first_round_du_reward = true;
    private bool first_round_observation = true;
    // private bool first_round_la = true;

    public List<float> laneweights = new List<float> { 0, 0, 0, 0 };  // Ensure 4 elements
    // List<int> pureweights = new List<int> { 0, 0, 0, 0 };  // Ensure 4 elements
    List<int> sortedweights = new List<int> { 0, 0, 0, 0 };
    // public float[] duration_weights = new float[7];
    public List<float> duration_weights = new List<float> { 0, 0, 0, 0, 0, 0};
    // List<List<int>> lane_weights = new List<List<int>>();
    float maxValue = float.MinValue;
    int maxIndex = -1;
    private float points;
    private bool first_round_du = true;
    public SemaphorePeople[] pedestrian_check;
    private float lane_points;
    public int TimeLimitation = 90;
    private SpawnCar spawnCar;
    public CarCounter[] carCounter;
    List<int> durations = new List<int> { 0, 0, 0, 0 };
    List<int> countedcars = new List<int> { 0, 0, 0, 0 };

    public int predicted_car_num;
    // float lane_request = 0;

    private float emptyStartTime = -1f;
    private float totalEmptyTime = 0f;
    private bool isEmpty = false;
    // private int carsExited = -1;

    private int passedDiff;
    float laneSpawn = 0f;
    // float double_used = -1f;

    bool cancelWait = false;
    private float targetTime = 0f;
    bool minimumDurationmet = false;



    TcpClient client;
    NetworkStream stream;
    StreamWriter writer;
    StreamReader reader;

    void Start()
    {
        // TL = FindObjectOfType<TLSystem>();
        TL = FindFirstObjectByType<TLSystem>();
        spawnCar = FindFirstObjectByType<SpawnCar>();
        TrafficController = FindFirstObjectByType<TrafficController>();

        if (AI_Handler)
        {
            ConnectToPython();
            StartCoroutine(Commander(false, true));
            TrafficController.FixedTimer = false;
        }
        else
        {
            TrafficController.FixedTimer = true;
        }
    }

    void Update()
    {
        // To Zero the wait time for lanes if they do not contain any vehicles. 
        for (int i = 0; i < 4; i++)
        {
            if (laneOpts[i].sum_vehicle == 0)
            {
                save_start_time[i] = Time.time;
            }
        }

        // Checking if the selected lane's containing none vehicle to calculate the duration reward.
        if (minimumDurationmet && select_empty_cal_penalty != null)
        {
            passedDiff = carCounter[lastlane].passedCarsCount - num_vehicle[lastlane];
            if (select_empty_cal_penalty.sum_vehicle == 0)
            {
                if (!isEmpty && passedDiff >= 0) // it just became empty again
                {
                    emptyStartTime = Time.time;
                    isEmpty = true;
                    // Debug.Log(" Empty ");
                }
                else if (isEmpty)
                {
                    // Still empty → keep accumulating time in Update
                    totalEmptyTime = Time.time - emptyStartTime;
                }

                // Accumulating happens later when state changes
            }
            else
            {
                if (isEmpty && passedDiff >= 0) // it just stopped being empty
                {
                    totalEmptyTime += Time.time - emptyStartTime;
                    // Debug.Log(" Start ");
                    emptyStartTime = -1f;
                    isEmpty = false;
                }
            }

            if (totalEmptyTime >= 1 && passedDiff >= 0)
            {
                cancelWait = true;
                emptyStartTime = -1f;
                isEmpty = false;
            }
        }
    }

    IEnumerator Commander(bool wait, bool? firstround = null)
    {
        if (firstround != null) 
        {
            yield return new WaitForSeconds(30);
            for(int i = 0; i < 4; i++)
            {
                save_start_time[i] = Time.time;
            } 
        }

        if (!first_round_du_reward && wait == true)
        {
            countedcars[lastlane] = carCounter[lastlane].passedCarsCount;
            passedDiff = carCounter[lastlane].passedCarsCount - num_vehicle[lastlane];
            CalRewardDuration(passedDiff);
        }
        first_round_du_reward = false;

        Debug.Log("-------------------------------------------------------------------");
    
        TL.Send(4);
        
        if (wait)
        {
            yield return new WaitForSeconds(4.7f);
            save_start_time[lastlane] = Time.time;
        }

        // -------------- LANE-CHOOSING ----------------

        // lane_weights.Clear(); // To ensure clean state

        // Observation();

        // string output = "{";
        // foreach (var lane in lane_weights)
        // {
        //     output += "(" + string.Join(",", lane) + ")";
        // }
        // output += "}";
        // Debug.Log(output);

        // List<int> flatList = lane_weights.SelectMany(inner => inner).ToList();

        // string laneweightsStr = string.Join(";", flatList);


        // if (first_round_la)
        // {
        //     SendWeightandReward(false, laneweightsStr);
        // }
        // else
        // {
        //     SendWeightandReward(false, laneweightsStr, lane_points);
        // }

        // first_round_la = false;

        // int recived_lane = ListenForDuration();

        // CalRewardLane(recived_lane);

        // if (recived_lane != maxIndex )
        // {
        //     // laneselector(true);
        //     Debug.Log("🔴🔴🔴Match found: " + recived_lane);
        //     StartCoroutine(Commander(false));
        //     yield break;
        // } 


        CalRewardLane();
        int recived_lane = maxIndex;

        TL.Send(recived_lane);

        Debug.Log($"🔴Selected value: {last_weight} , index: {recived_lane}");

        two_previous_lane = previous_lane;
        previous_lane = lastlane;
        lastlane = recived_lane;

        // -------------- DURATION-AI ----------------

        // Start the timer for the selectecd time to check in the "void Update" if the lane contains only one vehicle to calculat the Duration reward.
        select_empty_cal_penalty = laneOpts[recived_lane];
        Debug.Log($"🕓 Started tracking collider Opt'{recived_lane}'");

        first_round_observation = false;

        // lane_weights.Clear(); // To ensure clean state

        Observation();

        Debug.Log("Duration_weithts = [" + string.Join(", ", duration_weights) + "]");

        string durationweightsStr = string.Join(";", duration_weights);

        if (first_round_du)
        {
            SendWeightandReward(true, durationweightsStr);
        }
        else
        {
            SendWeightandReward(true, durationweightsStr, points);
        }

        carCounter[lastlane].passedCarsCount = 0;

        first_round_du = false;

        recieved_duration = ListenForDuration();

        targetTime = Time.time + recieved_duration;

        TL.Send(5, recieved_duration);

        durations[lastlane] = recieved_duration;

        // yield return new WaitForSeconds(recieved_duration);
        
        cancelWait = false;
        float elapsed = 0f;
        while (elapsed < recieved_duration)
        {
            if (elapsed > 10)
            {
                minimumDurationmet = true;
                if (cancelWait)
                {
                    Debug.Log("⛔ Wait cancelled.");
                    StartCoroutine(Commander(true));
                    yield break; // Exit the coroutine early
                }
            }

            yield return null; // Wait one frame
            elapsed += Time.deltaTime;
        }

        // save_start_time[recived_lane] = Time.time;
        StartCoroutine(Commander(true));
    }


    public void GUI(int nswe)
    {
        if (nswe == 1)
        {
            camera_view_SM[0].StoppedandMovingCars();
        }
        else if (nswe == 7)
        {
            CalRewardLane();
            Debug.Log($"Lane Points ⭐: {lane_points}" );
        }
    }

    // void laneselector(bool SameLane = false)
    // {
    //     if(!SameLane)
    //     {
    //         CalRewardLane(1);
    //     }
    //     else
    //     {
    //         // Get sorted indices by descending value
    //         List<int> sortedIndices = laneweights
    //             .Select((value, index) => new { value, index })
    //             .OrderByDescending(x => x.value)
    //             .Select(x => x.index)
    //             .ToList();

    //         // Select the second highest index
    //         maxIndex = sortedIndices[1];
    //         last_weight= laneweights[maxIndex];
    //     }

    //     // Debug.Log($"Pssenger: {maxIndex}");

    //     if (!SameLane)
    //     {
    //         StartCoroutine(Commander(maxIndex + 4 , true));
    //     }
    //     else
    //     {
    //         StartCoroutine(Commander(maxIndex + 4 , false));
    //     }
    // }

    private void CalRewardLane()
    {
        Observation();

        Debug.Log("Above limits = [" + string.Join(", ", is_above_limit) + "]");

        List<int> service = new List<int> { 0, 0, 0, 0 };  // Ensure 4 elements
        for(int i =0; i < 4; i++)
        {
            service[i] = ServiceCount[i];
        }

        Debug.Log("Service = [" + string.Join(", ", service) + "]");

        if (ServiceCount.All(value => value == 0) && is_above_limit.All(value => value < TimeLimitation))
        {
            for (int i = 0; i < laneweights.Count; i++)
            {
                if ( i == lastlane )
                {
                    laneweights[i] = 0;
                }
                else
                {
                    laneweights[i] = Line[i];
                }
            }
            Debug.Log("linesInfo Direct = [" + string.Join(", ", laneweights) + "]");
        }
        else
        {
            if (is_above_limit.Any(value => value > TimeLimitation))
            {
                if (ServiceCount.Any(value => value != 0))
                {
                    for (int i = 0; i < is_above_limit.Count; i++)
                    {
                        if (is_above_limit[i] > TimeLimitation )
                        {
                            if (ServiceCount[i] != 0)
                            {
                                if (i == lastlane)
                                {
                                    laneweights[i] = 0;
                                }
                                else
                                {
                                    laneweights[i] = Line[i];
                                }
                            }
                            else
                            {
                                if(is_above_limit[i] > TimeLimitation && i != lastlane)
                                {
                                    laneweights[i] = Line[i]; 
                                }
                                else
                                {
                                    laneweights[i] = 0;
                                }
                            }
                        }
                        else
                        {
                            laneweights[i] = 0;
                        }
                    }
                    Debug.Log($"linesInfo above {TimeLimitation} & emergency = [" + string.Join(", ", laneweights) + "]");
                    // TimeLimitation = 110
                    if (is_above_limit.Count(value => value > TimeLimitation) > 1)
                    {
                        float maxAboveValue = is_above_limit[0];
                        int maxAboveIndex = 0;
                        bool hasDuplicateMax = false;

                        for (int i = 1; i < 4; i++)
                        {
                            if (is_above_limit[i] > maxAboveValue)
                            {
                                maxAboveValue = is_above_limit[i];
                                maxAboveIndex = i;
                                hasDuplicateMax = false; // new highest found, reset duplicate flag
                            }
                            else if (is_above_limit[i] == maxAboveValue)
                            {
                                hasDuplicateMax = true; // same as current max, flag as duplicate
                            }
                        }

                        if (!hasDuplicateMax)
                        {
                            // Only one value is the max — safe to act
                            Debug.Log("Max is at index(Above) " + maxAboveIndex + " with value " + maxAboveValue);
                            for (int i = 0; i < is_above_limit.Count; i++)
                            {
                                if (i == maxAboveIndex)
                                {
                                    laneweights[i] = Line[i];
                                }
                                else
                                {
                                    laneweights[i] = 0;
                                }
                            }
                        }
                        else
                        {
                            // Do nothing, values are tied
                            Debug.Log("Multiple Above max values, doing nothing.");
                        }
                    }
                
                }
                else
                {
                    for (int i = 0; i < is_above_limit.Count; i++)
                    {
                        if (is_above_limit[i] > TimeLimitation )
                        {
                            if (i == lastlane)
                            {
                                laneweights[i] = 0;
                            }
                            else
                            {
                                laneweights[i] = Line[i];
                            }
                        }
                        else
                        {
                            laneweights[i] = 0;
                        }
                    }
                    Debug.Log($"linesInfo above {TimeLimitation} = [" + string.Join(", ", laneweights) + "]");
                
                    //TimeLimitation = 110
                    if (is_above_limit.Count(value => value > TimeLimitation) > 1)
                    {
                        float maxAboveValue = is_above_limit[0];
                        int maxAboveIndex = 0;
                        bool hasDuplicateMax = false;

                        for (int i = 1; i < 4; i++)
                        {
                            if (is_above_limit[i] > maxAboveValue)
                            {
                                maxAboveValue = is_above_limit[i];
                                maxAboveIndex = i;
                                hasDuplicateMax = false; // new highest found, reset duplicate flag
                            }
                            else if (is_above_limit[i] == maxAboveValue)
                            {
                                hasDuplicateMax = true; // same as current max, flag as duplicate
                            }
                        }

                        if (!hasDuplicateMax)
                        {
                            // Only one value is the max — safe to act
                            Debug.Log("Max is at index(Above) " + maxAboveIndex + " with value " + maxAboveValue);
                            for (int i = 0; i < is_above_limit.Count; i++)
                            {
                                if (i == maxAboveIndex)
                                {
                                    laneweights[i] = Line[i];
                                }
                                else
                                {
                                    laneweights[i] = 0;
                                }
                            }
                        }
                        else
                        {
                            // Do nothing, values are tied
                            Debug.Log("Multiple Above max values, doing nothing.");
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    if (ServiceCount[i] != 0)
                    {
                        if (i == lastlane)
                        {
                            laneweights[i] = 0;
                        }
                        else
                        {
                            laneweights[i] = Line[i];
                        }
                    }
                    else
                    {
                        laneweights[i] = 0;
                    }
                }
                Debug.Log("linesInfo emergency = [" + string.Join(", ", laneweights) + "]");
            

                if (ServiceCount.Count(value => value != 0) > 1)
                {
                    float maxServiceValue = ServiceCount[0];
                    int maxServiceIndex = 0;
                    bool hasDuplicateMax = false;

                    for (int i = 1; i < 4; i++)
                    {
                        if (ServiceCount[i] > maxServiceValue)
                        {
                            maxServiceValue = ServiceCount[i];
                            maxServiceIndex = i;
                            hasDuplicateMax = false; // new highest found, reset duplicate flag
                        }
                        else if (ServiceCount[i] == maxServiceValue)
                        {
                            hasDuplicateMax = true; // same as current max, flag as duplicate
                        }
                    }

                    if (!hasDuplicateMax)
                    {
                        // Only one value is the max — safe to act
                        Debug.Log("Max is at index (Service) " + maxServiceIndex + " with value " + maxServiceValue);
                        for (int i = 0; i < 4; i++)
                        {
                            if (i == maxServiceIndex)
                            {
                                laneweights[i] = Line[i];
                            }
                            else
                            {
                                laneweights[i] = 0;
                            }
                        }
                    }
                    else
                    {
                        // Do nothing, values are tied
                        Debug.Log("Multiple max Service values, doing nothing.");
                    }
                }
            }
        }

        if(laneweights.All(value => value == 0) )
        {
            for(int i = 0; i < 4; i++)
            {
                if (i == lastlane)
                {
                    laneweights[i] = 0;
                }
                else
                {
                    laneweights[i] = Line[i];
                }
            }
            Debug.Log("linesInfo edited all zero = [" + string.Join(", ", laneweights) + "]");
        }

        for (int i = 0; i < is_above_limit.Count; i++)
        {
            if (is_above_limit[i] == 0)
            {
                laneweights[i] = laneweights[i] * 1;
            }
            else
            {
                laneweights[i] = laneweights[i] * is_above_limit[i];
            }

            laneweights[i] = laneweights[i] * 0.1f;
        }
        Debug.Log("linesInfo * is_above_limit = [" + string.Join(", ", laneweights) + "]");

        maxIndex = -1;
        maxValue = float.MinValue;

        for (int i = 0; i < laneweights.Count; i++)
        {
            if (laneweights[i] > maxValue)
            {
                maxValue = laneweights[i];
                maxIndex = i;
            }
        }

        last_weight = (int)maxValue;

        // for (int i = 0; i < sortedweights.Count; i++)
        // {
        //     if (i == maxIndex || i ==lastlane)
        //     {
        //         sortedweights[i] = 0;
        //     }
        //     else
        //     {
        //         sortedweights[i] = Line[i];
        //     }
        // }
        // Debug.Log("Sorted weight = [" + string.Join(", ", sortedweights) + "]");

        // if(ServiceCount.Any(value => value != 0) && is_above_limit.Any(value => value > TimeLimitation))
        // {
        //     for (int i = 0; i < 4; i++)
        //     {
        //         if(is_above_limit[i] > TimeLimitation)
        //         {
        //             if(sortedweights[i] != 0)
        //             {
        //                 sortedweights[i] = Line[i];
        //             }
        //             else
        //             {
        //                 sortedweights[i] = 0; 
        //             }
        //         }
        //         else
        //         {
        //             if (ServiceCount[i] != 0)
        //             {
        //                 sortedweights[i] = Line[i];
        //             }
        //             else
        //             {
        //                 sortedweights[i] = 0;
        //             }
        //         }
        //     }
        //     Debug.Log("Sorted weight TE edited= [" + string.Join(", ", sortedweights) + "]");
        // }
        // else
        // {
        //     if (ServiceCount.Count(value => value != 0) > 1)
        //     {
        //         for (int i = 0; i < 4; i++)
        //         {
        //             if (ServiceCount[i] != 0)
        //             {
        //                 if (sortedweights[i] != 0)
        //                 {
        //                     sortedweights[i] = Line[i];
        //                 }
        //             }
        //             else
        //             {
        //                 sortedweights[i] = 0;
        //             }
        //         }
        //         Debug.Log("Sorted weight E edited= [" + string.Join(", ", sortedweights) + "]");
        //     }

        //     if (is_above_limit.Count(value => value > TimeLimitation) > 1)
        //     {
        //         for (int i = 0; i < 4; i++)
        //         {
        //             if (is_above_limit[i] > TimeLimitation)
        //             {
        //                 if (sortedweights[i] != 0)
        //                 {
        //                     sortedweights[i] = Line[i];
        //                 }
        //             }
        //             else
        //             {
        //                 sortedweights[i] = 0;
        //             }
        //         }
        //         Debug.Log("Sorted weight T edited= [" + string.Join(", ", sortedweights) + "]");
        //     }
        // }

        // if(sortedweights.All(value => value == 0) )
        // {
        //     for(int i = 0; i < 4; i++)
        //     {
        //         if (i == lastlane || i == maxIndex)
        //         {
        //             sortedweights[i] = 0;
        //         }
        //         else
        //         {
        //             sortedweights[i] = Line[i];
        //         }
        //     }
        //     Debug.Log("SortedWeights edited all zero = [" + string.Join(", ", sortedweights) + "]");
        // } 

        // // Get sorted indices by descending value
        // List<int> sortindexes = sortedweights
        //     .Select((value, index) => new { value, index })
        //     .OrderByDescending(x => x.value)
        //     .Select(x => x.index)
        //     .ToList();

        // if(reward == maxIndex)
        // {
        //     lane_points = 1;
        //     // Debug.Log("🔟" + lane_points);
        // }
        // else if (reward == lastlane)
        // {
        //     lane_points = -1;
        //     // Debug.Log("💩" + lane_points); 
        // }
        // else
        // {
        //     if(reward == sortindexes[0])
        //     {
        //         lane_points = 0.4f;
        //         // Debug.Log("👍" + lane_points); 
        //     }
        //     else
        //     {
        //         lane_points = -0.4f;
        //         // Debug.Log("⛔" + lane_points); 
        //     }
        // }
    }


    public void CalRewardDuration(int passedDiff)
    {
        minimumDurationmet = false;
        select_empty_cal_penalty = null;

        float realTime;
        float diff;
        if (cancelWait)
        {
            realTime = targetTime - Time.time;
            realTime = Mathf.Round(realTime * 100f) / 100f;

            float diffMin = 0f;
            float diffMax = (int)Math.Round(recieved_duration * 0.71, MidpointRounding.AwayFromZero);
            float newMin = 0f;
            float newMax = 1.0f;

            diff = ((realTime - diffMin) / (diffMax - diffMin)) * (newMax - newMin) + newMin;

            // if (realTime <= 1) { diff = 0; Debug.Log("🏆");}
        }
        else
        {
            realTime = 0;
            diff = 0;
        }

        // Debug.Log("🫗 " + totalEmptyTime);

        Debug.Log($"Before Green {num_vehicle[lastlane]} , carsExited {carCounter[lastlane].passedCarsCount} , passedDiff {passedDiff}" );

        if (recieved_duration == 10 && passedDiff >= 0)
        {
            points = 1;
            Debug.Log("☘️ " + points); 
            totalEmptyTime = 0f;
            return;
        }


        if (passedDiff < 0)
        {
            passedDiff = Mathf.Abs(passedDiff);

            if (recieved_duration >= 27)
            {
                if (passedDiff <= 2){ passedDiff = passedDiff * 3; Debug.Log("🟡3");}
                else if (passedDiff >= 2){ passedDiff = passedDiff * 2; Debug.Log("🟡2");}    
            }
            
            
            // int clamped = Mathf.Clamp(num_vehicle[lastlane], 0, 70);

            // int remained_cars;
            // if (clamped >= 35)
            // {
            //     remained_cars = Mathf.FloorToInt((7f * (1f - ((float)clamped / 70))) + 0.0001f);
            // }
            // else
            // {
            //     remained_cars = Mathf.RoundToInt(num_vehicle[lastlane] * 0.07f); // Up to 07% of the lane's car can be the difference
            // }

            // if (remained_cars >= passedDiff) 
            // { passedDiff = 0; }
            // else
            // {
            //     // passedDiff = (int)Math.Round(passedDiff - remained_cars, MidpointRounding.AwayFromZero);
            //     passedDiff = passedDiff - remained_cars; 
            // }
        }
        else
        {
            int cars = Mathf.Clamp(num_vehicle[lastlane], 20, 70);

            float cal_percentage = 20 - (((float)cars - 20) / (70 - 20)) * 10;

            cal_percentage = cal_percentage / 100;

            int remained_cars = Mathf.RoundToInt(num_vehicle[lastlane] * cal_percentage); // Up to 15% of the lane's car can be the difference

            if (remained_cars >= passedDiff)
            { passedDiff = 0; }
            else
            {
                // passedDiff = (int)Math.Round(passedDiff - remained_cars, MidpointRounding.AwayFromZero);
                passedDiff = passedDiff - remained_cars;
            }
        }

        Debug.Log("passedDiff " + passedDiff);

        float diffMinP = num_vehicle[lastlane] * 0.75f;  
        float diffMaxP = 0f;
        float newMinP = 0f;
        float newMaxP = 1.0f; 

        float diffP = ((passedDiff - diffMinP) / (diffMaxP - diffMinP)) * (newMaxP - newMinP) + newMinP;
        diffP = Mathf.Clamp(diffP, 0f, 1.0f);

        Debug.Log("computed_diffP " + diffP);
        Debug.Log("diff " + diff);
        Debug.Log("realTime " + realTime);

        points = diffP - diff;
        points = Mathf.Clamp(points, 0f, 1f);

        Debug.Log("⌚ " + points);
        totalEmptyTime = 0f;
    }

    private void Observation()
    {
        // Getting the number, type(service, car, van, ...) and sum of vehicle in each lanes.  
        for (int i = 0; i < camera_view_SM.Length; i++)
        {
            // camera_view_SM[i].StoppedandMovingCars();

            // carCount = camera_view_SM[i].SMcarCount;
            // bikeCount = camera_view_SM[i].SMbikeCount;
            // BusCount = camera_view_SM[i].SMBusCount;
            // TruckCount = camera_view_SM[i].SMTruckCount;
            // int ServiceCount = camera_view_SM[i].SMServiceCount;
            // VanCount = camera_view_SM[i].SMVanCount;

            carCount = camera_view_SM[i].carCount;
            bikeCount = camera_view_SM[i].bikeCount;
            BusCount = camera_view_SM[i].BusCount;
            TruckCount = camera_view_SM[i].TruckCount;
            int ServiceCount = camera_view_SM[i].ServiceCount;
            VanCount = camera_view_SM[i].VanCount;

            // beyond_camera_view[i].StoppedandMovingCars();
            int BeyondSM = beyond_camera_view[i].sum_vehicle;
            int CameraSM = camera_view_SM[i].sum_vehicle;
            Bsum = BeyondSM - CameraSM;
            // Debug.Log("Bsum: " + BeyondSM + " - " + CameraSM + " = " + Bsum);

            float weight_sum = carCount + ((float)bikeCount * 0.5f) + (ServiceCount * 2) + (VanCount * 2) + (TruckCount * 3) + (BusCount * 4) + Bsum;
            Line[i] = weight_sum;
            // Line[i] = (int)Math.Round(weight_sum, MidpointRounding.AwayFromZero);
            num_vehicle[i] = carCount + bikeCount + BusCount + TruckCount + ServiceCount + VanCount + Bsum;
        }

        // Checking the presence of service vehicle for each lane.
        for (int i = 0; i < camera_view_SM.Length; i++)
        {
            if (i != lastlane)
            {
                ServiceCount[i] =camera_view_SM[i].ServiceCount;
            }
            else
            { ServiceCount[i] = 0; }
            // Debug.Log("line" + i + ", " + ServiceCount[i]+ ", " +Line[i]); 
        }
        
        // Checking the wait time for each lane.
        for (int i = 0; i < 4; i++)
        {
            float elapsed = Time.time - save_start_time[i];
            is_above_limit[i] = (int)elapsed;
        }
        // Debug.Log("Timers = [" + string.Join(", ", is_above_limit) + "]");


        // Getting ready the state for Duration agent.
        if (!first_round_observation)
        {
            if (lastlane == 0) { laneSpawn = spawnCar.IndivitualDensityN; }
            else if (lastlane == 1) { laneSpawn = spawnCar.IndivitualDensityS; }
            else if (lastlane == 2) { laneSpawn = spawnCar.IndivitualDensityW; }
            else if (lastlane == 3) { laneSpawn = spawnCar.IndivitualDensityE; }

            // List<float> laneSpawns = new List<float>
            // {
            //     spawnCar.IndivitualDensityN,
            //     spawnCar.IndivitualDensityS,
            //     spawnCar.IndivitualDensityW,
            //     spawnCar.IndivitualDensityE
            // };

            // // Dictionary to count each value
            // Dictionary<float, int> counts = new Dictionary<float, int>();

            // foreach (float value in laneSpawns)
            // {
            //     if (counts.ContainsKey(value))
            //         counts[value]++;
            //     else
            //         counts[value] = 1;
            // }

            // // Find which value occurs twice
            // foreach (var kvp in counts)
            // {
            //     if (kvp.Value == 2)
            //     {
            //         double_used = kvp.Key;
            //         break;
            //     }
            // }




            if (lastlane == 0) { duration_weights[0] = 0; duration_weights[1] = 0; }
            if (lastlane == 1) { duration_weights[0] = 0; duration_weights[1] = 1; }
            if (lastlane == 2) { duration_weights[0] = 1; duration_weights[1] = 0; }
            if (lastlane == 3) { duration_weights[0] = 1; duration_weights[1] = 1; }

            // Scale directly into [0, 1]
            float scaled_line_weight = (float)Math.Round(Mathf.Clamp01(Line[lastlane] / 80f), 2);
            duration_weights[2] = scaled_line_weight;

            float scaled_wait_timing = (float)Math.Round(Mathf.Clamp01(is_above_limit[lastlane] / 150f), 2);
            duration_weights[3] = scaled_wait_timing;



            // // Use epsilon-safe comparison
            // // if (Mathf.Approximately(double_used, 1.6f))
            // // {
            // //     if (Mathf.Approximately(laneSpawn, 1.6f)) duration_weights[1] = 0f;
            // //     else if (Mathf.Approximately(laneSpawn, 4f)) duration_weights[1] = 0.1f;
            // //     else if (Mathf.Approximately(laneSpawn, 10f)) duration_weights[1] = 0.2f;
            // // }
            // // else if (Mathf.Approximately(double_used, 4f))
            // // {
            // //     if (Mathf.Approximately(laneSpawn, 1.6f)) duration_weights[1] = 0.4f;
            // //     else if (Mathf.Approximately(laneSpawn, 4f)) duration_weights[1] = 0.5f;
            // //     else if (Mathf.Approximately(laneSpawn, 10f)) duration_weights[1] = 0.6f;
            // // }
            // // else if (Mathf.Approximately(double_used, 10f))
            // // {
            // //     if (Mathf.Approximately(laneSpawn, 1.6f)) duration_weights[1] = 0.8f;
            // //     else if (Mathf.Approximately(laneSpawn, 4f)) duration_weights[1] = 0.9f;
            // //     else if (Mathf.Approximately(laneSpawn, 10f)) duration_weights[1] = 1f;
            // // }

            // if (Mathf.Approximately(laneSpawn, 1.6f)) duration_weights[1] = 0f;
            // else if (Mathf.Approximately(laneSpawn, 4f)) duration_weights[1] = 0.5f;
            // else if (Mathf.Approximately(laneSpawn, 10f)) duration_weights[1] = 1f;



            float hour = 0;
            if (spawnCar.OverallDensity == 0.3f) { hour = 15; } // example: 15:00
            else if (spawnCar.OverallDensity == 0.2f) { hour = 16; }
            else if (spawnCar.OverallDensity == 0.175f) { hour = 17; }
            else if (spawnCar.OverallDensity == 0.15f) { hour = 18; }
            else if (spawnCar.OverallDensity == 0.1f) { hour = 19; }
            else if (spawnCar.OverallDensity == 0) { hour = 20; }


            float angle = 2f * Mathf.PI * hour / 24f;

            float hour_sin = (float)Math.Round(Mathf.Sin(angle), 2);
            float hour_cos = (float)Math.Round(Mathf.Cos(angle), 2);

            Debug.Log("Hour sin: " + hour_sin + ", cos: " + hour_cos);
            duration_weights[4] = hour_sin;
            duration_weights[5] = hour_cos;

            // if (spawnCar.OverallDensity == 0.3f) { duration_weights[4] = 0.4f; }
            // else if (spawnCar.OverallDensity == 0.2f) { duration_weights[4] = 0.5f; }
            // else if (spawnCar.OverallDensity == 0.175f) { duration_weights[4] = 0.6f; }
            // else if (spawnCar.OverallDensity == 0.15f) { duration_weights[4] = 0.7f; }
            // else if (spawnCar.OverallDensity == 0.1f) { duration_weights[4] = 0.8f; }
            // else if (spawnCar.OverallDensity == 0) { duration_weights[4] = 1; }

        }
    }
    
    private void HandlePythonDisconnection()
    {
        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AI-TR traffic system simulation/shared_data.txt");
        File.WriteAllText(filePath, "2");

        // Optionally close everything
        try { stream?.Close(); } catch {}
        try { client?.Close(); } catch {}

        // Quit the app or handle gracefully
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void ConnectToPython()
    {
        try
        {
            client = new TcpClient("127.0.0.1", 65432);
            stream = client.GetStream();
            writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            reader = new StreamReader(stream, Encoding.UTF8);
            Debug.Log("Connected to Python server.");
        }
        catch (Exception e)
        {
            Debug.LogError("Connection failed: " + e.Message);
            HandlePythonDisconnection();
        }
    }

    public void SendWeightandReward(bool DorL, string weight, float? reward = null)
    {
        string message;
        if (DorL == true)
        {
            if (reward != null)
            {
                message = $"DW:{weight},R:{reward.Value}";
            }
            else
            {
                message = $"DW:{weight}";
            }
        }
        else
        {
            if (reward != null)
            {
                message = $"LW:{weight},R:{reward}";
            }
            else
            {
                message = $"LW:{weight}";
            }
        }

        try
        {
            writer.WriteLine(message); // sends message with newline
        }
        catch (IOException ioEx)
        {
            Debug.LogError("Lost connection to Python: " + ioEx.Message);
            HandlePythonDisconnection();
        }
        catch (Exception ex)
        {
            Debug.LogError("Unexpected error: " + ex.Message);
            HandlePythonDisconnection();
        }
    }

    public int ListenForDuration()
    {
        int command = 0;
        
        // string response = reader.ReadLine();
        string response = "";
        try
        {
            response = reader.ReadLine(); // sends message with newline
        }
        catch (IOException ioEx)
        {
            Debug.LogError("Lost connection to Python: " + ioEx.Message);
            HandlePythonDisconnection();
        }
        catch (Exception ex)
        {
            Debug.LogError("Unexpected error: " + ex.Message);
            HandlePythonDisconnection();
        }

        if (response.StartsWith("D:"))
        {
            command = int.Parse(response.Substring(2));
            Debug.Log("Received ⌛ from Python: " + command);
        }
        else if (response.StartsWith("L:"))
        {
            command = int.Parse(response.Substring(2));
            Debug.Log("Received 🛣️ from Python: " + command);
        }
        return command;
    }

    void OnApplicationQuit()
    {
        writer?.Close();
        reader?.Close();
        stream?.Close();
        client?.Close();
    }
}