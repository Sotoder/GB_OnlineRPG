using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

public class SystemStarter : MonoBehaviour
{
    [SerializeField] private SystemConfigurator _configurator;

    private TransformAccessArray _planetsTransform;
    private NativeArray<float> _axisAngles;
    private NativeArray<float> _axisSpeeds;
    private NativeArray<float> _starSpeeds;

    private NativeArray<float> _rndAngles;
    private NativeArray<float> _rndPositions;

    private void Awake()
    {
        if (!IsArrayCreated(_planetsTransform))
        {
            _planetsTransform = new TransformAccessArray(_configurator.Planets.Count);
        }

        if (!IsArrayCreated(_rndAngles))
        {
            _rndAngles = new NativeArray<float>(_configurator.Planets.Count, Allocator.Persistent);
        }

        if (!IsArrayCreated(_rndPositions))
        {
            _rndPositions = new NativeArray<float>(_configurator.Planets.Count, Allocator.Persistent);
        }

        GenerateSystem();

        if (!IsArrayCreated(_axisAngles))
        {
            _axisAngles = new NativeArray<float>(_configurator.Planets.Count, Allocator.Persistent);
        }

        if (!IsArrayCreated(_axisSpeeds))
        {
            _axisSpeeds = new NativeArray<float>(_configurator.Planets.Count, Allocator.Persistent);

            for(int i = 0; i < _configurator.Planets.Count; i++)
            {
                _axisSpeeds[i] = _configurator.Planets[i].RotateAroundAxisSpeed;
            }
        }

        if (!IsArrayCreated(_starSpeeds))
        {
            _starSpeeds = new NativeArray<float>(_configurator.Planets.Count, Allocator.Persistent);

            for (int i = 0; i < _configurator.Planets.Count; i++)
            {
                _starSpeeds[i] = _configurator.Planets[i].RotateAroundStarSpeed;
            }
        }
    }

    private void Update()
    {
        PlanetsRotateJob job = new PlanetsRotateJob
        {
            AxisSpeeds = _axisSpeeds,
            DeltaTime = Time.deltaTime,
            AxisAngles = _axisAngles,
            StarSpeeds = _starSpeeds
        };

        JobHandle jobHandle = job.Schedule(_planetsTransform);
        jobHandle.Complete();
    }

    private void GenerateSystem()
    {
        var star = Instantiate(_configurator.Star);
        star.transform.position = Vector3.zero;

        for(int i = 0; i < _configurator.Planets.Count; i++)
        {
            var planet = Instantiate(_configurator.Planets[i].Planet);
            _planetsTransform.Add(planet.transform);

            var rndPositionX = Random.Range(0, 1f) > 0.5f ? _configurator.Planets[i].Distance : -_configurator.Planets[i].Distance;
            _rndPositions[i] = rndPositionX;

            var rndAngle = Random.Range(0, 360);
            _rndAngles[i] = rndAngle;
        }

        PlanetsGenerateJob job = new PlanetsGenerateJob
        {
            RndAngles = _rndAngles,
            RndPositions = _rndPositions
        };

        JobHandle jobHandle = job.Schedule(_planetsTransform);
        jobHandle.Complete();

        DisposeArray(_rndAngles);
        DisposeArray(_rndPositions);
    }

    private bool IsArrayCreated<T>(NativeArray<T> array) where T : struct
    {
        if (array.IsCreated)
        {
            return true;
        }

        return false;
    }

    private bool IsArrayCreated(TransformAccessArray array)
    {
        if (array.isCreated)
        {
            return true;
        }

        return false;
    }

    private void OnDestroy()
    {
        DisposeArray(_planetsTransform);
        DisposeArray(_axisAngles);
        DisposeArray(_axisSpeeds);
        DisposeArray(_starSpeeds);
    }

    private void DisposeArray<T>(NativeArray<T> array) where T : struct
    {
        if (IsArrayCreated(array))
        {
            array.Dispose();
        }
    }

    private void DisposeArray(TransformAccessArray array)
    {
        if (IsArrayCreated(array))
        {
            array.Dispose();
        }
    }
}
