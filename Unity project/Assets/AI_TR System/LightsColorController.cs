using System;
using UnityEngine;

public class LightsColorController : MonoBehaviour
{
    private Color[] colors = { Color.red, Color.green, Color.yellow }; // Color array

    public GameObject[] plane;
    public MeshRenderer[] planeRenderer;
    public SemaphorePeople[] semaphore;
    public bool isPeople;
    public int c;

    void Start()
    {
        // Assign the correct Renderer for each plane in the array
        // for (int i = 0; i < plane.Length; i++)
        // {
        //     if (plane[i] != null)
        //     {
        //         planeRenderer[i] = plane[i].GetComponent<MeshRenderer>();
        //     }
        // }
    }

    public void Submit()
    {
        UpdateColors(c);
    }

    public void UpdateColors(int i)
    {
        // Update the color for each plane
        for (int j = 0; j < planeRenderer.Length; j++)
        {
            if (planeRenderer[j] != null)
            {
                planeRenderer[j].material.color = colors[i];
            }
            else
            {
                Debug.LogError("Renderer missing on one of the planes!");
            }
        }
            
        for (int s = 0; s < semaphore.Length; s++)
        {
            if (semaphore != null && (i == 0 || i == 2))
            {
                if (isPeople)
                {
                    semaphore[s].PEOPLE_CAN = false;
                }
                else
                {
                    semaphore[s].CAR_CAN = false;
                }
            }
            else
            {
                if (isPeople)
                {
                    semaphore[s].PEOPLE_CAN = true;
                }
                else
                {
                    semaphore[s].CAR_CAN = true;
                }
            }
        }
    }
}