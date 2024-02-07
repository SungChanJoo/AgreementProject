using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class Server : MonoBehaviour
{
    private const int port = 2421;

    // 클라이언트 여러명 받을 수 있도록 리스트
    private List<TcpClient> clients;
    private List<TcpClient> disconnectList;

    private TcpListener server;
    private bool isServerStarted;

    // Debug용
    private float debugTime = 5f;
    private float debugTimer;

    // 서버는 한번만 실행
    private void Start()
    {
        Debug.Log("Server start callback function");
        ServerCreate();
    }

    private void Update()
    {
        if (!isServerStarted) return;
        //Debug.Log("Client가 들어오지 않으면 실행이 되지 않을것");

        CheckClientsState();
        Receive();
        
    }

    // 서버 생성
    public void ServerCreate()
    {
        clients = new List<TcpClient>();
        disconnectList = new List<TcpClient>();

        try
        {
            Debug.Log("Try server create");
            server = new TcpListener(IPAddress.Any, port);
            server.Start(); // 동기적으로 클라이언트 연결 -> 클라이언트이 안들어오면 다음 코드라인으로 진행이 안됨
            Debug.Log("server.Start() 실행");

            StartListening();
            isServerStarted = true;
        }
        catch(Exception e)
        {
            Debug.Log($"Server-Client Connect Error! : {e.Message}");
        }
    }

    // 비동기로 클라이언트 연결요청 받기 시작
    private async void StartListening()
    {
        Debug.Log("Server is Listening");
        // 비동기 클라이언트 연결 / await 키워드는 비동기 호출이 완료될 때까지 대기
        TcpClient client = await server.AcceptTcpClientAsync();
        // 클라이언트가 연결되면 콜백 호출
        AcceptTcpClient(client);
    }

    // 콜백불러와지면 실행
    private void AcceptTcpClient(TcpClient client)
    {
        Debug.Log("Client connected");
        clients.Add(client);

        for (int i = 0; i < clients.Count; i++)
        {
            Debug.Log($"Connected client's IP : {((IPEndPoint)clients[i].Client.RemoteEndPoint).Address}");
        }

        StartListening(); // 클라이언트 받고 다시 실행    
    }

    bool IsConnected(TcpClient client)
    {
        try
        {
            if (client != null && client.Client != null && client.Client.Connected)
            {
                if (client.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(client.Client.Receive(new byte[1], SocketFlags.Peek) == 0); // 1byte씩 보내는데, 반응없으면 끊어진거

                }
                return true;
            }
            else return false;
        }
        catch
        {
            return false;
        }
    }

    private void CheckClientsState()
    {
        debugTimer += Time.deltaTime;

        while(debugTimer > debugTime)
        {
            Debug.Log($"현재 연결중인 Clients : {clients.Count}");
            
            foreach(TcpClient client in clients)
            {
                Debug.Log($"연결중인 Clients's IP : {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
            }

            debugTimer = 0f;
        }

        foreach (TcpClient client in clients)
        {
            // 클라이언트 연결이 끊어졌다면
            if (!IsConnected(client))
            {
                Debug.Log($"Disconnected client's IP {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
                client.Close();
                disconnectList.Add(client);
                continue;
            }
            // 클라이언트로부터 체크 메시지 받기
            else
            {

                //NetworkStream stream = client.tcp.GetStream();
                //if(stream.DataAvailable)
                //{
                //    string data = new StreamReader(stream, true).ReadLine();

                //    if(data != null)
                //    {
                //        On
                //    }
                //}
            }
        }
    }

    private void Receive()
    {

    }

    // 서버 종료
    private void OnApplicationQuit()
    {
        server.Stop();
    }


    #region Server 입력 Test
    //// 포트 적는 칸 (서버는 IP 필요없음)
    //public InputField portInput;

    //// 클라이언트 여러명 받아야하니까
    //List<ServerClient> clients;
    //List<ServerClient> disconnectList;

    //TcpListener server;
    //bool serverStarted;

    //// 연결 테스트용 로그
    //public Text testText;

    //private void Start()
    //{
    //    Debug.Log("Server start callback function");
    //    ServerCreate();
    //}

    //// 서버 생성 (버튼클릭)
    //public void ServerCreate()
    //{
    //    clients = new List<ServerClient>();
    //    disconnectList = new List<ServerClient>();

    //    try
    //    {
    //        Debug.Log("Try server created ");
    //        int port = portInput.text == "" ? 2421 : int.Parse(portInput.text);
    //        server = new TcpListener(IPAddress.Any, port); // 아무 IP, 해당 port로
    //        server.Start();

    //        StartListening();
    //        serverStarted = true;
    //        testText.text += testText.text + $"\n 서버가 {port}에서 시작되었습니다.";

    //    }
    //    catch(Exception e)
    //    {
    //        testText.text += testText.text + $"\n 서버 연결 에러 : {e.Message}";
    //    }
    //}

    //private void Update()
    //{
    //    if (!serverStarted) return;

    //    foreach(ServerClient client in clients)
    //    {
    //        // 클라이언트 연결이 끊어졌다면
    //        if(!IsConnected(client.tcp))
    //        {
    //            client.tcp.Close();
    //            disconnectList.Add(client);
    //            continue;
    //        }
    //        // 클라이언트로부터 체크 메시지 받기
    //        else
    //        {
    //            StreamWriter writer = new StreamWriter(client.tcp.GetStream());
    //            writer.WriteLine("클라이언트 연결됨?");
    //            writer.Flush();

    //            //NetworkStream stream = client.tcp.GetStream();
    //            //if(stream.DataAvailable)
    //            //{
    //            //    string data = new StreamReader(stream, true).ReadLine();

    //            //    if(data != null)
    //            //    {
    //            //        On
    //            //    }
    //            //}
    //        }
    //    }
    //}

    //bool IsConnected(TcpClient client)
    //{
    //    try
    //    {
    //        if(client != null && client.Client != null && client.Client.Connected)
    //        {
    //            if(client.Client.Poll(0, SelectMode.SelectRead))
    //            {
    //                return !(client.Client.Receive(new byte[1], SocketFlags.Peek) == 0);

    //            }
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }
    //    catch
    //    {
    //        return false;
    //    }
    //}

    //// 요청받기 시작
    //private void StartListening()
    //{
    //    Debug.Log("Server is listening");
    //    server.BeginAcceptTcpClient(AcceptTcpClient, server); // 비동기 콜백불러오기
    //}

    //// 콜백불러와지면 실행
    //private void AcceptTcpClient(IAsyncResult result)
    //{
    //    Debug.Log("Client connected");
    //    TcpListener listener = (TcpListener)result.AsyncState;
    //    clients.Add(new ServerClient(listener.EndAcceptTcpClient(result)));

    //    for(int i = 0; i < clients.Count; i++)
    //    {
    //        Debug.Log($"Connected client's IP : {clients[i].tcp.Client.RemoteEndPoint.AddressFamily}");
    //    }

    //    StartListening(); // 클라이언트 받고 다시 실행
    //}
    #endregion

    #region Chatgpt
    //// 클라이언트로부터 연결 요청 기다림(서버만 가짐)
    //private TcpListener server; 
    //// 서버, 클라이언트 양쪽에서 사용 
    //// 서버에서는 클라이언트의 요청을 수락하면 TcpClient를 반환(통신에 사용가능한)
    //private TcpClient client;
    //private NetworkStream stream; // 데이터 주고받기

    //private void Start()
    //{
    //    StartServer();
    //}

    //private void StartServer()
    //{
    //    server = new TcpListener(IPAddress.Any, 8888);
    //    server.Start();

    //    Debug.Log("Server is listening");

    //    // Async, 비동기 / 비동기로 듣기를 시작하고 또 다음꺼 바로바로 들을 수 있도록 비동기를 씀
    //    // 동기로 했으면 다음께 진행되는 동안 실행이 되지 않아 멈추게 됨
    //    // 콜백이 뜨면 -> HandleClientConnect로 넘어감 
    //    server.BeginAcceptTcpClient(HandleClientConnect, null);
    //}

    //// 비동기하는건데, 문법상 어떻게 가능한지 공부 필요함
    //private void HandleClientConnect(IAsyncResult result)
    //{
    //    client = server.EndAcceptTcpClient(result);
    //    stream = client.GetStream();

    //    Debug.Log("Client connected!"); // if문 예외처리해야하함

    //    // 연결이 되었다면 
    //    // 클라이언트한테 패킷을 전송하는 예시
    //    Packet packetToSend = new Packet("Player1", 100);
    //    SendPacket(packetToSend);

    //    // 클라이언트로부터 데이터 받기 시작
    //    // 데이터 크기는 알아서 정할 수 있지만 일반적으로 1024 크기로 송수신함
    //    byte[] receiveBuffer = new byte[1024];
    //    stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, HandleDataReceived, null);
    //}

    //private void HandleDataReceived(IAsyncResult result)
    //{
    //    int bytesRead = stream.EndRead(result); // 읽어들인 데이터의 크기?

    //    if(bytesRead > 0)
    //    {
    //        byte[] receivedData = new byte[bytesRead];
    //        Array.Copy((result.AsyncState as byte[]), receivedData, bytesRead); // 카피하는 이유?

    //        // Deserialized : 포맷형태(포맷 상태의 데이터)에서 객체로 변환하는 과정을 역직렬화
    //        // 예시, 받은 패킷을 역직렬화
    //        Packet receivedPacket = PacketFromBytes(receivedData);
    //        Debug.Log("Received data from client : " + receivedPacket.playerName + " - " + receivedPacket.playerScore);

    //        // 필요한대로 받은 데이터를 다뤄라(가공해라)

    //        // 데이터를 더 받기 위해 리스닝 계속하기
    //        byte[] newBuffer = new byte[1024];
    //        stream.BeginRead(newBuffer, 0, newBuffer.Length, HandleDataReceived, newBuffer);
    //    }
    //}

    //private void SendPacket(Packet packet)
    //{
    //    byte[] data = PacketToBytes(packet);
    //    stream.Write(data, 0, data.Length);
    //}

    //private byte[] PacketToBytes(Packet packet)
    //{
    //    string jsonData = JsonUtility.ToJson(packet);
    //    return Encoding.ASCII.GetBytes(jsonData);
    //}

    //private Packet PacketFromBytes(byte[] data)
    //{
    //    string jsonData = Encoding.ASCII.GetString(data);
    //    return JsonUtility.FromJson<Packet>(jsonData);
    //}
    #endregion
}

// 서버에서 다루는 클라이언트
public class ServerClient
{
    public TcpClient tcp;
    public string clientName;

    public ServerClient(TcpClient clientSocket)
    {
        clientName = "Guest";
        tcp = clientSocket;
    }

}