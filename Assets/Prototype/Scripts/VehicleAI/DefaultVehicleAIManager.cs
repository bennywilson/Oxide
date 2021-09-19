using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SplineMesh;
public class DefaultVehicleAIManager : VehicleAIManager
{
    [SerializeField] GameObject[] _carPrefabs = null;

    [SerializeField]
    int _maxNumAIVehicles = 32;

    [SerializeField]
    float _spawnAIAheadDistance = 100;

    [SerializeField]
    float _maxAIAheadDistance = 150;

    [SerializeField]
    float _spawnAIBehindDistance = 50;

    [SerializeField]
    float _maxAIBehindDistance = 15;

    [SerializeField]
    float _startingMinDistanceBetweenVehicleAI = 15;

    [SerializeField]
    float _endingMinDistanceBetweenVehicleAI = 5;

    [SerializeField]
    float _startingMaxDistanceBetweenVehicleAI = 20;

    [SerializeField]
    float _endingMaxDistanceBetweenVehicleAI = 5;

    [SerializeField]
    float _AISpeed = 1;

    LinkedList<VehicleAI> _vehicleAIList = new LinkedList<VehicleAI>();

    GameObject _road = null;
    Spline _roadSpline = null;

    // Start is called before the first frame update
    void Start()
    {

    }

    float GetRandomDistanceBetweenVehicleAI()
    {
        float normalizedDistanceTravelled = _gameController.GetNormalizedDistanceTravelled();
        float minVal = Mathf.Lerp(_startingMinDistanceBetweenVehicleAI, _endingMinDistanceBetweenVehicleAI, normalizedDistanceTravelled);
        float maxVal = Mathf.Lerp(_startingMaxDistanceBetweenVehicleAI, _endingMaxDistanceBetweenVehicleAI, normalizedDistanceTravelled);
        return Random.Range(minVal, maxVal);
    }

    public override void OnRaceStart()
    {
        _road = GameObject.Find("ProceduralRoadPiece(Clone)");
        _roadSpline = _road.GetComponent<Spline>();

        _gameController = gameObject.GetComponent<GameController>();
        VehicleBase Player = _gameController.GetPlayer();
        float PlayerDist = Player.DistanceAlongSpline;

        for (float distance = PlayerDist + GetRandomDistanceBetweenVehicleAI(); distance < PlayerDist + _spawnAIAheadDistance - _startingMinDistanceBetweenVehicleAI; distance += GetRandomDistanceBetweenVehicleAI())
        {
            SpawnCar(distance);
        }
    }

    void FixedUpdate()
    {
        if (_gameController == null)
        {
            return;
        }

        VehicleBase Player = _gameController.GetPlayer();
        float PlayerDist = Player.DistanceAlongSpline;
        VehicleAI FrontVehicle = (_vehicleAIList.First == null) ? (null) : (_vehicleAIList.First.Value);
        VehicleAI LastVehicle = (_vehicleAIList.Last == null) ? (null) : (_vehicleAIList.Last.Value);

        float normalizedDistanceTravelled = _gameController.GetNormalizedDistanceTravelled();
        float minDistanceBetweenVehicleAI = Mathf.Lerp(_startingMinDistanceBetweenVehicleAI, _endingMinDistanceBetweenVehicleAI, normalizedDistanceTravelled);
        float maxDistanceBetweenVehicleAI = Mathf.Lerp(_startingMaxDistanceBetweenVehicleAI, _endingMaxDistanceBetweenVehicleAI, normalizedDistanceTravelled);

        if (PlayerDist < _roadSpline.Length - _spawnAIAheadDistance)
        {
            if (_vehicleAIList.Count < _maxNumAIVehicles)
            {
                if (FrontVehicle == null || FrontVehicle.Distance < (PlayerDist + _spawnAIAheadDistance - minDistanceBetweenVehicleAI))
                {
                    VehicleAI newCar = SpawnCar(PlayerDist + _spawnAIAheadDistance + Random.Range(0, maxDistanceBetweenVehicleAI - minDistanceBetweenVehicleAI));
                    if (newCar != null)
                    {
                        _vehicleAIList.AddFirst(newCar);
                        FrontVehicle = _vehicleAIList.First.Value;
                    }
                }


                LastVehicle = (_vehicleAIList.Last == null) ? (null) : (_vehicleAIList.Last.Value);

                if (LastVehicle != null && LastVehicle.Distance > PlayerDist)
                {
                    VehicleAI newCar = SpawnCar(Mathf.Clamp(PlayerDist - _spawnAIBehindDistance - Random.Range(0, minDistanceBetweenVehicleAI), 0, _roadSpline.Length));
                    if (newCar != null)
                    {
                        _vehicleAIList.AddLast(newCar);
                        LastVehicle = _vehicleAIList.Last.Value;
                    }
                }
            }
        }

        if (FrontVehicle.Distance > PlayerDist + _maxAIAheadDistance)
        {
            _vehicleAIList.RemoveFirst();
            Object.Destroy(FrontVehicle.gameObject);
        }

        if (LastVehicle != null && LastVehicle.Distance < (PlayerDist - _maxAIBehindDistance))
        {
            _vehicleAIList.RemoveLast();

            Object.Destroy(LastVehicle.gameObject);
        }
    }
    Vector3 RandomHorizontalOffset(float maxRadius)
    {
        var randomInside = Random.insideUnitCircle;

        return new Vector3(randomInside.x * maxRadius, 0, randomInside.y * maxRadius);
    }

    VehicleAI SpawnCar(float splineDist)
    {
        var car = _carPrefabs[Random.Range(0, _carPrefabs.Length)];
        if (car == null)
        {
            return null;
        }

        var sample = _roadSpline.GetSampleAtDistance(splineDist);
        var roadTangent = Vector3.ProjectOnPlane(sample.tangent, Vector3.up);
        roadTangent = Vector3.Cross(roadTangent, Vector3.up);

        var roadPoint = _road.transform.TransformPoint(sample.location) + RandomHorizontalOffset(1);

        var carPos = RandomHorizontalOffset(1);
        GameObject spawnedCar = Instantiate(car, carPos, Quaternion.LookRotation(sample.tangent), transform);
        var Mesh = spawnedCar.GetComponentsInChildren<MeshRenderer>();

        Vector3 randomColor = Random.insideUnitSphere;
        randomColor.Normalize();

        VehicleAI carAI = spawnedCar.GetComponentInChildren<VehicleAI>();
        carAI.Distance = splineDist;

        Mesh[0].materials[0].SetVector("BaseColor", randomColor);
        Mesh[1].materials[0].SetVector("OutlineColor", randomColor * 0.25f);
        carAI.Speed = _AISpeed;
        return carAI;
    }
        
}
