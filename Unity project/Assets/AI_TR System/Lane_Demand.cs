using UnityEngine;
using TMPro;

public class Lane_Demand : MonoBehaviour
{
    private Results Results;
    public TMP_Text[] LaneDemand = new TMP_Text[4];

    void Awake()
    {
        Results = FindFirstObjectByType<Results>();
    }

    public void update_laneDemand()
    {
        Results.lanespawn();

        for (int i = 0; i < 4; i++)
        {
            if (Results.laneSpawn[i] > 0 && Results.laneSpawn[i] < 2)
            {
                LaneDemand[i].text = "Low";
            }
            else if (Results.laneSpawn[i] == 4)
            {
                LaneDemand[i].text = "Moderate";
            }
            else if (Results.laneSpawn[i] == 10)
            {
                LaneDemand[i].text = "High";
            }
        }
    }
}
