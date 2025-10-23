using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class TrafficController : MonoBehaviour
{
    private TLSystem TLSystem;
    public bool FixedTimer;
    private bool firstlight = true;
    private Results Results;
    private int lastlane = 0;


    void Start()
    {
        // TLSystem = FindObjectOfType<TLSystem>();
        TLSystem = FindFirstObjectByType<TLSystem>();
        Results = FindFirstObjectByType<Results>();
        StartCoroutine(Type_one());                     
    }

    IEnumerator Type_one()
    {
        yield return new WaitForSeconds(30f);
        int TLtype;
        TLtype = TLSystem.TLtype;

        while(FixedTimer)
        {
            for (int i = 0; i < 4; i++)
            {
                int x = 0;
                if (TLtype == 1)
                {
                    if (i == 1) { x = 3; }
                    else if (i == 2) { x = 1; }
                    else if (i == 3) { x = 2; }
                    

                    if (firstlight)
                    {
                        firstlight = false;
                        // Results.Main(lastlane, true);
                       
                        TLSystem.Send(x);
                        lastlane = x;
                        Results.select_empty_cal_penalty = Results.laneOpts[lastlane];
                        TLSystem.Send(5, 30);
                        yield return new WaitForSeconds(30f);
                    }
                    else
                    {
                        // Results.Main(lastlane, false);
                        TLSystem.Send(4);
                        yield return new WaitForSeconds(4.7f);
                        // Results.Main(lastlane, true);

                        TLSystem.Send(x);
                        lastlane = x;
                        Results.select_empty_cal_penalty = Results.laneOpts[lastlane];
                        TLSystem.Send(5, 30);
                        yield return new WaitForSeconds(30f);
                    }

                }
                else if (TLtype == 2)
                {
                    if (i == 1 || i == 2 || i == 4)
                    {
                        if (firstlight)
                        {
                            firstlight = false;
                            TLSystem.Send(i);
                            yield return new WaitForSeconds(30f);
                        }
                        else
                        {
                            TLSystem.Send(i);
                            yield return new WaitForSeconds(34.5f);
                        }
                    }
                }
                else if (TLtype == 3)
                {
                    if (i == 1 || i == 2 || i == 3 || i == 4)
                    {
                        if (firstlight)
                        {
                            firstlight = false;
                            TLSystem.Send(i);
                            yield return new WaitForSeconds(20f);
                        }
                        else
                        {
                            TLSystem.Send(i);
                            yield return new WaitForSeconds(24.5f);
                        }
                    }
                }
            }
        }
    }
}
