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
using System.Linq;

public class Server : MonoBehaviour
{
    private const int port = 2421;
                      
    // Ŭ���̾�Ʈ ������ ���� �� �ֵ��� ����Ʈ
    private List<TcpClient> clients;
    private List<TcpClient> disconnectList;

    private TcpListener server;
    private bool isServerStarted;

    // Ŭ���̾�Ʈ���� ���̼�����ȣ�� �ο��ϱ����� ����
    private int clientLicenseNumber;

    // Rank�� Timer ����, 5�и��� �ǽð� ����(DB������ �ҷ��ͼ�) �� �� Ŭ���̾�Ʈ���� �������(UDP) todo
    private float rankTime = 300f;

    // Debug��
    private float debugTime = 5f;
    private float debugTimer;

    // ������ �ѹ��� ����
    private void Start()
    {
        Debug.Log("[Server] Server start callback function");
        ServerCreate();
    }

    private void Update()
    {
        if (!isServerStarted) return;
        //Debug.Log("Client�� ������ ������ ������ ���� ������");
        //DayTimer();
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
            Debug.Log("[Server] Try server create");
            server = new TcpListener(IPAddress.Any, port);
            server.Start(); // ���� ����
            Debug.Log("[Server] server.Start()");

            StartListening();
            isServerStarted = true;
        }
        catch(Exception e)
        {
            Debug.Log($"[Server] Server-Client Connect Error! : {e.Message}");
        }
    }

    // �񵿱�� Ŭ���̾�Ʈ �����û �ޱ� ����
    private async void StartListening()
    {
        Debug.Log("[Server] Server is listening until client is comming");
        // �񵿱� Ŭ���̾�Ʈ ���� / await Ű����� �񵿱� ȣ���� �Ϸ�� ������ ���
        TcpClient client = await server.AcceptTcpClientAsync();
        Debug.Log("[Server] Asynchronously server accept client's request");

        // Ŭ���̾�Ʈ�� ����Ǹ� ���� �޼��� ȣ��
        HandleClient(client);
    }

    // ��� �޼���, Ŭ���̾�Ʈ ����Ǹ� �ٽ� �����û ����
    private void HandleClient(TcpClient client)
    {
        Debug.Log("[Server] Client connected");
        clients.Add(client);

        for (int i = 0; i < clients.Count; i++)
        {
            Debug.Log($"[Server] Connected client's IP : {((IPEndPoint)clients[i].Client.RemoteEndPoint).Address}");
        }

        // ����� Ŭ���̾�Ʈ -> �����͹����غ�
        //ReceiveDataFromClient(client);
        ReceiveRequestFromClient(client);

        StartListening(); // Ŭ���̾�Ʈ �ް� �ٽ� ����    
    }

    private void CheckClientsState()
    {
        debugTimer += Time.deltaTime;

        while(debugTimer > debugTime)
        {
            Debug.Log($"[Server] Present Connected Clients : {clients.Count}");
            
            foreach(TcpClient client in clients)
            {
                Debug.Log($"[Server] Connected Clients's IP : {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
            }

            DayTimer();

            debugTimer = 0f;
        }

        foreach (TcpClient client in clients)
        {
            if (!IsConnected(client))
            {
                Debug.Log("[Server] Client connection is closed");
                Debug.Log($"[Server] Disconnected clients's IP : {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
                client.Close();
                disconnectList.Add(client);
                Debug.Log($"[Server] After client.Close(), Test check client.Connected bool value : {client.Connected}");
                clients.Remove(client);
                continue;
            }
            #region etc
            //// Ŭ���̾�Ʈ ������ �������ٸ�
            //if (!CheckConnectState(client))
            //{
            //    Debug.Log($"Disconnected client's IP {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
            //    client.Close();
            //    disconnectList.Add(client);
            //    continue;
            //}
            // Ŭ���̾�Ʈ�κ��� üũ �޽��� �ޱ�
            //else
            //{

            //    //NetworkStream stream = client.tcp.GetStream();
            //    //if(stream.DataAvailable)
            //    //{
            //    //    string data = new StreamReader(stream, true).ReadLine();

            //    //    if(data != null)
            //    //    {
            //    //        On
            //    //    }
            //    //}
            //}
            #endregion
        }
    }

    private async void ReceiveRequestFromClient(TcpClient client)
    {
        try
        {
            NetworkStream stream = client.GetStream();

            while (true)
            {
                // ������ �������� break
                if (!IsConnected(client))
                {
                    Debug.Log("[Server] Client connection is closed");
                    break;
                }

                // Ŭ���̾�Ʈ�κ��� ��û�޼���(�̸�)�� ����
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // �޼��� ���������� ���
                string receivedRequestData = Encoding.UTF8.GetString(buffer, 0, bytesRead); // ������ ��ȯ
                List<string> dataList = receivedRequestData.Split('|').ToList(); // ���� data �����ؼ� list�� ����
                string receivedRequestName = dataList[0]; // dataList[0]�� Ŭ���̾�Ʈ�� ��û�� ����, ���� ���� �̸�
                Debug.Log($"[Server] Received request name from client : {receivedRequestName}");

                // Ŭ���̾�Ʈ�κ��� ��û ���� ���� ���� ������ ó���ؼ� Ŭ���̾�Ʈ���� ���������
                if(receivedRequestName == "[Load]PlayerData")
                {
                    HandleRequestDataForLoad(stream, dataList);
                }
                else
                {
                    HandleRequestData(stream, dataList);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log($"[Server] Data Communicate error : {e.Message}");
        }

    }

    // Ŭ���̾�Ʈ�κ��� ���� ��û�� ���� �°� �з��ؼ� ó����
    private void HandleRequestData(NetworkStream stream, List<string> dataList)
    {
        // dataList -> 0 : requestName, 1~ : values
        string requestName = dataList[0];
        string replyRequestMessage = "";
        Debug.Log($"[Server] HandleRequestData requestName : {dataList[0]}");
        

        switch (requestName)
        {
            case "[Create]LicenseNumber":
                // Ŭ���̾�Ʈ�� LicenseNumber�� ��û�ϴ°� ó�� �����ϱ⶧���� LicenseNumber�� ���°�
                // ���� DB�� ������ LicenseNumber�� ��ִ��� Ȯ��(Count)�ϰ� Ŭ���̾�Ʈ���� LicenseNumber �ο�
                Debug.Log($"[Server] Creating... User_LicenseNumber");
                string clientdata = DBManager.instance.CreateLicenseNumber(); // �� ���̼��� �߱�
                List<string> clientdata_List = clientdata.Split('|').ToList();
                clientLicenseNumber = Int32.Parse(clientdata_List[0]);
                Debug.Log($"[Server] Creating... new PlayerData");
                DBManager.instance.CreateNewPlayerData(clientLicenseNumber); // �� �÷��̾� ���� ����
                Debug.Log($"[Server] Finish Create LicenseNumber and new PlayerData");
                replyRequestMessage =  "[Create]LicenseNumber|" + clientdata;
                break;
            case "[Create]Charactor":
                break;
            case "[Save]venezia_kor":
                DBManager.instance.SaveGameResultData(dataList);
                break;
            case "[Save]venezia_eng":
                DBManager.instance.SaveGameResultData(dataList);
                break;
            case "[Save]venezia_chn":
                DBManager.instance.SaveGameResultData(dataList);
                break;
            case "[Save]gugudan":
                DBManager.instance.SaveGameResultData(dataList);
                break;
            case "[Save]calculation":
                DBManager.instance.SaveGameResultData(dataList);
                break;
            default:
                Debug.Log($"[Server] Handling error that request from client, request name : {requestName}");
                break;
        }

        byte[] data = Encoding.UTF8.GetBytes(replyRequestMessage); // ������ ��ȯ
        stream.Write(data, 0, data.Length); // �޼��� ����
        Debug.Log($"[Server] Reply request message to client");

    }

    private void HandleRequestDataForLoad(NetworkStream stream, List<string> dataList)
    {
        Debug.Log($"[Server] receivedRequestName : {dataList[0]}");

        int newClientLicenseNumber = Int32.Parse(dataList[1]);
        List<string> replyMassiveRequestMessage = DBManager.instance.LoadPlayerData(ClientLoginStatus.New, newClientLicenseNumber); // �� �÷��̾� ���� �ҷ�����

        for (int i = 0; i < replyMassiveRequestMessage.Count; i++)
        {
            byte[] data = Encoding.UTF8.GetBytes(replyMassiveRequestMessage[i]);
            stream.Write(data, 0, data.Length);
            Debug.Log("[Server] Replying... massive request message to client");
        }

        byte[] finishData = Encoding.UTF8.GetBytes("Finish");
        stream.Write(finishData, 0, finishData.Length);
        Debug.Log("[Server] End reply massive request message to client");
    }

    // �Ϸ簡 ������ �� PresentDB�� �ִ� gamedata�� �� DB(ex) 24-02-21)�� ����
    private void DayTimer()
    {
        // ���� �ð�
        DateTime currentTime = DateTime.Now;
        // �ʱ�ȭ�� �Ǵ� �ð� (23�� 59�� 55��)
        DateTime criterionTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 11, 50, 55);

        // TimeSpan, ����ð��� �ʱ�ȭ�ð� ����
        TimeSpan timeDiff = criterionTime - currentTime;

        // uint ����� ���
        uint diffHours = (uint)(timeDiff.Hours >= 0? timeDiff.Hours : 0);
        uint diffMinutes = (uint)(timeDiff.Minutes >= 0? timeDiff.Minutes : 0);
        uint diffSeconds = (uint)(timeDiff.Seconds >= 0? timeDiff.Seconds : 0);

        // ���� �ð��� �ʱ�ȭ �Ǵ� �ð����� ũ�ٸ�, timeDiff�� ������ �ƴϰ�, 0�� �ƴϸ�
        if (diffHours == 0 && diffMinutes == 0 && diffSeconds > 0 && diffSeconds <= 5)
        {
            // �� DB ���� �� ���� / presentDB gamedata�ʱ�ȭ
            Debug.Log("[Server] A day has passed. Create new DB for save data and Reset presentDB some datas");

        }

        Debug.Log($"[Server] currenTime = {currentTime}");
        Debug.Log($"[Server] criterionTime = {criterionTime}");
        Debug.Log($"[Server] timeDiff = {timeDiff}");

    }

    // �������� ������ �� PresentDB�� �ִ� rankTable data�� �� DB(�ְ� ��ŷ ��¿�)? or � DB�� ����
    private void WeekTimer()
    {

    }

    private void SendMessageToClients(TcpClient client)
    {

    }

    private void ReceiveDataFromClients()
    {

    }

    private bool IsConnected(TcpClient client)
    {
        // �ؼ� �ʿ�
        bool isConnCheck = !((client.Client.Poll(1000, SelectMode.SelectRead) && (client.Client.Available == 0)) || !client.Client.Connected);
        //bool isConnCheck = client.Client.Poll(1000, SelectMode.SelectRead) && (client.Client.Available == 0) && client.Client.Connected;

        return isConnCheck;
    }

    // ���� ����
    private void OnApplicationQuit()
    {
        server.Stop();
    }


    
}