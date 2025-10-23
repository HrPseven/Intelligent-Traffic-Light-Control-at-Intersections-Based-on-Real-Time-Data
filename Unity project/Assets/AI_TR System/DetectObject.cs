using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting;

public class DetectObjectsInArea : MonoBehaviour
{
    // This will hold the count of cars and bikes
    public int carCount = 0;
    public int bikeCount = 0;
    public int VanCount = 0;
    public int BusCount = 0;
    public int ServiceCount = 0;
    public int TruckCount = 0;
    public int sum_vehicle;

    public int SMcarCount = 0;
    public int SMbikeCount = 0;
    public int SMVanCount = 0;
    public int SMBusCount = 0;
    public int SMServiceCount = 0;
    public int SMTruckCount = 0;

    public int stopC;
    public int moveC;
    public int isHeavy;
    public bool is_service = false;


    
    private readonly List<CarAIController> carsInBox = new List<CarAIController>();
    private float distanceThreshold = 150f;
    private CarAIController lastStopped;
    public Transform fallbackPoint; // assign in the Inspector
    // public Transform MaxPoint; // assign in the Inspector



    // ✅ Call this to get the current speed of all cars inside the trigger
    public void StoppedandMovingCars()
    {
        // 1. Get all stopped cars (speed ~ 0)
        List<CarAIController> stoppedCars = carsInBox
            .Where(car => car.curMoveSpeed == 0)
            .ToList();

        if (stoppedCars.Count == 0)
        {
            // Debug.Log("No stopped cars.");
        }
        else
        {
            lastStopped = stoppedCars[stoppedCars.Count - 1];
        }

        // 2. Get the *last* stopped car (latest in list — adjust logic as needed)
        Vector3 refPos;
        if (lastStopped != null)
        {
            refPos = lastStopped.transform.position;
        }
        else
        {
            refPos = fallbackPoint.position;
        }
        // Debug.Log("refpos " + refPos);

        // 3. Count moving cars within 50m of last stopped car
        List<CarAIController> movingCars = carsInBox
            .Where(car => car != lastStopped &&
             car.curMoveSpeed != 0 &&
             Vector3.Distance(car.transform.position, refPos) <= distanceThreshold)
            .ToList();
        //  && Vector3.Distance(refPos, car.transform.position) >= Vector3.Distance(refPos, MaxPoint.position))


        is_service = stoppedCars.Any(car =>
            car != null &&
            (
                car.transform.Find("Checking Box") is Transform box &&
                box.CompareTag("Service") 

            ));

        bool heavyVehicle = stoppedCars.Any(car =>
            car != null &&
            (
                car.transform.Find("Checking Box") is Transform box &&
                box.CompareTag("Bus") 

            ));
            //  ||
            // movingCars.Any(car =>
            // car != null &&
            // (
            //     car.transform.Find("Checking Box") is Transform box &&
            //     box.CompareTag("Bus")

            // ));

        var allCars = stoppedCars.Concat(movingCars); // Combine both lists

        var vehicleCounts = allCars
            .Select(car => car?.transform.Find("Checking Box"))
            .Where(box => box != null)
            .GroupBy(box => box.tag)
            .ToDictionary(group => group.Key, group => group.Count());

        // foreach (var kvp in vehicleCounts)
        // {
        //     Debug.Log($"Tag: {kvp.Key}, Count: {kvp.Value}");
        // }

        SMcarCount = vehicleCounts.ContainsKey("Car") ? vehicleCounts["Car"] : 0;
        SMBusCount = vehicleCounts.ContainsKey("Bus") ? vehicleCounts["Bus"] : 0;
        SMTruckCount = vehicleCounts.ContainsKey("Truck") ? vehicleCounts["Truck"] : 0;
        SMVanCount = vehicleCounts.ContainsKey("Van") ? vehicleCounts["Van"] : 0;
        SMServiceCount = vehicleCounts.ContainsKey("Service") ? vehicleCounts["Service"] : 0;
        SMbikeCount = vehicleCounts.ContainsKey("Bike") ? vehicleCounts["Bike"] : 0;

        stopC = stoppedCars.Count;
        moveC = movingCars.Count;
        if (heavyVehicle) { isHeavy = 1; }
        else { isHeavy = 0; }

        // Debug.Log($"Last stopped car: {lastStopped}");
        // Debug.Log($"Moving cars within {distanceThreshold}m: {movingCars.Count} | Stopped Cars: {stoppedCars.Count} | has heavy: {heavyVehicle}");
    }

    // private void OnDrawGizmos()
    // {
    //     if (carsInBox == null || carsInBox.Count == 0) return;

    //     float speedThreshold = 0.1f;
    //     List<CarAIController> stoppedCars = carsInBox
    //         .Where(car => car != null && car.curMoveSpeed < speedThreshold)
    //         .ToList();

    //     if (stoppedCars.Count == 0) return;

    //     CarAIController lastStopped = stoppedCars[stoppedCars.Count - 1];

    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(lastStopped.transform.position, distanceThreshold);
    // }

    // Called when another collider enters this trigger area
    private void OnTriggerEnter(Collider other)
    {
        CarAIController car = other.GetComponentInParent<CarAIController>();
        if (car != null && !carsInBox.Contains(car))
        {
            carsInBox.Add(car);
        }

        if (other.CompareTag("Car"))
        {
            carCount++;
            // Debug.Log($"A car entered. Total Cars in {gameObject.name}: " + carCount);
        }
        else if (other.CompareTag("Bike"))
        {
            bikeCount++;
            // Debug.Log($"A bike entered. Total Bike in {gameObject.name}: " + bikeCount);
        }
        else if (other.CompareTag("Bus"))
        {
            BusCount++;
            // Debug.Log($"A Bus entered. Total Bus in {gameObject.name}: " + BusCount);
        }
        else if (other.CompareTag("Van"))
        {
            VanCount++;
            // Debug.Log($"A Van entered. Total Van in {gameObject.name}: " + VanCount);
        }
        else if (other.CompareTag("Service"))
        {
            ServiceCount++;
            // Debug.Log($"A Service car entered. Total Service in {gameObject.name}: " + ServiceCount);
        }
        else if (other.CompareTag("Truck"))
        {
            TruckCount++;
            // Debug.Log($"A Truck entered. Total truck in {gameObject.name}: " + TruckCount);
        }

        sum_vehicle = carCount + bikeCount + BusCount + TruckCount + ServiceCount + VanCount;
    }


    // Called when another collider exits this trigger area
    private void OnTriggerExit(Collider other)
    {
        CarAIController car = other.GetComponentInParent<CarAIController>();
        if (car != null && carsInBox.Contains(car))
        {
            carsInBox.Remove(car);
        }

        if (other.CompareTag("Car"))
        {
            carCount--;
            // Debug.Log($"A car exited. Total Cars in {gameObject.name}: " + carCount);
        }
        else if (other.CompareTag("Bike"))
        {
            bikeCount--;
            // Debug.Log($"A bike exited. Total Bike in {gameObject.name}: " + bikeCount);
        }
        else if (other.CompareTag("Bus"))
        {
            BusCount--;
            // Debug.Log($"A Bus exited. Total Bus in {gameObject.name}: " + BusCount);
        }
        else if (other.CompareTag("Van"))
        {
            VanCount--;
            // Debug.Log($"A Van exited. Total Van in {gameObject.name}: " + VanCount);
        }
        else if (other.CompareTag("Service"))
        {
            ServiceCount--;
            // Debug.Log($"A Service car exited. Total Service in {gameObject.name}: " + ServiceCount);
        }
        else if (other.CompareTag("Truck"))
        {
            TruckCount--;
            // Debug.Log($"A Truck exited. Total truck in {gameObject.name}: " + TruckCount);
        }

        sum_vehicle = carCount + bikeCount + BusCount + TruckCount + ServiceCount + VanCount;
    }
}
