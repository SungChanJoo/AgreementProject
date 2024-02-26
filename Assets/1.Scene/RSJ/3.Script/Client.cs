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

// 처음 접속하는 유저인지(First), 접속했었던 유저인지(Continue) 
public enum ClientLoginStatus
{
    New,
    Exist
}

public class Client : MonoBehaviour
{
    // IP, Port 고정됨
    [SerializeField] private string server_IP = "15.165.159.141"; // aws EC2 IP : 15.165.159.141
    [SerializeField] private int server_Port = 2421;

    bool socketReady;
    private TcpClient client;
    private static NetworkStream stream;

    // login - license
    public static ClientLoginStatus loginStatus;
    public static string clientLicenseNumber;
    public static string clientCharactor;
    public string licenseFolderPath = string.Empty;

    // 서버로 부터 받은 data를 1차적으로 거른 List
    public List<string> CharactorData_FromServer = new List<string>();

    // Charactor data를 사용하기 위한 Dictionary
    public Dictionary<string, List<string>> CharactorData_Dic;

    // DB Table Name
    private TableName table;

    // 서버-클라이언트 string으로 data 주고받을때 구분하기 위한 문자열
    private const string separatorString = "E|";

    // Rank data
    private RankData clientRankData;

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
        Invoke("LoadCharactorDataFromDB", 8f);
    }

    // 클라이언트가 실행할 때 서버 연결 시도
    public void ConnectToServer()
    {
        // 이미 연결되었다면 함수 무시
        if (socketReady) return;

        // 서버에 연결
        try
        {
            client = new TcpClient();
            client.Connect(server_IP, server_Port);
            Debug.Log("[Client] Success Connect to Server!");
            socketReady = true;

            stream = client.GetStream();// 연결에 성공하면 stream도 계속 연결할 수 있도록
        }
        catch (Exception e)
        {
            Debug.Log($"[Client] Fail Connect to Server : {e.Message}");
        }

    }

    public void ClientLoginSet()
    {
        // 연결되지 않았다면 return
        if (!client.Connected) return;

        licenseFolderPath = Application.dataPath + "/License";
        string licenseFilePath = licenseFolderPath + "/clientlicense.json";

        Debug.Log($"[Client] Directory.Exists(licenseFolderPath) value ? {Directory.Exists(licenseFolderPath)}");
        // 경로에 파일이 존재하지 않는다면 라이센스넘버가 없다는것이고, 처음 접속한다는 뜻
        if (!File.Exists(licenseFilePath))
        {
            loginStatus = ClientLoginStatus.New;
            // 서버에서 라이센스 넘버를 받아와야함, 그러기 위해 서버에 요청 todo
            string requestName = "[Create]LicenseNumber";
            RequestToServer(requestName);
            Debug.Log($"[Client] This client's licensenumber(first) : {clientLicenseNumber}");
            return; // 처음 접속이라면 폴더 및 파일 저장하고 return
        }

        loginStatus = ClientLoginStatus.Exist;
        // 해당 경로에 있는 파일을 읽어 클라이언트 라이센스 넘버를 불러옴
        string jsonStringFromFile = File.ReadAllText(licenseFilePath);
        JsonData client_JsonFile = JsonMapper.ToObject(jsonStringFromFile);
        clientLicenseNumber = client_JsonFile["LicenseNumber"].ToString();
        clientCharactor = client_JsonFile["Charactor"].ToString();
        Debug.Log($"[Client] Use already existed licensenumber?");
        Debug.Log($"[Client] This client's licensenumber(existing) : {clientLicenseNumber}");
    }

    // 게임 시작할 때 유저정보 불러오기 - 서버에 요청(서버-DB) // 나중에 Player Class 정리되면 수정해야함. todo
    private void LoadCharactorDataFromDB()
    {
        Debug.Log("[Clinet] Request LoadCharactorDataFromDB");
        string requestData; // 이 메서드에서 requestData는 requestName/clientLicenseNumber/clientCharactor
        requestData = $"[Load]CharactorData|{clientLicenseNumber}|{clientCharactor}";
        RequestToServer(requestData);
    }

    // 서버에 요청할때 string으로 보내는데, 서버에서 받을 때 string case로 구분해서 처리
    private void RequestToServer(string requestData)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(requestData);
            stream.Write(data, 0, data.Length); // 데이터를 보낼때 까지 대기? 그냥 보내면 되잖어
            List<string> requestDataList = requestData.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
            string requestName = requestDataList[0];
            Debug.Log($"[Client] Request to server : {requestData}");

            ReceiveRequestDataFromServer(stream); // 메서드가 실행될때 까지 대기 / 대기시키기 위해 메서드 앞에 await을 붙여서 실행시키려면 메서드가 Task 붙여야하는듯?
        }
        catch (Exception e)
        {
            Debug.Log($"[Client] Error Request to sever : + {e.Message}");
        }
    }

    // 서버에 요청보낸거 받음, 받은 string, case로 구분해서 처리
    private async void ReceiveRequestDataFromServer(NetworkStream stream)
    {
        try
        {
            List<string> receivedRequestData_List = new List<string>();

            while (true)
            {
                //byte[] buffer = new byte[1024]; // 일반적인 버퍼사이즈
                byte[] buffer = new byte[16000000]; // 16MiB 버퍼 사이즈 
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // 데이터를 읽어올때까지 대기
                string receivedRequestData = Encoding.UTF8.GetString(buffer, 0, bytesRead); // 데이터 변환

                receivedRequestData_List.Add(receivedRequestData);
                Debug.Log($"[Client] Receiving... request data from server : {receivedRequestData}");

                // 서버로부터 데이터 전송이 끝나면(Finish) break;
                List<string> endCheck = receivedRequestData.Split('|').ToList();
                if (endCheck.Contains("Finish"))
                {
                    // receivedRequestData에 Finish가 있는 경우 Finish를 제거
                    receivedRequestData_List.RemoveAt(receivedRequestData_List.Count - 1);
                    endCheck.RemoveAt(endCheck.Count - 1);

                    string fixLastIndexInList = null;

                    for(int i = 0; i < endCheck.Count; i++)
                    {
                        fixLastIndexInList += $"{endCheck[i]}|";
                    }

                    receivedRequestData_List.Add(fixLastIndexInList);

                    Debug.Log($"[Client] Finish Receive Data From Server");
                    break;
                }

            }

            HandleRequestData(receivedRequestData_List);
        }
        catch (Exception e)
        {
            Debug.Log($"[Client] Error receiving data from server : {e.Message}");
        }
    }

    // 서버로부터 받은 데이터 처리
    private void HandleRequestData(List<string> dataList)
    {
        // requestname : 클라이언트-서버에서 데이터를 처리하기 위한 구분
        // requestdata : Server에서 requestName으로 처리된 결과를 보낸 data
        // LicenseNumber -> clientLicenseNumber를 server가 보내줌 (Client가 첫 접속인 경우 처리됨)
        // LoadNewCharactorData -> 새 플레이어의 경우 DB에 licenseNumber를 제외한 데이터가 없으므로 모든 테이블에 존재하는 열항목에 0값을 부여한 CharactorData를 가질 것임
        // LoadExistCharactorData -> 기존 플레이어의 경우 DB에 저장된 CharactorData를 가질 것임

        // dataList[0] = "[Load]CharactorData|value|value..|value|E|";
        string requestName = dataList[0].Split('|')[0];
        Debug.Log($"[Client] HandleRequestData method, request name : {requestName}");

        switch (requestName)
        {
            case "[Create]LicenseNumber":
                foreach (string data in dataList[0].Split('|'))
                {
                    Debug.Log($"[Client] Check User data : {data} ");
                }
                clientLicenseNumber = dataList[0].Split('|')[1];
                clientCharactor = dataList[0].Split('|')[2];
                SaveClientDataToJsonFile();
                Debug.Log($"[Client] RequestName : {requestName}, get and save licenseNumber to jsonfile");
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
            case "[Load]CharactorData":
                HandleLoadCharactorData(dataList);
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            case "[Load]RankData":
                HandleLoadRankData(dataList);
                Debug.Log($"[Client] RequestName : {requestName}, End handling data");
                break;
            default:
                Debug.Log("[Client] HandleRequestMessage Method Something Happend");
                break;
        }
    }

    // 최종적으로 사용할 수 있는 player data
    // 서버로부터 받고 1차적으로 정리한 data중 table name을 가지고 최종적으로 data 정리 
    private void FilterCharactorData()
    {
        Debug.Log("[Client] Filtering... player data");
        for(int i = 0; i < CharactorData_FromServer.Count; i++)
        {
            for(int j = 0; j < table.list.Count; j++)
            {
                if(CharactorData_FromServer[i].Contains(table.list[j]))
                {
                    List<string> values = CharactorData_FromServer[i].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList(); // User테이블기준으로 User|User_Name|User_Profile|User_Coin이 있을것임
                    values.RemoveAt(0); // 0번째 인덱스는 테이블명이므로 values에 필요하지 않다.
                    CharactorData_Dic.Add(table.list[j], values);
                }
            }
        }
    }

    // Json파일에 LicenseNumber 등록 / 비동기로 호출된 것이 끝났을때 동기적으로 호출시키기 위해
    private void SaveClientDataToJsonFile()
    {
        // JsonData 생성
        JsonData client_Json = new JsonData();
        client_Json["LicenseNumber"] = clientLicenseNumber;
        client_Json["Charactor"] = clientCharactor;
        // Json 데이터를 문자열로 변환하여 파일에 저장
        string jsonString = JsonMapper.ToJson(client_Json);
        File.WriteAllText(licenseFolderPath + "/clientlicense.json", jsonString);
    }

    // Start DBTable 세팅
    private void ETCInitSetting()
    {
        Debug.Log("[Client] ETCInitSetting");
        // DB TableName 생성
        table = new TableName();

        // Dictionary 생성
        CharactorData_Dic = new Dictionary<string, List<string>>();
    }

    // 플레이어 데이터 처리
    private void HandleLoadCharactorData(List<string> dataList)
    {
        // dataList[0] = "[Load]CharactorData|user_info|value|value..|value|E|";
        // dataList[1] = "rank|value|value|...|value|E|{gameTableName}|value|value|...|value|E|";
        // dataList[2] = "{gameTableName}|value|value|...|value|E|{gameTableName}|value|value|...|value|E|";
        // ... dataList[Last] = "{gameTableName}|value|value|...|value|E|Finish|;

        Debug.Log("[Client] Handling LoadCharactorData");

        List<string> firstFilterData = new List<string>(); // "E|" 제거

        // "E|" separatorString으로 구분
        //List<string> parts = new List<string>();  
        //requestdata.Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();

        //List<string> parts = dataList[0].Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList());
        for (int i = 0; i < dataList.Count; i++)
        {
            // requestName 제거
            if (i == 0) dataList[0] = dataList[0].Substring("[Load]CharactorData".Length);

            List<string> parts = new List<string>();
            parts = dataList[i].Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (string part in parts) // part : rank|value|value|...|value|
            {
                //part.Split('|', StringSplitOptions.RemoveEmptyEntries);
                CharactorData_FromServer.Add(part);
            }

        }

        FilterCharactorData();
    }

    // 랭크 데이터 처리
    private void HandleLoadRankData(List<string> dataList)
    {
        // return_string = $"{i}|{timeList[i].userProfile}|{timeList[i].userName}|{timeList[i].totalTime}|{separatorString}";
        // dataList[0] = "[Load]RankData|place|profile|name|time or score|E|";
        // dataList[1] = "place|profile|name|time or score|E|"; // parts.Count == 1
        // or dataList[1] = "place|profile|name|time or score|E|place|profile|name|time or score|E|"; // parts.Count == 2

        Debug.Log("[Client] HandleLoadRankData..");

        // clientRankData InitSetting
        clientRankData = new RankData();
        clientRankData.rank_Score = new Rank_Score[6];
        clientRankData.rank_Time = new Rank_Time[6];

        // separatorString ( "E|" )을 기준으로 Split 한 List
        List<string> filterDataList = new List<string>();

        for(int i =0; i < dataList.Count; i++)
        {
            // requestName 제거
            if (i == 0) dataList[0] = dataList[0].Substring("[Load]RankData|".Length);

            List<string> parts = new List<string>();
            parts = dataList[i].Split(separatorString, StringSplitOptions.RemoveEmptyEntries).ToList();
            parts.ForEach(data => filterDataList.Add(data)); // 인덱스(string) 옮김
        }
        Debug.Log("[Client] Check HandleLoadRankData..");
        for(int i = 0; i < filterDataList.Count; i++)
        {
            Debug.Log($"{filterDataList[i]}");
        }

        for (int i = 0; i < filterDataList.Count; i++)
        {
            List<string> parts = filterDataList[i].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();

            if(i < 6) // score
            {
                for (int j = 0; j < parts.Count; j++)
                {
                    if (j == 0) clientRankData.rank_Score[i].place = Int32.Parse(parts[j]);
                    else if (j == 1) clientRankData.rank_Score[i].userProfile = Encoding.UTF8.GetBytes(parts[j]);
                    else if (j == 2) clientRankData.rank_Score[i].userName = parts[j];
                    else if (j == 3) clientRankData.rank_Score[i].totalScore = Int32.Parse(parts[j]);
                }

                // 자기 자신의 userlicensenumber와 usercharactor를 저장해둬야 개인의 데이터를 가져다 쓸수있다.
                if(i == 5)
                {
                    clientRankData.rank_Score[i].userlicensenumber = Int32.Parse(clientLicenseNumber);
                    clientRankData.rank_Score[i].usercharactor = Int32.Parse(clientCharactor);
                }
                else
                {
                    clientRankData.rank_Score[i].userlicensenumber = 0;
                    clientRankData.rank_Score[i].usercharactor = 0;
                }
            }
            else // time
            {
                for (int j = 0; j < parts.Count; j++)
                {
                    if (j == 0) clientRankData.rank_Time[i].place = Int32.Parse(parts[j]);
                    else if (j == 1) clientRankData.rank_Time[i].userProfile = Encoding.UTF8.GetBytes(parts[j]);
                    else if (j == 2) clientRankData.rank_Time[i].userName = parts[j];
                    else if (j == 3) clientRankData.rank_Time[i].totalTime = float.Parse(parts[j]);
                }

                // 자기 자신의 userlicensenumber와 usercharactor를 저장해둬야 개인의 데이터를 가져다 쓸수있다.
                if (i == 11)
                {
                    clientRankData.rank_Score[i].userlicensenumber = Int32.Parse(clientLicenseNumber);
                    clientRankData.rank_Score[i].usercharactor = Int32.Parse(clientCharactor);
                }
                else
                {
                    clientRankData.rank_Score[i].userlicensenumber = 0;
                    clientRankData.rank_Score[i].usercharactor = 0;
                }
            }
        }

        Debug.Log("[Client] End HandleLoadRankData..");

    }

    /*
   Client가 DB에 있는 파일을 Save Load 하는 경우
   1. 앱 시작 -> Load (모든 데이터, 게임데이터, 재화, crew 소유권한 등)
   2. 플레이어(charactor) 생성 -> Save, Load
   3. 앱 게임 시작 -> TotalScore(int형) Load (game_type, level, step 받음) -> Save GameData
   4. 랭킹 UI 접속 시(or 랭킹새로고침) -> rank Load
   5. 앱 종료 -> Save (모든 데이터)
    */

    // 앱 시작시 모든 데이터 Load
    public Result_DB AppStart_LoadAllDataFromDB()
    {
        Result_DB resultdb = new Result_DB();

        // user_info table -> [0]:User_LicenseNumber, [1]:User_Charactor, [2]:User_Name, [3]:User_Profile, [4]:User_Coin
        // rank table - > [0]:User_LicenseNumber, [1]:User_Charactor, [2]:TotalTime, [3]:TotalScore
        resultdb.playerName = CharactorData_Dic["user_info"][2];
        resultdb.image = Encoding.UTF8.GetBytes(CharactorData_Dic["user_info"][3]);
        resultdb.Day = "";
        resultdb.TotalAnswers = Int32.Parse(CharactorData_Dic["rank"][3]);
        resultdb.TotalTime = float.Parse(CharactorData_Dic["rank"][2]);

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
                            Debug.Log("[Client] AppStart_LoadAllDataFromDB() Game_Type default Problem");
                            break;
                    }

                    string levelpart = $"level{levels[j]}";
                    if (game_Names[i] == "venezia_chn")
                    {
                        j = 2; // venezia_chn 게임은 level이 1개뿐이므로 한번만 돌아야함.
                        levelpart = "level";
                    }

                    string game_TableName = $"{game_Names[i]}_{levelpart}_step{steps[k]}";

                    float reactionRate = float.Parse(CharactorData_Dic[$"{game_TableName}"][2]);
                    int answersCount = Int32.Parse(CharactorData_Dic[$"{game_TableName}"][3]);
                    int answers = Int32.Parse(CharactorData_Dic[$"{game_TableName}"][4]);
                    float playTime = float.Parse(CharactorData_Dic[$"{game_TableName}"][5]);
                    int totalScore = Int32.Parse(CharactorData_Dic[$"{game_TableName}"][6]);

                    Data_value datavalue = new Data_value(reactionRate, answersCount, answers, playTime, totalScore);

                    resultdb.Data.Add((game_type, j, k), datavalue);
                }
            }
        }

        return resultdb;
    }

    // Charactor 생성 Save (기존에 플레이하던 Charator Data)
    public void CreateCharactor_SaveAllDataToDB(Result_DB resultdb)
    {
        
    }

    // Charactor 생성 Load (새로 생성한 Charatror Data)
    public Result_DB CreateCharactor_LoadAllDataFromDB()
    {
        Result_DB resultdb = new Result_DB();

        return resultdb;
    }

    // 게임 시작시 TotalScore Load
    public int AppGame_LoadTotalScoreFromDB(Game_Type game_type, int level, int step)
    {

        return 0;
    }

    // 재백이가 만든 Result_Data를 매개변수로 받아서 DB에 저장하는 메서드(server에 요청 -> RequestServer)
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

    // 랭킹 UI 접속 시 Rank Load
    public RankData Ranking_LoadRankDataFromDB()
    {
        RankData return_RankData = new RankData();

        string requestData = $"[Load]RankData|{clientLicenseNumber}|{clientCharactor}|";
        RequestToServer(requestData);

        return_RankData = clientRankData;

        return return_RankData;
    }

    // 앱 종료시 모든 데이터 Save
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
        //Reult_DB가 Null일 때 처리
        if (!result_DB.Data.ContainsKey((game_Type, level, step)))
        {
            result_DB.Data.Add((game_Type, level, step), data_Value);
        }
        else
        {
            //만약 totalScore가 DB에 있는 점수보다 크다면 다시 할당
            if (result_DB.Data[(game_Type, level, step)].TotalScore < 20000)
            {
                result_DB.Data[(game_Type, level, step)] = data_Value;
            }
        }

        AppGame_SaveResultDataToDB(result_DB, game_Type, level, step);

    }

    

    // 서버로부터 받은 CharactorData를 게임에서 사용하는 Player Class에 설정
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
        for(int i = 0; i< clientRankData.rank_Score.Length; i++)
        {
            Debug.Log($"clientRankData.rank_Score[i].place : {clientRankData.rank_Score[i].place}");
            Debug.Log($"clientRankData.rank_Score[i].userProfile : {clientRankData.rank_Score[i].userProfile}");
            Debug.Log($"clientRankData.rank_Score[i].userName : {clientRankData.rank_Score[i].userName}");
            Debug.Log($"clientRankData.rank_Score[i].totalScore : {clientRankData.rank_Score[i].totalScore}");
        }

        for (int i = 0; i < clientRankData.rank_Time.Length; i++)
        {
            Debug.Log($"clientRankData.rank_Time[i].place : {clientRankData.rank_Time[i].place}");
            Debug.Log($"clientRankData.rank_Time[i].userProfile : {clientRankData.rank_Time[i].userProfile}");
            Debug.Log($"clientRankData.rank_Time[i].userName : {clientRankData.rank_Time[i].userName}");
            Debug.Log($"clientRankData.rank_Time[i].totalScore : {clientRankData.rank_Time[i].totalTime}");
        }

    }

    private void CloseSocket()
    {
        Debug.Log("Close Socket으로 들어오는가");
        if (!socketReady) return;

        //writer.Close();
        //reader.Close();
        client.Close();
        socketReady = false;
    }
}
