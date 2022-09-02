using System;
using UnityEngine;

[Serializable]
public class PlanetConfig
{
    [SerializeField] private GameObject _planet;
    [SerializeField] private float _distance;
    [SerializeField] private float _rotateAroundAxisSpeed;
    [SerializeField] private float _rotateAroundStarSpeed;

    public GameObject Planet => _planet;
    public float Distance => _distance;
    public float RotateAroundAxisSpeed => _rotateAroundAxisSpeed;
    public float RotateAroundStarSpeed => _rotateAroundStarSpeed;
}
