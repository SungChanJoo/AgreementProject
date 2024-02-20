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
    public string clientCharactor;

    // ������ ���� ���� data�� 1�������� �Ÿ� List
    public List<string> playerdata_FromServer = new List<string>();

    // DB Table Name
    private TableName table;

    // Player data�� ����ϱ� ���� Dictionary
    public Dictionary<string, List<string>> playerdata_Dic;

    // ����-Ŭ���̾�Ʈ string���� data �ְ������ �����ϱ� ���� ���ڿ�
    private const string separatorString = "E|";

    // timer
    private float transmissionTime = 1f;

    public Client(NetworkStream _stream)
    {
        stream = _stream;
    }

    public static Client instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
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
            string requestName = "[Create]LicenseNumber";
            RequestToServer(requestName);
            Debug.Log($"[Client] Create licensenumber?");
            Debug.Log($"[Client] This client's licensenumber(first) : {clientLicenseNumber}");
            return; // ó�� �����̶�� ���� �� ���� �����ϰ� return
        }

        loginStatus = ClientLoginStatus.Exist;
        // �ش� ��ο� �ִ� ������ �о� Ŭ���̾�Ʈ ���̼��� �ѹ��� �ҷ���
        string jsonStringFromFile = File.ReadAllText(licensePath + "/clientlicense.json");
        JsonData client_JsonFile = JsonMapper.ToObject(jsonStringFromFile);
        clientLicenseNumber = client_JsonFile["LicenseNumber"].ToString();
        clientCharactor = client_JsonFile["Charactor"].ToString();
        Debug.Log($"[Client] Use already existed licensenumber?");
        Debug.Log($"[Client] This client's licensenumber(existing) : {clientLicenseNumber}");
    }

    // ���� ������ �� �������� �ҷ����� - ������ ��û(����-DB) // ���߿� Player Class �����Ǹ� �����ؾ���. todo
    private void LoadPlayerDataFromDB()
    {
        Debug.Log("[Clinet] Request LoadPlayerDataFromDB");
        string requestData; // �� �޼��忡�� requestData�� requestName/clientLicenseNumber/clientCharactor
        requestData = $"[Load]PlayerData|{clientLicenseNumber}|{clientCharactor}";
        RequestToServer(requestData);
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
            if(requestName == "[Load]PlayerData")
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
            List<string> dataList = receivedRequestData.Split('|').ToList();
            HandleRequestData(dataList);
        }
        catch (Exception e)
        {
            Debug.Log($"Error receiving message from server : {e.Message}");
        }
    }

    // �����κ��� ���� ������ ó��
    private void HandleRequestData(List<string> dataList)
    {
        // requestname : Ŭ���̾�Ʈ-�������� �����͸� ó���ϱ� ���� ����
        // requestdata : Server���� requestName���� ó���� ����� ���� data
        // LicenseNumber -> clientLicenseNumber�� server�� ������ (Client�� ù ������ ��� ó����)
        // LoadNewPlayerData -> �� �÷��̾��� ��� DB�� licenseNumber�� ������ �����Ͱ� �����Ƿ� ��� ���̺� �����ϴ� ���׸� 0���� �ο��� PlayerData�� ���� ����
        // LoadExistPlayerData -> ���� �÷��̾��� ��� DB�� ����� PlayerData�� ���� ����
        string requestName = dataList[0];
        Debug.Log($"[Client] HandleRequestData method, request name : {requestName}");

        switch (requestName)
        {
            case "[Create]LicenseNumber":
                clientLicenseNumber = dataList[1];
                clientCharactor = dataList[2];
                SaveClientDataToJsonFile();
                Debug.Log($"[Client] RequestName : LicenseNumber, get and save licenseNumber to jsonfile");
                break;
            //case "[Save]venezia_kor":
            //    break;
            //case "[Save]venezia_eng":
            //    break;
            //case "[Save]venezia_chn":
            //    Debug.Log($"[Client] RequestName : venezia_chn, End handling data");
            //    DBManager.instance.SaveGameResultData(dataList);
            //    break;
            //case "[Save]gugudan":
            //    break;
            //case "[Save]calculation":
            //    break;
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
                    //ClientPlayerDataToPlayerClassVariable();
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
    private void SaveClientDataToJsonFile()
    {
        // JsonData ����
        JsonData client_Json = new JsonData();
        client_Json["LicenseNumber"] = clientLicenseNumber;
        client_Json["Charactor"] = clientCharactor;
        // Json �����͸� ���ڿ��� ��ȯ�Ͽ� ���Ͽ� ����
        string jsonString = JsonMapper.ToJson(client_Json);
        File.WriteAllText(licensePath + "/clientlicense.json", jsonString);
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

    // ����̰� ���� Result_Data�� �Ű������� �޾Ƽ� DB�� �����ϴ� �޼���(server�� ��û -> RequestServer)
    public void SaveResultDataToDB(Result_DB resultdata)
    {
        // requestData = RequestName[0]/User_Licensenumber[1]/User_Charactor[2]/ReactionRate[3]/.../StarPoint[8]
        string requestData;
        string requestName;
        string values;
        string gameName;

        switch(resultdata.game_type)
        {
            case Game_Type.A:
                gameName = "venezia_kor";
                break;
            case Game_Type.B:
                gameName = "venezia_eng";
                break;
            case Game_Type.C:
                gameName = "venezia_chn";
                break;
            case Game_Type.D:
                gameName = "calculation";
                break;
            case Game_Type.E:
                gameName = "gugudan";
                break;
            default:
                Debug.Log("[Client] Game_Type error");
                gameName = "error";
                break;
        }

        Data_value datavalue = resultdata.Data[(resultdata.game_type, resultdata.Level, resultdata.Step)];

        requestName = $"[Save]{gameName}";
        values = $"{resultdata.Level}|{resultdata.Step}|{clientLicenseNumber}|{clientCharactor}|{datavalue.ReactionRate}|{datavalue.AnswersCount}|{datavalue.Answers}|{datavalue.PlayTime}|{datavalue.TotalScore}|";

        requestData = $"{requestName}|{values}";

        RequestToServer(requestData);
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

    public void OnClickLoadPlayerDataFromServerTest()
    {
        Debug.Log($"[Client] Test... {playerdata_FromServer}");
        for (int i = 0; i < playerdata_FromServer.Count; i++)
        {
            Debug.Log($"playerdata_FromServer[{i}]'s value : {playerdata_FromServer[i]}");
        }
    }

    public void OnClickLoadFilterPlayerDataTest()
    {
        foreach (KeyValuePair<string, List<string>> item in playerdata_Dic)
        {
            Debug.Log($"item.Key : {item.Key}, item.Value : {item.Value}");
            foreach (string value in item.Value)
            {
                Debug.Log($"item.Key : {item.Key}, value(string) in item.Value : {value}");
            }
        }
    }

    public void OnClickLoadGameDataForRankTest()
    {

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
}
