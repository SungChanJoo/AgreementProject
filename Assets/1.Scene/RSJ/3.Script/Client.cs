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
//using Unity.Android;

public class Client : MonoBehaviour
{
    // IP, Port ������
    [SerializeField] private string server_IP = "15.164.166.133"; // aws EC2 IP : 15.164.166.133
    [SerializeField] private int server_Port = 2421;

    bool socketReady;
    // TCP ���
    private TcpClient client;
    private static NetworkStream stream;

    // login - license, charactor / local(json)�� ����
    public static string clientLicenseNumber;
    public static string clientCharactor;
    public string licenseFolderPath = string.Empty;

    // ������ ���� ���� data�� 1�������� �Ÿ� List
    public List<string> CharactorData_FromServer;

    // Charactor data�� ����ϱ� ���� Dictionary
    public Dictionary<string, List<string>> CharactorData_Dic;

    // ����-Ŭ���̾�Ʈ string���� data �ְ������ �����ϱ� ���� ���ڿ�
    private const string separatorString = "E|";

    // DB Table Name
    private TableName table;

    // DB�κ��� Load�� Data���� ���� ����, �ٸ� ���� ��ȯ�� �� �ֵ��� class�� ����
    // ������ Request�� �� �޴� Received Data�� ���� ����
    private UserData clientUserData;
    private Player_DB clientPlayerData;
    private AnalyticsData clientAnalyticsData;
    private RankData clientRankData;
    private ExpenditionCrew clientExpenditionCrew;
    private LastPlayData clientLastPlayData;
    private AnalyticsProfileData clientAnalyticsProfileData;

    // timer
    private float transmissionTime = 1f;

    // ��Ÿ ������ ó���� Handler
    private ETCMethodHandler etcMethodHandler;

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
    }

    #region Start() Methods, Setting and Connect to Server
    // Start�� ��Ÿ ������� �ʱ�ȭ
    private void ETCInitSetting()
    {
        Debug.Log("[Client] ETCInitSetting");

        // DB TableName ����
        table = new TableName();

        // CharactorData Load������ �޾ƿ��� List ����
        CharactorData_FromServer = new List<string>();

        // Dictionary ����
        CharactorData_Dic = new Dictionary<string, List<string>>();

        // ��Ÿ ������ ó���� Handler ����
        etcMethodHandler = new ETCMethodHandler();
    }

    // Ŭ���̾�Ʈ�� ������ �� ���� ���� �õ�
    public void ConnectToServer()
    {
        // �̹� ����Ǿ��ٸ� �Լ� ����
        if (socketReady) return;

        try
        {
            // ���� ����
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


    // ������ ������ �� Ŭ���̾�Ʈ�� ������ �ҷ����� ���� �޼���
    public void ClientLoginSet()
    {
        // ������� �ʾҴٸ� return
        if (!client.Connected) return;
        //licenseFolderPath = Application.dataPath + "/License";

        // ���߿� �ȵ���̵� �Ҷ� Application.persistentDataPath + "/License";

        if (Application.platform == RuntimePlatform.Android)
        {

            licenseFolderPath = Application.persistentDataPath + "/License";
        }
        else
        {
            licenseFolderPath = Application.dataPath + "/License";
        }

        string licenseFilePath = licenseFolderPath + "/clientlicense.json";
        Debug.Log($"���߿� ���ϰ�θ� �˷��ּ���{licenseFilePath}");
        // ��ο� ������ �������� �ʴ´ٸ� ���̼����ѹ��� ���ٴ°��̰�, ó�� �����Ѵٴ� ��
        if (!File.Exists(licenseFilePath))
        {

            Directory.CreateDirectory(licenseFolderPath);
            // �������� ���̼��� �ѹ��� �޾ƿ;���, �׷��� ���� ������ ��û todo
            Debug.Log($"[Client] This client is entering game for the first time..");
            string requestName = "[Create]LicenseNumber|Finish";
            RequestToServer(requestName);
            return; // ó�� �����̶�� ���� �� ���� �����ϰ� return
        }

            // �ش� ��ο� �ִ� ������ �о� Ŭ���̾�Ʈ ���̼��� �ѹ��� �ҷ���
            string jsonStringFromFile = File.ReadAllText(licenseFilePath);
            JsonData client_JsonFile = JsonMapper.ToObject(jsonStringFromFile);
            clientLicenseNumber = client_JsonFile["LicenseNumber"].ToString();
            clientCharactor = client_JsonFile["Charactor"].ToString();
            Debug.Log($"[Client] This client's licensenumber(have) : {clientLicenseNumber}");
            Debug.Log($"[Client] This client's charactor(last charactor) : {clientCharactor}");
        }
#endregion

#region Server-Client Communication
    // ���� ��û - �Ű������� string���� �ް�, requestName���� ��û���� ����
    private void RequestToServer(string requestData)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(requestData);
            stream.Write(data, 0, data.Length); // �����͸� ������ ���� ���? �׳� ������ ���ݾ�
            List<string> requestDataList = requestData.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
            string requestName = requestDataList[0];
            Debug.Log($"[Client] Request to server : {requestName}");

            ReceiveRequestDataFromServer(stream); // �޼��尡 ����ɶ� ���� ��� / ����Ű�� ���� �޼��� �տ� await�� �ٿ��� �����Ű���� �޼��尡 Task �ٿ����ϴµ�?
        }
        catch (Exception e)
        {
            Debug.Log($"[Client] Error Request to server : + {e.Message}");
        }
    }

    // ���� ��û �� Receive, ��������� ������ �޴´� (�񵿱�X, ������ �񵿱��, Ŭ���̾�Ʈ�� �����)
    private void ReceiveRequestDataFromServer(NetworkStream stream)
    {
        try
        {
            List<string> receivedRequestData_List = new List<string>();

            while (true)
            {
                //byte[] buffer = new byte[1024]; // �Ϲ����� ���ۻ�����
                byte[] buffer = new byte[16000000]; // 16MiB ���� ������ 
                int bytesRead = stream.Read(buffer, 0, buffer.Length); // �����͸� �о�ö����� ��� (�����)
                //int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // �����͸� �о�ö����� ��� (�񵿱��)
                string receivedRequestData = Encoding.UTF8.GetString(buffer, 0, bytesRead); // ������ ��ȯ

                receivedRequestData_List.Add(receivedRequestData);
                Debug.Log($"[Client] Receiving... request data from server : {receivedRequestData}");

                // todo �����κ��� ���۵� �����͸� ����� �޾Ҵ��� Ȯ���ϴ°� �ʿ��ϴ�
                // �������� ���û
                // ��Ʈ��ũ ��Ŷ�ս� Ȯ���ϴ� ���
                // requestName���� ��û�� �����͸� ���� �� , E| separtorString ������ �ľ��ؼ� ����� ������ �´��� Ȯ���� Handling�ϰ� ������ ���� ������ �������ϴ� ������?

                // �����κ��� ������ ������ ������(Finish) break;
                List<string> endCheck = receivedRequestData.Split('|').ToList();
                if (endCheck.Contains("Finish"))
                {
                    Debug.Log($"[Client] Received Finish Data from server : {receivedRequestData}");
                    // receivedRequestData�� Finish�� �ִ� ��� Finish�� ����
                    etcMethodHandler.RemoveFinish(receivedRequestData_List, endCheck);
                    //receivedRequestData_List.RemoveAt(receivedRequestData_List.Count - 1);
                    //endCheck.RemoveAt(endCheck.Count - 1);
                    //
                    //string fixLastIndexInList = null;
                    //
                    //for(int i = 0; i < endCheck.Count; i++)
                    //{
                    //    fixLastIndexInList += $"{endCheck[i]}|";
                    //}
                    //
                    //receivedRequestData_List.Add(fixLastIndexInList);

                    Debug.Log($"[Client] Finish Receive Data From Server");
                    break;
                }
            }

            HandleReceivedRequestData(receivedRequestData_List);
        }
        catch (Exception e)
        {
            Debug.Log($"[Client] Error receiving data from server : {e.Message}");
        }
    }
#endregion

#region Handle Data
    // �����κ��� ���� ������ ó��
    private void HandleReceivedRequestData(List<string> dataList)
    {
        // requestname : Ŭ���̾�Ʈ-�������� �����͸� ó���ϱ� ���� ����
        // requestdata : Server���� requestName���� ó���� ����� ���� data
        // LicenseNumber -> clientLicenseNumber�� server�� ������ (Client�� ù ������ ��� �����)
        // LoadNewCharactorData -> �� �÷��̾��� ��� DB�� licenseNumber�� ������ �����Ͱ� �����Ƿ� ��� ���̺� �����ϴ� ���׸� 0���� �ο��� CharactorData�� ���� ����
        // LoadExistCharactorData -> ���� �÷��̾��� ��� DB�� ����� CharactorData�� ���� ����

        // dataList[0] = {requestName}|
        string requestName = dataList[0].Split('|')[0];
        Debug.Log($"[Client] HandleRequestData method, request name : {requestName}");

        switch (requestName)
        {
            case "[Create]LicenseNumber":
                // dataList[1] = {clientLicenseNumber}|{clientCharactor}|
                string tempAllocate = null;
                for (int i = 0; i < dataList.Count; i++)
                {
                    tempAllocate += dataList[i];
                    Debug.Log($"[Client] Check User dataList{i} : {dataList[i]}");
                }
                //clientLicenseNumber = dataList[0].Split('|')[1];
                //clientCharactor = dataList[0].Split('|')[2];
                clientLicenseNumber = tempAllocate.Split('|')[1];
                clientCharactor = tempAllocate.Split('|')[2];
                SaveClientDataToJsonFile();
                Debug.Log($"[Client] RequestName : {requestName}, get and save licenseNumber to jsonfile");
                break;
            case "[Create]Charactor": // todo
                for (int i = 0; i < dataList.Count; i++)
                {
                    Debug.Log($"[Client] Check [Create]Charactor, dateList[{i}] : {dataList[i]}");
                }
                clientCharactor = dataList[0].Split('|')[1];
                Debug.Log($"[Client] RequestName : {requestName}, get new charactor number : {clientCharactor}");
                SaveClientDataToJsonFile();
                break;
            case "[Save]CharactorName":
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Save]CharactorProfile":
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Save]CharactorBirthday":
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Save]CharactorData": // todo
                //clientCharactor = "22";
                SaveClientDataToJsonFile();
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Save]GameResult": // todo
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
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
            case "[Load]UserData":
                HanlelLoadUserData(dataList);
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Load]CharactorData":
                // dataList[0] = "[Load]CharactorData|value|value..|value|E|";
                HandleLoadCharactorData(dataList);
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Load]AnalyticsData":
                // dataList[0] = "[Load]AnalyticsData|value|value|...|value|E|"
                Debug.Log($"[Client] RequestName : {requestName}, dataList[0] : {dataList[0]}");
                HandleLoadAnalyticsData(dataList);
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Load]RankData":
                HandleLoadRankData(dataList);
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Load]ExpenditionCrew":
                HandleLoadExpenditionCrew(dataList);
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Load]LastPlayData":
                HandleLoadLastPlayData(dataList);
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Load]AnalyticsProfileData":
                HandleLoadAnalyticsProfileData(dataList);
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Change]Charactor":
                HandleChangeCharactorData(dataList);
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Delete]Charactor":
                // DB�� �ִ� data�� �����ϸ� �ǹǷ� ���� Ŭ���̾�Ʈ�� ó���� �ʿ� ����
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Reset]CharactorProfile":
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            default:
                Debug.Log("[Client] HandleRequestData Method Something Happend");
                break;
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

    // ����Json���Ͽ� LicenseNumber ��� / �񵿱�� ȣ��� ���� �������� ���������� ȣ���Ű�� ����
    public void PublicSaveClientDataToJsonFile(int charactorNumber)
    {
        // JsonData ����
        JsonData client_Json = new JsonData();
        client_Json["LicenseNumber"] = clientLicenseNumber;
        client_Json["Charactor"] = charactorNumber;

        // Json �����͸� ���ڿ��� ��ȯ�Ͽ� ���Ͽ� ����
        string jsonString = JsonMapper.ToJson(client_Json);
        File.WriteAllText(licenseFolderPath + "/clientlicense.json", jsonString);
    }

    // ���� ������ ó��
    private void HanlelLoadUserData(List<string> dataList)
    {
        Debug.Log("[Client] Handle LoadUserData..");

        #region dataList Format / filterList Format
        // ������ ����
        // dataList[0] = "[Load]UserData|createdCharactorCount|E|CharactorNumber|Name|Profile|E|"
        // dataList[1] = "CharactorNumber|Name|Profile|E|CharactorNumber|Name|Profile|E|"
        // ... dataList[Last] = "CharactorNumber|Name|Profile|E|

        // �ϳ��� string
        // string = "[Load]UserData|createdCharactorCount|E|CharactorNumber|Name|Profile|E|CharactorNumber|Name|Profile|E|CharactorNumber|Name|Profile|E| ... CharactorNumber|Name|Profile|E|";

        // ���ϴ� ����(filter)
        // filterList[0] = createdCharactorCount|
        // filterList[1] = CharactorNumber|Name|Profile|"
        // filterList[2] = CharactorNumber|Name|Profile|"
        // ... dataList[Last] = CharactorNumber|Name|Profile|
        // '|' Split�� �� �����͸� UserData_Value �����ڿ� �����ؼ� ���
        #endregion

        // �ϳ��� string
        string oneData = etcMethodHandler.CreateOneDataToFilter(dataList);

        // Filtering dataList
        List<string> filterList = new List<string>();

        // E|�� ���� �� RequestName ����
        filterList = oneData.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();
        filterList[0] = filterList[0].Substring("[Load]UserData|".Length);

        // clientUserData Initiate
        clientUserData = new UserData();
        clientUserData.user = new List<UserData_Value>();

        // clientUserData�� data �Ҵ�
        for (int i = 0; i < filterList.Count; i++)
        {
            List<string> tempList = filterList[i].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

            if(i == 0) clientUserData.createdCharactorCount = Int32.Parse(tempList[0]);
            else
            {
                int _charactorNumber = Int32.Parse(tempList[0]);
                string _name = tempList[1];
                byte[] _profile = Convert.FromBase64String(tempList[2]);
                UserData_Value userdata_value = new UserData_Value(_charactorNumber, _name, _profile);
                clientUserData.user.Add(userdata_value);
            }
        }
    }

    // �÷��̾� ������ ó��
    private void HandleLoadCharactorData(List<string> dataList)
    {
        Debug.Log("[Client] Handle LoadCharactorData");

        #region dataList Format / filterList Format
        // dataList[0] = "[Load]CharactorData|user_info|value|value..|value|E|";
        // dataList[1] = "rank|value|value|...|value|E|{gameTableName}|value|value|...|value|E|";
        // dataList[2] = "{gameTableName}|value|value|...|value|E|{gameTableName}|value|value|...|value|E|";
        // ... dataList[Last] = "{gameTableName}|value|value|...|value|E|"

        // dataList�� ���͸� ���� �� ���� ����
        // filterList[0] = user_info|name|profile|birthday|totalanswer|totaltime|coin|E|
        // filterList[1] = venezia_kor_level1_step1|ReactionRate|AnswerCount|AnswerRate|Playtime|TotalScore|StarPoint|
        // filterList[2] = venezia_kor_level1_step2|ReactionRate|AnswerCount|AnswerRate|Playtime|TotalScore|StarPoint|
        // filterList[3] = venezia_kor_level1_step3|ReactionRate|AnswerCount|AnswerRate|Playtime|TotalScore|StarPoint|
        // ...
        // filterList[19] = venezia_eng_level1_step1|ReactionRate|AnswerCount|AnswerRate|Playtime|TotalScore|StarPoint|
        // ...
        // filterList[37] = venezia_chn_level_step1|ReactionRate|AnswerCount|AnswerRate|Playtime|TotalScore|StarPoint|
        // ...
        // filterList[43] = calculation_level1_step1|ReactionRate|AnswerCount|AnswerRate|Playtime|TotalScore|StarPoint|
        // ...
        // filterList[61] = gugudan_level1_step1|ReactionRate|AnswerCount|AnswerRate|Playtime|TotalScore|StarPoint|
        // ...
        // filterList[last=79] = gugudan_level3_step6|ReactionRate|AnswerCount|AnswerRate|Playtime|TotalScore|StarPoint|
        #endregion

        // �ϳ��� string
        string oneData = etcMethodHandler.CreateOneDataToFilter(dataList);

        for (int i = 0; i < dataList.Count; i++)
        {
            Debug.Log($"[Client] dataList[{i}] : {dataList[i]}");
        }

        Debug.Log(oneData);

        List<string> filterList = new List<string>();

        // E|�� ���� �� RequestName ����
        filterList = oneData.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();
        filterList[0] = filterList[0].Substring("[Load]CharactorData|".Length);

        // clientPlayerData InitSetting
        clientPlayerData = new Player_DB();

        for (int i = 0; i < filterList.Count; i++)
        {
            List<string> tempList = filterList[i].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

            if (i == 0) // user_info table���� ������ data
            {
                clientPlayerData.playerName = tempList[1];
                clientPlayerData.image = Convert.FromBase64String(tempList[2]); // Convert.FromBase64String(CharactorData_Dic["user_info"][1]);
                clientPlayerData.Day = "";
                clientPlayerData.BirthDay = tempList[3];
                clientPlayerData.TotalAnswers = int.Parse(tempList[4]);
                clientPlayerData.TotalTime = float.Parse(tempList[5]);
                //clientPlayerData.Coin = int.Parser(tempList[6]);
            }
            else // game table���� ������ data
            {
                float reactionRate = float.Parse(tempList[1]);
                int answersCount = Int32.Parse(tempList[2]);
                int answers = Int32.Parse(tempList[3]);
                float playTime = float.Parse(tempList[4]);
                int totalScore = Int32.Parse(tempList[5]);
                int starPoint = Int32.Parse(tempList[6]);

                Data_value datavalue = new Data_value(reactionRate, answersCount, answers, playTime, totalScore, starPoint);

                // i == 1 ���� 80���� 80ȸ �ݺ��� i == 1, 19, 37, 43, 61
                if (i <= 18) // i�� 1~18���� venezia_kor_level1_step1
                {
                    int level = ((i - 1) / 6) + 1;
                    int step = ((i - 1) % 6) + 1;

                    clientPlayerData.Data.Add((Game_Type.A, level, step), datavalue);
                }
                else if(i <=36) // i�� 19~36���� venezia_eng_level1_step1
                {
                    int level = ((i - 1) / 6) - 2;
                    int step = ((i - 1) % 6) + 1;

                    clientPlayerData.Data.Add((Game_Type.B, level, step), datavalue);
                }
                else if(i <= 42) // i�� 37~42���� venezia_chn_level_step1
                {
                    int level = 1;
                    int step = ((i - 1) % 6) + 1;

                    clientPlayerData.Data.Add((Game_Type.C, level, step), datavalue);
                }
                else if(i <= 60) // i�� 43~60���� calculation_level1_step1
                {
                    int level = ((i - 1) / 6) - 6;
                    int step = ((i - 1) % 6) + 1;

                    clientPlayerData.Data.Add((Game_Type.D, level, step), datavalue);
                }
                else // i�� 61���� gugudan_level1_step1
                {
                    int level = ((i - 1) / 6) - 9;
                    int step = ((i - 1) % 6) + 1;

                    clientPlayerData.Data.Add((Game_Type.E, level, step), datavalue);
                }
                
            }
        }

        Debug.Log("[Client] End HandleLoadCharactorData..");
    }

    // �м� ������ ó��
    private void HandleLoadAnalyticsData(List<string> dataList)
    {
        Debug.Log("[Client] Handle LoadAnlayticsData..");

        #region dataList Format / filterList Format
        // ������ ����
        // dataList[0] = "[Load]AnalyticsData|day1|venezia_kor_level1_analytics|ReactionRate|AnswerRate|E|...??"
        // dataList[1] = "day1|venezia_kor_level1_analytics|ReactionRate|AnswerRate|E|??"
        // ... dataList[Last] = "CharactorNumber|Name|Profile|E|

        // Filterd List or Array�� �ؿ�ó�� ���еǾ����
        // filterList[0] = "[Load]AnalyticsData|"day1|venezia_kor_level1_analytics|ReactionRate|AnswerRate|E|"
        // filterList[1] = "day1|venezia_kor_level2_analytics|ReactionRate|AnswerRate|E|"
        // filterList[2] = "day1|venezia_kor_level3_analytics|ReactionRate|AnswerRate|E|"
        // filterList[3] = "day1|venezia_eng_level1_analytics|ReactionRate|AnswerRate|E|"
        // filterList[4] = "day1|venezia_eng_level2_analytics|ReactionRate|AnswerRate|E|"
        // filterList[5] = "day1|venezia_eng_level3_analytics|ReactionRate|AnswerRate|E|"
        // filterList[6] = "day1|venezia_chn_analytics|ReactionRate|AnswerRate|E|"
        // filterList[7] = "day1|calculation_level1_anlaytics|ReactionRate|AnswerRate|E|"
        // filterList[8] = "day1|calculation_level2_anlaytics|ReactionRate|AnswerRate|E|"
        // filterList[9] = "day1|calculation_level3_anlaytics|ReactionRate|AnswerRate|E|"
        // filterList[10] = "day1|gugudan_level1_analytics|ReactionRate|AnswerRate|E|"
        // filterList[11] = "day1|gugudan_level2_analytics|ReactionRate|AnswerRate|E|"
        // filterList[12] = "day1|gugudan_level3_analytics|ReactionRate|AnswerRate|E|"
        // 
        // filterList[78] = "day7|venezia_kor_level1_analytics|ReactionRate|AnswerRate|E|"
        // filterList[79] = "day7|venezia_kor_level2_analytics|ReactionRate|AnswerRate|E|"
        // filterList[80] = "day7|venezia_kor_level3_analytics|ReactionRate|AnswerRate|E|"
        // filterList[81] = "day7|venezia_eng_level1_analytics|ReactionRate|AnswerRate|E|"
        // filterList[82] = "day7|venezia_eng_level2_analytics|ReactionRate|AnswerRate|E|"
        // filterList[83] = "day7|venezia_eng_level3_analytics|ReactionRate|AnswerRate|E|"
        // filterList[84] = "day7|venezia_chn_analytics|ReactionRate|AnswerRate|E|"
        // filterList[85] = "day7|calculation_level1_anlaytics|ReactionRate|AnswerRate|E|"
        // filterList[86] = "day7|calculation_level2_anlaytics|ReactionRate|AnswerRate|E|"
        // filterList[87] = "day7|calculation_level3_anlaytics|ReactionRate|AnswerRate|E|"
        // filterList[88] = "day7|gugudan_level1_analytics|ReactionRate|AnswerRate|E|"
        // filterList[89] = "day7|gugudan_level2_analytics|ReactionRate|AnswerRate|E|"
        // filterList[90] = "day7|gugudan_level3_analytics|ReactionRate|AnswerRate|E|"
        #endregion

        // �����κ��� �޾ƿ� dataList�� �ϳ��� string�� ��Ƽ� ó��
        string oneData = etcMethodHandler.CreateOneDataToFilter(dataList);

        // Filtering dataList
        List<string> filterList = new List<string>();

        // E|�� ���� �� RequestName ����
        filterList = oneData.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();
        filterList[0] = filterList[0].Substring("[Load]AnalyticsData|".Length);

        for (int i = 0; i < filterList.Count; i++)
        {
            Debug.Log($"[Client] AnalyticsData filterList[{i}] : {filterList[i]}");
        }

        // clientAnalyticsData InitSetting
        clientAnalyticsData = new AnalyticsData();

        // clientAnalyticsData�� ��������� Data�� Value �߰�
        etcMethodHandler.AddClientAnalyticsDataValue(filterList, clientAnalyticsData);

        Debug.Log($"[Client] Check clientAnalyticsData.Data.Count : {clientAnalyticsData.Data.Count}");

        int days = 7;
        int games = 5;
        int levels = 3;

        for(int i = 0; i < days; i++)
        {
            for(int j = 0; j < games; j++)
            {
                for(int k = 0; k < levels; k++)
                {
                    Debug.Log($"[Client] Check clientAnalyticsData.Data.Value(reactionRate), i,j,k : {i},{j}, {k} : {clientAnalyticsData.Data[(i + 1, (Game_Type)j, k + 1)].reactionRate}");
                }
            }
        }
        //clientAnalyticsData.Data[(1, 0, 1)].answerRate;

        Debug.Log("[Client] End HandleLoadAnalyticsData..");
    }

    // ��ũ ������ ó��
    private void HandleLoadRankData(List<string> dataList)
    {
        Debug.Log("[Client] Handle LoadRankData..");

        #region dataList Format / filterList Format
        // ������ ���� == dataList
        // dataList[0] = "[Load]RankData|profile|name|score|E|profile|name|totalScore|E|";
        // dataList[1] = profile|name|totalScore|E|profile|name|totalScore|E|"profile|name|totalScore|E| ??

        // ���ϴ� ���� == filterList
        // filterList[0] = "[Load]RankData|profile|name|score|E|";
        // filterList[1] = "profile|name|totalScore|E|
        // filterList[2] = profile|name|totalScore|E|
        // ...
        // filterList[6] = profile|name|totalScore|scorePlace|highestScorePlace|E|
        // filterList[7] = profile|name|totalTime|E|
        // ...
        // filterList[last] = profile|name|totalTime|timePlace|highestTimePlace|E|
        #endregion

        // �ϳ��� string
        string oneData = etcMethodHandler.CreateOneDataToFilter(dataList);

        // Filtering dataList
        List<string> filterList = new List<string>();

        // E| ���� �� RequestName ����
        filterList = oneData.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();
        filterList[0] = filterList[0].Substring("[Load]RankData|".Length);

        // clientRankData InitSetting
        clientRankData = new RankData();
        clientRankData.rankdata_score = new RankData_value[6];
        clientRankData.rankdata_time = new RankData_value[6];

        // clientRankData�� data �Ҵ�
        for (int i = 0; i < filterList.Count; i++)
        {
            List<string> part = filterList[i].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

            if(i < 5) // score 1~5 ��
            {
                clientRankData.rankdata_score[i].userProfile = Convert.FromBase64String(part[0]);
                clientRankData.rankdata_score[i].userName = part[1];
                clientRankData.rankdata_score[i].totalScore = Int32.Parse(part[2]);
            }
            else if(i == 5) // Ŭ���̾�Ʈ score
            {
                clientRankData.rankdata_score[i].userProfile = Convert.FromBase64String(part[0]);
                clientRankData.rankdata_score[i].userName = part[1];
                clientRankData.rankdata_score[i].totalScore = Int32.Parse(part[2]);
                clientRankData.rankdata_score[i].scorePlace = Int32.Parse(part[3]);
                clientRankData.rankdata_score[i].highestScorePlace = Int32.Parse(part[4]);
            }
            else if(i < 11) // time 1~5��
            {
                clientRankData.rankdata_time[i - 6].userProfile = Convert.FromBase64String(part[0]);
                clientRankData.rankdata_time[i - 6].userName = part[1];
                clientRankData.rankdata_time[i - 6].totalTime = float.Parse(part[2]);
            }
            else // Ŭ���̾�Ʈ time
            {
                clientRankData.rankdata_time[i - 6].userProfile = Convert.FromBase64String(part[0]);
                clientRankData.rankdata_time[i - 6].userName = part[1];
                clientRankData.rankdata_time[i - 6].totalTime = float.Parse(part[2]);
                clientRankData.rankdata_time[i - 6].timePlace = Int32.Parse(part[3]);
                clientRankData.rankdata_time[i - 6].highestTimePlace = Int32.Parse(part[4]);
            }
        }

        Debug.Log("[Client] End HandleLoadRankData..");
    }

    // Ž���� ó��
    private void HandleLoadExpenditionCrew(List<string> dataList)
    {
        Debug.Log("[Client] HandleLoadExpenditionCrew..");

        #region dataList Format / filterList Format
        // dataList[0] = "[Load]ExpenditionCrew|LastSelectCrew|crew1|crew2|...|crew(n)|"

        // ����ϰ��� �ϴ� List ����
        // filterList[0] = [Load]ExpenditionCrew|LastSelectCrew|crew1|crew2|...|crew(n)|
        #endregion

        // �ϳ��� string
        string oneData = etcMethodHandler.CreateOneDataToFilter(dataList);

        // filterList
        List<string> filterList = new List<string>();

        // E| ���� �� RequestName ����
        filterList = oneData.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();
        filterList[0] = filterList[0].Substring("[Load]ExpenditionCrew|".Length);

        for (int i = 0; i < filterList.Count; i++)
        {
            Debug.Log($"[Client] ExpenditionCrew filterList[{i}] : {filterList[i]}");
        }

        // ExpenditionCrew's member variable
        int SelectedCrew; //������ ���
        List<bool> OwnedCrew = new List<bool>(); //������ ��� ����Ʈ

        List<string> tempList = filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

        SelectedCrew = Int32.Parse(tempList[0]);

        bool haveThatCrew;
        for (int i = 1; i < tempList.Count; i++)
        {
            if (tempList[i] == "0") haveThatCrew = false;
            else haveThatCrew = true;

            OwnedCrew.Add(haveThatCrew);
        }

        // clientExpenditionCrew Initiate and Setting
        clientExpenditionCrew = new ExpenditionCrew(SelectedCrew, OwnedCrew);

        Debug.Log("[Client] End HandleLoadExpendtionCrew...");
    }

    // ������ �÷��� ���� ó��
    private void HandleLoadLastPlayData(List<string> dataList)
    {
        Debug.Log("[Client] Come in HandleLoadLastPlayData..");

        #region dataList Format / filterList Format
        // dataList[0] = [Load]LastPlayData|(game1_level1)�� value|(game1_level2)�� value| ... |(game5_level3)�� value|
        // dataList[1] = ... |(game5_level3)�� value| ??

        // filterList ����
        // filterList[0] = [Load]LastPlayData|(game1_level1)�� value|(game1_level2)�� value| ... |(game5_level3)�� value|

        // tempList
        // tempList[0] = (game1_level1)�� value
        // tempList[1] = (game1_level2)�� value
        // ...
        // tempList[n] = (game5_level3)�� value
        #endregion

        // �ϳ��� string
        string oneData = etcMethodHandler.CreateOneDataToFilter(dataList);

        // filterList
        List<string> filterList = new List<string>();

        // E| ���� �� RequestName ����
        filterList = oneData.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();
        filterList[0] = filterList[0].Substring("[Load]LastPlayData|".Length);

        List<string> tempList = filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

        for (int i = 0; i < tempList.Count; i++)
        {
            Debug.Log($"[Client] LastPlayData tempList[{i}] : {tempList[i]}");
        }

        // clientLastPlayData Initiate
        clientLastPlayData = new LastPlayData();

        // clientLastPlayData Setting
        for(int i = 0; i < tempList.Count; i++)
        {
            if (i < 3) clientLastPlayData.Step.Add((Game_Type.A, i + 1), Int32.Parse(tempList[i])); // venezia_kor
            else if (i < 6) clientLastPlayData.Step.Add((Game_Type.B, (i + 1) - 3), Int32.Parse(tempList[i])); // venezia_eng
            else if (i == 6) clientLastPlayData.Step.Add((Game_Type.C, (i + 1) - 6), Int32.Parse(tempList[i])); // venezia_chn
            else if (i < 10) clientLastPlayData.Step.Add((Game_Type.D, (i + 1) - 7), Int32.Parse(tempList[i])); // calculation
            else clientLastPlayData.Step.Add((Game_Type.E, (i + 1) - 10), Int32.Parse(tempList[i])); // gugudan;
        }

        Debug.Log("[Client] End Handle LoadLastPlayData...");
    }

    // �����ʿ� �м������� ó��
    private void HandleLoadAnalyticsProfileData(List<string> dataList)
    {
        Debug.Log("[Client] Come in HandleLoadAnalyticsProfileData..");

        // ������ dataList
        // dataList[0] = [Load]AnalyticsProfileData|Level1_���Ӹ�|Level1_��չ����ӵ�|Level1_��������|
        //                + Level2_���Ӹ�|Leve2_��չ����ӵ�|Level2_��������|Level3_���Ӹ�|Leve3_��չ����ӵ�|Level3_��������|

        // filterList ����
        // filterList[0] = [Load]AnalyticsProfileData|Level1_���Ӹ�|Level1_��չ����ӵ�|Level1_��������|Level2_���Ӹ�|Leve2_��չ����ӵ�|Level2_��������|Level3_���Ӹ�|Leve3_��չ����ӵ�|Level3_��������|

        // tempList ����
        // tempList[0] = Level1_���Ӹ�
        // tempList[1] = Level1_��չ����ӵ�
        // tempList[2] = Level1_��������
        // ...
        // tempList[n] = Level3_��������

        // �ϳ��� string
        string oneData = etcMethodHandler.CreateOneDataToFilter(dataList);

        // filterList
        List<string> filterList = new List<string>();

        // E| ���� �� RequestName ����
        filterList = oneData.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();
        filterList[0] = filterList[0].Substring("[Load]ExpenditionCrew|".Length);

        List<string> tempList = filterList[0].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

        for (int i = 0; i < tempList.Count; i++)
        {
            Debug.Log($"[Client] AnalyticsProfileData tempList[{i}] : {tempList[i]}");
        }

        // clientAnalyticsProfileData Initiate
        clientAnalyticsProfileData = new AnalyticsProfileData();

        // clientAnalyticsProfileData Setting
        for (int i = 0; i < tempList.Count; i++)
        {
            if (i % 3 == 0)
            {
                string mostPlayedGame = tempList[i];
                float reactionRate = float.Parse(tempList[i + 1]);
                int answerRate = Int32.Parse(tempList[i + 2]);
                clientAnalyticsProfileData.Data[i / 3] = new Tuple<string, float, int>(mostPlayedGame, reactionRate, answerRate);
            }
        }

        Debug.Log("[Client] End HandleLoadAnalyticsProfileData..");
    }

    // ĳ���� ����� ������ ó�� - clientCharactor�� ����
    private void HandleChangeCharactorData(List<string> dataList)
    {
        Debug.Log("[Client] HandleChangeCharactorData..");

        for (int i = 0; i < dataList.Count; i++)
        {
            Debug.Log($"[Client] ChangeCharactorData[{i}] : {dataList[i]}");
        }

        // dataList[0] = [Change]Charactor|ChangeCharctorNumber|
        List<string> filterList = dataList[0].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

        // requestName ����
        filterList.RemoveAt(0);

        // clientCharactor ����
        clientCharactor = filterList[0];

        Debug.Log("[Client] End ChangeCharactorData..");
    }
#endregion

#region Ŭ���̾�Ʈ-������û

    /*
    Client�� DB�� �ִ� ������ Save Load �ϴ� ���
    1. �� ���� -> UserData Load (User Charactors) // Only DB
    1. �� ���� -> Player_DB Load (Charactor Info, GameData) // DB <-> Client
    1. �� ���� -> AnalyticsData Load (���� �м�ǥ) // Only DB
    1. �� ���� -> RankData Load (��ŷ) // Only DB
    1. �� ���� -> ExpenditionCrew Load (Ž����) // DB <-> Client
    1. �� ���� -> LastPlay Load (���������� �� ����) // DB <-> Client
    1. �� ���� -> AnalyticsProfileData Load (�����ʿ� �м�ǥ) // Only DB
    ------------------------------------------------------------------------------------
    2. �÷��̾�(charactor) ���� -> Save, Load
    2-1. Player_DB Save (���� CharactorData)
    2-1. ExpendtionCrew Save (���� CharactorData)
    2-1. LastPlay Save (���� CharactorData)
    2-2. New Charactor Create (�� ChractorData ����)
    2-3. UserData Load (�� CharactorData ��������)
    2-3. Player_DB Load (�� CharactorData)
    2-3. GameAnalytics Load (�� CharactorData)
    2-3. RankData Load (�� CharactorData)
    2-3. ExpenditionCrew Load (�� CharactorData)
    2-3. LastPlay Load (�� CharactorData)
    2-3. AnalyticsProfileData Load (�� CharactorData)
    -------------------------------------------------------------------------------------
    3. �÷��̾�(Charactor) ���� -> Save, Load
    3-1. Player_DB Save (���� CharactorData)
    3-1. ExpendtionCrew Save (���� CharactorData)
    3-1. LastPlay Save (���� CharactorData)
    3-2. UserData Load (���� CharactorData ��������)
    3-2. Player_DB Load (���� CharactorData)
    3-2. GameAnalytics Load (���� CharactorData)
    3-2. RankData Load (���� CharactorData)
    3-2. ExpenditionCrew Load (���� CharactorData)
    3-2. LastPlay Load (���� CharactorData)
    3-2. AnalyticsProfileData (���� CharactorData)
    ---------------------------------------------------------------------------------------
    4. �÷��̾�(Charactor) ���� -> UserData Save(Update) and Load
    5. Charactor Name ���
    6. Charactor Profile ���
    7. Charactor Birthday ���
    8. ���� ������ �� Game Data(reactionrate, score, time ��) DB�� ����
    9. Reset Charactor Profile -> PlayerDB, AnalyticsProfileData Load
    ---------------------------------------------------------------------------------------
    10. �� ���� -> Player_DB Save 
    10. �� ���� -> ExpenditionCrew Save 
    10. �� ���� -> LastPlay Save
    
    7. �� ���� -> ������ ������ Charactor - LitJson���Ϸ� ���� ����(clientlicense)

##. �� ���� ���� -> TotalScore(int��) Load (game_type, level, step ����) -> Save GameData
    --. ��ŷ UI ���� ��(or ��ŷ���ΰ�ħ) -> rank Load  // �ϴ� �׽�Ʈ��
    */

#region �� ���۽�
    // �� ���۽� UserData Load
    public UserData AppStart_LoadUserDataFromDB()
    {
        string requestData = $"[Load]UserData|{clientLicenseNumber}|Finish";
        RequestToServer(requestData);

        return clientUserData;
    }

    // �� ���۽� Player_DB Load
    public Player_DB AppStart_LoadAllDataFromDB()
    {
        //Player_DB playerDB = new Player_DB();

        string requestData = $"[Load]CharactorData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        //playerDB = clientPlayerData;

        return clientPlayerData;
    }

    // �� ���۽� AnalyticsData Load
    public AnalyticsData AppStart_LoadAnalyticsDataFromDB()
    {
        //AnalyticsData return_AnalyticsData = new AnalyticsData();

        string requestData = $"[Load]AnalyticsData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        //return_AnalyticsData = ;

        return clientAnalyticsData;
    }

    // �� ���۽� RankData Load
    public RankData AppStart_LoadRankDataFromDB()
    {
        //RankData return_RankData = new RankData();

        string requestData = $"[Load]RankData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        //return_RankData = clientRankData;

        return clientRankData;
    }

    // �� ���۽� ExpenditionCrew Load
    public ExpenditionCrew AppStart_LoadExpenditionCrewFromDB()
    {
        string requestData = $"[Load]ExpenditionCrew|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientExpenditionCrew;
    }

    // �� ���۽� LastPlay Load
    public LastPlayData AppStart_LoadLastPlayFromDB()
    {
        string requestData = $"[Load]LastPlayData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientLastPlayData;
    }

    // �� ���۽� AnalyticsProfileData Load
    public AnalyticsProfileData AppStart_LoadAnalyticsProfileDataFromDB()
    {
        string requestData = $"[Load]AnalyticsProfileData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientAnalyticsProfileData;
    }

#endregion

#region ĳ���� ����
    // Charactor ������ Player_DB Save (���� CharactorData)
    public void CreateCharactor_SaveCharactorDataToDB(Player_DB playerdb)
    {
        SaveCharactorDataToDB(playerdb);
    }

    // Charactor ������ ExpenditionCrew Save (���� CharactorData)
    public void CreateCharactor_SaveExpenditionCrewDataToDB(ExpenditionCrew crew)
    {
        SaveExpenditionCrewDataToDB(crew);
    }

    // Charactor ������ LastPlay Save (���� CharactorData)
    public void CreateCharactor_SaveLastPlayDataToDB(LastPlayData lastplaydata)
    {
        SaveLastPlayDataToDB(lastplaydata);
    }

    // Charactor ���� ��û -> Server���� ���� ������ �޾��� �� clientCharactor -> ������ charactor��ȣ�� ����
    public void CreateCharactorData()
    {
        string reqeustData = $"[Create]Charactor|{clientLicenseNumber}|{clientCharactor}|Finish";

        RequestToServer(reqeustData);
    }

    // Charactor ������ UserData Load (�� CharactorData ��������)
    public UserData CreateCharactor_LoadUserDataFromDB()
    {
        string requestData = $"[Load]UserData|{clientLicenseNumber}|Finish";
        RequestToServer(requestData);

        return clientUserData;
    }

    // Charactor ������ Player_DB Load (�� CharactorData)
    public Player_DB CreateCharactor_LoadCharactorDataFromDB()
    {
        string requestData = $"[Load]CharactorData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientPlayerData;
    }

    // Charactor ������ GameAnalytics Load (�� CharactorData)
    public AnalyticsData CreateCharactor_LoadAnalyticsDataFromDB()
    {
        string requestData = $"[Load]AnalyticsData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientAnalyticsData;
    }

    // Charactor ������ RankData Load (�� CharactorData)
    public RankData CreateCharactor_LoadRankDataFromDB()
    {
        string requestData = $"[Load]RankData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientRankData;
    }

    // Charactor ������ ExpenditionCrew Load (�� CharactorData) 
    public ExpenditionCrew CreateCharactor_LoadExpenditionCrewFromDB()
    {
        string requestData = $"[Load]ExpenditionCrew|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientExpenditionCrew;
    }

    // Charactor ������ LastPlay Load (�� CharactorData) 
    public LastPlayData CreateCharactor_LoadLastPlayFromDB()
    {
        string requestData = $"[Load]LastPlayData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientLastPlayData;
    }

    // Charactor ������ AnalyticsProfileData Load (�� CharactorData)
    public AnalyticsProfileData CreateCharactor_LoadAnalyticsProfileDataFromDB()
    {
        string requestData = $"[Load]AnalyticsProfileData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientAnalyticsProfileData;
    }
#endregion

#region ĳ���� ����
    // Charactor ����� Player_DB Save (���� CharactorData)
    public void ChangeCharactor_SaveCharactorDataToDB(Player_DB playerdb)
    {
        SaveCharactorDataToDB(playerdb);
    }

    // Charactor ����� ExpenditionCrew Save (���� CharactorData)
    public void ChangeCharactor_SaveExpenditionCrewDataToDB(ExpenditionCrew crew)
    {
        SaveExpenditionCrewDataToDB(crew);
    }

    // Charactor ����� LastPlay Save (���� CharactorData)
    public void ChangeCharactor_SaveLastPlayDataToDB(LastPlayData lastplaydata)
    {
        SaveLastPlayDataToDB(lastplaydata);
    }

    // Charactor ���� ��û -> Server���� ���� ������ �޾��� �� clientCharactor -> ������ charactor��ȣ�� ����, todo
    public void ChangeCharactorData(int changeCharactor)
    {
        string reqeustData = $"[Change]Charactor|{clientLicenseNumber}|{changeCharactor}|Finish";

        RequestToServer(reqeustData);
    }

    // Charactor ����� UserData Load (���� CharactorData ��������)
    public UserData ChangeCharactor_LoadUserDataFromDB()
    {
        string requestData = $"[Load]UserData|{clientLicenseNumber}|Finish";
        RequestToServer(requestData);

        return clientUserData;
    }

    // Charactor ����� Player_DB Load (���� CharatrorData)
    public Player_DB ChangeCharactor_LoadCharactorDataFromDB()
    {
        string requestData = $"[Load]CharactorData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientPlayerData;
    }

    // Charactor ����� GameAnalytics Load (���� CharactorData)
    public AnalyticsData ChangeCharactor_LoadAnalyticsDataFromDB()
    {
        string requestData = $"[Load]AnalyticsData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientAnalyticsData;
    }

    // Charactor ����� RankData Load (���� CharactorData)
    public RankData ChangeCharactor_LoadRankDataFromDB()
    {
        string requestData = $"[Load]RankData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientRankData;
    }

    // Charactor ����� ExpenditionCrew Load (���� CharactorData) 
    public ExpenditionCrew ChangeCharactor_LoadExpenditionCrewFromDB()
    {
        string requestData = $"[Load]ExpenditionCrew|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientExpenditionCrew;
    }

    // Charactor ����� LastPlay Load (���� CharactorData) 
    public LastPlayData ChangeCharactor_LoadLastPlayFromDB()
    {
        string requestData = $"[Load]LastPlayData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientLastPlayData;
    }

    // Charactor ����� AnalyticsProfileData Load
    public AnalyticsProfileData ChangeCharactor_LoadAnalyticsProfileDataFromDB()
    {
        string requestData = $"[Load]AnalyticsProfileData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return clientAnalyticsProfileData;
    }
#endregion

    // Charactor Delete
    public void DeleteCharactor(int deleteCharactor)
    {
        string requestData = $"[Delete]Charactor|{clientLicenseNumber}|{deleteCharactor}|Finish";

        RequestToServer(requestData);
    }

    // Charactor ���� �� UserData Load
    public UserData DeleteCharactor_LoadUserData()
    {
        string requestData = $"[Load]UserData|{clientLicenseNumber}|Finish";
        RequestToServer(requestData);

        return clientUserData;
    }

    // Charactor Name ���
    public void RegisterCharactorName_SaveDataToDB(string name)
    {
        // DB�� ������ �� user_licensenumber�� user_charactor�� value�� �´� row�� �־�� �ϹǷ� �� ������ �ʿ�
        // requestData = ��û����|���̼���|ĳ���͹�ȣ|{name}|
        string requestData = $"[Save]CharactorName|{clientLicenseNumber}|{clientCharactor}|{name}|Finish";

        RequestToServer(requestData);
    }

    // Charactor Profile ���
    public void RegisterCharactorProfile_SaveDataToDB(byte[] image)
    {
        /*
         string base64 = "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";
        byte[] profile = Convert.FromBase64String(base64);

        RegisterCharactorProfile_SaveDataToDB(profile);
         */


        // DB�� ������ �� user_licensenumber�� user_charactor�� value�� �´� row�� �־�� �ϹǷ� �� ������ �ʿ�
        // requestData = ��û����|���̼���|ĳ���͹�ȣ|{image}|

        // byte[] image -> Base64 ���ڿ��� ��ȯ
        string imageBase64 = Convert.ToBase64String(image);
        string requestData = $"[Save]CharactorProfile|{clientLicenseNumber}|{clientCharactor}|{imageBase64}|Finish";

        RequestToServer(requestData);
    }

    // Charactor Birthday ���
    public void RegisterCharactorBirthday_SaveDataToDB(string birthday)
    {
        string requestData = $"[Save]CharactorBirthday|{clientLicenseNumber}|{clientCharactor}|{birthday}|Finish";

        RequestToServer(requestData);
    }

    // ���� ���۽� TotalScore Load
    public int AppGame_LoadTotalScoreFromDB(Game_Type game_type, int level, int step)
    {

        return 0;
    }

    // ����̰� ���� Result_Data�� �Ű������� �޾Ƽ� DB�� �����ϴ� �޼���(server�� ��û -> RequestServer)
    public void AppGame_SaveResultDataToDB(Player_DB resultdata, Game_Type game_type, int level, int step)
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

        requestData = $"{requestName}|{values}Finish";

        RequestToServer(requestData);
    }

    // ĳ���� ������ �Ϻ� ������ �ʱ�ȭ
    public void ResetCharactorProfile()
    {
        string requestData = $"[Reset]CharactorProfile|{clientLicenseNumber}|{clientCharactor}|Finish";

        RequestToServer(requestData);
    }

    // ��ŷ UI ���� �� Rank Load
    public RankData Ranking_LoadRankDataFromDB()
    {
        RankData return_RankData = new RankData();

        string requestData = $"[Load]RankData|{clientLicenseNumber}|{clientCharactor}|Finish";
        RequestToServer(requestData);

        return_RankData = clientRankData;

        return return_RankData;
    }

    // �� ����� Player_DB Save
    public void AppExit_SaveCharactorDataToDB(Player_DB playerdb)
    {
        SaveCharactorDataToDB(playerdb);
    }

    // �� ����� ExpendtionCrew Save
    public void AppExit_SaveExpenditionCrewDataToDB(ExpenditionCrew crew)
    {
        SaveExpenditionCrewDataToDB(crew);
    }

    // �� ����� LastPlay Save
    public void AppExit_SaveLastPlayDataToDB(LastPlayData lastplaydata)
    {
        SaveLastPlayDataToDB(lastplaydata);
    }

#endregion

#region �ߺ� ��� Method (Save)
    // Save Charactor(Player_DB) Data To DB
    private void SaveCharactorDataToDB(Player_DB playerdb)
    {
        string requestData;
        string requestName = $"[Save]CharactorData";
        requestData = $"{requestName}|{clientLicenseNumber}|{clientCharactor}|{separatorString}";

        string base64 = "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";
        string imagebase64 = Convert.ToBase64String(playerdb.image);

        /*
         list.Add("user_info");
        list.Add("rank");
        list.Add("crew");
        list.Add("lastplaygame");

        // AnalyticsProfile table
        list.Add("analyltics_level1_profile");
        list.Add("analyltics_level2_profile");
        list.Add("analyltics_level3_profile");
         */

        // table.list[0] == user_info
        requestData += $"{table.list[0]}|{playerdb.playerName}|{imagebase64}|{playerdb.BirthDay}|{playerdb.TotalAnswers}|{playerdb.TotalTime}|{separatorString}";
        //requestData += $"{table.list[0]}|{separatorString}";
        // table.list[1] == rank
        //requestData += $"{table.list[1]}|{}";
        // table.list[2] == crew
        //requestData += $"{}";
        // table.list[3] == lastplaygame
        //requestData += $"{}";
        // table.list[4] == analyltics_level1_profile
        // table.list[5] == analyltics_level2_profile
        // table.list[6] == analyltics_level3_profile
        // table.list[7~n] == gametable
        // gametable's column : reactionrate/answercount/answerrate/playtime/totalscore/starpoint
        List<float> reactionRate_List = new List<float>();
        List<int> answersCount_List = new List<int>();
        List<int> answers_List = new List<int>();
        List<float> playTime_List = new List<float>();
        List<int> totalScore_List = new List<int>();
        List<int> starCount_List = new List<int>();

        ICollection<Data_value> allvalues = playerdb.Data.Values;
        foreach (Data_value datavalue in allvalues)
        {
            reactionRate_List.Add(datavalue.ReactionRate);
            answersCount_List.Add(datavalue.AnswersCount);
            answers_List.Add(datavalue.Answers);
            playTime_List.Add(datavalue.PlayTime);
            totalScore_List.Add(datavalue.TotalScore);
            starCount_List.Add(datavalue.StarCount);
        }

        for (int i = 0; i < reactionRate_List.Count; i++)
        {
            Debug.Log($"reactionRate_List[i] : {reactionRate_List[i]}, count : {i}");
        }

        // gameTable(i=4����)
        for (int i = 7; i < table.list.Count; i++)
        {
            requestData += $"{table.list[i]}|{reactionRate_List[i - 7]}|{answersCount_List[i - 7]}|{answers_List[i - 7]}|{playTime_List[i - 7]}|{totalScore_List[i - 7]}|{starCount_List[i - 7]}|{separatorString}";
        }
        requestData += "Finish";
        Debug.Log($"[Client] before request to server savedata {requestData}");

        RequestToServer(requestData);
    }

    // Save ExpendtionCrew Data To DB
    private void SaveExpenditionCrewDataToDB(ExpenditionCrew crew)
    {
        //requestData = [Save]ExpenditionCrew|license|charactor|LastSelectCrew|Crew1|Crew2|... |Crew(n)|Finish
        string requestName = "[Save]ExpenditionCrew";
        string requestData = $"{requestName}|{clientLicenseNumber}|{clientCharactor}|";

        // SelectedCrew �߰�
        requestData += $"{crew.SelectedCrew}|";

        // True(����) -> 1 / False(�̺���) -> 0
        for(int i = 0; i < crew.OwnedCrew.Count; i++)
        {
            if (crew.OwnedCrew[i]) requestData += "1|";
            else requestData += "0|";
        }

        requestData += "Finish";

        RequestToServer(requestData);
    }

    // Save LastPlay Data To DB
    private void SaveLastPlayDataToDB(LastPlayData lastplaydata)
    {
        //requestData = [Save]LastPlayData|license|charactor| (game1_level1)�� value | (game1_level2)�� value | ... | (game5_level3)�� value |Finish
        string requestName = "[Save]LastPlayData";
        string requestData = $"{requestName}|{clientLicenseNumber}|{clientCharactor}|";

        for(int i = 0; i < lastplaydata.Step.Values.Count; i++)
        {
            if (i < 3) requestData += $"{lastplaydata.Step[(Game_Type.A, i + 1)]}|"; // venezia_kor
            else if (i < 6) requestData += $"{lastplaydata.Step[(Game_Type.B, (i + 1) - 3)]}|"; // venezia_kor
            else if (i == 6) requestData += $"{lastplaydata.Step[(Game_Type.C, (i + 1) - 6)]}|"; // venezia_chn
            else if (i < 10) requestData += $"{lastplaydata.Step[(Game_Type.D, (i + 1) - 7)]}|"; // calculation
            else requestData += $"{lastplaydata.Step[(Game_Type.E, (i + 1) - 10)]}|"; // gugudan
        }

        requestData += "Finish";

        RequestToServer(requestData);
    }

#endregion

#region TestMethods
    public void OnClickSaveGameDataTest()
    {
        Game_Type game_Type = Game_Type.A;
        int level = 1;
        int step = 2;
        Player_DB result_DB = new Player_DB();
        Data_value data_Value = new Data_value(234.21f, 23, 12, 22.44f, 18000,3);
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

    public void OnClickLoadCharactorDataFromServerTest()
    {
        Debug.Log($"[Client] Test... {CharactorData_FromServer}");
        for (int i = 0; i < CharactorData_FromServer.Count; i++)
        {
            Debug.Log($"CharactorData_FromServer[{i}]'s value : {CharactorData_FromServer[i]}");
        }
    }

    public void OnClickLoadFilterCharactorDataTest()
    {
        foreach (KeyValuePair<string, List<string>> item in CharactorData_Dic)
        {
            Debug.Log($"item.Key : {item.Key}, item.Value : {item.Value}");
            foreach (string value in item.Value)
            {
                Debug.Log($"item.Key : {item.Key}, value(string) in item.Value : {value}");
            }
        }
    }

    public void OnClickRanking_LoadRankDataTest()
    {
        RankData rankDataFromDB = Ranking_LoadRankDataFromDB();
    }

    public void OnClickLoadRankDataTest()
    {
        for(int i = 0; i< clientRankData.rankdata_score.Length; i++)
        {
            Debug.Log($"clientRankData.rankdata_score[i].place : {clientRankData.rankdata_score[i].scorePlace}");
            Debug.Log($"clientRankData.rankdata_score[i].userProfile : {clientRankData.rankdata_score[i].userProfile}");
            Debug.Log($"clientRankData.rankdata_score[i].userName : {clientRankData.rankdata_score[i].userName}");
            Debug.Log($"clientRankData.rankdata_score[i].totalScore : {clientRankData.rankdata_score[i].totalScore}");
        }

        for (int i = 0; i < clientRankData.rankdata_time.Length; i++)
        {
            Debug.Log($"clientRankData.rankdata_time[i].place : {clientRankData.rankdata_time[i].timePlace}");
            Debug.Log($"clientRankData.rankdata_time[i].userProfile : {clientRankData.rankdata_time[i].userProfile}");
            Debug.Log($"clientRankData.rankdata_time[i].userName : {clientRankData.rankdata_time[i].userName}");
            Debug.Log($"clientRankData.rankdata_time[i].totalScore : {clientRankData.rankdata_time[i].totalTime}");
        }

    }

    public void OnClickSaveNameTest()
    {
        string name = "���ȫ";

        RegisterCharactorName_SaveDataToDB(name);
    }

    public void OnClickSaveProfileTest()
    {
        string base64 = "/9j/4AAQSkZJRgABAQAAAQABAAD/2wCEAAkGBxMSERUTExMVFhUVFRcXGBgXFRUVGhodFhUXGBUXFxgYHSggGBolGxcaIjEhJSkrLi4uFx8zODMtNygtLisBCgoKDg0OFxAQFy0eHyUtLS0tLS0tLS0tLS0tLS0tLS0tNy0tLS0rLS0tKystKy03Ky0rKy0tLS0rLS0tLS0tK//AABEIAK8BIAMBIgACEQEDEQH/xAAcAAEBAAIDAQEAAAAAAAAAAAAAAQUGAwQHAgj/xAA+EAABAgQEAgYJBAIBAwUAAAABABECITFBAwRRYTJCBQYScZHwBxMiYoGCoaLBUnKxshQj0TNDkiREU+Hx/8QAFgEBAQEAAAAAAAAAAAAAAAAAAAEC/8QAGxEBAQEAAwEBAAAAAAAAAAAAAAERAiFBMRL/2gAMAwEAAhEDEQA/APanm/NoglSZNdkm7c2qDat90EFGHDcoaMeGxQUlw3CGjnhsEFO8mpujzfm0Q7/Lt5kk3bmsUAG4rcKCVJg1Oioe3FcqDal0Czcuqp3kBTdSz8uip3pZAJua2CPN+a4Qu8+KxSbsOK58/BAEqTeuylm5dVRt8d1LPy6IBoxpYqkvWRFBqoaT4bBUvetkB5vzaIC1Jk12SbtzaoNq3QQUYcNylm5dUFJcNwho54bBBTOsmpujzfmsEO/y7eZJN25rHz8UAG4rcKDaYNdlQ9uK5UG1LoFm5dUO8gKbpZ+XRDvSyCk3NbBHm/Nohe/FYoHdua58/BAEqTeuylm5dVRt82/mago44bhANGPDYqmdZEU3UNJ8Ngqd62QHm/NogLTEzcaJdubVA9q3QQUYTFylm5dUFJcNwln5dEFa33Iz7NfVSXyp305UFd5021R712Tv4rJ3cV0Cm7/Tz+Ea33KDb5vPikvlugrPKjX1Ss6NbVQtfhsqd62QN/tSm720T+3n8INq3QGtV76I1vrr5/Cga3DdJX4befFBa7N9Ue/2od/l8+Cf2sgUnV7aIzSq99EG3FdQNal0Fa33Izyo19VJfKh3pZBXedGtqm/2p38Vk7uK6BTd/p5/CNb7vPmag2+bz4pL5b+fBBWeVGvqlZ0a2qkr8NlTvWyBv9qUnV7aJ/bz+EG1boDNKr30Rrfd58yUDW4bpL5befFBa7N9fP5R7/aod/l8+CvfxWQSk6vbRVmlV76J3cV1BtS6Ctb7kZ5Ua+qkvlQtelkFrOjW1R7/AGp38Vk/sgj35dEdqzBpsq83vogLUm9dkDY1sU2HFcqAMGEwalCJNbVArSTV3R78uipnWTU3R5vfRBHaZoaBUykZk0OiAtMTJqNFBKQmDU6ILtzaoJ0kRXdSzW1QzrICm6ADcUFQj35bBUmbmooEeb30QDKs3psm3NqglSb12UaTW1QWshUVKgLzEgKjVDMMZAUOqpLzMiKDVBHvy6I7TMwabKvN76IC0xMmo0QKSNTQptzaqAMGEwalGk1tUFE6Sau6j35dFTOsmpujze+iCO0zQ0CplIzJpsgLTFTUaKEgA6XOmqC7c2qCchIiu66fRnSmBmIO1gY2Hi4YLGPDjhjAIsWMiu2ZyMgKHVABeYoKhHvy6LW+t/W7DybQBo8zGHgw3lCP/kxTywO+8RDC5H31HwscYEWNj40WNiY+IY2JlBCwhhhghEoR7LsP1fFBsJlWb02V25tUEqTeuyjSa2qC1kKipUd5iQFd0IcMZAUKpLzMiKboI9+XRCWmZg0Girze+iAtMTJqNECkjMmhTbm1UEgwmDU6I0mtqgs395BtW6Na/wCpAH2a+qCCkuG6Gk+GyCc6AW1Te2miCnf5fPgk396yENu/08/hGtf9SAHtxXUG1LqgPKjX1QTnRraoJb3fP5VO9LKb2/SqZbvbRAL34rJN5cV/PgjWqTfRGtfXXz+EAbfMpb3U1s1d+9YTN9cMhhk9rN4IIrAIxH4iB/4QZs0nw2VO9bLW4OvnRx/91hnZo5eMK7eW61ZGMtDnMvETriwAj4EumDM395BtW64MHN4ccoMSCL3oYoT/AAVzgPSTX1QQUlw3S0+GytZ0Atqpvb9KCnf5fPgk3963nxQy3f6efwjWvr58zQA9uK61v0jY8UHRebOHfBMBm0oyIIj/AOMRXH091+yOUiOHHjdvEhLGHCHrIg1oiCIYTsS68+68elLBzWUxcvg4GJD6wAGKMwgMIgSwhJclm+KLGq9WOnIsnD67CjhGIOKGZEYeUMUI4herizVGzZr0xZnEhAgwcPCIE64pFnDsG7wdO/zCCIEku0qLs4EcQiBHtH9Ni9mFUhXYHTWMMePHMfrI8Q9qKKMCMkyme4MBowC/SvVHMjFyOWjgrFgYZnLkAiBGrrw/qN0Bh58ZvAjBw8WH1cWGTIwxe04INiAxld9FvforzGYwcWPJZiIcBxIRN4IoI+ziQ7guC9HhPxrOvSxt83nxUtLhuqJ7N9fP5U3t+lRQ0nw2VO9bKGU6g20VIaVXvogTf3kD2rdGtf8AUgDyo19UEFJcN0t7qCc6NbVN7fpQGty6p30FN0e/LolK0NNkF3PFYJuOK4TY8Vimw4rlBBteu3maNbl1QbWrv5mj35dEAixpYqnetlCRU0sFTvWyBvzaINqmuybc2q4sxjwwQRRxRCCGCExRxGQAhDxEnQAEoPuOIQgzAgrESWb42WndYfSNlMuDDhn18QoICOy/vYlG/a68i65ddsbpHGiAiOHlREfV4QkCAZR4n6ozVjKGguTrxxQysgz3WTrpnM44xsY9g/8Abg9jD7jCJxfMStcijK5IWquPFCqHr91yDG7l0IykBLpq475jB08F2Mtn8TDnh4kcH7IooP6ldHDjC5hF8POqaYzmX6352AyzeY+ONiRf2KyOT9I3SMBlmYov3Q4cf1MLrTxiTYr5xcWckR6XkvSvnIXEQwI3qTBED9sYH0WL6wekbOZjDOEY4cOCLiGDCYDENDEYjEB3EP3LTcPEdMtlYsWMwQTLE+Ac/BkVxxYsLMIQAFxYGHFHF2YQ5NAvnEDON1n+pQh/yITECTNmBkd1Ksa5G8BIIYizXFVzZLpCLCjGJCxiGocTDEEdy9I6U6vZfGjMcUJc8QhLCLcsp0b1by8EQ7GEH1JMZG7xH+Fj9Rq8WQ9G3RWKYoukMRoAcPsQ4cIZwDCxE5ACAAVftLdehsnlMfNjO4J/29kwx9mL2SIgQ5E2jBAHwKxWcy2ahwoRhYEcQAYMD9QJ/RbD1Q6Liw8P12OD6yMkkaBgIZasLres/npsQc8Umo3nuV3PFYITremybHisVENxxXCg2oa7K7DiuVBtQV3QGty6oRrSyPfl0QnWhogp1PFYJvzaIdDxWKbc2qA976IC2720Sb+8g2rdBBKVQb6JtbVBSXDdDSfDZBSX2am/n8o976Id/l8+CTf3rIALTqTbRQSlV76Kh7cV1BtS6Btb9S8z9O/Txwsnh5WAscxGe018PDYxD4xRQd47QXplvd8/lfnH009Lev6VjgB9nLwQYI0fjjPjG3yoNQOIwdBirrxAkFgTc7AVJ0CsBmkadsRL5iiXz8F84hVR9M/ll8YYcrJdAZYYuLBA04ogHnLWi4OsHRWJlMxHhRvI+yW4oTwxDzYp4jjw4A9Vy3LaLpgrngxGv/CiuWMAwuupDNc4is9lwYQYlB2sCJge9bP6P8t6zNxaCFz8ZfFargAMSV6T6JcsDDjYpFYhCD3Bz/K1ErBdN9Ssf10XqoDFCSSG/OhWy9UOrBy8PbxA2JEG7NeyP+VuscIBkuHNUks1WDzGXPa71n+rPRAMbmgLn4W/gfErHQQB3W4dX8JoO0QwoPopx4+nLl4y5N6EW1R730Qvfisk396/nwVQEt3rt5/Cm1tVRt83nxUtLhugGcqAX1VJedGtqoaT4bKnetkB730QFp1e2iX95A9q3QQSlUG+ibW/UgpLhulvdQVptfVAHpJq7qNbl1RnrICm6AC4cUFkJk9tFdzxWCbjiuEAyrN6bI02vqozUm9dka3LqgoDyEiKnVQTmJAVGqM8jQUKpnMyIoNUHxi4ohhMcRaCEEnQAVP0dfkLpTO+vzGLjn/u4seJ3esjMTfVe2+mrrnDg4UWRwj/ALsaD/cRTDw4uX90YDNaEk3C8JhhdFj0X0I9GDH6QjiigEWHBl8QROHD4hhhhBF3Ha8Fy9NdQhls7iwmMDB7XbgAYHsxe0IXOjt8F6J6Her3+L0fDiRBsTNNiRm4hn6mH/xPa74yvrrhlxiY0QABAgENtH/P0UHhHTGNDHiRdiGGAOwhDGknJuV0uybjwXYz+W7GLHAQzREMQy4KUMwqr6yGaiwoxFDWEghepdNdDQ9K5TDxICBjQAMbF+KGLaS8lxcV56L1H0Z5xsrFJ3xC3whH5WpWbHnHSHRmJgYsWHiAiOH4ysRsuGTOV6t1u6rYubMGLhYcRiA7LgGc5LG9FeirNRgnFg7Pao8UMLd83+izWo8+wYniXEYarfOlfRrmME0iA1A7Y8RT4rH4fVIQn/ZFEdZM+yzq4wHRHRmLmYxh4YncmgFHK9l6r5GDK4MOBDEIjDxENMmZOy866V6UGUh9Tlx2DGHijebUYHVYvojprEwYnERmXM6rcZr3fHaTBdDMRrr5TpHt4UMQuAfHvXxiPIXKlRkehsmcXEnOETi7hX/j4rd4YRCBKVhosd0BlBh4YI4jbby6yYlMTJrsgENIzJodEabX1UAaQoalGty6oKJ0k1d1Hk9tEM6yam6u/NoghLBzMGgVIaRmTQ6JSYqahRmkJg12QVptfVAHkJEVOqjW5dUIeRkBQ6oAmHEgKjVHk9tFazMiKBN+bRBJfKnfTlVe/wBqO2720QO/isndxXRmlV76I1vqg8n9OmbzWD/i4mBi4uHhPHDGcOOKD/Z7Jw+12SH9kRtaRWQ9F/pCGah/xs3HCMaEezEWh9cPoPWC4FRMCRW89O9EYWcy+Jl8aH2Iwx1esMUJtECAQV+Y+tvVXMdHYpw8eEmAn/XigexiCzaRawmY3DE0fqLOdIYOFCYsbFw4IBeOOGADvJIXmXXj0vYWHAcPIEYuLT1xh/1wbwAt6yLSXZvOh8MYCbDwC7/RXRePm4xBl8GPFiNoIXA/dFwwjckKLjp5nMRYkcUccUUccZMUUUReKImZJJqVvnor6hx5/FGPjQtlcOIE9of9Yg8EOsP6jS1abT1L9DUMMQxOkIoYjIw4MBJgF/8AZHLtftEtyvXcHBhhhEEAEEMAAhAAAYSAAFAhrqdJ9IQ4MMmmC0Pdc7CS03Hx4Iie0TFEbsDP4UWR69kwxYWIAQCIoCO5oh9O14LCHNhgBLWX1U0xq/XXqucaH1uE3rAJhm7QGlnXmOJARF2ezEI37LMXJdmAudl7PBmu1idkEs7RRNvOV1vvQ3VTKZfEONh4cJxYh/1ixiYjlNIX2QeX9Q/RHHiRQ4+f9mHiGX5otPWkH2R7ombtML2bK5TDw4BBhQQwYcIbswwiEAaACi56yo19Ud50a2qqH9VDvSyr3+1HadXtogd/FZfMeGIpEAxXcAhfTNKr30Rrfcg8c9PnQ0PZy2YwsMA9qLCjMMOo7cBLftjXkMEWy/VXWnJeuy8TCeH7Y3YF2+V/FeaZToHK9v1nqIO0Z0l3tRTWnf6GypwMnAYxMYcL3mRTxK7mTLtEsvk8mMeCLDPNCQNj2fZPwP8ACwWSiML4cQIihJBGhBmrfqT49E6OI9XC1WXZG1brodD4j4YH1XfrKjX1VrMQNbhukvlt58VXedGtqj3+1RUO/wAvnwV7+KyU3f6efwjW+qB3cV1BtS6M8qNfVV3nRraoJL5ULXpZV7/ajtOr20QDvxWT+yUlV76Jt9yBN/e0QbVvsjTbm1QTpIiu6CCkuG5Q0nw2KCjjhuENHPDYIKd/l38yXDm8rBiwmDFggjB5I4RFCe8GS5jKs3psjTbm1QYLD6m9HQl4chlO1f8A9PhFu5wszgYMMEPZw4RDALACEDuAouQCwrcqCcxICo1QLNy6od6WSz8uiplWYNNkGH625M4uVxA3twjtwj9kz9O0PitF6HHbDRORMVsvUiLGtitAzmQ/xsxFAB7LvD+2L/gy+Cmd6u9Y17O5L/CxxhxEnDxPawYzcfpPvB5/DVbT0d0pGIA2IWFA7jwXLmhDiYYw8UCPDkTDEHDihBrCdwQV28h1WysUIjg9bDDeH1kUXgYnLfFXMN1meic767DeJvZLONWB/grvF71suLL4EGHABDCBBYD+S9TuuUhpGZNDoiE397RBtW6NNubVBOkiK7oIKS4blLT4bFBRxS4Sz8uiBGHlF8N+9eZerMGJHhxVgjihroZNsvTjKs3pstG61ZQwZvtSaOGGKtSPZMvgFKsZjq2falVisb12yPq4hmcPhLQ4mx5YvjT4Bd/q7hkR1Z6P+0n8FZrpPKeuwcTDEu1CR8awn4FiryiSsZ1ZxHgYmTT+lFnHBrQU/wDtaL0Tm8fLwmHGwooSAwi7JY9xEj4rPdF53ExCD2D2XnEQRC161Pcr9TuM6XvxWCTf3rjz8EIsa2KNNua5UUG3zbeZqCkuG5VE6Sau6ln5dEA0nw2Kp3rZQ0c8NgqZVmTTZAm/vaIHtW6NNubVAHkJEVOqCCkuG5SzcuqCjiQuEs/LogNa2qVrJqbpL5U76cqCu86HTVHvfRO/isndxXQSlJvXZGtbVO75vPikvlugM8jIC+qtZmRFBqpK/DZU71sgPe+iUpN6jRP7efwg2rdBGtUG+i6nSPRuHjgQxuOzwxgsQ9WNPhOi7crcN0lfht58UGs4fVnEEf8A1x2LPB7RGlWJ3+i2DJ5cYcEMIc9kNOZO5/8Axc53+Xz4J/ZAdpiZNtFGaQmDU6K93FdQNal0BrW1RnkZNQ6pL5Ve+lkEd5mRFtVXvfRO/isndxXQSlJvXZa/1xy/+uDEE+xExO0cv5A8VsI2+bz4r5xIIYgQQDAQ0QM3exCDVslnhCBDf4v8FnOjc76zdgCCL6ArrYfVvLwxdpo2sPWRkfy6yeXy8GGOzDDDD+kQgAfRXUxyve+iO0xMmo0T+3n8INq3UVGaVQb6I1rapK3DdJfLbz4oFayam6r3vood/l8+CvfxWQR2mJk20RmkJvU6K93FdO6l0Ea1tUZ5GQFDqkvlSV6WQHeZkRbVV730Tv4rJ/ZAe9tFHas3psj35tEBalTXZBWtU66I1r6qUkKXKbHhsUFrSTV3Ue9tEJetqbo9+bRAdp1BtorSRmTQ6KO0xU1CCUhQ1QXa+qVpJq7qbcuqVrQUQHvQC2qPe2miPc1FAj35rhBaVm9Nka19VAWpeuybcuqC1lQi+qO8xJqjVQzkaChQl5moogPe2irtMzeg0Ue/NogLTFTVBaSqTfRGtfVSkhQ1KbcuqC1pJq7qPe2iEvW1N0e/NogrtOoNtEpKr0OijtMVNQlJChqgu19UrKjVOqm3LqlZGgogrvOgFtVHvbRHeZqKBHvzaIFKzemyrWvqo7UvXZNuXVBWeVCL6qO8xJqjVKyNBQoS8zUUQV720UdpmYNtEe/NogLTFTVBaSMyb6I1r6qCUhQ1KbcuqD//2Q==";
        byte[] profile = Convert.FromBase64String(base64);

        RegisterCharactorProfile_SaveDataToDB(profile);
    }

    public void OnClickSavePlayerDataTest()
    {
        Player_DB player_DB = new Player_DB();
        player_DB.playerName = "�׽�Ʈ";
        string base64 = "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";
        player_DB.image = Convert.FromBase64String(base64);
        player_DB.Day = "monday";
        player_DB.BirthDay = "95.03.21";
        player_DB.TotalAnswers = 44;
        player_DB.TotalTime = 123f;

        Game_Type game_Type = Game_Type.A;
        int level = 1;
        int step = 2;
        Data_value data_Value = new Data_value(234.21f, 23, 12, 22.44f, 18000, 3);
        //Reult_DB�� Null�� �� ó��
        if (!player_DB.Data.ContainsKey((game_Type, level, step)))
        {
            player_DB.Data.Add((game_Type, level, step), data_Value);
        }
        else
        {
            //���� totalScore�� DB�� �ִ� �������� ũ�ٸ� �ٽ� �Ҵ�
            if (player_DB.Data[(game_Type, level, step)].TotalScore < 20000)
            {
                player_DB.Data[(game_Type, level, step)] = data_Value;
            }
        }

        for(int i = 0; i < 3; i++)
        {
            for(int j = 0; j < 6; j++)
            {
                player_DB.Data[(Game_Type.A, i+1, j+1)] = data_Value;
            }
        }

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                player_DB.Data[(Game_Type.B, i + 1, j + 1)] = data_Value;
            }
        }

        for (int i = 0; i < 1; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                player_DB.Data[(Game_Type.C, i + 1, j + 1)] = data_Value;
            }
        }

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                player_DB.Data[(Game_Type.D, i + 1, j + 1)] = data_Value;
            }
        }

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                player_DB.Data[(Game_Type.E, i + 1, j + 1)] = data_Value;
            }
        }

        AppExit_SaveCharactorDataToDB(player_DB);
    }

    public void OnClickCreateDateDBTest()
    {
        string requestData = "[Test]CreateDB|Finish";

        RequestToServer(requestData);
    }
#endregion

    private void CloseSocket()
    {
        Debug.Log("Close Socket���� �����°�");
        if (!socketReady) return;

        //writer.Close();
        //reader.Close();
        client.Close();
        socketReady = false;
        Application.Quit();
    }
}
