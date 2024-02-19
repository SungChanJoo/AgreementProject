using System.Collections;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Threading.Tasks;
using LitJson;
using System.Linq;

// ó�� �����ϴ� ��������(First), �����߾��� ��������(Continue) 
public enum ClientLoginStatus
{
    New,
    Exist
}

public class Client : MonoBehaviour
{
    // IP, Port ������
    [SerializeField] private string server_IP = "15.165.159.141"; // aws EC2 IP : 15.165.159.141
    [SerializeField] private int server_Port = 2421;

    bool socketReady;
    private TcpClient client;
    private static NetworkStream stream;

    // login - license
    public static ClientLoginStatus loginStatus;
    public static string clientLicenseNumber;
    public string licensePath = string.Empty;

    // ������ ���� ���� data�� 1�������� �Ÿ� List
    public List<string> playerdata_FromServer = new List<string>();

    // DB Table Name
    private TableName table;

    // Player data�� ����ϱ� ���� Dictionary
    private Dictionary<string, List<string>> playerdata_Dic;

    // ����-Ŭ���̾�Ʈ string���� data �ְ������ �����ϱ� ���� ���ڿ�
    private const string separatorString = "E|";

    // timer
    private float transmissionTime = 1f;

    // Test GameData
    public string testGameName;
    public int testLevel;
    public int testStep;
    public int testScore;
    public int testTime;

    //// Login
    //public InputField login_ID_Input;
    //public InputField login_PW_Input;
    //public Text loginLog;

    //// Create Account
    //public InputField create_ID_Input;
    //public InputField create_PW_Input;

    public Client(NetworkStream _stream)
    {
        stream = _stream;
    }

    private void Awake()
    {
        
    }

    private void Start()
    {
        ETCInitSetting();
        ConnectToServer();
        ClientLoginSet();
        Invoke("LoadPlayerDataFromDB", 5f);
    }

    // Ŭ���̾�Ʈ�� ������ �� ���� ���� �õ�
    public void ConnectToServer()
    {
        // �̹� ����Ǿ��ٸ� �Լ� ����
        if (socketReady) return;

        // ������ ����
        try
        {
            client = new TcpClient();
            client.Connect(server_IP, server_Port);
            Debug.Log("[Client] Success Connect to Server!");
            socketReady = true;

            stream = client.GetStream();// ���ῡ �����ϸ� stream�� ��� ������ �� �ֵ���
        }
        catch (Exception e)
        {
            Debug.Log($"[Client] Fail Connect to Server : {e.Message}");
        }

    }

    public void ClientLoginSet()
    {
        // ������� �ʾҴٸ� return
        if (!client.Connected) return;

        licensePath = Application.dataPath + "/License";

        Debug.Log($"[Client] Directory.Exists(licensePath) value ? {Directory.Exists(licensePath)}");
        // ��ο� ������ �������� �ʴ´ٸ� ���̼����ѹ��� ���ٴ°��̰�, ó�� �����Ѵٴ� ��
        if (!Directory.Exists(licensePath))
        {
            loginStatus = ClientLoginStatus.New;
            Directory.CreateDirectory(licensePath);
            // �������� ���̼��� �ѹ��� �޾ƿ;���, �׷��� ���� ������ ��û todo
            string requestName = "LicenseNumber";
            RequestToServer(requestName);
            Debug.Log($"[Client] Create licensenumber?");
            Debug.Log($"[Client] This client's licensenumber(first) : {clientLicenseNumber}");
            return; // ó�� �����̶�� ���� �� ���� �����ϰ� return
        }

        loginStatus = ClientLoginStatus.Exist;
        // �ش� ��ο� �ִ� ������ �о� Ŭ���̾�Ʈ ���̼��� �ѹ��� �ҷ���
        string jsonStringFromFile = File.ReadAllText(licensePath + "/clientlicense.json");
        JsonData licenseNumber_JsonFile = JsonMapper.ToObject(jsonStringFromFile);
        clientLicenseNumber = licenseNumber_JsonFile["LicenseNumber"].ToString();
        Debug.Log($"[Client] Use already existed licensenumber?");
        Debug.Log($"[Client] This client's licensenumber(existing) : {clientLicenseNumber}");
    }

    // ���� ������ �� �������� �ҷ����� - ������ ��û(����-DB) // ���߿� Player Class �����Ǹ� �����ؾ���. todo
    private void LoadPlayerDataFromDB()
    {
        string requestData; // �� �޼��忡�� requestData�� requestName�� clientLicenseNumber 
        switch (loginStatus)
        {
            case ClientLoginStatus.New:
                requestData = $"LoadPlayerData|{clientLicenseNumber}";
                RequestToServer(requestData);
                break;
            case ClientLoginStatus.Exist:
                requestData = $"LoadPlayerData|{clientLicenseNumber}";
                RequestToServer(requestData);
                break;
            default:
                Debug.Log("[Client] Something unknown happend in LoadPlayer method");
                break;
        }
    }

    // ������ ��û�Ҷ� string���� �����µ�, �������� ���� �� string case�� �����ؼ� ó��
    private void RequestToServer(string requestData)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(requestData);
            stream.Write(data, 0, data.Length); // �����͸� ������ ���� ���? �׳� ������ ���ݾ�
            List<string> requestDataList = requestData.Split('|').ToList();
            string requestName = requestDataList[0];
            Debug.Log($"[Client] Request to server : {requestData}");

            // MassiveData���� �ƴ��� �ʱ⿡ �÷��̾� �����͸� ������ ���� ���� �����͸� �޾ƾ��ؼ�
            // ���� ó���ؾ���
            if(requestName == "LoadPlayerData")
            {
                ReceiveMassiveRequestFromServer(stream);
            }
            else
            {
                ReceiveRequestFromServer(stream, requestName); // �޼��尡 ����ɶ� ���� ��� / ����Ű�� ���� �޼��� �տ� await�� �ٿ��� �����Ű���� �޼��尡 Task �ٿ����ϴµ�?
            }
        }
        catch (Exception e)
        {
            Debug.Log($"[Client] Fail Request to sever : + {e.Message}");
        }
    }

    // ������ ��û������ ����, ���� string, case�� �����ؼ� ó��
    private async void ReceiveRequestFromServer(NetworkStream stream, string requestName)
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // �����͸� �о�ö����� ���
            string receivedRequestData = Encoding.UTF8.GetString(buffer, 0, bytesRead); // ������ ��ȯ
            Debug.Log($"[Client] Received request message from server : {receivedRequestData}");
            HandleRequestData(requestName, receivedRequestData);
        }
        catch (Exception e)
        {
            Debug.Log($"Error receiving message from server : {e.Message}");
        }
    }

    // �����κ��� ���� ������ ó��
    private void HandleRequestData(string requestname, string requestdata)
    {
        // requestname : Ŭ���̾�Ʈ-�������� �����͸� ó���ϱ� ���� ����
        // requestdata : Server���� requestName���� ó���� ����� ���� data
        // LicenseNumber -> clientLicenseNumber�� server�� ������ (Client�� ù ������ ��� ó����)
        // LoadNewPlayerData -> �� �÷��̾��� ��� DB�� licenseNumber�� ������ �����Ͱ� �����Ƿ� ��� ���̺� �����ϴ� ���׸� 0���� �ο��� PlayerData�� ���� ����
        // LoadExistPlayerData -> ���� �÷��̾��� ��� DB�� ����� PlayerData�� ���� ����
        Debug.Log($"[Client] HandleRequestData method, request name : {requestname}");

        switch (requestname)
        {
            case "LicenseNumber":
                clientLicenseNumber = requestdata;
                SaveLicenseNumberToJsonFile();
                Debug.Log($"[Client] RequestName : LicenseNumber, get and save licenseNumber to jsonfile");
                break;
            case "venezia_cha":
                Debug.Log($"[Client] RequestName : venezia_cha, End handling data");
                break;
            default:
                Debug.Log("[Client] HandleRequestMessage Method Something Happend");
                break;
        }
    }

    // �������� �뷮���� �����͸� �޴� �޼���
    private async void ReceiveMassiveRequestFromServer(NetworkStream stream)
    {
        Debug.Log($"[Client] ReceiveMassiveRequestFromServer");

        try
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // �����͸� �о�ö����� ���
                string receivedRequestData = Encoding.UTF8.GetString(buffer, 0, bytesRead); // ������ ��ȯ

                Debug.Log($"[Client] Receiving... massive request message from server : {receivedRequestData}");
                HandleMassiveRequestData(receivedRequestData);

                // �����Ͱ� �� �ҷ��������� break;
                List<string> endCheck = receivedRequestData.Split('|').ToList();
                if (endCheck.Contains("Finish"))
                {
                    FilterPlayerData(); // �÷��̾� ������ ����
                    ClientPlayerDataToPlayerClassVariable();
                    break;
                }
            }

        }
        catch (Exception e)
        {
            Debug.Log($"Error receiving message from server : {e.Message}");
        }
    }

    // �����κ��� ���� �뷮 ������ ó��
    private void HandleMassiveRequestData(string requestdata)
    {
        Debug.Log("[Client] Handling... massive request data");
        List<string> parts = requestdata.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();

        foreach(string part in parts)
        {
            playerdata_FromServer.Add(part);
        }
    }

    // ���������� ����� �� �ִ� player data
    // �����κ��� �ް� 1�������� ������ data�� table name�� ������ ���������� data ���� 
    private void FilterPlayerData()
    {
        Debug.Log("[Client] Filtering... player data");
        for(int i = 0; i < playerdata_FromServer.Count; i++)
        {
            for(int j = 0; j < table.list.Count; j++)
            {
                if(playerdata_FromServer[i].Contains(table.list[j]))
                {
                    List<string> values = playerdata_FromServer[i].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList(); // User���̺�������� User|User_Name|User_Profile|User_Coin�� ��������
                    values.RemoveAt(0); // 0��° �ε����� ���̺���̹Ƿ� values�� �ʿ����� �ʴ�.
                    playerdata_Dic.Add(table.list[j], values);
                }
            }
        }
    }

    // Json���Ͽ� LicenseNumber ��� / �񵿱�� ȣ��� ���� �������� ���������� ȣ���Ű�� ����
    private void SaveLicenseNumberToJsonFile()
    {
        // JsonData ����
        JsonData licenseNumber_Json = new JsonData();
        licenseNumber_Json["LicenseNumber"] = clientLicenseNumber;
        // Json �����͸� ���ڿ��� ��ȯ�Ͽ� ���Ͽ� ����
        string jsonString = JsonMapper.ToJson(licenseNumber_Json);
        File.WriteAllText(licensePath + "/clientlicense.json", jsonString);
    }

    // ���ӽ�ũ��Ʈ���� ���� �� �ð� �������� / Enum���� level step �����ϰ��ֳ�
    public static void GameResult_SaveDataToDB(string gamename, int level, int step, int score, int time)
    {
        string requestData = $"{gamename}|{level.ToString()}|{step.ToString()}|{score.ToString()}|{time.ToString()}";
        Client client = new Client(stream);
        client.RequestToServer(requestData);
    }

    // Start DBTable ����
    private void ETCInitSetting()
    {
        Debug.Log("[Client] ETCInitSetting");
        // DB TableName ����
        table = new TableName();

        // Dictionary ����
        playerdata_Dic = new Dictionary<string, List<string>>();
    }


    // �����κ��� ���� PlayerData�� ���ӿ��� ����ϴ� Player Class�� ����
    private void SetPlayer(User_Info user)
    {

    }

    private void OnDestroy()
    {
        CloseSocket();
    }

    private void OnApplicationQuit()
    {
        CloseSocket();
    }

    public void OnClickCheckCloseSocket()
    {
        CloseSocket();
    }

    public void OnClickGameDataTest()
    {
        GameResult_SaveDataToDB(testGameName,testLevel,testStep,testScore,testTime);
    }

    public void OnClickLoadPlayerDataFromServerTest()
    {
        Debug.Log($"[Client] Test... {playerdata_FromServer}");
        for(int i=0; i<playerdata_FromServer.Count;i++)
        {
            Debug.Log($"playerdata_FromServer[{i}]'s value : {playerdata_FromServer[i]}");
        }
    }

    public void OnClickLoadFilterPlayerDataTest()
    {
        foreach(KeyValuePair<string, List<string>> item in playerdata_Dic)
        {
            Debug.Log($"item.Key : {item.Key}, item.Value : {item.Value}");
            foreach(string value in item.Value)
            {
                Debug.Log($"item.Key : {item.Key}, value(string) in item.Value : {value}");
            }
        }
    }

    private void CloseSocket()
    {
        Debug.Log("Close Socket���� �����°�");
        if (!socketReady) return;

        //writer.Close();
        //reader.Close();
        client.Close();
        socketReady = false;
    }

    // ���� ���ӽ� Player Class Script�� �ִ� ����(�ΰ��ӿ��� ����� ������)�� Client PlayerData ���� �ִ� �޼���
    private void ClientPlayerDataToPlayerClassVariable()
    {

    }

    //// ��ư ������ �α���
    //public void OnClickLogin()
    //{
    //    // id, password�� �Է����� �ʾ����� return;
    //    if (login_ID_Input.text == null || login_PW_Input.text == null)
    //    {
    //        loginLog.text = "���̵�� ��й�ȣ�� �Է��ϼ���";
    //        return;
    //    }

    //    // id, password�� DB�� �ִ� User_Name�� User_Password ��
    //    // id_Input.text == DBManager.instance.user_Info.user_Name && password_Input.text == DBManager.instance.user_Info.user_Password
    //    if (DBManager.instance.Login(login_ID_Input.text, login_PW_Input.text))
    //    {
    //        loginLog.text = "�α��ο� �����߽��ϴ�.";

    //        // todo.. ���̵��ϴ���, ��ȭ �ҷ������� ��� �Ұ� 

    //        User_Info user = DBManager.instance.user_Info;
    //        Debug.Log(user.user_Name + "|" + user.user_Password);
    //    }
    //    else // �α��� ����
    //    {
    //        loginLog.text = "���̵� �Ǵ� ��й�ȣ�� Ȯ�����ּ���";
    //    }
    //}

    //// ��ư ������ ��������
    //public void OnClickCreateAccount()
    //{
    //    DBManager.instance.CreateAccount(create_ID_Input.text, create_PW_Input.text);
    //}

    //// ��ư ������ ������ �޽��� ������
    //public void OnCilckSendMessage()
    //{
    //    SendMessageToServer();
    //}

    //private void SendMessageToServer()
    //{
    //    string sendMessage = "Hello World";
    //    try
    //    {

    //        byte[] data = Encoding.UTF8.GetBytes(sendMessage);
    //        stream.Write(data, 0, data.Length); // �����͸� ������ ���� ���? �׳� ������ ���ݾ�
    //        Debug.Log($"Sent message to server : {sendMessage}");

    //        ReceiveMessageFromServer(stream); // �޼��尡 ����ɶ� ���� ��� / ����Ű�� ���� �޼��� �տ� await�� �ٿ��� �����Ű���� �޼��尡 Task �ٿ����ϴµ�?
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.Log($"Fail sending message to sever : + {e.Message}");
    //    }
    //}

    //private async void ReceiveMessageFromServer(NetworkStream stream)
    //{
    //    try
    //    {
    //        byte[] buffer = new byte[1024];
    //        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // �����͸� �о�ö����� ���
    //        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead); // ������ ��ȯ
    //        Debug.Log($"Received message from server : {receivedMessage}");
    //    }
    //    catch(Exception e)
    //    {
    //        Debug.Log($"Error receiving message from server : {e.Message}");
    //        Debug.LogError($"Error receiving message from server : {e.Message}");
    //    }
    //}

    #region chatgpt
    //private TcpClient client;
    //private NetworkStream stream;
    //public Text clientLog;

    //// �÷��̾ ���� �����ų �� DB���� �����͸� �����;���
    //// �����;� �ϴ� ������. ����. 
    //// ������ �����ϱ� ���� �ʿ��� IP�� License�� ������? ��ũ��Ʈ?
    //// ��¶�� �����ص� ��ũ��Ʈ�� ���� �� ������. ���� �Ű����ϳ�?
    //// need Fix

    //public void StartClient()
    //{
    //    client = new TcpClient();
    //    client.BeginConnect("127.0.0.1", 8888, HandleClientConnect, null);
    //}

    //private void HandleClientConnect(IAsyncResult result)
    //{
    //    client.EndConnect(result);
    //    stream = client.GetStream();

    //    Debug.Log("Connected to server!");
    //    clientLog.text += "\nConnected to server!";

    //    // �����κ��� ������ �ޱ� ����
    //    byte[] receiveBuffer = new byte[1024];
    //    stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, HandleDataReceived, receiveBuffer);

    //    // ���� : ������ ��Ŷ ������
    //    Packet packetToSend = new Packet("Player2", 150);
    //    SendPacket(packetToSend);
    //}

    //private void HandleDataReceived(IAsyncResult result)
    //{
    //    int bytesRead = stream.EndRead(result);

    //    if(bytesRead > 0)
    //    {
    //        byte[] receivedData = new byte[bytesRead];
    //        Array.Copy((result.AsyncState as byte[]), receivedData, bytesRead);

    //        // ���� : ���� ��Ŷ ������ȭ
    //        Packet receivedPacket = PacketFromBytes(receivedData);
    //        Debug.Log("Received data from server: " + receivedPacket.playerName + " - " + receivedPacket.playerScore);

    //        // Handle the received data as needed

    //        // Continue listening for more data
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

    //// need Fix
    //private void OnDestroy()
    //{
    //    if(client != null)
    //    {
    //        client.Close();
    //    }
    //}
    #endregion
}
