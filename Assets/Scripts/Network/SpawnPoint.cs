using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable 618
public class SpawnPoint : NetworkStartPosition
#pragma warning restore 618
{
    [SerializeField] private Transform lookTarget;
    [SerializeField] private GameObject _mark;
    private void OnEnable()
    {
        transform.rotation = Quaternion.LookRotation(lookTarget.position - transform.position);
    }

    public void HideMark()
    {
        _mark.SetActive(false);
    }

    public void ShowMark()
    {
        _mark.SetActive(true);
    }
}
