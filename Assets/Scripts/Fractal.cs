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
                
        transforms.Add(transform);

        var parentTransforms = new List<Transform>();
        var childTransforms = new List<Transform>();

        parentTransforms.Add(transform);

        for (int i = 0; i < _depth; i++)
        {
            foreach (var parent in parentTransforms)
            {
                var childA = CreateChild(Vector3.up, Quaternion.identity);
                var childB = CreateChild(Vector3.right, Quaternion.Euler(0f, 0f, -90f));
                var childC = CreateChild(Vector3.left, Quaternion.Euler(0f, 0f, 90f));
                var childD = CreateChild(Vector3.forward, Quaternion.Euler(90f, 0f, 0f));
                var childE = CreateChild(Vector3.back, Quaternion.Euler(-90f, 0f, 0f));

                transforms.Add(childA.transform);
                transforms.Add(childB.transform);
                transforms.Add(childC.transform);
                transforms.Add(childD.transform);
                transforms.Add(childE.transform);

                childA.transform.SetParent(parent, false);
                childB.transform.SetParent(parent, false);
                childC.transform.SetParent(parent, false);
                childD.transform.SetParent(parent, false);
                childE.transform.SetParent(parent, false);

                childTransforms.Add(childA.transform);
                childTransforms.Add(childB.transform);
                childTransforms.Add(childC.transform);
                childTransforms.Add(childD.transform);
                childTransforms.Add(childE.transform);                
            }

            parentTransforms.Clear();
            parentTransforms.AddRange(childTransforms);
            childTransforms.Clear();
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
        RotationJobStruct rotationJobStruct = new RotationJobStruct()
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