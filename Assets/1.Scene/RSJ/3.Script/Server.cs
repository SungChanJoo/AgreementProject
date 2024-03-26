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

    // ���� ����Ÿ�̸�(����) ����ϱ� ���� ����
    private DateTime nextMidnight;
    private WaitForSeconds waitUntilMidnight;
    // ���� �� �ָ� üũ�ϱ� ���� ī��Ʈ
    private DateTime standardDay;

    // Client Check Timer
    private float clientCheckTime = 3f;
    private float clientCheckTimer;


    // ��Ÿ ������ ó���� Handler
    private ETCMethodHandler etcMethodHandler = new ETCMethodHandler();

    // ������ �ѹ��� ����
    private void Start()
    {
        Debug.Log("[Server] Server start callback function");
        ServerCreate();
        Invoke("TimerSet", 5f);
    }

    private void Update()
    {
        if (!isServerStarted) return;
        if (clients.Count == 0) return;

        CheckClientsState();
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

    // Ŭ���̾�Ʈ�κ��� ��û����
    private async void ReceiveRequestFromClient(TcpClient client)
    {
        try
        {
            NetworkStream stream = client.GetStream();

            // ������ true�ε��� -> ������ �Ȳ��䵿�� 
            while (IsConnected(client))
            {
                //// ������ �������� break
                //if (!IsConnected(client))
                //{
                //    Debug.Log("[Server] Client connection is closed");
                //    break;
                //}

                List<string> receivedRequestData_List = new List<string>();

                while (true)
                {
                    // Ŭ���̾�Ʈ�κ��� ��û�޼���(�̸�)�� ����
                    //byte[] buffer = new byte[1024]; // �Ϲ������� �޴� ���ۻ����� 1024byte
                    byte[] buffer = new byte[327680];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // �޼��� ���������� ���
                    string receivedRequestData = Encoding.UTF8.GetString(buffer, 0, bytesRead); // ������ ��ȯ

                    receivedRequestData_List.Add(receivedRequestData);

                    // Ŭ���̾�Ʈ�κ��� ������ ������ ������(Finish) break;
                    List<string> endCheck = receivedRequestData.Split('|').ToList();
                    if (endCheck.Contains("Finish"))
                    {
                        etcMethodHandler.RemoveFinish(receivedRequestData_List, endCheck);

                        Debug.Log($"[Server] Finish Receive Data From Client");
                        break;
                    }
                }

                // Ŭ���̾�Ʈ���� �� ��û�� ó���ؼ� Ŭ���̾�Ʈ���� ȸ��
                if (receivedRequestData_List.Count > 0) HandleRequestData(stream, receivedRequestData_List);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"[Server] Error in ReceiveRequestFromClient Method : {e.Message}");

            // ����-Ŭ���̾�Ʈ ���̿� ������ �ִٸ�, �ش� Ŭ���̾�Ʈ �����ؾ���
            CheckClients(client);
        }

    }

    // Ŭ���̾�Ʈ�κ��� ���� ��û�� ���� �°� �з��ؼ� ó����
    private void HandleRequestData(NetworkStream stream, List<string> dataList)
    {
        Debug.Log($"[Server] Received request name from client : {dataList[0]}");

        // Ŭ���̾�Ʈ�κ��� �� ������ �ϳ��� string���� ����
        string oneData = etcMethodHandler.CreateOneDataToFilter(dataList);

        // E| ����
        List<string> filterList = oneData.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();

        Debug.Log($"[Server] HandleRequestData filterList.Count : {filterList.Count}");

        for(int i = 0; i <filterList.Count; i ++)
        {
            //Debug.Log($"[Server] Received Data From Client to filterList[{i}] : {filterList[i]}");
        }

        //string requestName = $"[Save]CharactorData";
        //requestData = $"{requestName}|{clientLicenseNumber}|{clientCharactor}|{separatorString}";

        // filterList[0] '|' ���� -> requestName, clientLicenseNumber, clientCharactor�� ��� ���ؼ�
        List<string> baseDataList = filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

        string requestName = baseDataList[0];
        int clientLicenseNumber = 0;
        int clientCharactor = 0;
        string createCharactorName = null;

        if(baseDataList.ElementAtOrDefault(1) != null && baseDataList.ElementAtOrDefault(2) != null) // �Ϲ������� clientLicenseNumber�� clientCharactor�� ���� ����
        {
            clientLicenseNumber = Int32.Parse(baseDataList[1]);
            clientCharactor = Int32.Parse(baseDataList[2]);
        }
        else if(baseDataList.ElementAtOrDefault(1) != null) // UserData, clientLicenseNumber�� ���� ��
        {
            clientLicenseNumber = Int32.Parse(baseDataList[1]);
        }
        
        if(baseDataList.ElementAtOrDefault(3) != null)
        {
            createCharactorName = baseDataList[3];
        }

        // Reply -> Client���� ���� List<string>, [0]�� requestName
        List<string> replyRequestData_List = new List<string>();
        replyRequestData_List.Add($"{requestName}|");

        // TempAllocate -> DB���� ��ȯ�ϴ� List<string> �ӽ÷� ���
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
                string newClientCharactor = DBManager.instance.CreateNewCharactorData(clientLicenseNumber, clientCharactor, createCharactorName);
                Debug.Log($"[Server] Finish Create new CharactorData");
                //replyRequestData_List.Add($"{newClientCharactor}|");
                break;
            case "[Save]CharactorName":
                DBManager.instance.SaveCharactorName(filterList);
                break;
            case "[Save]CharactorProfile":
                DBManager.instance.SaveCharactorProfile(filterList);
                break;
            case "[Save]CharactorBirthday":
                DBManager.instance.SaveCharactorBirthday(filterList);
                break;
            case "[Save]CharactorData":
                DBManager.instance.SaveCharactorData(filterList);
                break;
            case "[Save]ExpenditionCrew":
                DBManager.instance.SaveCrewData(filterList);
                break;
            case "[Save]LastPlayData":
                DBManager.instance.SaveLastPlayData(filterList);
                break;
            case "[Save]GameResult":
                break;
            case "[Save]venezia_kor":
                DBManager.instance.SaveGameResultData(filterList);
                break;
            case "[Save]venezia_eng":
                DBManager.instance.SaveGameResultData(filterList);
                break;
            case "[Save]venezia_chn":
                DBManager.instance.SaveGameResultData(filterList);
                break;
            case "[Save]gugudan":
                DBManager.instance.SaveGameResultData(filterList);
                break;
            case "[Save]calculation":
                DBManager.instance.SaveGameResultData(filterList);
                break;
            case "[Load]UserData":
                tempAllocate = DBManager.instance.LoadUserData(clientLicenseNumber);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
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
                tempAllocate = DBManager.instance.LoadRankData(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Load]ExpenditionCrew":
                tempAllocate = DBManager.instance.LoadExpenditionCrew(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Load]LastPlayData":
                tempAllocate = DBManager.instance.LoadLastPlayData(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Load]AnalyticsProfileData":
                tempAllocate = DBManager.instance.LoadAnalyticsProfileData(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Change]Charactor":
                tempAllocate = DBManager.instance.ChangeCharactor(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Delete]Charactor":
                DBManager.instance.DeleteCharactor(clientLicenseNumber, clientCharactor);
                break;
            case "[Reset]CharactorProfile":
                DBManager.instance.ResetCharactorProfile(clientLicenseNumber, clientCharactor);
                break;
            case "[Test]CreateDB":
                DBManager.instance.CreateDateDB();
                break;
            case "[Test]UpdateWeeklyRankDB":
                DBManager.instance.UpdateWeeklyRankDB();
                break;
            default:
                Debug.Log($"[Server] Handling error that request from client, request name : {requestName}");
                break;
        }

        for (int i = 0; i < replyRequestData_List.Count; i++)
        {
            byte[] data = Encoding.UTF8.GetBytes(replyRequestData_List[i]); // string -> byte[] ������ ���� ��ȯ
            stream.Write(data, 0, data.Length);
            //Debug.Log("[Server] Replying... request data to client");
        }

        byte[] finishData = Encoding.UTF8.GetBytes("Finish");
        stream.Write(finishData, 0, finishData.Length);
        Debug.Log("[Server] Finish reply request data to client");
    }

    // Timer ����
    private void TimerSet()
    {
        try
        {
            // ������ ���� �� �������� ���� ���������� �ð� ���
            nextMidnight = DateTime.Today.AddDays(1);
            TimeSpan timeUntilMidnight = nextMidnight - DateTime.Now;

            // Test��
            DateTime testAfterOneMinute = DateTime.Now.AddSeconds(20);
            timeUntilMidnight = testAfterOneMinute - DateTime.Now;

            // �������� ����� WaitforSeconds ����
            waitUntilMidnight = new WaitForSeconds((float)timeUntilMidnight.TotalSeconds);

            // DB�� ����Ǿ��ִ� weeklyrank ���ſ� ���� ������
            // �� �����Ϸκ��� �������� ������ weeklyrankDB table update
            standardDay = DBManager.instance.LoadStandardDay();

            TimeSpan timeSpan = DateTime.Now.Date - standardDay.Date;

            Debug.Log($"[Server] standardDay : {standardDay}");
            Debug.Log($"[Server] standardDay.Date : {standardDay.Date}");
            Debug.Log($"[Server] DateTime.Now : {DateTime.Now}");
            Debug.Log($"[Server] DateTime.Now.Date : {DateTime.Now.Date}");
            Debug.Log($"[Server] timeSpan : {timeSpan}");
            Debug.Log($"[Server] timeSpan.Days : {timeSpan.Days}");

            if(timeSpan.Days == 7)
            {

            }

            // 43200
            Debug.Log($"[Server] Waited time until next midnight, timeUntilMidnight : {timeUntilMidnight.TotalSeconds} ");

            // ���� üũ ����
            StartCoroutine(CheckMidnight_Co());
        }
        catch(Exception e)
        {
            Debug.Log($"[Server] Can't execute timerset method, {e.Message}");
            // ������ �ȵǸ� 5���Ŀ� �ٽ� ����
            Invoke("TimerSet", 5f);
        }
    }

    // �Ϸ簡 ������ �� PresentDB�� �ִ� gamedata�� �� DateDB(ex) 24.02.21)�� ����
    private IEnumerator CheckMidnight_Co()
    {
        while(true)
        {
            Debug.Log($"[Server] Come in CheckMidnight Coroutine");
            yield return waitUntilMidnight;

            Debug.Log("[Server] A day has passed. Start creating new DateDB for save data");

            Debug.Log($"[Server] DateTime.Now : {DateTime.Now}");
            // ������ �Ǹ� DateDB ����
            DBManager.instance.CreateDateDB();

            Debug.Log("[Server] Complete Create DateDB");

            // ���� �������� �Ǹ� weeklyrankDB�� ���ְ� rank table ����
            TimeSpan timeSpan = DateTime.Now.Date - standardDay.Date;
            Debug.Log("[Server] Complete Create DateDB");

            if (timeSpan.Days == 7)
            {
                Debug.Log("[Server] Start Update WeeklyRankDB");
                DBManager.instance.UpdateWeeklyRankDB();
                Debug.Log("[Server] Complete Update WeeklyRankDB");
                standardDay = DBManager.instance.LoadStandardDay();
                Debug.Log("[Server] Server class's standardDay member variable was re allocated after update weeklyRankDB");
            }

            // ������ ������ ���� DateDB�� ������ �� ���� üũ �ð� ����
            // CreateDateDB()�� �����ϴµ� �ð��� ������� �ɸ��Ƿ�, ������ ������ ���� �� �ִ�.
            nextMidnight = DateTime.Today.AddDays(1);
            TimeSpan timeUntilNextMidnight = nextMidnight - DateTime.Now;
            waitUntilMidnight = new WaitForSeconds((float)timeUntilNextMidnight.TotalSeconds);

            Debug.Log($"[Server] Waited time until next midnight, timeUntilNextMidnight : {timeUntilNextMidnight.TotalSeconds} ");
        }
    }

    // ����� Ŭ���̾�Ʈ Ȯ��
    private void CheckClientsState()
    {
        clientCheckTimer += Time.deltaTime;

        if(clientCheckTimer > clientCheckTime)
        {
            // ���� ������ �������� Ŭ���̾�Ʈ�� �������
            Debug.Log($"[Server] Present Connected Clients : {clients.Count}");

            foreach (TcpClient client in clients)
            {
                if(client != null) // Ŭ���̾�Ʈ�� null�� �ƴ϶��
                {
                    Debug.Log($"[Server] Connected clients's IP : {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
                }
                else
                {
                    Debug.Log($"[Server] Unknown client is null");
                }

                if (!IsConnected(client)) // client�� ������ null�� �ƴϾ �ѹ� Ȯ���غ��µ�, client�� �񷯺ôµ� ������ ���ٸ� == ������ ����ٸ�
                {
                    // clients List�� ������ client �߰�
                    disconnectList.Add(client);
                    Debug.Log($"[Server] The client is not null but it's disconnected, : {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
                }
            }

            // ������� client ����
            foreach(TcpClient client in disconnectList)
            {
                Debug.Log($"[Server] Remove client in clients where disconnectList : {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
                clients.Remove(client);
            }

            // disconnectList ����ϰ��� �ʱ�ȭ
            disconnectList.Clear();

            clientCheckTimer = 0f;
        }

        //// �� ������Ʈ����? 1�ʸ��� Ȯ��client ������� Ȯ��
        //if (clientCheckTimer > clientCheckTime)
        //{
        //    try
        //    {
        //        Debug.Log($"[Server] Present Connected Clients : {clients.Count}");

        //        foreach (TcpClient client in clients)
        //        {
        //            // ������ ���� client�� ������ client.Close �� disconnectList�� Add
        //            if (!IsConnected(client))
        //            {
        //                if(client == null)
        //                {
        //                    Debug.Log("[Server] client is null");
        //                    Debug.Log($"[Server] clients.Count : {clients.Count}");
        //                    Debug.Log($"[Server] disconnectList.Count : {disconnectList.Count}");
        //                }
        //                else
        //                {
        //                    Debug.Log($"[Server] client is not null, but connection is disconnected");
        //                }

        //                Debug.Log("[Server] Client connection is closed");
        //                // client�� null�� �ƴ����� client�� ������Ƽ�� null�� �� �ִ�.
        //                if(((IPEndPoint)client.Client.RemoteEndPoint).Address != null)
        //                {
        //                    Debug.Log($"[Server] Disconnected clients's IP : {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
        //                }
        //                client.Close();
        //                disconnectList.Add(client);
        //                //Debug.Log($"[Server] After client.Close(), Test check client.Connected bool value : {client.Connected}");
        //                //clients.Remove(client); // �÷����� �ݺ��ϰ� �ִ� ���߿� �÷����� �����ҷ��� �ϸ� �ȵ�
        //                // ������ ����� �ٸ� ����Ʈ�� �߰��ϰ�, �ݺ��� ���� �Ŀ� �ش� ����Ʈ�� �׸��� ����
        //                continue;
        //            }
        //            else // �������� Client�� IP �ּ� Ȯ��
        //            {
        //                Debug.Log($"[Server] Connected Clients's IP : {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
        //            }
        //            #region etc
        //            //// Ŭ���̾�Ʈ ������ �������ٸ�
        //            //if (!CheckConnectState(client))
        //            //{
        //            //    Debug.Log($"Disconnected client's IP {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
        //            //    client.Close();
        //            //    disconnectList.Add(client);
        //            //    continue;
        //            //}
        //            // Ŭ���̾�Ʈ�κ��� üũ �޽��� �ޱ�
        //            //else
        //            //{

        //            //    //NetworkStream stream = client.tcp.GetStream();
        //            //    //if(stream.DataAvailable)
        //            //    //{
        //            //    //    string data = new StreamReader(stream, true).ReadLine();

        //            //    //    if(data != null)
        //            //    //    {
        //            //    //        On
        //            //    //    }
        //            //    //}
        //            //}
        //            #endregion
        //        }
        //    }
        //    catch(Exception e)
        //    {
        //        Debug.Log($"[Server] CheckClientState Exception occurred while Check every 2 seconds, e.Message : {e.Message}");
        //    }
            

        //    foreach (TcpClient disconnectClient in disconnectList)
        //    {
        //        clients.Remove(disconnectClient);
        //    }
    }

    private void CheckClients(TcpClient client)
    {
        Debug.Log("[Server] Connection is disconnected, so come in CheckCliets Method");

        Debug.Log($"[Server] Before Remove, clients.Count : {clients.Count}");
        // �ʱ⿡ ������ List�鿡 �ش� ������ ���� client ó��
        // disconnectList.Add(client);
        clients.Remove(client);
        Debug.Log("[Server] clients.Remove(client)");
        
        Debug.Log($"[Server] After Remove, clients.Count : {clients.Count}");

        // ������ ����ٸ� �̹� DisPose�Ǿ������� �� �ѹ� Dispose()
        client.Dispose();
        Debug.Log("[Server] cliens.Close()");

        // client ������ null �Ҵ��Ͽ� ���� ����, ���߿� GC�� ������ �� �ֵ���
        client = null;

        if(client == null)
        {
            Debug.Log($"[Server] client is null");
        }
        else
        {
            Debug.Log($"[Server] client is not null");
        }

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

    private bool IsConnected(TcpClient client)
    {
        try
        {
            bool isConnCheck;

            if (client != null)
            {
                // �ؼ� �ʿ�
                // �ϴ� ����� �񷶴µ� ������ ������ true(������ ����Ǿ��ִ�) ������ false
                isConnCheck = !((client.Client.Poll(1000, SelectMode.SelectRead) && (client.Client.Available == 0)) || !client.Client.Connected);
            }
            else // �׳� client�� null�̸� ������ �ȵȰ��� = false
            {
                isConnCheck = false;
            }
            //bool isConnCheck = client.Client.Poll(1000, SelectMode.SelectRead) && (client.Client.Available == 0) && client.Client.Connected;

            return isConnCheck;
        }
        catch(Exception e)
        {
            Debug.Log($"[Server] IsConnected Exeception occurred : {e.Message}");
            return false;
        }
        
    }

    // ���� ����
    private void OnApplicationQuit()
    {
        server.Stop();
    }


    
}