using UnityEngine;
using System;
using System.Net.Sockets;
using System.Threading;
using Google.Protobuf;
using Game;

public class TcpProtobufClient : MonoBehaviour
{
    public static TcpProtobufClient Instance { get; private set; }
    
    private TcpClient tcpClient;
    private NetworkStream stream;
    private bool isRunning = false;

    private const string SERVER_IP = "127.0.0.1";
    private const int SERVER_PORT = 9090;

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
        ConnectToServer();
    }

    void ConnectToServer()
    {
        try
        {
            tcpClient = new TcpClient(SERVER_IP, SERVER_PORT);
            stream = tcpClient.GetStream();
            isRunning = true;
            StartReceiving();

            Debug.Log("Connected to server.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error connecting to server: {e.Message}");
        }
    }

    void StartReceiving()
    {
        byte[] lengthBuffer = new byte[4];
        stream.BeginRead(lengthBuffer, 0, 4, OnLengthReceived, lengthBuffer);
    }
    
    void OnLengthReceived(IAsyncResult ar)
    {
        try
        {
            int bytesRead = stream.EndRead(ar);
            if (bytesRead == 0) return; // 연결 종료

            byte[] lengthBuffer = (byte[])ar.AsyncState;
            int length = BitConverter.ToInt32(lengthBuffer, 0);

            byte[] messageBuffer = new byte[length];
            stream.BeginRead(messageBuffer, 0, length, OnMessageReceived, messageBuffer);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error receiving message length: {e.Message}");
        }
    }

    void OnMessageReceived(IAsyncResult ar)
    {
        try
        {
            int bytesRead = stream.EndRead(ar);
            if (bytesRead == 0) return; // 연결 종료

            byte[] messageBuffer = (byte[])ar.AsyncState;
            GameMessage gameMessage = GameMessage.Parser.ParseFrom(messageBuffer);
            UnityMainThreadDispatcher.Instance.Enqueue(gameMessage);

            StartReceiving(); // 다음 메시지 수신 대기
        }
        catch (Exception e)
        {
            Debug.LogError($"Error receiving message: {e.Message}");
        }
    }
    
    public void SendPlayerPosition(string playerId, float x, float y, float z, float vx, float vy, float vz, float speed, float rotationY)
    {
        var position = new PlayerPosition
        {
            PlayerId = playerId,
            X = x,
            Y = y,
            Z = z,
            Fx = vx,
            Fy = vy,
            Fz = vz,
            Speed = speed,
            RotationY = rotationY
        };
        var message = new GameMessage
        {
            PlayerPosition = position
        };
        SendMessage(message);
    }
    
    public void SendPlayerLogout(string playerId)
    {
        var msg = new LogoutMessage()
        {
            PlayerId = playerId,
        };
        var message = new GameMessage
        {
            Logout = msg
        };
        SendMessage(message);
    }

    public void SendChatMessage(string sender, string content)
    {
        var chat = new ChatMessage
        {
            Sender = sender,
            Content = content
        };
        var message = new GameMessage
        {
            Chat = chat
        };
        SendMessage(message);
    }
    
    public void SendLoginMessage(string playerId)
    {
        var login = new LoginMessage()
        {
            PlayerId = playerId
        };
        var message = new GameMessage
        {
            Login = login
        };
        SendMessage(message);
    }

    private void SendMessage(GameMessage message)
    {
        if (tcpClient != null && tcpClient.Connected)
        {
            byte[] messageBytes = message.ToByteArray();
            byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

            // 메시지 길이를 먼저 보냅니다
            stream.Write(lengthBytes, 0, 4);
            // 메시지 본문을 보냅니다
            stream.Write(messageBytes, 0, messageBytes.Length);
        }
    }

    void OnDisable()
    {
        isRunning = false;
        if (stream != null) stream.Close();
        if (tcpClient != null) tcpClient.Close();
    }
}