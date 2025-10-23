using System.Security.Cryptography;
using System.Threading;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;


public class TLSystem : MonoBehaviour
{
    private GameObject[] Cpath = new GameObject[4]; // Stores GameObjects
    private LightsColorController[] C = new LightsColorController[4]; // Stores components
    private GameObject[] Ppath = new GameObject[4]; // Stores GameObjects
    private LightsColorController[] P = new LightsColorController[4]; // Stores components
    bool YellowFound = false;
    // bool Pedestrians = false;
    public GameObject[] LeftTurns;
    public int TLtype;
    private bool DoubleEntery = false;
    private int SelectedStore;
    private bool NoneStore;
    private bool? last_p = null;

    private int lastlane = -1;
    private Results Results;
    private Visuals Visuals;


    void Start()
    {
        Results = FindFirstObjectByType<Results>();
        Visuals = FindFirstObjectByType<Visuals>();
        Initialize();
    }

    void Update()
    {

    }

    void Initialize()
    {
        for (int i = 0; i < 4 ; i++)
        {
            Cpath[i] = GameObject.Find($"Cpath{i + 1}");
            C[i] = Cpath[i].GetComponent<LightsColorController>(); // C[0-3] for cars.
            Ppath[i] = GameObject.Find($"Ppath{i + 1}");
            P[i] = Ppath[i].GetComponent<LightsColorController>(); // P[0-3] for people.
        }

        for (int i = 0; i < LeftTurns.Length; i++)
        {
            if (TLtype == 2)
            {
                LeftTurns[i].SetActive(false);
            }
            else if (TLtype == 3 && (i == 0 || i == 1))
            {
                LeftTurns[i].SetActive(false);
            }
        }
    }

    void PassengersHandler(bool switchlight, bool OnOff, bool? allnotOff = true)
    {
        // for (int i = 0; i < 4; i++)
        // {
        //     if(OnOff == true)
        //     {
        //         P[i].c = 1;
        //         P[i].Submit();
        //         Pedestrians = true;
        //     }
        //     else if (OnOff == false)
        //     {
        //         P[i].c = 0;
        //         P[i].Submit();
        //         Pedestrians = false;
        //     }
        // }

            if (allnotOff == false)
            {
                for(int i = 0; i < 4; i++)
                {
                    P[i].c=0;
                    P[i].Submit();
                }
            }
            else
            {

                last_p = switchlight;

                if(OnOff)
                {}
                else
                {
                    for(int i = 0; i < 4; i++)
                    {
                        P[i].c=0;
                        P[i].Submit();
                    }
                    return;
                }

                if ( switchlight == false) // if (C[0].c == 0 && C[1].c == 0)
                {
                    P[0].c = 1;
                    P[0].Submit();
                    P[1].c = 1;
                    P[1].Submit();
                    // Debug.Log("P1");
                }
                else
                {
                    P[0].c = 0;
                    P[0].Submit();
                    P[1].c = 0;
                    P[1].Submit();
                    // Debug.Log("P1.1");
                }

                if (switchlight == true)  // if (C[2].c == 0 && C[3].c == 0)
                {
                    P[2].c = 1;
                    P[2].Submit();
                    P[3].c = 1;
                    P[3].Submit();
                    // Debug.Log("P2");
                }
                else
                {
                    P[2].c = 0;
                    P[2].Submit();
                    P[3].c = 0;
                    P[3].Submit();
                    // Debug.Log("P2.2");
                }
            }
    }

    IEnumerator LightsHandler(int Selected, bool None)
    {
        DoubleEntery = false;
        for (int i = 0; i < 4; i++)
        {
            if (C[i].c == 1)
            {
                C[i].c = 2;
                C[i].Submit();
                YellowFound = true;
            }
        }


        if(last_p == true && (Selected == 0 || Selected == 1))
        {
            if (!None)
            {
                PassengersHandler(true,true); 
            }
            else
            {
                PassengersHandler(true,true, false);
            }
        }
        else
        {
            if (!None)
            {
                PassengersHandler(false,false);
            }
            else
            {
                PassengersHandler(false,false, false);
            }
        }


        if (YellowFound)
        {
            Results.Redlight[lastlane] = false;
            Results.Yellowlight[lastlane] = true;
            yield return new WaitForSeconds(3f);
        }

        for (int i = 0; i < 4; i++)
        {
            if (C[i].c == 2)
            {
                C[i].c = 0;
                C[i].Submit();
            }
        }

        if (YellowFound)
        {
            Results.Yellowlight[lastlane] = false;
            Results.Redlight[lastlane] = true;
            yield return new WaitForSeconds(1.5f);
            Results.Redlight[lastlane] = false; 
        }

        if(Selected == 0 || Selected == 1) 
        {
            if (!None)
            {
                PassengersHandler(true,true);
            }
            else
            {
                PassengersHandler(true,true, false);
            }
        }
        else
        {
            if(!None)
            {
                PassengersHandler(false,true);
            }
            else
            {
                PassengersHandler(false,true, false);
            }
        }


        // if (Pedestrians)
        // {
        //     // PassengersHandler(false);
        //     yield return new WaitForSeconds(4.5f);
        // }

        if (!None)
        {
            for (int i = 0; i < 4; i++)
            {
                if (C[i].c == 1)
                {
                    DoubleEntery = true;
                    LightsHandler(SelectedStore, NoneStore);
                }
            }
            if (TLtype == 1)
            {
                if (!DoubleEntery)
                {
                    C[Selected].c = 1;
                    C[Selected].Submit();
                }
            }
            else if (TLtype == 2)
            {
                if (!DoubleEntery)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        if (Selected == 0 || Selected == 1)
                        {
                            if (C[x] == C[0] || C[x] == C[1])
                            {
                                C[x].c = 1;
                                C[x].Submit();
                            }
                        }
                        else
                        {
                            if (C[x] == C[2] || C[x] == C[3])
                            {
                                C[x].c = 1;
                                C[x].Submit();
                            }
                        }
                    }
                }
            }
            else if (TLtype == 3)
            {
                if(!DoubleEntery)
                {
                    if (Selected == 0 || Selected == 1)
                    {
                        for (int x = 0; x < 2; x++)
                        {
                            C[x].c = 1;
                            C[x].Submit();
                        }
                    }
                    else
                    {
                        C[Selected].c = 1;
                        C[Selected].Submit();
                    }
                }
            }
        }
        else
        {
            // PassengersHandler(true);
        }
        YellowFound = false;
    }

    public void Send(int newOption, float? du = null)
    {
        if (newOption < 4)
        {
            for (int i = 0; i < 4; i++) Results.Redlight[i] = false;
            Results.lastlane2 = newOption;
            Results.lastlane1 = lastlane;
            Visuals.visuals();
            Results.triger = true;
            Results.Main();
            lastlane = newOption;
            Results.select_empty_cal_penalty = Results.laneOpts[newOption];
        }
        else if (newOption == 4)
        { 
            Results.empty_green();
        }

        if (newOption > 4 && du != null)
        {
            Results.targetTime[lastlane] = Time.time + (float)du + 1;
        }

        switch (newOption)
            {
                case 0:
                    StartCoroutine(LightsHandler(0, false));
                    SelectedStore = 0;
                    break;
                case 1:
                    StartCoroutine(LightsHandler(1, false));
                    SelectedStore = 1;
                    break;
                case 2:
                    StartCoroutine(LightsHandler(2, false));
                    SelectedStore = 2;
                    break;
                case 3:
                    StartCoroutine(LightsHandler(3, false));
                    SelectedStore = 3;
                    break;
                case 4:
                    StartCoroutine(LightsHandler(0, true));
                    SelectedStore = 3;
                    break;
                // case 5:
                //     StartCoroutine(LightsHandler(1, true));
                //     SelectedStore = 3;
                //     break;
                // case 6:
                //     StartCoroutine(LightsHandler(2, true));
                //     SelectedStore = 3;
                //     break;
                // case 7:
                //     StartCoroutine(LightsHandler(3, true));
                //     SelectedStore = 3;
                //     break;
                    // default:
                    //     StartCoroutine(LightsHandler(4, true));
                    //     SelectedStore = 4; 
                    //     break;
            }
    }
}
