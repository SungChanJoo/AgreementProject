using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Threading.Tasks;

public class Server : MonoBehaviour
{
    private const int port = 2421;
                      
    // Ŭ���̾�Ʈ ������ ���� �� �ֵ��� ����Ʈ
    private List<TcpClient> clients;
    private List<TcpClient> disconnectList;

    private TcpListener server;
    private bool isServerStarted;

    // Debug��
    private float debugTime = 5f;
    private float debugTimer;

    // ������ �ѹ��� ����
    private void Start()
    {
        Debug.Log("Server start callback function");
        ServerCreate();
    }

    private void Update()
    {
        if (!isServerStarted) return;
        //Debug.Log("Client�� ������ ������ ������ ���� ������");
        if (clients.Count == 0) return;

        CheckClientsState();
        ReceiveDataFromClients();
        
    }

    // ���� ����
    public void ServerCreate()
    {
        clients = new List<TcpClient>();
        disconnectList = new List<TcpClient>();

        try
        {
            Debug.Log("Try server create");
            server = new TcpListener(IPAddress.Any, port);
            server.Start(); // ���� ����
            Debug.Log("server.Start()");

            StartListening();
            isServerStarted = true;
        }
        catch(Exception e)
        {
            Debug.Log($"Server-Client Connect Error! : {e.Message}");
        }
    }

    // �񵿱�� Ŭ���̾�Ʈ �����û �ޱ� ����
    private async void StartListening()
    {
        Debug.Log("Server is listening until client is comming");
        // �񵿱� Ŭ���̾�Ʈ ���� / await Ű����� �񵿱� ȣ���� �Ϸ�� ������ ���
        TcpClient client = await server.AcceptTcpClientAsync();
        Debug.Log("Asynchronously server accept client's request");

        // Ŭ���̾�Ʈ�� ����Ǹ� ���� �޼��� ȣ��
        HandleClient(client);
    }

    // ��� �޼���, Ŭ���̾�Ʈ ����Ǹ� �ٽ� �����û ����
    private void HandleClient(TcpClient client)
    {
        Debug.Log("Client connected");
        clients.Add(client);

        for (int i = 0; i < clients.Count; i++)
        {
            Debug.Log($"Connected client's IP : {((IPEndPoint)clients[i].Client.RemoteEndPoint).Address}");
        }

        // ����� Ŭ���̾�Ʈ -> �����͹����غ�
        ReceiveDataFromClient(client);

        StartListening(); // Ŭ���̾�Ʈ �ް� �ٽ� ����    
    }

    private void CheckClientsState()
    {
        debugTimer += Time.deltaTime;

        while(debugTimer > debugTime)
        {
            Debug.Log($"Present Connected Clients : {clients.Count}");
            
            foreach(TcpClient client in clients)
            {
                Debug.Log($"Connected Clients's IP : {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
            }

            debugTimer = 0f;
        }

        //foreach (TcpClient client in clients)
        //{
        //    // Ŭ���̾�Ʈ ������ �������ٸ�
        //    if (!CheckConnectState(client))
        //    {
        //        Debug.Log($"Disconnected client's IP {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
        //        client.Close();
        //        disconnectList.Add(client);
        //        continue;
        //    }
        //    // Ŭ���̾�Ʈ�κ��� üũ �޽��� �ޱ�
        //    else
        //    {

        //        //NetworkStream stream = client.tcp.GetStream();
        //        //if(stream.DataAvailable)
        //        //{
        //        //    string data = new StreamReader(stream, true).ReadLine();

        //        //    if(data != null)
        //        //    {
        //        //        On
        //        //    }
        //        //}
        //    }
        //}
    }

    // Ŭ���̾�Ʈ�� ������ ������� üũ
    bool CheckConnectState(TcpClient client)
    {
        try
        {
            // 
            if (client.Client.Poll(0, SelectMode.SelectRead))
            {
                return !(client.Client.Receive(new byte[1], SocketFlags.Peek) == 0); // 1byte�� �����µ�, ���������� ��������

            }
            else return false;
        }
        catch
        {
            return false;
        }
    }

    private async void ReceiveDataFromClient(TcpClient client)
    {
        while(true)
        {
            try
            {
                NetworkStream stream = client.GetStream();

                // Ŭ���̾�Ʈ�κ��� �޽��� ����
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // �޼��� ���������� ���
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead); // ������ ��ȯ
                Debug.Log($"Received Message from client : {receivedMessage}");

                // Ŭ���̾�Ʈ���� �޽��� ���� // todo. �޼����ȿ� Ŭ���̾�Ʈ���� � ��û�� ���� ����� ���������
                string sendMessage = "Get a message from client And I Give you respond Message";
                byte[] data = Encoding.UTF8.GetBytes(sendMessage); // ������ ��ȯ
                stream.Write(data, 0, data.Length); // �޼��� ����
                Debug.Log($"Sent Message to client");
            }
            catch (Exception e)
            {
                Debug.Log($"Data Communicate error : {e.Message}");
                break;
            }
        }
    }

    private void SendMessageToClient(TcpClient client)
    {

    }

    private void ReceiveDataFromClients()
    {

    }

    // ���� ����
    private void OnApplicationQuit()
    {
        server.Stop();
    }


    #region Server �Է� Test
    //// ��Ʈ ���� ĭ (������ IP �ʿ����)
    //public InputField portInput;

    //// Ŭ���̾�Ʈ ������ �޾ƾ��ϴϱ�
    //List<ServerClient> clients;
    //List<ServerClient> disconnectList;

    //TcpListener server;
    //bool serverStarted;

    //// ���� �׽�Ʈ�� �α�
    //public Text testText;

    //private void Start()
    //{
    //    Debug.Log("Server start callback function");
    //    ServerCreate();
    //}

    //// ���� ���� (��ưŬ��)
    //public void ServerCreate()
    //{
    //    clients = new List<ServerClient>();
    //    disconnectList = new List<ServerClient>();

    //    try
    //    {
    //        Debug.Log("Try server created ");
    //        int port = portInput.text == "" ? 2421 : int.Parse(portInput.text);
    //        server = new TcpListener(IPAddress.Any, port); // �ƹ� IP, �ش� port��
    //        server.Start();

    //        StartListening();
    //        serverStarted = true;
    //        testText.text += testText.text + $"\n ������ {port}���� ���۵Ǿ����ϴ�.";

    //    }
    //    catch(Exception e)
    //    {
    //        testText.text += testText.text + $"\n ���� ���� ���� : {e.Message}";
    //    }
    //}

    //private void Update()
    //{
    //    if (!serverStarted) return;

    //    foreach(ServerClient client in clients)
    //    {
    //        // Ŭ���̾�Ʈ ������ �������ٸ�
    //        if(!IsConnected(client.tcp))
    //        {
    //            client.tcp.Close();
    //            disconnectList.Add(client);
    //            continue;
    //        }
    //        // Ŭ���̾�Ʈ�κ��� üũ �޽��� �ޱ�
    //        else
    //        {
    //            StreamWriter writer = new StreamWriter(client.tcp.GetStream());
    //            writer.WriteLine("Ŭ���̾�Ʈ �����?");
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

    //// ��û�ޱ� ����
    //private void StartListening()
    //{
    //    Debug.Log("Server is listening");
    //    server.BeginAcceptTcpClient(AcceptTcpClient, server); // �񵿱� �ݹ�ҷ�����
    //}

    //// �ݹ�ҷ������� ����
    //private void AcceptTcpClient(IAsyncResult result)
    //{
    //    Debug.Log("Client connected");
    //    TcpListener listener = (TcpListener)result.AsyncState;
    //    clients.Add(new ServerClient(listener.EndAcceptTcpClient(result)));

    //    for(int i = 0; i < clients.Count; i++)
    //    {
    //        Debug.Log($"Connected client's IP : {clients[i].tcp.Client.RemoteEndPoint.AddressFamily}");
    //    }

    //    StartListening(); // Ŭ���̾�Ʈ �ް� �ٽ� ����
    //}
    #endregion

    #region Chatgpt
    //// Ŭ���̾�Ʈ�κ��� ���� ��û ��ٸ�(������ ����)
    //private TcpListener server; 
    //// ����, Ŭ���̾�Ʈ ���ʿ��� ��� 
    //// ���������� Ŭ���̾�Ʈ�� ��û�� �����ϸ� TcpClient�� ��ȯ(��ſ� ��밡����)
    //private TcpClient client;
    //private NetworkStream stream; // ������ �ְ�ޱ�

    //private void Start()
    //{
    //    StartServer();
    //}

    //private void StartServer()
    //{
    //    server = new TcpListener(IPAddress.Any, 8888);
    //    server.Start();

    //    Debug.Log("Server is listening");

    //    // Async, �񵿱� / �񵿱�� ��⸦ �����ϰ� �� ������ �ٷιٷ� ���� �� �ֵ��� �񵿱⸦ ��
    //    // ����� ������ ������ ����Ǵ� ���� ������ ���� �ʾ� ���߰� ��
    //    // �ݹ��� �߸� -> HandleClientConnect�� �Ѿ 
    //    server.BeginAcceptTcpClient(HandleClientConnect, null);
    //}

    //// �񵿱��ϴ°ǵ�, ������ ��� �������� ���� �ʿ���
    //private void HandleClientConnect(IAsyncResult result)
    //{
    //    client = server.EndAcceptTcpClient(result);
    //    stream = client.GetStream();

    //    Debug.Log("Client connected!"); // if�� ����ó���ؾ�����

    //    // ������ �Ǿ��ٸ� 
    //    // Ŭ���̾�Ʈ���� ��Ŷ�� �����ϴ� ����
    //    Packet packetToSend = new Packet("Player1", 100);
    //    SendPacket(packetToSend);

    //    // Ŭ���̾�Ʈ�κ��� ������ �ޱ� ����
    //    // ������ ũ��� �˾Ƽ� ���� �� ������ �Ϲ������� 1024 ũ��� �ۼ�����
    //    byte[] receiveBuffer = new byte[1024];
    //    stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, HandleDataReceived, null);
    //}

    //private void HandleDataReceived(IAsyncResult result)
    //{
    //    int bytesRead = stream.EndRead(result); // �о���� �������� ũ��?

    //    if(bytesRead > 0)
    //    {
    //        byte[] receivedData = new byte[bytesRead];
    //        Array.Copy((result.AsyncState as byte[]), receivedData, bytesRead); // ī���ϴ� ����?

    //        // Deserialized : ��������(���� ������ ������)���� ��ü�� ��ȯ�ϴ� ������ ������ȭ
    //        // ����, ���� ��Ŷ�� ������ȭ
    //        Packet receivedPacket = PacketFromBytes(receivedData);
    //        Debug.Log("Received data from client : " + receivedPacket.playerName + " - " + receivedPacket.playerScore);

    //        // �ʿ��Ѵ�� ���� �����͸� �ٷ��(�����ض�)

    //        // �����͸� �� �ޱ� ���� ������ ����ϱ�
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

// �������� �ٷ�� Ŭ���̾�Ʈ
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