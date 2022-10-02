using System.Collections.Generic;
using UnityEngine;

public class SpawnPointsMap : MonoBehaviour
{
    [SerializeField] public Camera Camera;
    [SerializeField][HideInInspector] public GameObject PointPref;
    [SerializeField][HideInInspector] public float WorldScreenHeight = 600;
    [SerializeField][HideInInspector] public int RenderTextureHeight = 1080;
    [SerializeField][HideInInspector] public GameObject SpawnPointsHolder;

    [SerializeField][HideInInspector] private List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();

    public List<SpawnPoint> SpawnPoints => _spawnPoints;

    public void AddSpawnPoint(GameObject gameObject)
    {
        var spawnPoint = gameObject.GetComponent<SpawnPoint>();

        if (_spawnPoints.Contains(spawnPoint)) return;

        _spawnPoints.Add(spawnPoint);
    }

    public void RemoveLastSpawnPoint()
    {
        DestroyImmediate(_spawnPoints[_spawnPoints.Count - 1].gameObject);
        _spawnPoints.Remove(_spawnPoints[_spawnPoints.Count - 1]);
    }

    public void ClearSpawnPoints()
    {
        for (int i = 0; i < _spawnPoints.Count; i++)
        {
            if (_spawnPoints[i] != null)
            {
                DestroyImmediate(_spawnPoints[i].gameObject);
            }
        }

        _spawnPoints.Clear();
    }

    public void SetCoordinateX(int index, float value)
    {
        var pos = _spawnPoints[index].transform.position;
        pos.x = value;
        _spawnPoints[index].transform.position = pos;
    }

    public void SetCoordinateZ(int index, float value)
    {
        var pos = _spawnPoints[index].transform.position;
        pos.z = value;
        _spawnPoints[index].transform.position = pos;
    }

}
