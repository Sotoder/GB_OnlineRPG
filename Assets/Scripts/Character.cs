using System;
using UnityEngine;
using UnityEngine.Networking;
[RequireComponent(typeof(CharacterController))]
public abstract class Character : NetworkBehaviour
{
    protected Action OnUpdateAction { get; set; }
    protected abstract RayShooter fireAction { get; set; }
    [SyncVar] protected Vector3 serverPosition;
    [SyncVar] protected Quaternion serverRotation;
    [SyncVar] protected int serverHealthPoints;
    [SyncVar] protected int CountPlayers;
    [SyncVar] public int money = 100;

    public NetworkConnection Connection;

    protected virtual void Initiate()
    {
        OnUpdateAction += Movement;
        OnUpdateAction += HealthChange;
    }

    private void Update()
    {
        OnUpdate();
    }

    private void OnUpdate()
    {
        OnUpdateAction?.Invoke();
    }

    [Command]
    protected void CmdUpdatePosition(Vector3 position,Quaternion rotation)
    {
        serverPosition = position;
        serverRotation = rotation;
    }

    [Command]
    protected void CmdSetHealthOnStart(int startHealth)
    {
        serverHealthPoints = startHealth;
    }

    [Command]
    public void CmdChekHit()
    {
        fireAction.ChekHit();
    }

    [Server]
    public void GetDamage(int damage)
    {
        serverHealthPoints -= damage;

        if (serverHealthPoints <= 0)
        {
            serverHealthPoints = 0;
            Connection.Disconnect();
        }
    }

    public abstract void Movement();
    public abstract void HealthChange();
}
