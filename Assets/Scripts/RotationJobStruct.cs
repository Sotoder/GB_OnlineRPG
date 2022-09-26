using UnityEngine;
using UnityEngine.Jobs;

public partial class Fractal
{
    public struct RotationJobStruct : IJobParallelForTransform
    {
        public float RotationSpeed;

        public void Execute(int index, TransformAccess transform)
        {
            var rot = transform.localRotation;
            rot = rot * Quaternion.Euler(0, RotationSpeed, 0);
            transform.localRotation = rot;
        }
    }
}