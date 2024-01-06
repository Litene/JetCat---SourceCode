using System.Collections;
using System.Collections.Generic;
using Car;
using UnityEngine;

public class CarSpawn : MonoBehaviour
{
    [Range(-1, 1)] [SerializeField] private int driveDirection;
    [SerializeField] private float roadWidth;

    [SerializeField] private float noiseStrength = .05f;
    [Range(2, 100)][SerializeField] private int pathPoints = 3;

    [SerializeField] private bool useOverride;
    [SerializeField] private Transform startOverride, endOverride; //could replace these with custom editor

    public CarBehaviour SpawnCar(GameObject carPrefab)
    {
        float directionSign = driveDirection == 0 ? MathHelper.RandomSign() : driveDirection;

        var position = transform.position;
        Vector3 startPos = position + Vector3.right * (roadWidth * directionSign);
        Vector3 endPos = position - Vector3.right * (roadWidth * directionSign);

        if (useOverride)
        {
            startPos = directionSign > 0 ? startOverride.position : endOverride.position;
            endPos = directionSign > 0 ? endOverride.position : startOverride.position;
        }

        WayPoint[] wayPoints = CarHelpers.GenerateStraightPath(startPos, endPos, noiseStrength, pathPoints);
        
        CarBehaviour car = Instantiate(carPrefab, wayPoints[0].Position, Quaternion.Euler(0, 90*directionSign, 0)).GetComponent<CarBehaviour>();
        
        car.SetWayPointData(wayPoints);

        return car;
    }
}
