using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
public class UIController : NetworkBehaviour
{
    [SerializeField]
    TextMeshProUGUI _meshProUGUI;
    [SerializeField]
    TextMeshProUGUI _money;


    [SyncVar] private int playerConnections;
    [SyncVar] private int allMoney; 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        { 
            
            //playerConnections = NetworkServer.connections.Count;
            GameObject[] monayCount= GameObject.FindGameObjectsWithTag("Fors");
            allMoney = 0;
            playerConnections = monayCount.Length;
            foreach (GameObject monay in monayCount)
            {
                allMoney =allMoney+ monay.GetComponent<PlayerCharacter>().money;
            }
            
        }
        _money.text= allMoney.ToString();
       _meshProUGUI.text = playerConnections.ToString();
        
       // }
       
    }
}
