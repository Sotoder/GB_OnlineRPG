using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.UI;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public class end : MonoBehaviour
{

    [SerializeField] private GameObject _prefab;
    [SerializeField] private Vector3 _direction;
    [SerializeField] private int _count;
    [SerializeField] private float _spawnRadius;
    private NativeArray<Color> _colorsA;
    private NativeArray<Color> _Output;
    private NativeArray<int> _angle;
    private TransformAccessArray _accessArray;
    private Material[] _Cubesu;

    IEnumerator ChangeDirection()
    {
       
            while (true)
            {
            shuffle job = new shuffle() 
            {
                Colors = _colorsA,
            seed=(uint)(UnityEngine.Random.value*10000)
            };
            JobHandle jobHandle = job.Schedule();
            jobHandle.Complete();
                yield return new WaitForSeconds(1f);
            }

        
    }
  
    private void Start()
    {
        _Cubesu = new Material[_count];
        _colorsA = new NativeArray<Color>(_count, Allocator.Persistent);
        _Output = new NativeArray<Color>(_count, Allocator.Persistent);
        _angle = new NativeArray<int>(_count, Allocator.Persistent);
        _accessArray = new TransformAccessArray(SpawnObj(_prefab, _count, _spawnRadius));
        for (int i = 0; i < _count; i++)
        {
            _colorsA[i] = Random.ColorHSV();
            _Output[i]=Random.ColorHSV();
            _angle[i] = Random.Range(0, 180);
        }
      StartCoroutine(ChangeDirection());
    }
    private void OnDestroy()
    {
        if (_accessArray.isCreated)
        { 
         _accessArray.Dispose();
        _colorsA.Dispose();
        _Output.Dispose();
        _angle.Dispose();
        }
    }

    public NativeArray<Color> Shuffle(NativeArray<Color> col)
    {
        for (int i = 0; i < col.Length; i++)
        {
            Color temp;
            int rnd = Random.Range(0, col.Length);
            temp = col[rnd];
            col[rnd] = col[i];
            col[i] = temp;
        }
        return col;
    }

  


    private void Update()
    {
        MyJobParTransform myJobParTransform = new MyJobParTransform()
        {
            direction = _direction,
            deltaTime = Time.deltaTime,
            random = Random.Range(-1f, 1f),
            A = _colorsA ,
            Output = _Output,
            angles=_angle
        };

        JobHandle jobHandle = myJobParTransform.Schedule(_accessArray);
        jobHandle.Complete();


        for (int i = 0; i < _count; i++)
        {
             _Cubesu[i].color = _Output[i];
  
        }

    }

    Transform[] SpawnObj(GameObject prefab, int count, float spawnRadius)
    {
        Transform[] objects = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            objects[i] = Instantiate(prefab).transform;
            _Cubesu[i] = objects[i].gameObject.GetComponent<MeshRenderer>().material;
            objects[i].position = Random.insideUnitSphere * spawnRadius;


        }
        return objects;
    }

 
}
[BurstCompile]
public struct shuffle: IJob
{
    public NativeArray<Color> Colors;
    public uint seed;
    public void Execute()
    {
        Unity.Mathematics.Random random=new Unity.Mathematics.Random(seed);
        for (int i = 0; i < Colors.Length; i++)
        {
            Color temp;
            int rnd =random.NextInt(0, Colors.Length);
            temp = Colors[rnd];
            Colors[rnd] = Colors[i];
            Colors[i] = temp;
        }
      
    }

}

[BurstCompile]
public struct MyJobParTransform : IJobParallelForTransform
{
    public Vector3 direction;
    public float deltaTime;
    public float random;
    public NativeArray<Color> A;
    public NativeArray<Color> Output;
    public NativeArray<int> angles;

    public void Execute(int index, TransformAccess transform)
    {
        transform.position += direction * deltaTime;
        transform.localRotation = Quaternion.AngleAxis(angles[index], Vector3.up);
        angles[index] = angles[index] == 180 ? 0 : angles[index]+1;
        Output[index] = Color.Lerp(Output[index],A[index], deltaTime);
      
    }

}