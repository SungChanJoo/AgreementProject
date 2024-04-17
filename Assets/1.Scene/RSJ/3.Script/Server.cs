using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class thread_queue
{
    public Thread th;
    public TcpClient tcpc;

    public thread_queue(Thread t, TcpClient c)
    {
        th = t;
        tcpc = c;
    }
}


public class Server : MonoBehaviour
{
    private const int port = 2421;


    private List<thread_queue> thread_List = new List<thread_queue>();

    public Thread[] thread_a = new Thread[10];

    // Ŭ���̾�Ʈ ������ ���� �� �ֵ��� ����Ʈ
    private List<TcpClient> clients;
    private List<TcpClient> disconnectList;

    private TcpListener server;
    private bool isServerStarted;

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

    public int client_count=3;

    // ��Ÿ ������ ó���� Handler
    private ETCMethodHandler etcMethodHandler = new ETCMethodHandler();

    // ������ �ѹ��� ����
    private void Start()
    {
        Debug.Log("[Server] Server start callback function");
        ServerCreate();
        Invoke("TimerSet", 5f);
        //Debug.Log(Thread.CurrentThread.ManagedThreadId);
    }

    private void Update()
    {
        if (!isServerStarted) return;
        //if (clients.Count == 0) return;//todo0417

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
            isServerStarted = true;

            for(int i=0;i< client_count; i++)
            {

                Thread th=new Thread(StartListening);
                th.Start();
                Debug.LogWarning(th.ManagedThreadId+" : "+ th.ThreadState);
                thread_a[i]=th;
            }

            //StartListening();

        }
        catch (Exception e)
        {
            Debug.Log($"[Server] Server-Client Connect Error! : {e.Message}");
        }
    }
    // �񵿱�� Ŭ���̾�Ʈ �����û �ޱ� ����
    private async void StartListening()
    {
        //int thread_id = Thread.CurrentThread.ManagedThreadId;

        Debug.Log("[Server] Server is listening until client is comming");
        // �񵿱� Ŭ���̾�Ʈ ���� / await Ű����� �񵿱� ȣ���� �Ϸ�� ������ ���

        TcpClient client = await server.AcceptTcpClientAsync();
        Debug.Log("[Server] Asynchronously server accept client's request");
        //thread_List.Add(new thread_queue(Thread.CurrentThread,client));
        

        // Ŭ���̾�Ʈ�� ����Ǹ� ���� �޼��� ȣ��
        HandleClient(client);
    }

    // ��� �޼���, Ŭ���̾�Ʈ ����Ǹ� �ٽ� �����û ����
    private void HandleClient(TcpClient client)
    {
        Debug.Log("[Server] Client connected");
        clients.Add(client);
        //connect_thread_close();

        for (int i = 0; i < clients.Count; i++)
        {
            Debug.Log($"[Server] Connected client's IP : {((IPEndPoint)clients[i].Client.RemoteEndPoint).Address}");
        }

        // ����� Ŭ���̾�Ʈ -> �����͹����غ�
        ReceiveRequestFromClient(client);
      
        //StartListening(); // Ŭ���̾�Ʈ �ް� �ٽ� ����    
    }
    /*
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

        Thread th = new Thread(
            ()=> { ReceiveRequestFromClient(client); }
            );

         th.Start();
        if(th.IsAlive)
        {
            thread_key.Add(client, th);
        }
        StartListening(); // Ŭ���̾�Ʈ �ް� �ٽ� ����    
    }
    */
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

        for (int i = 0; i < filterList.Count; i++)
        {
            //Debug.Log($"[Server] Received Data From Client to filterList[{i}] : {filterList[i]}");
        }

        //string requestName = $"[Save]CharactorData";
        //requestData = $"{requestName}|{clientLicenseNumber}|{clientCharactor}|{_separatorString}";

        // filterList[0] '|' ���� -> requestName, clientLicenseNumber, clientCharactor�� ��� ���ؼ�
        List<string> baseDataList = filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

        string requestName = baseDataList[0];
        int clientLicenseNumber = 0;
        int clientCharactor = 0;
        string createCharactorName = null;

        if (baseDataList.ElementAtOrDefault(1) != null && baseDataList.ElementAtOrDefault(2) != null) // �Ϲ������� clientLicenseNumber�� clientCharactor�� ���� ����
        {
            clientLicenseNumber = Int32.Parse(baseDataList[1]);
            clientCharactor = Int32.Parse(baseDataList[2]);
        }
        else if (baseDataList.ElementAtOrDefault(1) != null) // UserData, clientLicenseNumber�� ���� ��
        {
            clientLicenseNumber = Int32.Parse(baseDataList[1]);
        }

        if (baseDataList.ElementAtOrDefault(3) != null)
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
                string clientdata = DBManager.Instance.CreateLicenseNumber();
                clientLicenseNumber = Int32.Parse(clientdata.Split('|')[0]);

                // �� ĳ���� ���� ���� (���� �Ѹ�� ������ ù ĳ����)
                Debug.Log($"[Server] Creating... new Charactor Data");
                DBManager.Instance.CreateNewCharactorData(clientLicenseNumber, 1);

                Debug.Log($"[Server] Finish Create LicenseNumber and new CharactorData");
                replyRequestData_List.Add($"{clientdata}");
                break;
            case "[Create]Charactor":
                string newClientCharactor = DBManager.Instance.CreateNewCharactorData(clientLicenseNumber, clientCharactor, createCharactorName);
                Debug.Log($"[Server] Finish Create new CharactorData");
                //replyRequestData_List.Add($"{newClientCharactor}|");
                break;
            case "[Save]CharactorName":
                DBManager.Instance.SaveCharactorName(filterList);
                break;
            case "[Save]CharactorProfile":
                DBManager.Instance.SaveCharactorProfile(filterList);
                break;
            case "[Save]CharactorBirthday":
                DBManager.Instance.SaveCharactorBirthday(filterList);
                break;
            case "[Save]CharactorData":
                DBManager.Instance.SaveCharactorData(filterList);
                break;
            case "[Save]ExpenditionCrew":
                DBManager.Instance.SaveCrewData(filterList);
                break;
            case "[Save]LastPlayData":
                DBManager.Instance.SaveLastPlayData(filterList);
                break;
            case "[Save]venezia_kor":
                DBManager.Instance.SaveGameResultData(filterList);
                break;
            case "[Save]venezia_eng":
                DBManager.Instance.SaveGameResultData(filterList);
                break;
            case "[Save]venezia_chn":
                DBManager.Instance.SaveGameResultData(filterList);
                break;
            case "[Save]gugudan":
                DBManager.Instance.SaveGameResultData(filterList);
                break;
            case "[Save]calculation":
                DBManager.Instance.SaveGameResultData(filterList);
                break;
            case "[Save]Coin":
                DBManager.Instance.SaveCoinData(filterList);
                break;
            case "[Load]UserData":
                tempAllocate = DBManager.Instance.LoadUserData(clientLicenseNumber);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Load]CharactorData":
                // dataList[1] = user_LicenseNumber, dataList[2] = user_Charactor
                tempAllocate = DBManager.Instance.LoadCharactorData(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data)); // TempList�� �� ���(data = string value) ReplyList�� �߰�
                break;
            case "[Load]AnalyticsData":
                tempAllocate = DBManager.Instance.LoadAnalyticsData(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Load]RankData":
                tempAllocate = DBManager.Instance.LoadRankData(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Load]ExpenditionCrew":
                tempAllocate = DBManager.Instance.LoadExpenditionCrew(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Load]LastPlayData":
                tempAllocate = DBManager.Instance.LoadLastPlayData(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Load]AnalyticsProfileData":
                tempAllocate = DBManager.Instance.LoadAnalyticsProfileData(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Change]Charactor":
                tempAllocate = DBManager.Instance.ChangeCharactor(clientLicenseNumber, clientCharactor);
                tempAllocate.ForEach(data => replyRequestData_List.Add(data));
                break;
            case "[Delete]Charactor":
                DBManager.Instance.DeleteCharactor(clientLicenseNumber, clientCharactor);
                break;
            case "[Reset]CharactorProfile":
                DBManager.Instance.ResetCharactorProfile(clientLicenseNumber, clientCharactor);
                break;
            case "[Test]CreateDB":
                DBManager.Instance.CreateDateDB();
                break;
            case "[Test]UpdateWeeklyRankDB":
                DBManager.Instance.UpdateWeeklyRankDB();
                break;
            default:
                Debug.Log($"[Server] Handling error that request from client, request name : {requestName}");
                break;
        }

        for (int i = 0; i < replyRequestData_List.Count; i++)
        {
            byte[] data = Encoding.UTF8.GetBytes(replyRequestData_List[i]);
            stream.Write(data, 0, data.Length);
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

            //// Test��_Timer 10��
            //DateTime testAfterOneMinute = DateTime.Now.AddSeconds(10);
            //timeUntilMidnight = testAfterOneMinute - DateTime.Now;

            // �������� ����� WaitforSeconds ����
            waitUntilMidnight = new WaitForSeconds((float)timeUntilMidnight.TotalSeconds);

            // DB�� ����Ǿ��ִ� weeklyrank ���ſ� ���� ������
            // �� �����Ϸκ��� �������� ������ weeklyrankDB table update
            standardDay = DBManager.Instance.LoadStandardDay();

            TimeSpan timeSpan = DateTime.Now.Date - standardDay.Date;

            Debug.Log($"[Server] standardDay : {standardDay}");
            Debug.Log($"[Server] standardDay.Date : {standardDay.Date}");
            Debug.Log($"[Server] DateTime.Now : {DateTime.Now}");
            Debug.Log($"[Server] DateTime.Now.Date : {DateTime.Now.Date}");
            Debug.Log($"[Server] timeSpan : {timeSpan}");
            Debug.Log($"[Server] timeSpan.Days : {timeSpan.Days}");

            Debug.Log($"[Server] Waited time until next midnight, timeUntilMidnight : {timeUntilMidnight.TotalSeconds} ");

            // ���� üũ ����
            StartCoroutine(CheckMidnight_Co());
        }
        catch (Exception e)
        {
            Debug.Log($"[Server] Can't execute timerset method, {e.Message}");
            // ������ �ȵǸ� 5���Ŀ� �ٽ� ����
            Invoke("TimerSet", 5f);
        }
    }

    // �Ϸ簡 ������ �� PresentDB�� �ִ� gamedata�� �� DateDB(ex) 24.02.21)�� ����
    private IEnumerator CheckMidnight_Co()
    {

        Debug.Log($"[Server] Come in CheckMidnight Coroutine");
        yield return waitUntilMidnight;

        Debug.Log("[Server] A day has passed. Start creating new DateDB for save data");

        Debug.Log($"[Server] DateTime.Now : {DateTime.Now}");
        // ������ �Ǹ� DateDB ����
        DBManager.Instance.CreateDateDB();

        Debug.Log("[Server] Complete Create DateDB");

        // ���� �������� �Ǹ� weeklyrankDB�� ���ְ� rank table ����
        TimeSpan timeSpan = DateTime.Now.Date - standardDay.Date;

        if (timeSpan.Days == 7)
        {
            Debug.Log("[Server] Start Update WeeklyRankDB");
            DBManager.Instance.UpdateWeeklyRankDB();
            Debug.Log("[Server] Complete Update WeeklyRankDB");
            standardDay = DBManager.Instance.LoadStandardDay();
            Debug.Log("[Server] Server class's standardDay member variable was reallocated after update weeklyRankDB");
        }

        // ������ ������ ���� DateDB�� ������ �� ���� üũ �ð� ����
        // CreateDateDB()�� �����ϴµ� �ð��� ������� �ɸ��Ƿ�, ������ ������ ���� �� �ִ�.
        nextMidnight = DateTime.Today.AddDays(1);
        TimeSpan timeUntilNextMidnight = nextMidnight - DateTime.Now;
        waitUntilMidnight = new WaitForSeconds((float)timeUntilNextMidnight.TotalSeconds);

        //Debug.Log("[Server] Test TimerSet Coroutine ");
        //waitUntilMidnight = new WaitForSeconds(10f);

        Debug.Log($"[Server] Waited time until next midnight, timeUntilNextMidnight : {timeUntilNextMidnight.TotalSeconds} ");

        // �Ϸ簡 ������ DateDB�����ϰ� �������� ������ WeeklyRankDB ������ �� �ٽ� �Ϸ簡 �������� üũ
        StartCoroutine(CheckMidnight_Co());
    }

    // ����� Ŭ���̾�Ʈ Ȯ��
    private void CheckClientsState()
    {
        clientCheckTimer += Time.deltaTime;

        if (clientCheckTimer > clientCheckTime)
        {
            // ���� ������ �������� Ŭ���̾�Ʈ�� �������
            Debug.Log($"[Server] Present Connected Clients : {clients.Count}");

            foreach (TcpClient client in clients)
            {
                if (client != null) // Ŭ���̾�Ʈ�� null�� �ƴ϶��
                {
                    Debug.Log($"[Server] Connected clients's IP : {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
                }
                else
                {
                    Debug.Log($"[Server] Unknown client is null");
                }

                if (!IsConnected(client)) // client�� ������ null�� �ƴϾ �ѹ� Ȯ���غ��µ�, client�� �񷯺ôµ� ������ ���ٸ� == ������ ����ٸ�
                {
                    // _clients List�� ������ client �߰�
                    disconnectList.Add(client);
                    //thread_manager(client);
                    connect_thread_close();
                    Debug.Log($"[Server] The client is not null but it's disconnected, : {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
                }
            }


            // ������� client ����
            foreach (TcpClient client in disconnectList)
            {
                Debug.Log($"[Server] Remove client in clients where disconnectList : {((IPEndPoint)client.Client.RemoteEndPoint).Address}");
                clients.Remove(client);
            }

            // _disconnectList ����ϰ��� �ʱ�ȭ
            //disconnectList.Clear();

            clientCheckTimer = 0f;
        }

    }


    private void connect_thread_close()
    {
        for(int i=0;i< thread_a.Length;i++)
        {
            if (thread_a[i].ThreadState.Equals(ThreadState.Stopped))
            {
                thread_a[i].Abort();
                thread_a[i].Join();
                if(!thread_a[i].IsAlive)
                {
                    thread_a[i] = null;
                    connect_thread_create(i);
                }
            }
            return;
        }

       
    }

    private void connect_thread_create(int i)
    {
        Thread th = new Thread(StartListening);
        th.Start();
        Debug.LogWarning(th.ManagedThreadId + " : " + th.ThreadState);
        thread_a[i] = th;
    }




    private void CheckClients(TcpClient client)
    {
        Debug.Log("[Server] Connection is disconnected, so come in CheckCliets Method");

        Debug.Log($"[Server] Before Remove, clients.Count : {clients.Count}");
        // �ʱ⿡ ������ List�鿡 �ش� ������ ���� client ó��
        clients.Remove(client);
        Debug.Log("[Server] clients.Remove(client)");

        Debug.Log($"[Server] After Remove, clients.Count : {clients.Count}");

        // ������ ����ٸ� �̹� DisPose�Ǿ������� �� �ѹ� Dispose()
        client.Dispose();
        Debug.Log("[Server] cliens.Close()");
        //if(thread_key[client].ThreadState.Equals(ThreadState.Running))
        //{
        //    thread_key[client].Abort();
        //    thread_key[client].Join();
        //
        //}

        // client ������ null �Ҵ��Ͽ� ���� ����, ���߿� GC�� ������ �� �ֵ���
        client = null;

        if (client == null)
        {
            Debug.Log($"[Server] client is null");
        }
        else
        {
            Debug.Log($"[Server] client is not null");
        }

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
        catch (Exception e)
        {
            Debug.Log($"[Server] IsConnected Exeception occurred : {e.Message}");
            return false;
        }

    }

    // ���� ����
    private void OnApplicationQuit()
    {

        //��� thread ����.
        //foreach(TcpClient c in clients)
        //{
        //    if (thread_key[c].IsAlive)
        //    {
        //        thread_key[c].Abort();
        //        thread_key[c].Join();
        //    }
        //}
        //foreach (TcpClient c in disconnectList)
        //{
        //    if (thread_key[c].IsAlive)
        //    {
        //        thread_key[c].Abort();
        //        thread_key[c].Join();
        //    }
        //}

        server.Stop();
    }



}