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

    // ����-Ŭ���̾�Ʈ string���� data �ְ������ �����ϱ� ���� ���ڿ�
    private const string separatorString = "E|";

    // Rank�� Timer ����, 5�и��� �ǽð� ����(DB������ �ҷ��ͼ�) �� �� Ŭ���̾�Ʈ���� �������(UDP) todo
    private float rankTime = 300f;

    // Debug��
    private float debugTime = 5f;
    private float debugTimer;

    // Test�� bool
    private bool testBool = true;

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


        if (testBool)
        {
            Debug.Log("[Server] TestCreatDBTest");
            testBool = false;
            Debug.Log($"[Server] TestCreatDBTest, testBool value : {testBool}");
            Invoke("CreateDBTest", 5f);
        }


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

            //DayTimer();

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

                List<string> dataList = new List<string>();

                while (true)
                {
                    // Ŭ���̾�Ʈ�κ��� ��û�޼���(�̸�)�� ����
                    //byte[] buffer = new byte[1024]; // �Ϲ������� �޴� ���ۻ����� 1024byte
                    byte[] buffer = new byte[327680];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // �޼��� ���������� ���
                    string receivedRequestData = Encoding.UTF8.GetString(buffer, 0, bytesRead); // ������ ��ȯ


                    if (receivedRequestData.Contains(separatorString)) // �ӽ÷� E| ����ؼ� ����, ĳ���� ��� ���� db�� �����Ҷ�
                    {
                        Debug.Log("[Server] recieved data from client");
                        Debug.Log($"[Server] recievedRequestData : {receivedRequestData}");
                        List<string> tempAllocate = new List<string>();
                        tempAllocate = receivedRequestData.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList(); // ���� data �����ؼ� list�� ����
                        if (tempAllocate.Count > 0)
                        {
                            Debug.Log("[Server] tempAllocate have at least one index ");
                            tempAllocate.ForEach(data => dataList.Add(data));
                                    
                        }
                    }
                    else
                    {
                        dataList = receivedRequestData.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList(); // ���� data �����ؼ� list�� ����
                    }

                    if (receivedRequestData.Contains("Finish"))
                    {
                        Debug.Log("[Server] received data contains Finish from client");
                        break;
                    }
                }

                // Ŭ���̾�Ʈ���� �� ��û�� ó���ؼ� Ŭ���̾�Ʈ���� ȸ��
                if (dataList.Count > 0) HandleRequestData(stream, dataList);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"[Server] Error in ReceiveRequestFromClient Method : {e.Message}");
        }

    }

    // Ŭ���̾�Ʈ�κ��� ���� ��û�� ���� �°� �з��ؼ� ó����
    private void HandleRequestData(NetworkStream stream, List<string> dataList)
    {
        Debug.Log($"[Server] Recieved request name from client : {dataList[0]}");

        // dataList -> 0 : requestName, 1~ : values
        string requestName = dataList[0];
        int clientLicenseNumber = 0;
        int clientCharactor = 0;
        if (dataList[0].Contains('|'))
        {
            Debug.Log($"[Server] dataList[0] have '|' so check dataList[0], {dataList[0]}");
            requestName = dataList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[0]; // �ӽ÷� ���, playerdata save
            Debug.Log($"[Server] Check requestName, dataList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[0] : {dataList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[0]}");
        }
        else if(dataList.ElementAtOrDefault(1) != null && dataList.ElementAtOrDefault(2) != null) // ����ó��, index 1,2�� ������ �Ѿ 
        {
            clientLicenseNumber = Int32.Parse(dataList[1]);
            clientCharactor = Int32.Parse(dataList[2]);
        }

        //// ����ó��, index 1,2�� ������ �Ѿ 
        //if (dataList.ElementAtOrDefault(1) != null && dataList.ElementAtOrDefault(2) != null) 
        //{
        //    clientLicenseNumber = Int32.Parse(dataList[1]);
        //    clientCharactor = Int32.Parse(dataList[2]);
        //}

        // Reply -> Client���� ���� List<string>, [0]�� requestName
        List<string> replyRequestData_List = new List<string>();
        replyRequestData_List.Add($"{requestName}|");

        // TempAllocate -> DB���� �޾ƿ��� List<string> �ӽ÷� ���
        List<string> tempAllocate = new List<string>();

        switch (requestName)
        {
            case "[Create]LicenseNumber":
                // Ŭ���̾�Ʈ�� LicenseNumber�� ��û�ϴ°� ó�� �����ϱ⶧���� LicenseNumber�� ���°�
                // ���� DB�� ������ LicenseNumber�� ��ִ��� Ȯ��(Count)�ϰ� Ŭ���̾�Ʈ���� LicenseNumber �ο�

                // �� ���̼��� �߱� (����(�÷��̾�), ���ÿ� ����Ǵ� �� ���� ���̼���)
                Debug.Log($"[Server] Creating... User_LicenseNumber");
                string clientdata = DBManager.instance.CreateLicenseNumber(); 
                clientLicenseNumber = Int32.Parse(clientdata.Split('|')[0]);

                // �� ĳ���� ���� ���� (���� �Ѹ�� ������ ù ĳ����)
                Debug.Log($"[Server] Creating... new Charactor Data");
                DBManager.instance.CreateNewCharactorData(clientLicenseNumber, 1); 

                Debug.Log($"[Server] Finish Create LicenseNumber and new CharactorData");
                replyRequestData_List.Add($"{clientdata}");
                break;
            case "[Create]Charactor":
                // to do fix
                DBManager.instance.CreateNewCharactorData(clientLicenseNumber, clientCharactor);
                Debug.Log($"[Server] Finish Create new CharactorData");
                break;
            case "[Save]CharactorName":
                DBManager.instance.SaveCharactorName(dataList);
                break;
            case "[Save]CharactorProfile":
                // dataList[1] = user_LicenseNumber, dataList[2] = user_Charactor, dataList[3] = profile
                Debug.Log($"[Server] Check Profile Data, dataList[3], Base64 : {dataList[3]}");
                DBManager.instance.SaveCharactorProfile(dataList);
                break;
            case "[Save]CharactorData":
                Debug.Log($"[Server] Check come in charactordata, dataList[0] : {dataList[0]}");
                for(int i = 0; i < dataList.Count; i++)
                {
                    Debug.Log($"[Server] Check charactor dataList{i}, : {dataList[i]}");
                }
                DBManager.instance.SaveCharactorData(dataList);
                break;
            case "[Save]GameResult":
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
            case "[Load]CharactorData":
                // dataList[1] = user_LicenseNumber, dataList[2] = user_Charactor
                tempAllocate = DBManager.instance.LoadCharactorData(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data)); // TempList�� �� ���(data = string value) ReplyList�� �߰�
                break;
            case "[Load]AnalyticsData":
                tempAllocate = DBManager.instance.LoadAnalyticsData(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Load]RankData":
                tempAllocate = DBManager.instance.RankOrderByUserData(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Test]CreateDB":
                DBManager.instance.CreateDateDB();
                break;
            default:
                Debug.Log($"[Server] Handling error that request from client, request name : {requestName}");
                break;
        }

        for (int i = 0; i < replyRequestData_List.Count; i++)
        {
            byte[] data = Encoding.UTF8.GetBytes(replyRequestData_List[i]); // string -> byte[] ������ ���� ��ȯ
            stream.Write(data, 0, data.Length);
            Debug.Log("[Server] Replying... request data to client");
        }

        byte[] finishData = Encoding.UTF8.GetBytes("Finish");
        stream.Write(finishData, 0, finishData.Length);
        Debug.Log("[Server] Finish reply request data to client");
    }

    // �Ϸ簡 ������ �� PresentDB�� �ִ� gamedata�� �� DB(ex) 24.02.21)�� ����
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

    private void CreateDBTest()
    {
        //DBManager.instance.CreateDaysExGameDataDB();
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