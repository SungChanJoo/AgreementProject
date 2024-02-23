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
    public string licenseFolderPath = string.Empty;
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

        licenseFolderPath = Application.dataPath + "/License";
        string licenseFilePath = licenseFolderPath + "/clientlicense.json";

        Debug.Log($"[Client] Directory.Exists(licenseFolderPath) value ? {Directory.Exists(licenseFolderPath)}");
        // ��ο� ������ �������� �ʴ´ٸ� ���̼����ѹ��� ���ٴ°��̰�, ó�� �����Ѵٴ� ��
        if (!File.Exists(licenseFilePath))
        {
            loginStatus = ClientLoginStatus.New;
            // �������� ���̼��� �ѹ��� �޾ƿ;���, �׷��� ���� ������ ��û todo
            string requestName = "[Create]LicenseNumber";
            RequestToServer(requestName);
            Debug.Log($"[Client] Create licensenumber?");
            Debug.Log($"[Client] This client's licensenumber(first) : {clientLicenseNumber}");
            return; // ó�� �����̶�� ���� �� ���� �����ϰ� return
        }

        loginStatus = ClientLoginStatus.Exist;
        // �ش� ��ο� �ִ� ������ �о� Ŭ���̾�Ʈ ���̼��� �ѹ��� �ҷ���
        string jsonStringFromFile = File.ReadAllText(licenseFilePath);
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
            List<string> requestDataList = requestData.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
            string requestName = requestDataList[0];
            Debug.Log($"[Client] Request to server : {requestData}");

            ReceiveRequestDataFromServer(stream, requestName); // �޼��尡 ����ɶ� ���� ��� / ����Ű�� ���� �޼��� �տ� await�� �ٿ��� �����Ű���� �޼��尡 Task �ٿ����ϴµ�?
        }
        catch (Exception e)
        {
            Debug.Log($"[Client] Error Request to sever : + {e.Message}");
        }
    }

    // ������ ��û������ ����, ���� string, case�� �����ؼ� ó��
    private async void ReceiveRequestDataFromServer(NetworkStream stream, string requestName)
    {
        try
        {
            List<string> receivedRequestData_List = new List<string>();

            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // �����͸� �о�ö����� ���
                string receivedRequestData = Encoding.UTF8.GetString(buffer, 0, bytesRead); // ������ ��ȯ

                receivedRequestData_List.Add(receivedRequestData);
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

            HandleRequestData(receivedRequestData_List);

        }
        catch (Exception e)
        {
            Debug.Log($"[Client] Error receiving data from server : {e.Message}");
        }

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

        // dataList[0] = "[Load]PlayerData|value|value..|value|E|";
        string requestName = dataList[0].Split('|')[0];
        Debug.Log($"[Client] HandleRequestData method, request name : {requestName}");

        switch (requestName)
        {
            case "[Create]LicenseNumber":
                clientLicenseNumber = dataList[1];
                clientCharactor = dataList[2];
                SaveClientDataToJsonFile();
                Debug.Log($"[Client] RequestName : LicenseNumber, get and save licenseNumber to jsonfile");
                break;
            case "[Create]Charactor":
                break;
            case "[Save]venezia_kor":
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Save]venezia_eng":
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Save]venezia_chn":
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Save]gugudan":
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Save]calculation":
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Load]PlayerData":
                HandleLoadPlayerData(dataList);
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
    private void HandleLoadPlayerData(List<string> dataList)
    {
        // dataList[0] = "[Load]PlayerData|user_info|value|value..|value|E|";
        // dataList[1] = "rank|value|value|...|value|E|{gameTableName}|value|value|...|value|E|";
        // dataList[2] = "{gameTableName}|value|value|...|value|E|{gameTableName}|value|value|...|value|E|";
        // ... dataList[Last] = "{gameTableName}|value|value|...|value|E|Finish|;

        Debug.Log("[Client] Handling LoadPlayerdata");

        List<string> firstFilterData = new List<string>(); // "E|" ����

        // "E|" separatorString���� ����
        //List<string> parts = new List<string>();  
        //requestdata.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();

        //List<string> parts = dataList[0].Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList());
        for (int i = 0; i < dataList.Count; i++)
        {
            List<string> parts = new List<string>();
            parts = dataList[i].Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach(string )
            
        }



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
        File.WriteAllText(licenseFolderPath + "/clientlicense.json", jsonString);
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

    /*
   Client�� DB�� �ִ� ������ Save Load �ϴ� ���
   1. �� ���� -> Load (��� ������, ���ӵ�����, ��ȭ, crew �������� ��)
   2. �÷��̾�(charactor) ���� -> Save, Load
   3. �� ���� ���� -> TotalScore(int��) Load (game_type, level, step ����) -> Save GameData
   4. ��ŷ UI ���� ��(or ��ŷ���ΰ�ħ) -> rank Load
   5. �� ���� -> Save (��� ������)
    */

    // �� ���۽� ��� ������ Load
    public Result_DB AppStart_LoadAllDataFromDB()
    {
        Result_DB resultdb = new Result_DB();

        // user_info table -> [0]:User_LicenseNumber, [1]:User_Charactor, [2]:User_Name, [3]:User_Profile, [4]:User_Coin
        // rank table - > [0]:User_LicenseNumber, [1]:User_Charactor, [2]:TotalTime, [3]:TotalScore
        resultdb.playerName = playerdata_Dic["user_info"][2];
        resultdb.image = Encoding.UTF8.GetBytes(playerdata_Dic["user_info"][3]);
        resultdb.Day = "";
        resultdb.TotalAnswers = Int32.Parse(playerdata_Dic["rank"][3]);
        resultdb.TotalTime = float.Parse(playerdata_Dic["rank"][2]);

        string[] game_Names = { "venezia_kor", "venezia_eng", "venezia_chn", "calculation", "gugudan" };
        int[] levels = { 1, 2, 3 };
        int[] steps = { 1, 2, 3, 4, 5, 6 };

        for (int i = 0; i < game_Names.Length; i++)
        {
            for (int j = 0; j < levels.Length; j++)
            {
                for (int k = 0; k < steps.Length; k++)
                {
                    Game_Type game_type;
                    switch (i)
                    {
                        case 0:
                            game_type = Game_Type.A;
                            break;
                        case 1:
                            game_type = Game_Type.B;
                            break;
                        case 2:
                            game_type = Game_Type.C;
                            break;
                        case 3:
                            game_type = Game_Type.D;
                            break;
                        case 4:
                            game_type = Game_Type.E;
                            break;
                        default:
                            game_type = Game_Type.A;
                            Debug.Log("[Clinet] AppStart_LoadAllDataFromDB() Game_Type default Problem");
                            break;
                    }

                    string levelpart = $"level{levels[j]}";
                    if (game_Names[i] == "venezia_chn")
                    {
                        j = 2; // venezia_chn ������ level�� 1�����̹Ƿ� �ѹ��� ���ƾ���.
                        levelpart = "level";
                    }

                    string game_TableName = $"{game_Names[i]}_{levelpart}_step{steps[k]}";

                    float reactionRate = float.Parse(playerdata_Dic[$"{game_TableName}"][2]);
                    int answersCount = Int32.Parse(playerdata_Dic[$"{game_TableName}"][3]);
                    int answers = Int32.Parse(playerdata_Dic[$"{game_TableName}"][4]);
                    float playTime = float.Parse(playerdata_Dic[$"{game_TableName}"][5]);
                    int totalScore = Int32.Parse(playerdata_Dic[$"{game_TableName}"][6]);

                    Data_value datavalue = new Data_value(reactionRate, answersCount, answers, playTime, totalScore);

                    resultdb.Data.Add((game_type, j, k), datavalue);
                }
            }
        }

        return resultdb;
    }

    // Charactor ���� Save (������ �÷����ϴ� Charator Data)
    public void CreateCharactor_SaveAllDataToDB(Result_DB resultdb)
    {
        
    }

    // Charactor ���� Load (���� ������ Charatror Data)
    public Result_DB CreateCharactor_LoadAllDataFromDB()
    {
        Result_DB resultdb = new Result_DB();

        return resultdb;
    }

    // ���� ���۽� TotalScore Load
    public int AppGame_LoadTotalScoreFromDB(Game_Type game_type, int level, int step)
    {

        return 0;
    }

    // ����̰� ���� Result_Data�� �Ű������� �޾Ƽ� DB�� �����ϴ� �޼���(server�� ��û -> RequestServer)
    public void AppGame_SaveResultDataToDB(Result_DB resultdata, Game_Type game_type, int level, int step)
    {
        // requestData = RequestName[0]/User_Licensenumber[1]/User_Charactor[2]/ReactionRate[3]/.../StarPoint[8]
        string requestData;
        string requestName;
        string values;
        string gameName;

        switch(game_type)
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

        Data_value datavalue = resultdata.Data[(game_type, level, step)];

        requestName = $"[Save]{gameName}";
        values = $"{level}|{step}|{clientLicenseNumber}|{clientCharactor}|{datavalue.ReactionRate}|{datavalue.AnswersCount}|{datavalue.Answers}|{datavalue.PlayTime}|{datavalue.TotalScore}|";

        requestData = $"{requestName}|{values}";

        RequestToServer(requestData);
    }

    // ��ŷ UI ���� �� Rank Load
    public RankData Ranking_LoadRankDataFromDB()
    {
        RankData
      
    }

    // �� ����� ��� ������ Save
    public void AppEnd_SaveAlldataToDB()
    {

    }

    public void OnClickSaveGameDataTest()
    {
        Game_Type game_Type = Game_Type.A;
        int level = 1;
        int step = 2;
        Result_DB result_DB = new Result_DB();
        Data_value data_Value = new Data_value(234.21f, 23, 12, 22.44f, 18000);
        //Reult_DB�� Null�� �� ó��
        if (!result_DB.Data.ContainsKey((game_Type, level, step)))
        {
            result_DB.Data.Add((game_Type, level, step), data_Value);
        }
        else
        {
            //���� totalScore�� DB�� �ִ� �������� ũ�ٸ� �ٽ� �Ҵ�
            if (result_DB.Data[(game_Type, level, step)].TotalScore < 20000)
            {
                result_DB.Data[(game_Type, level, step)] = data_Value;
            }
        }

        AppGame_SaveResultDataToDB(result_DB, game_Type, level, step);

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
