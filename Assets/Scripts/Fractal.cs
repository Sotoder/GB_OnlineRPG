using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;

public partial class Fractal : MonoBehaviour
{
    [SerializeField, Range(1, 8)] private int _depth = 4;
    [SerializeField, Range(0, 4)] private float _rotationSpeed;
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    private const float _positionOffset = .75f;
    private const float _scaleBias = .5f;

    private TransformAccessArray _fractalTransformsArray;

    private void Start()
    {
        var transforms = new List<Transform>();
        var parents = new List<Transform>();
        var childs = new List<Transform>();

        transforms.Add(transform);
        parents.Add(transform);

        for (int i = 0; i < _depth - 1; i++) // так как уже поклали первый трансформ в лист, то по идее глубину надо уменьшить на 1
        {
            foreach (var parent in parents)
            {
                var childA = CreateChild(Vector3.up, Quaternion.identity);
                var childB = CreateChild(Vector3.right, Quaternion.Euler(0f, 0f, -90f));
                var childC = CreateChild(Vector3.left, Quaternion.Euler(0f, 0f, 90f));
                var childD = CreateChild(Vector3.forward, Quaternion.Euler(90f, 0f, 0f));
                var childE = CreateChild(Vector3.back, Quaternion.Euler(-90f, 0f, 0f));
                childA.transform.SetParent(parent, false);
                childB.transform.SetParent(parent, false);
                childC.transform.SetParent(parent, false);
                childD.transform.SetParent(parent, false);
                childE.transform.SetParent(parent, false);

                childs.Add(childA.transform);
                childs.Add(childB.transform);
                childs.Add(childC.transform);
                childs.Add(childD.transform);
                childs.Add(childE.transform);
                transforms.Add(childA.transform);
                transforms.Add(childB.transform);
                transforms.Add(childC.transform);
                transforms.Add(childD.transform);
                transforms.Add(childE.transform);
            }

            parents.Clear();
            parents.AddRange(childs);
            childs.Clear();
        }

        _fractalTransformsArray = new TransformAccessArray(transforms.ToArray());
    }

    private void Update()
    {

        Rotation();
    }

    private GameObject CreateChild(Vector3 direction, Quaternion rotation)
    {
        var child = new GameObject("Fractal Part");
        var meshRenderer = child.AddComponent<MeshRenderer>();
        var meshFilter = child.AddComponent<MeshFilter>();

        meshRenderer.material = _material;
        meshFilter.mesh = _mesh;

        child.transform.localPosition = _positionOffset * direction;
        child.transform.localRotation = rotation;
        child.transform.localScale = _scaleBias * Vector3.one;

        return child;
    }

    private void Rotation()
    {
        RotationJob rotationJobStruct = new RotationJob()
        {
            RotationSpeed = _rotationSpeed
        };

        JobHandle jobHandle = rotationJobStruct.Schedule(_fractalTransformsArray);
        jobHandle.Complete();
    }

    private void OnDestroy()
    {
        if(_fractalTransformsArray.isCreated)
        {
            _fractalTransformsArray.Dispose();
        }
    }
}