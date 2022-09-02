using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(SystemConfigurator), menuName = "System Config", order = 0)]
public class SystemConfigurator : ScriptableObject
{
    [SerializeField] private GameObject _star;
    [SerializeField] private List<PlanetConfig> _planets;

    public GameObject Star => _star;
    public List<PlanetConfig> Planets => _planets;
}
