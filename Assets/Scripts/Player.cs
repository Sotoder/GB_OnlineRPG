using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class Player : NetworkBehaviour
{
    
    [SerializeField]
    private GameObject playerPrefab;
    private GameObject playerCharacter;

    public PlayerCharacter PlayerComponent;
    
    private void Start()
    {
        SpawnCharacter();
    }
    private void SpawnCharacter()
    {
        if (!isServer)
        {
            return;
        }
        
        playerCharacter = Instantiate(playerPrefab,transform);
        NetworkServer.SpawnWithClientAuthority(playerCharacter,
        connectionToClient);

        PlayerComponent = playerCharacter.GetComponent<PlayerCharacter>();
        PlayerComponent.Connection = connectionToClient;
    }
}
