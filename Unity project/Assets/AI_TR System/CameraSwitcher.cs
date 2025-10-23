using UnityEngine;
using System.Collections;

public class CameraSwitcher : MonoBehaviour
{
    public Transform[] cameraPositions;  // Assign in Inspector
    public Camera mainCamera;

    public float transitionDuration = 1.0f; // Seconds
    private Coroutine currentTransition;

        void Start()
    {
        mainCamera = FindFirstObjectByType<Camera>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToPosition(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToPosition(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToPosition(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchToPosition(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SwitchToPosition(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) SwitchToPosition(5);

    }

    public void SwitchToPosition(int index)
    {
        if (index < 0 || index >= cameraPositions.Length) return;

        if (currentTransition != null)
            StopCoroutine(currentTransition); // Stop any current transition

        currentTransition = StartCoroutine(SmoothMove(cameraPositions[index]));
    }

    private IEnumerator SmoothMove(Transform target)
    {
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            // Smooth interpolation
            mainCamera.transform.position = Vector3.Lerp(startPos, target.position, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, target.rotation, t);

            yield return null;
        }

        // Ensure final position is exactly target
        mainCamera.transform.position = target.position;
        mainCamera.transform.rotation = target.rotation;
    }
}
