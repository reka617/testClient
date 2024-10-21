using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using Unity.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private Player myPlayer;
    private Dictionary<string, OtherPlayer> _otherPlayers = new();

    public Player MyPlayerTemplate;
    public OtherPlayer OtherPlayerTemplate;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        
    }

    void SpawnMyPlayer( Vector3 spawnPos)
    {
        GameObject SpawnPlayer = Instantiate(MyPlayerTemplate.gameObject, new Vector3(0, 1, 0), Quaternion.identity);
        myPlayer = SpawnPlayer.GetComponent<Player>();
    }

    public void SpawnOtherPlayer(string playerId, Vector3 spawnPos)
    {
        GameObject SpawnPlayer = Instantiate(OtherPlayerTemplate.gameObject, spawnPos, Quaternion.identity);

        OtherPlayer otherPlayer = SpawnPlayer.GetComponent<OtherPlayer>();
        otherPlayer.transform.position = spawnPos;
        //화면적용을 위한 임시코드
        var vector3 = otherPlayer.transform.position;
        vector3.y = 1;
        otherPlayer.transform.position = vector3;
        
        Camera otherPlayerCamera = SpawnPlayer.GetComponentInChildren<Camera>();
        if (otherPlayerCamera != null)
        {
            otherPlayerCamera.gameObject.SetActive(false);
        }
        
        _otherPlayers.Add(playerId, otherPlayer);
    }
    
    public void DestroyOtherPlayer(string playerId)
    {
        if (_otherPlayers.TryGetValue(playerId, out OtherPlayer otherPlayer))
        {
            Destroy(otherPlayer.gameObject);
            _otherPlayers.Remove(playerId);
        }
    }

    public void OnSpawnMyPlayer( Vector3 spawnPos)
    {
        SpawnMyPlayer(spawnPos);
    }

    public void OnRecevieChatMsg(ChatMessage chatmsg)
    {
        UIManager.Instance.OnRecevieChatMsg(chatmsg);
    }
    
    public void OnOtherPlayerPositionUpdate(PlayerPosition playerPosition)
    {
        if (_otherPlayers.TryGetValue(
             playerPosition.PlayerId, out OtherPlayer otherPlayer))
        {
            otherPlayer.UpdatePlayerPosition(playerPosition);
        }
    }
}
