using UnityEngine;

public class CarCounter : MonoBehaviour
{
    public int passedCarsCount = 0; // Count of cars passed through this tube (cube)
    private Results Results; // Reference to the CarCounterManager

    void Start()
    {
        Results = FindFirstObjectByType<Results>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object passing through is tagged as a car
        if (other.CompareTag("Car") || other.CompareTag("Bike") || other.CompareTag("Truck") || other.CompareTag("Bus") || other.CompareTag("Van") || other.CompareTag("Service")) // Assuming the cars are tagged with "Car" and ... .
        {
            passedCarsCount++; // Increase the count for this cube
            Results.UpdateTotalPassedCars(); // Update the total count in the CarCounterManager
            // Debug.Log($"{name} - Passed cars: {passedCarsCount}"); // Print individual count for this cube
        }
    }
}