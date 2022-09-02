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

    private void Awake()
    {
        var transforms = GenerateSystem();

        if (!IsArrayCreated(_planetsTransform))
        {
            _planetsTransform = new TransformAccessArray(transforms);
        }

        if(!IsArrayCreated(_axisAngles))
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

    private Transform[] GenerateSystem()
    {
        Transform[] transforms = new Transform[_configurator.Planets.Count];

        var star = Instantiate(_configurator.Star);
        star.transform.position = Vector3.zero;

        for(int i = 0; i < _configurator.Planets.Count; i++)
        {
            var planet = Instantiate(_configurator.Planets[i].Planet);
            var rndPositionZ = Random.insideUnitSphere.z * _configurator.Planets[i].Distance;
            var rndPositionX = Random.Range(0, 1f) > 0.5f ? _configurator.Planets[i].Distance : -_configurator.Planets[i].Distance;
            planet.transform.position = new Vector3(rndPositionX, 0, rndPositionZ);

            transforms[i] = planet.transform;
        }

        return transforms;
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
        if (IsArrayCreated(_planetsTransform))
        {
            _planetsTransform.Dispose();
        }

        if(IsArrayCreated(_axisAngles))
        {
            _axisAngles.Dispose();
        }

        if (IsArrayCreated(_axisSpeeds))
        {
            _axisSpeeds.Dispose();
        }

        if (IsArrayCreated(_starSpeeds))
        {
            _starSpeeds.Dispose();
        }
    }
}
