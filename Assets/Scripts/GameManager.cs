using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Monster monsterPrefab;
    public Monster monsterGameObject;
    
    private void OnApplicationQuit()
    {
        TcpProtobufClient.Instance.SendPlayerLogout(SuperManager.Instance.playerId);
    }
    
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

    void Update()
    {
        while (UnityMainThreadDispatcher.Instance.ExecutionQueue.Count > 0)
        {
            GameMessage msg = UnityMainThreadDispatcher.Instance.ExecutionQueue.Dequeue();
            if (msg.MessageCase == GameMessage.MessageOneofCase.Chat)
            {
                PlayerController.Instance.OnRecevieChatMsg(msg.Chat);
            }
            else if (msg.MessageCase == GameMessage.MessageOneofCase.PlayerPosition)
            {
                PlayerController.Instance.OnOtherPlayerPositionUpdate(msg.PlayerPosition);
            }
            else if (msg.MessageCase == GameMessage.MessageOneofCase.SpawnMyPlayer)
            {
                Vector3 spawnPos = new Vector3(msg.SpawnMyPlayer.X, msg.SpawnMyPlayer.Y, msg.SpawnMyPlayer.Z);
                
                PlayerController.Instance.OnSpawnMyPlayer(spawnPos);
            }
            else if (msg.MessageCase == GameMessage.MessageOneofCase.SpawnOtherPlayer)
            {
                Vector3 spawnPos = new Vector3(msg.SpawnOtherPlayer.X, msg.SpawnOtherPlayer.Y, msg.SpawnOtherPlayer.Z);
                
                Debug.Log(spawnPos);
                
                PlayerController.Instance.SpawnOtherPlayer(msg.SpawnOtherPlayer.PlayerId, spawnPos);
            }
            else if (msg.MessageCase == GameMessage.MessageOneofCase.Logout)
            {
                PlayerController.Instance.DestroyOtherPlayer(msg.Logout.PlayerId);
            }
            else if (msg.MessageCase == GameMessage.MessageOneofCase.PathTest)
            {
                foreach (var pathTestPath in msg.PathTest.Paths)
                {
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    go.transform.position = new Vector3(pathTestPath.X, pathTestPath.Z, pathTestPath.Y);
                    go.transform.localScale = new Vector3(30,30,30);
                }
            }
            else if (msg.MessageCase == GameMessage.MessageOneofCase.SpawnMonster)
            {
                GameObject go = GameObject.Instantiate(monsterPrefab.gameObject, Vector3.zero, Quaternion.identity);
                go.transform.position = new Vector3(msg.SpawnMonster.X, 0, msg.SpawnMonster.Z);
                monsterGameObject = go.GetComponent<Monster>();
            }
            else if (msg.MessageCase == GameMessage.MessageOneofCase.MoveMonster)
            {
                monsterGameObject.transform.position = new Vector3(msg.MoveMonster.X, 0, msg.MoveMonster.Z);
            }
        }
    }
}
