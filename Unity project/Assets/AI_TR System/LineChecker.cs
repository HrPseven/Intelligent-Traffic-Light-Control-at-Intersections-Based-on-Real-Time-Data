using UnityEngine;

public class LineChecker : MonoBehaviour
{
    private int _insideObjectsCount = 0;
    public bool IsSpawnPointFree
    {
        get { return _insideObjectsCount == 0; }
    }

    private void OnTriggerEnter(Collider other)
    {
        _insideObjectsCount++;
    }

    private void OnTriggerExit(Collider other)
    {
        _insideObjectsCount--;
    }
}