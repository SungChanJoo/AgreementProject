using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data;
using MySql.Data.MySqlClient;
using UnityEngine.UI;
using System;

// 싱글톤 쓰자
// 클라이언트가 서버에 연결했을때? 클라이언트는 이미 자체적으로 IP와 Port를 알아서 서버에 연결함
// 클라이언트가 로그인 할때 ID와 Password를 입력할텐데, 그때 서버에 있는 DB에서 login table과 비교해서 true면 로그인
// 회원등록도 일단 만들어놓는다
// DBManager는 서버에서 관리하는게 맞다

// 유저 정보
public class User_Info
{
    public string user_Name { get; private set; }
    public string user_Password { get; private set; }

    public User_Info(string _name, string _password)
    {
        user_Name = _name;
        user_Password = _password;
    }
        
}

public class DBManager : MonoBehaviour
{
    public User_Info user_Info;

    MySqlConnection connection; // DB에 연결하는 클래스
    MySqlDataReader reader;

    //public string path = string.Empty;

    // 연결할때 필요한 정보
    private string str_Connection;

    // Table List
    private List<string> table_List;
    private TableName table;

    // 서버-클라이언트 string으로 data 주고받을때 구분하기 위한 문자열
    private const string separatorString = "E|";

    // LicenseNumber 기본, Table 게임명(편의성을 위해)
    private int clientLicenseNumber_Base = 10000;
    private string licenseNumber_Column = "User_LicenseNumber";
    public static string venezia_kor = "venezia_kor";
    public static string venezia_eng = "venezia_eng";
    public static string venezia_chn = "venezia_chn";
    public static string calculation = "calculation";
    public static string gugudan = "gugudan";

    public static DBManager instance = null; // 싱글톤 쓸거임

    private void Awake()
    {
        if(instance == null)
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
        SetStringConnection();
        ConnectDB();
        InitSetting();
    }

    // DB 세션에 연결할 때 필요한 정보 초기화
    private void SetStringConnection()
    {
        string ip = "127.0.0.1"; // 우선 로컬(127.0.0.1)로, aws EC2 IP : 15.165.159.141
        string db = "test";
        string uid = "root"; //string.IsNullOrEmpty(user_Info.user_Name)? "" : user_Info.user_Name;
        string pw = "12345678"; //string.IsNullOrEmpty(user_Info.user_Password)? "" : user_Info.user_Password;
        string port = "3306";
        //str_Connection = $"Server={ip};Database={db};Uid={uid};Pwd={pw};Port={port};Charset=utf8;";
        str_Connection = $"Server={ip};" + $"Database={db};" + $"Uid={uid};" + $"Pwd={pw};" + $"Port={port};" + "CharSet=utf8;"; // ; 세미콜론 주의해라
        Debug.Log($"[DB] DB connect info : {str_Connection}");
    }

    // DB에 연결
    private void ConnectDB()
    {
        try
        {
            connection = new MySqlConnection(str_Connection);
            connection.Open(); // DB 열음
            Debug.Log("[DB] Success connect to db!");
        }
        catch(Exception e)
        {
            Debug.Log($"[DB] Fail connect to db + {e.Message}");
        }
    }

    private bool CheckConnection(MySqlConnection con)
    {
        // open이 안되어있다면
        if(con.State != System.Data.ConnectionState.Open)
        {
            con.Open(); // 한번 더 열어보기
            if (con.State != System.Data.ConnectionState.Open)
            {
                return false; //재오픈을 했는데도 안되면 이상함 감지
            }
        }
        return true;
    }

    // DB Table등 data에 조작할 때 필요한 변수 설정
    private void InitSetting()
    {
        table_List = new List<string>();
        table_List.Add("user_info");
        table = new TableName();
    }

    // 계정등록 // todo 비동기화 생각해봐야함
    public string CreateLicenseNumber()
    {
        Debug.Log("[DB] CreateLicenseNumber method");

        if(!CheckConnection(connection))
        {
            Debug.Log("[DB] Didn't connect to DB");
            return "";
        }

        int count = 0;

        // DB user_info 테이블에서 User_LicenseNumber가 몇개 있는지 체크
        string checkUserInfo_Command = $"SELECT User_LicenseNumber FROM user_info";
        MySqlCommand check_SqlCmd = new MySqlCommand(checkUserInfo_Command, connection);
        reader = check_SqlCmd.ExecuteReader();

        // table 체크
        if (reader.HasRows)
        {
            while(reader.Read())
            {
                count++;
            }
        }

        // user_info 변수
        string table_Name = "user_info";
        string[] columns = { "User_LicenseNumber", "User_Charactor", "User_Name", "User_Profile", "User_Coin" };
        int clientLicenseNumber = clientLicenseNumber_Base + count;
        int user_Charactor = 1;
        string user_Name = "Guest";
        int user_Profile = 1;
        int user_Coin = 0;

        // Binary Parameter
        //MySqlParameter binaryParameter = new MySqlParameter

        // 생성하는 쿼리문
        string createAccount_Command = $"INSERT INTO {table_Name} ({columns[0]}, {columns[1]}, {columns[2]}, {columns[3]}, {columns[4]}) " +
                                        $"VALUES (@clientLicenseNumber, @user_Charactor, @user_Name, @user_Profile, @user_Coin)"; // `(따옴표), '(백틱) 구분하기
        MySqlCommand insert_SqlCmd = new MySqlCommand(createAccount_Command, connection);
        insert_SqlCmd.Parameters.Add("@clientLicenseNumber", MySqlDbType.Int32).Value = clientLicenseNumber;
        insert_SqlCmd.Parameters.Add("@user_Charactor", MySqlDbType.Int32).Value = user_Charactor;
        insert_SqlCmd.Parameters.Add("@user_Name", MySqlDbType.VarChar).Value = user_Name;
        insert_SqlCmd.Parameters.Add("@user_Profile", MySqlDbType.Int32).Value = user_Profile;
        insert_SqlCmd.Parameters.Add("@user_Coin", MySqlDbType.Int32).Value = user_Coin;
        //insert_SqlCmd.ExecuteNonQuery(); 
        
        reader.Close(); // DataReader가 열려있는 동안 추가적인 명령 실행 불가
        insert_SqlCmd.ExecuteNonQuery(); // command.ExecuteNonQuery()은 DB에서 변경 작업을 수행하는 SQL 명령문을 실행하고, 영향을 받은 행의 수를 반환하는 메서드

        string returnData = $"{clientLicenseNumber}|{user_Charactor}";
        return returnData;
    }

    // 새 플레이어 데이터 생성
    public void CreateNewPlayerData(int clientlicensenumber)
    {
        Debug.Log($"[DB] Create new Player data, client's licensenumber : {clientlicensenumber}");
        // ClientLicenseNumber가 발급되면 새 플레이어 정보를 만드는 것이므로, 모든 테이블에 data를 추가해야함
        string insertCreatePlayerData_Command;
        MySqlCommand insert_SqlCmd = new MySqlCommand();
        insert_SqlCmd.Connection = connection;

        string updatePlayerData_Command;
        MySqlCommand update_SqlCmd = new MySqlCommand();
        update_SqlCmd.Connection = connection;

        // rank table
        string rank_TableName = "rank";
        string[] rank_Columns = { "User_LicenseNumber", "User_Charactor", "TotalTime", "TotalScore" };
        int rank_valuepart;
        //table_List.Add(rank_TableName);

        // insert row
        insertCreatePlayerData_Command = $"INSERT INTO {rank_TableName} ({rank_Columns[0]}) VALUES ({clientlicensenumber})";
        insert_SqlCmd.CommandText = insertCreatePlayerData_Command;
        insert_SqlCmd.ExecuteNonQuery();

        for (int i = 0; i < rank_Columns.Length; i++)
        {
            if (i == 0) rank_valuepart = clientlicensenumber;
            else if (i == 1) rank_valuepart = 1;
            else rank_valuepart = 0;

            updatePlayerData_Command = $"UPDATE {rank_TableName} SET {rank_Columns[i]} = {rank_valuepart}";
            update_SqlCmd.CommandText = updatePlayerData_Command;
            update_SqlCmd.ExecuteNonQuery();
        }
        Debug.Log("[DB] Rank table complete!");

        // achievement table
        string achievement_TableName = "achievement";
        string[] achievement_Columns = { "User_LicenseNumber", "User_Charactor", "Something"};
        int achievement_valuepart;
        //table_List.Add(achievement_TableName);

        // insert row
        insertCreatePlayerData_Command = $"INSERT INTO {achievement_TableName} ({achievement_Columns[0]}) VALUES ({clientlicensenumber})";
        insert_SqlCmd.CommandText = insertCreatePlayerData_Command;
        insert_SqlCmd.ExecuteNonQuery();

        for (int i = 0; i < achievement_Columns.Length; i++)
        {
            if (i == 0) achievement_valuepart = clientlicensenumber;
            else if (i == 1) achievement_valuepart = 1;
            else achievement_valuepart = 0;

            updatePlayerData_Command = $"UPDATE {achievement_TableName} SET {achievement_Columns[i]} = {achievement_valuepart}";
            update_SqlCmd.CommandText = updatePlayerData_Command;
            update_SqlCmd.ExecuteNonQuery();
        }
        Debug.Log("[DB] achievement table complete!");

        // pet table
        string pet_TableName = "pet";
        string[] pet_Columns = { "User_LicenseNumber", "User_Charactor", "White" };
        int pet_valuepart;
        //table_List.Add(pet_TableName);

        // insert row
        insertCreatePlayerData_Command = $"INSERT INTO {pet_TableName} ({pet_Columns[0]}) VALUES ({clientlicensenumber})";
        insert_SqlCmd.CommandText = insertCreatePlayerData_Command;
        insert_SqlCmd.ExecuteNonQuery();

        for (int i = 0; i < pet_Columns.Length; i++)
        {
            if (i == 0) pet_valuepart = clientlicensenumber;
            else if (i == 1) pet_valuepart = 1;
            else pet_valuepart = 0;

            updatePlayerData_Command = $"UPDATE {pet_TableName} SET {pet_Columns[i]} = {pet_valuepart}";
            update_SqlCmd.CommandText = updatePlayerData_Command;
            update_SqlCmd.ExecuteNonQuery();
        }
        Debug.Log("[DB] pet table complete!");

        // game table
        string game_TableName;

        string[] game_Names = { "calculation", "venezia_chn" };
        int[] levels = { 1, 2, 3 };
        int[] steps = { 1, 2, 3, 4, 5, 6 };

        string[] game_Columns = { "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerCount", "AnswerRate", "Playtime", "TotalScore", "StarPoint" };

        int user_Charactor = 1;

        List<string> game_TableList = new List<string>();

        for (int i = 0; i < game_Names.Length; i++)
        {
            for (int j = 0; j < levels.Length; j++)
            {
                for (int k = 0; k < steps.Length; k++)
                {
                    string levelpart = $"level{levels[j]}";
                    if (game_Names[i] == "venezia_chn")
                    {
                        j = 2; // venezia_chn 게임은 level이 1개뿐이므로 한번만 돌아야함.
                        levelpart = "level";
                    }
                    game_TableName = $"{game_Names[i]}_{levelpart}_step{steps[k]}";
                    game_TableList.Add(game_TableName);
                    //table_List.Add(game_TableName);

                }
            }
        }

        for (int i = 0; i < game_TableList.Count; i++)
        {
            // insert row
            insertCreatePlayerData_Command = $"INSERT INTO {game_TableList[i]} (User_LicenseNumber, User_Charactor) VALUES ({clientlicensenumber}, {user_Charactor})";
            insert_SqlCmd.CommandText = insertCreatePlayerData_Command;
            insert_SqlCmd.ExecuteNonQuery();

            // update rowm j=2부터 시작하는 이유는 0은 licensenumber고 1은 charactor번호이고 insert로 넣어줬기 때문에 굳이 또 업데이트를 할 필요가 없음
            for (int j = 2; j < game_Columns.Length; j++)
            {
                updatePlayerData_Command = $"UPDATE {game_TableList[i]} SET {game_Columns[j]} = '0'";
                update_SqlCmd.CommandText = updatePlayerData_Command;
                update_SqlCmd.ExecuteNonQuery();
            }
        }
        Debug.Log("[DB] game table complete!");
    }

    // 플레이어 데이터 불러오기, DB에서 불러와서 서버가 클라이언트한테 쏴줄수 있게 string으로 묶어서 반환형 string
    public List<string> LoadPlayerData(ClientLoginStatus loginstatus, int clientlicensenumber)
    {
        List<string> return_TableData = new List<string>();
        string return_TempData;

        string selectTable_Command;
        MySqlCommand select_SqlCmd = new MySqlCommand();
        select_SqlCmd.Connection = connection;


        for(int i = 0; i < table.list.Count; i++) // table_List
        {
            // 저 테이블에 있는 licensenumber만 읽어오고 다른 컬럼을 읽어오지 않았는데 다른 컬럼에 있는 값을 가져오려니 당연히 오류가 나지 않을까?
            //selectTable_Command = $"SELECT User_LicenseNumber FROM {table.list[i]}";
            selectTable_Command = $"SELECT * FROM {table.list[i]}";
            select_SqlCmd.CommandText = selectTable_Command;
            reader = select_SqlCmd.ExecuteReader();

            // table, User_LicenseNumber열에서 행이 있다면
            if (reader.HasRows)
            {
                // 읽는다. 읽는동안
                while (reader.Read())
                {
                    int dbLicenseNumber = reader.GetInt32("User_LicenseNumber");
                    // int dbcharactor = reader.GetInt32("User_Charactor"); 나중에 캐릭터 변경시 필요할 변수. todo

                    // 클라이언트 라이센스와 db에 있는 라이센스가 같다면 데이터를 불러온다
                    if (clientlicensenumber == dbLicenseNumber)
                    {
                        switch(table.list[i])
                        {
                            case "user_info":
                                //string selectColumn_Command = $"SELECT * FROM {table.list[i]}";
                                //MySqlCommand selectColumn_SqlCmd = new MySqlCommand(selectColumn_Command, connection);
                                //MySqlDataReader tempReader = selectColumn_SqlCmd.ExecuteReader();

                                Debug.Log($"[DB] tableName : {table.list[i]}, dbLicenseNumber : {dbLicenseNumber}");
                                Debug.Log("[DB] LoadPlayerData - user_info table");
                                string user_Name = reader.GetString("User_Name");
                                //string user_Name = reader["User_Name"].ToString();
                                Debug.Log("[DB] User_Name Get????");
                                // db에서 binary 타입 데이터 가져와서 string으로 변환
                                //// 실제 읽은 데이터의 길이
                                //int bytesRead = (int)reader.GetBytes(reader.GetOrdinal("User_Profile"), 0, null, 0, 1000);
                                //// 읽은 데이터 길이만큼 배열 생성
                                //byte[] binaryData = new byte[bytesRead];
                                //// 데이터를 읽어와서 배열에 저장 - binaryData(buffer)에 저장됨
                                //reader.GetBytes(reader.GetOrdinal("User_Profile"), 0, binaryData, 0, 1000);
                                //string user_Profile = Convert.ToBase64String(binaryData);
                                string user_Profile = reader.GetInt32("User_Profile").ToString();
                                Debug.Log("[DB] User_Profile Get????");
                                string user_Coin = reader.GetInt32("User_Coin").ToString();
                                Debug.Log("[DB] User_Coin Get????");
                                return_TempData = $"{table.list[i]}|{user_Name}|{user_Profile}|{user_Coin}|{separatorString}"; // 맨 앞에 tag처럼 구분할 수 있게 특정명 기입(User)
                                return_TableData.Add(return_TempData);
                                break;
                            case "rank":
                                Debug.Log("[DB] LoadPlayerData - rank table");
                                string rank_TotalTime = reader.GetString("TotalTime");
                                string rank_TotalScore = reader.GetString("TotalScore");
                                return_TempData = $"{table.list[i]}|{rank_TotalTime}|{rank_TotalScore}|{separatorString}";
                                return_TableData.Add(return_TempData);
                                break;
                            case "achievement":
                                Debug.Log("[DB] LoadPlayerData - achievement table");
                                string achievement_Something = reader.GetInt32("Something").ToString();
                                return_TempData = $"{table.list[i]}|{achievement_Something}|{separatorString}";
                                return_TableData.Add(return_TempData);
                                break;
                            case "pet":
                                Debug.Log("[DB] LoadPlayerData - pet table");
                                string pet_White = reader.GetInt32("White").ToString();
                                return_TempData = $"{table.list[i]}|{pet_White}|{separatorString}";
                                return_TableData.Add(return_TempData);
                                break;
                            default:
                                Debug.Log("[DB] LoadPlayerData - game table");
                                string game_ReactionRate = reader.GetInt32("ReactionRate").ToString();
                                string game_AnswerCount = reader.GetInt32("AnswerCount").ToString();
                                string game_AnswerRate = reader.GetInt32("AnswerRate").ToString();
                                string game_Playtime = reader.GetInt32("Playtime").ToString();
                                string game_TotalScore = reader.GetInt32("TotalScore").ToString();
                                string game_StarPoint = reader.GetInt32("StarPoint").ToString();
                                return_TempData = $"{table.list[i]}|{game_ReactionRate}|{game_AnswerCount}|{game_AnswerRate}|{game_Playtime}|{game_TotalScore}|{game_StarPoint}|{separatorString}";
                                return_TableData.Add(return_TempData);
                                break;
                        }
                    }
                }
            }

            reader.Close();
        }

        return return_TableData;
    }

    public void SaveGameResultData(List<string> dataList)
    {
        // DB gametable column순 : User_Licensenumber/User_Charactor/ReactionRate/AnswerCount/AnswerRate/Playtime/TotalScore/StarPoint
        // dataList는 [0]을 제외하고 value(int)만 있음. index순으로 RequestName[0]/level[1]/step[2]/User_Licensenumber[3]/User_Charactor[4]/ReactionRate[5]/.../TotalScore[9]
        string gameName = dataList[0].Split("[Save]", StringSplitOptions.RemoveEmptyEntries).ToString();
        string table_Name = $"{gameName}_level{dataList[1]}_step{dataList[2]}";
        if(gameName == "venezia_chn") table_Name = $"{gameName}_level_step{dataList[2]}";

        string[] columns_Name = { "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerCount", "AnswerRate", "Playtime", "TotalScore", "StarPoint"};
        // gametable에 저장된 totalscore 가져와서 비교 (한 게임의 최고점수)
        int gameresult_Score = Int32.Parse(dataList[9]);
        int db_Score = 0;

        // Score로 StarPoint(0,1,2,3) 할당
        int starPoint = 0;

        // DB 조회
        string selectGameTable_Command = $"SELECT ({columns_Name[0]}, {columns_Name[1]}, {columns_Name[6]}) FROM {table_Name}";
        MySqlCommand select_SqlCmd = new MySqlCommand(selectGameTable_Command, connection);
        MySqlDataReader reader = select_SqlCmd.ExecuteReader();
        
        // DB에 있는 licensenumber와 charactor가 저장하려는 데이터의 licensenumber와 charactor가 같다면 (해당 행에 있는 값 가져옴)
        if(reader.GetInt32(0) == Int32.Parse(dataList[3]) && reader.GetInt32(1) == Int32.Parse(dataList[4]))
        {
            db_Score = reader.GetInt32(columns_Name[6]);
        }
        reader.Close();

        // 새 게임점수와 db 게임점수 비교
        if(gameresult_Score >= db_Score) // 저장
        {
            // gameresult_Score로 StarPoint 할당함
            if (gameresult_Score >= 25000) starPoint = 3;
            else if (gameresult_Score >= 12500) starPoint = 2;
            else if (gameresult_Score >= 6500) starPoint = 1;

            // rank table에 TotalTime과 TotalScore에 점수 누적
            UpdateRankTable(Int32.Parse(dataList[1]), Int32.Parse(dataList[2]), Int32.Parse(dataList[8]), gameresult_Score);
        }
        else // 저장 X
        {
            // 저장할 필요가 없으므로 rank table에만 TotalTime과 TotalScore에 점수 누적 후 return
            UpdateRankTable(Int32.Parse(dataList[1]), Int32.Parse(dataList[2]), Int32.Parse(dataList[8]), gameresult_Score);
            return;
        }

        // starpoint value값 list에 추가
        dataList.Add(starPoint.ToString());

        // 컬럼명-값 Tuple
        Tuple<string, int>[] columns_Tuple = new Tuple<string, int>[8];
        for(int i = 0; i < columns_Tuple.Length; i++)
        {
            columns_Tuple[i] = new Tuple<string, int>(columns_Name[i], Int32.Parse(dataList[i+3]));
        }

        // 게임테이블에 values update
        // i=0 -> licensenumber / i=1 -> charactor ==> table에서 특정조건(라이센스,캐릭터)을 만족하는 column을 업데이트함
        for(int i = 2; i < columns_Tuple.Length; i++)
        {
            string updateGameData_Command = $"UPDATE {table_Name} SET {columns_Tuple[i].Item1} = {columns_Tuple[i].Item2} WHERE {columns_Tuple[0].Item1} = {columns_Tuple[0].Item2} AND {columns_Tuple[1].Item1} = {columns_Tuple[1].Item2}";
            MySqlCommand update_SqlCmd = new MySqlCommand(updateGameData_Command, connection);
            update_SqlCmd.ExecuteNonQuery();
        }
    }

    // 게임이 끝날때마다 rank table에 시간, 점수 누적
    private void UpdateRankTable(int licensenumber, int charactor, int time, int score)
    {
        Debug.Log($"[DB] Updating... rank table, licensenumber : {licensenumber}, characator : {charactor}");
        string rankTable = "rank";
        string[] columns = { "User_LicenseNumber", "User_Charactor", "TotalTime", "TotalScore"};
        int totalTime = 0;
        int totalScore = 0;

        string selectRankData_Command = $"SELECT * FROM {rankTable}";
        MySqlCommand select_SqlCmd = new MySqlCommand(selectRankData_Command, connection);
        MySqlDataReader reader = select_SqlCmd.ExecuteReader();

        if((reader.GetInt32(columns[0]) == licensenumber) && (reader.GetInt32(columns[1]) == charactor))
        {
            totalTime = reader.GetInt32(columns[3]);
            totalScore = reader.GetInt32(columns[4]);
        }
        reader.Close();

        totalTime += time;
        totalScore += score;

        string updateRankData_Command = $"UPDATE {rankTable} SET {columns[3]} = {totalTime} WHERE {columns[0]} = {licensenumber} AND {columns[1]} = {charactor}";
        MySqlCommand update_SqlCmd = new MySqlCommand(updateRankData_Command, connection);
        update_SqlCmd.ExecuteNonQuery();
        update_SqlCmd.CommandText = $"UPDATE {rankTable} SET {columns[4]} = {totalScore} WHERE {columns[0]} = {licensenumber} AND {columns[1]} = {charactor}";
        update_SqlCmd.ExecuteNonQuery();

        //UPDATE `test`.`rank` SET `TotalTime`= '5' WHERE  `User_LicenseNumber`= 2000 AND `User_Charactor`= 1 AND `TotalTime`= 1 AND `TotalScore`= 1 LIMIT 1;
        //updatePlayerData_Command = $"UPDATE {achievement_TableName} SET {achievement_Columns[i]} = {achievement_valuepart}";
        //update_SqlCmd.CommandText = updatePlayerData_Command;
        //update_SqlCmd.ExecuteNonQuery();
    }



    // Login, Client에서 불러올것임, 로그인할때 아이디, 비번 입력
    //public bool Login(string id, string pw)
    //{
    //    // 데이터를 DB에서 가져옴
    //    // 조회되는 데이터가 없으면 False
    //    // 조회가 되는 데이터가 있으면 True인데, 위 user_Info에다가 담을거임
    //    // 1. Connection이 Open인지 확인 - 메서드
    //    // 2. Reader 상태가 읽고 있는지 확인 - 한 쿼리문당 하나
    //    // 3. 데이터를 다 읽었으면 Close();

    //    try
    //    {
    //        // 1번
    //        if (!CheckConnection(connection))
    //        {
    //            Debug.Log("DB에 연결되지 않았습니다.");
    //            return false;
    //        }

    //        string login_Command = string.Format(@"SELECT User_Name, User_Password FROM User_Info WHERE User_Name = '{0}' AND User_Password = '{1}';", id, pw);
    //        MySqlCommand cmd = new MySqlCommand(login_Command, connection);
    //        reader = cmd.ExecuteReader(); // SELECT

    //        // Reader가 읽은게 하나이상 존재하면
    //        if(reader.HasRows)
    //        {
    //            // 읽은 데이터를 하나씩 나열
    //            while (reader.Read())
    //            {
    //                string name = (reader.IsDBNull(0)) ? string.Empty : (string)reader["User_Name"].ToString();
    //                string password = (reader.IsDBNull(1)) ? string.Empty : (string)reader["User_Password"].ToString();

    //                // 둘 다 비어있지 않다면 어떤 값이 되었든 데이터가 불러와졌다는것
    //                if (!name.Equals(string.Empty) || !password.Equals(string.Empty))
    //                {
    //                    user_Info = new User_Info(name, password); // DB에서 해당 유저의 이름과 비밀번호를 가져옴

    //                    // 리더기가 닫혀있지 않다면 닫기
    //                    if (!reader.IsClosed) reader.Close();
    //                    return true;
    //                }
    //                else
    //                {
    //                    break;
    //                }
    //            }
    //        }

    //        // 위에서 제대로 파일을 읽지 못했다면 리더기 종료하고 false 반환
    //        if (!reader.IsClosed) reader.Close();
    //        return false;
    //    }
    //    catch(Exception e)
    //    {
    //        Debug.Log(e.Message);
    //        // 에러나면 리더기 종료 후 false반환
    //        if (!reader.IsClosed) reader.Close();
    //        return false;
    //    }
    //}
}
