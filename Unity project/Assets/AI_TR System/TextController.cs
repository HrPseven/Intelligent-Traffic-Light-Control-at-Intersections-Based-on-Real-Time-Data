using TMPro;
using UnityEngine;
using System.Collections;


public class TextController : MonoBehaviour
{
    public TMP_Text GreenDu;
    private TMP_Text GreenTitle;
    private TMP_Text Yellow;
    private TMP_Text Red;
    private TMP_Text Wait;
    private TMP_Text WaitTitle;
    private TMP_Text LaneDemand;

    private Results Results;
    public int lane = 0;
    private Camera mainCamera;

    private string[] dynamicOptions = { "Signal: Green", "Flow: High", "Density: Moderate" };

    void Awake()
    {
        foreach (TMP_Text t in GetComponentsInChildren<TMP_Text>())
        {
            if (t.name == "GreenDu") GreenDu = t;
            if (t.name == "GreenTitle") GreenTitle = t;
            if (t.name == "Yellow") Yellow = t;
            if (t.name == "Red") Red = t;
            if (t.name == "Wait") Wait = t;
            if (t.name == "WaitTitle") WaitTitle = t;
            if (t.name == "LaneDemand") LaneDemand = t;
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
        Results = FindFirstObjectByType<Results>();

        // StartCoroutine(DoAfterStart());
    }

    // IEnumerator DoAfterStart()
    // {
    //     yield return null; // wait one frame
        
    //     if (Results.laneSpawn[lane] > 0 && Results.laneSpawn[lane] < 2)
    //     {
    //         LaneDemand.text = "Low";
    //     }
    //     else if (Results.laneSpawn[lane] == 4)
    //     {
    //         LaneDemand.text = "Medium";
    //     }
    //     else if (Results.laneSpawn[lane] == 10)
    //     {
    //         LaneDemand.text = "High";
    //     }
    // }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Make the panel face the camera with its front (-Z) side
            Vector3 directionToCamera = transform.position - mainCamera.transform.position;
            transform.rotation = Quaternion.LookRotation(directionToCamera);

            // Keep it upright (optional)
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        }
    }

    void Update()
    {
        if (Results.Redlight[lane] && Results.greenDu[lane] == 0 && Results.is_above_limit[lane] == 0)
        {
            Yellow.gameObject.SetActive(false);
            GreenDu.gameObject.SetActive(false);
            GreenTitle.gameObject.SetActive(false);
            Wait.gameObject.SetActive(false);
            WaitTitle.gameObject.SetActive(false);

            Red.gameObject.SetActive(true);
        }
        else if (Results.Yellowlight[lane])
        {
            Red.gameObject.SetActive(false);
            GreenDu.gameObject.SetActive(false);
            GreenTitle.gameObject.SetActive(false);
            Wait.gameObject.SetActive(false);
            WaitTitle.gameObject.SetActive(false);

            Yellow.gameObject.SetActive(true);
        }
        else if (Results.greenDu[lane] != 0)
        {
            Red.gameObject.SetActive(false);
            Yellow.gameObject.SetActive(false);
            Wait.gameObject.SetActive(false);
            WaitTitle.gameObject.SetActive(false);

            GreenDu.gameObject.SetActive(true);
            GreenTitle.gameObject.SetActive(true);
            UpdatePanel();
        }
        else if (Results.is_above_limit[lane] != 0)
        {
            Red.gameObject.SetActive(false);
            Yellow.gameObject.SetActive(false);
            GreenDu.gameObject.SetActive(false);
            GreenTitle.gameObject.SetActive(false);

            Wait.gameObject.SetActive(true);
            WaitTitle.gameObject.SetActive(true);
            UpdatePanel();
        }
    }

    public void UpdatePanel()
    {
        GreenDu.text = $"{Results.greenDu[lane]}s";
        Wait.text = $"{Results.is_above_limit[lane]}s";
    }
}