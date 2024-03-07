using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data;
using MySql.Data.MySqlClient;
using UnityEngine.UI;
using System;
using System.Text;
using System.Linq;

// �̱��� ����
// Ŭ���̾�Ʈ�� ������ ����������? Ŭ���̾�Ʈ�� �̹� ��ü������ IP�� Port�� �˾Ƽ� ������ ������
// Ŭ���̾�Ʈ�� �α��� �Ҷ� ID�� Password�� �Է����ٵ�, �׶� ������ �ִ� DB���� login table�� ���ؼ� true�� �α���
// ȸ����ϵ� �ϴ� �������´�
// DBManager�� �������� �����ϴ°� �´�
public class DBManager : MonoBehaviour
{
    MySqlConnection connection; // DB�� �����ϴ� Ŭ����
    MySqlDataReader reader;

    //public string path = string.Empty;

    // �����Ҷ� �ʿ��� ����
    private string str_Connection;

    // Table List, AnalyticsTable List
    private TableName table;
    private AnalyticsTableName analyticsTable;

    // Table - Columns
    private string[] userinfo_Columns;
    private string[] rank_Columns;
    private string[] achievement_Columns;
    private string[] pet_Columns;
    private string[] game_Columns;
    private string[] analytics_Columns;

    // ����-Ŭ���̾�Ʈ string���� data �ְ������ �����ϱ� ���� ���ڿ�
    private const string separatorString = "E|";

    // LicenseNumber �⺻, Table ���Ӹ�(���Ǽ��� ����)
    private int clientLicenseNumber_Base = 10000;

    // ��Ÿ ������ ó���� Handler
    private IETCMethodHandler etcMethodHandler;

    public static DBManager instance = null; // �̱��� ������

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

    // DB ���ǿ� ������ �� �ʿ��� ���� �ʱ�ȭ
    private void SetStringConnection()
    {
        string ip = "127.0.0.1"; // �켱 ����(127.0.0.1)��, aws EC2 IP : 15.165.159.141
        string db = "present";
        string uid = "root"; //string.IsNullOrEmpty(user_Info.user_Name)? "" : user_Info.user_Name;
        string pw = "12345678"; //string.IsNullOrEmpty(user_Info.user_Password)? "" : user_Info.user_Password;
        string port = "3306";
        //str_Connection = $"Server={ip};Database={db};Uid={uid};Pwd={pw};Port={port};Charset=utf8;";
        str_Connection = $"Server={ip};" + $"Database={db};" + $"Uid={uid};" + $"Pwd={pw};" + $"Port={port};" + "CharSet=utf8;"; // ; �����ݷ� �����ض�
        Debug.Log($"[DB] DB connect info : {str_Connection}");
    }

    // DB�� ����
    private void ConnectDB()
    {
        try
        {
            connection = new MySqlConnection(str_Connection);
            connection.Open(); // DB ����
            Debug.Log("[DB] Success connect to db!");
        }
        catch(Exception e)
        {
            Debug.Log($"[DB] Fail connect to db + {e.Message}");
        }
    }

    // DB �翬��
    private bool CheckConnection(MySqlConnection con)
    {
        // open�� �ȵǾ��ִٸ�
        if(con.State != System.Data.ConnectionState.Open)
        {
            con.Open(); // �ѹ� �� �����
            if (con.State != System.Data.ConnectionState.Open)
            {
                return false; //������� �ߴµ��� �ȵǸ� �̻��� ����
            }
        }
        return true;
    }

    // DB Table�� data�� ������ �� �ʿ��� ���� ����
    private void InitSetting()
    {
        // Tables / TableName ��ü ������ table.list �ڵ� ����
        table = new TableName(); 

        // Table - Columns
        userinfo_Columns = new string[]{ "User_LicenseNumber", "User_Charactor", "User_Name", "User_Profile", "User_Birthday", "User_TotalAnswers", "User_TotalTime", "User_Coin" };
        rank_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "User_Profile", "User_Name", "TotalTime", "TotalScore" };
        achievement_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "Something" };
        pet_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "White" };
        game_Columns = new string[]{ "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerCount", "AnswerRate", "Playtime", "TotalScore", "StarPoint" };
        analytics_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerRate" };
    }

    // ������� // todo �񵿱�ȭ �����غ�����
    public string CreateLicenseNumber()
    {
        Debug.Log("[DB] Come in CreateLicenseNumber method");

        if(!CheckConnection(connection))
        {
            Debug.Log("[DB] Didn't connect to DB");
            return "";
        }

        int count = 0;

        // DB user_info ���̺��� User_LicenseNumber�� � �ִ��� üũ
        string checkUserInfo_Command = $"SELECT `User_LicenseNumber` FROM `user_info`";
        MySqlCommand check_SqlCmd = new MySqlCommand(checkUserInfo_Command, connection);
        reader = check_SqlCmd.ExecuteReader();

        // table üũ
        if (reader.HasRows)
        {
            while(reader.Read())
            {
                count++;
            }
        }
        reader.Close(); // DataReader�� �����ִ� ���� �߰����� ��� ���� �Ұ�

        // user_info ����
        string table_Name = "user_info";
        string[] columns = { "User_LicenseNumber", "User_Charactor", "User_Name", "User_Profile", "User_Birthday", "User_TotalAnswers", "User_TotalTime", "User_Coin" };
        int clientLicenseNumber = clientLicenseNumber_Base + count;
        int user_Charactor = 1;
        string user_Name = "Guest";
        byte[] user_Profile = { 0x00};
        string user_Birthday = "96.02.01";
        int user_TotalAnswers = 0;
        float user_TotalTime = 0f;
        int user_Coin = 0;

        // Binary Parameter
        //MySqlParameter binaryParameter = new MySqlParameter

        // �����ϴ� ������
        string createAccount_Command = $"INSERT INTO {table_Name} ({columns[0]}, {columns[1]}, {columns[2]}, {columns[3]}, {columns[4]}, {columns[5]}, {columns[6]}, {columns[7]}) " +
                                        $"VALUES (@clientLicenseNumber, @user_Charactor, @user_Name, @user_Profile, @user_Birthday, @user_TotalAnswers, @user_TotalTime, @user_Coin)"; // `(����ǥ), '(��ƽ) �����ϱ�
        MySqlCommand insert_SqlCmd = new MySqlCommand(createAccount_Command, connection);
        insert_SqlCmd.Parameters.Add("@clientLicenseNumber", MySqlDbType.Int32).Value = clientLicenseNumber;
        insert_SqlCmd.Parameters.Add("@user_Charactor", MySqlDbType.Int32).Value = user_Charactor;
        insert_SqlCmd.Parameters.Add("@user_Name", MySqlDbType.VarChar).Value = user_Name;
        insert_SqlCmd.Parameters.Add("@user_Profile", MySqlDbType.MediumBlob).Value = user_Profile;
        insert_SqlCmd.Parameters.Add("@user_Birthday", MySqlDbType.MediumBlob).Value = user_Birthday;
        insert_SqlCmd.Parameters.Add("@user_TotalAnswers", MySqlDbType.MediumBlob).Value = user_TotalAnswers;
        insert_SqlCmd.Parameters.Add("@user_TotalTime", MySqlDbType.MediumBlob).Value = user_TotalTime;
        insert_SqlCmd.Parameters.Add("@user_Coin", MySqlDbType.Int32).Value = user_Coin;
        
        insert_SqlCmd.ExecuteNonQuery(); // command.ExecuteNonQuery()�� DB���� ���� �۾��� �����ϴ� SQL ��ɹ��� �����ϰ�, ������ ���� ���� ���� ��ȯ�ϴ� �޼���

        string returnData = $"{clientLicenseNumber}|{user_Charactor}";
        return returnData;
    }

    // �� ĳ���� ������ ����
    public void CreateNewCharactorData(int clientlicensenumber, int clientcharactor)
    {
        Debug.Log($"[DB] Create new Charactor data, client's licensenumber : {clientlicensenumber}");

        // ClientLicenseNumber�� �߱޵Ǹ� �� �÷��̾� ������ ����� ���̹Ƿ�, ��� ���̺� data�� �߰��ؾ���
        string insertCreatePlayerData_Command;
        MySqlCommand insert_SqlCmd = new MySqlCommand();
        insert_SqlCmd.Connection = connection;

        // Update Query
        string updatePlayerData_Command;
        MySqlCommand update_SqlCmd = new MySqlCommand();
        update_SqlCmd.Connection = connection;

        // table.list[0] -> userinfo, [1]->rank, [2]->achievement, [3]->pet

        // rank table
        // table.list[1] = "rank"
        // rank_Columns = { "User_LicenseNumber", "User_Charactor", "User_Profile", "User_Name", "TotalTime", "TotalScore" };
        int rank_valuepart;

        // insert row
        insertCreatePlayerData_Command = $"INSERT INTO {table.list[1]} ({rank_Columns[0]}) VALUES ({clientlicensenumber})";
        insert_SqlCmd.CommandText = insertCreatePlayerData_Command;
        insert_SqlCmd.ExecuteNonQuery();

        for (int i = 0; i < rank_Columns.Length; i++) // 0->licensenumber, 1->charactor
        {
            if (i == 0) rank_valuepart = clientlicensenumber;
            else if (i == 1) rank_valuepart = clientcharactor;
            else rank_valuepart = 0;

            updatePlayerData_Command = $"UPDATE `{table.list[1]}` SET `{rank_Columns[i]}` = '{rank_valuepart}' WHERE `{rank_Columns[0]}` = '{clientlicensenumber}'";
            update_SqlCmd.CommandText = updatePlayerData_Command;
            update_SqlCmd.ExecuteNonQuery();
        }
        Debug.Log("[DB] Rank table complete!");

        // achievement table
        //string achievement_TableName = "achievement";
        //string[] achievement_Columns = { "User_LicenseNumber", "User_Charactor", "Something"};
        int achievement_valuepart;

        // insert row
        insertCreatePlayerData_Command = $"INSERT INTO {table.list[2]} ({achievement_Columns[0]}) VALUES ({clientlicensenumber})";
        insert_SqlCmd.CommandText = insertCreatePlayerData_Command;
        insert_SqlCmd.ExecuteNonQuery();

        for (int i = 0; i < achievement_Columns.Length; i++)
        {
            if (i == 0) achievement_valuepart = clientlicensenumber;
            else if (i == 1) achievement_valuepart = 1;
            else achievement_valuepart = 0;

            updatePlayerData_Command = $"UPDATE `{table.list[2]}` SET `{achievement_Columns[i]}` = '{achievement_valuepart}' WHERE `{achievement_Columns[0]}` = '{clientlicensenumber}'";
            update_SqlCmd.CommandText = updatePlayerData_Command;
            update_SqlCmd.ExecuteNonQuery();
        }
        Debug.Log("[DB] achievement table complete!");

        // pet table
        //string pet_TableName = "pet";
        //string[] pet_Columns = { "User_LicenseNumber", "User_Charactor", "White" };
        int pet_valuepart;

        // insert row
        insertCreatePlayerData_Command = $"INSERT INTO `{table.list[3]}` (`{pet_Columns[0]}`) VALUES ('{clientlicensenumber}')";
        insert_SqlCmd.CommandText = insertCreatePlayerData_Command;
        insert_SqlCmd.ExecuteNonQuery();

        for (int i = 0; i < pet_Columns.Length; i++)
        {
            if (i == 0) pet_valuepart = clientlicensenumber;
            else if (i == 1) pet_valuepart = 1;
            else pet_valuepart = 0;

            updatePlayerData_Command = $"UPDATE `{table.list[3]}` SET `{pet_Columns[i]}` = '{pet_valuepart}' WHERE `{pet_Columns[0]}` = '{clientlicensenumber}'";
            if (i==2) // float
            {
                updatePlayerData_Command = $"UPDATE `{table.list[3]}` SET `{pet_Columns[i]}` = '{(float)pet_valuepart}'";
            }
            update_SqlCmd.CommandText = updatePlayerData_Command;
            update_SqlCmd.ExecuteNonQuery();
        }
        Debug.Log("[DB] pet table complete!");

        // game table
        string game_TableName;

        string[] game_Names = { "venezia_kor", "venezia_eng", "venezia_chn", "calculation", "gugudan"};
        int[] levels = { 1, 2, 3 };
        int[] steps = { 1, 2, 3, 4, 5, 6 };

        //string[] game_Columns = { "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerCount", "AnswerRate", "Playtime", "TotalScore", "StarPoint" };

        int user_Charactor = 1;
        float init_Float = 0f;
        int init_Int = 0;

        List<string> game_TableList = new List<string>();

        // game_TableList �߰�
        for (int i = 0; i < game_Names.Length; i++)
        {
            for (int j = 0; j < levels.Length; j++)
            {
                for (int k = 0; k < steps.Length; k++)
                {
                    string levelpart = $"level{levels[j]}";
                    if (game_Names[i] == "venezia_chn")
                    {
                        j = 2; // venezia_chn ������ level�� 1�����̹Ƿ� �ѹ��� ���ƾ���.
                        levelpart = "level";
                    }
                    game_TableName = $"{game_Names[i]}_{levelpart}_step{steps[k]}";
                    game_TableList.Add(game_TableName);
                }
            }
        }

        // ���� ���̺� ���� DB�� insert �� update
        for (int i = 0; i < game_TableList.Count; i++)
        {
            // insert row
            insertCreatePlayerData_Command = $"INSERT INTO {game_TableList[i]} (`User_LicenseNumber`, `User_Charactor`) VALUES ({clientlicensenumber}, {user_Charactor})";
            insert_SqlCmd.CommandText = insertCreatePlayerData_Command;
            insert_SqlCmd.ExecuteNonQuery();

            // update rowm j=2���� �����ϴ� ������ 0�� licensenumber�� 1�� charactor��ȣ�̰� insert�� �־���� ������ ���� �� ������Ʈ�� �� �ʿ䰡 ����
            for (int j = 2; j < game_Columns.Length; j++)
            {
                updatePlayerData_Command = $"UPDATE {game_TableList[i]} SET {game_Columns[j]} = @value  WHERE `{game_Columns[0]}` = '{clientlicensenumber}'" ;
                update_SqlCmd.CommandText = updatePlayerData_Command;

                // parameter �ʱ�ȭ
                update_SqlCmd.Parameters.Clear();

                if (j == 2 || j == 5)
                {
                    update_SqlCmd.Parameters.Add("@value", MySqlDbType.Float).Value = init_Float;
                }
                else
                {
                    update_SqlCmd.Parameters.Add("@value", MySqlDbType.Int32).Value = init_Int;
                }
                update_SqlCmd.ExecuteNonQuery();
            }
        }

        Debug.Log("[DB] game table complete!");
    }

    // DB�� ĳ���� �̸� ����
    public void SaveCharactorName(List<string> dataList)
    {
        // user_info table�� rank table�� ������Ʈ
        // dataList -> [0]requestName / [1]license / [2]charactor / [3]name
        Debug.Log("[DB] Come in SaveCharactorName method");

        // ��������
        string update_Command;
        MySqlCommand update_sqlCmd;

        // user_info == table.list[0]
        update_Command = $"UPDATE `{table.list[0]}` SET `{userinfo_Columns[2]}` = '{dataList[3]}' " +
                         $"WHERE `{userinfo_Columns[0]}` = '{Int32.Parse(dataList[1])}' AND `{userinfo_Columns[1]}` = '{Int32.Parse(dataList[2])}';";
        update_sqlCmd = new MySqlCommand(update_Command, connection);
        update_sqlCmd.ExecuteNonQuery();

        // rank == table.list[1]
        update_Command = $"UPDATE `{table.list[1]}` SET `{rank_Columns[3]}` = '{dataList[3]}' " +
                         $"WHERE `{rank_Columns[0]}` = '{Int32.Parse(dataList[1])}' AND `{rank_Columns[1]}` = '{Int32.Parse(dataList[2])}';";
        update_sqlCmd = new MySqlCommand(update_Command, connection);
        update_sqlCmd.ExecuteNonQuery();

        Debug.Log("[DB] Complete save charactor name to DB");
    }

    // DB�� ĳ���� ������(�̹���) ����
    public void SaveCharactorProfile(List<string> dataList)
    {
        // user_info table�� rank table�� ������Ʈ
        // dataList -> [0]requestName / [1]license / [2]charactor / [3]profile(Base64)
        // Profile Base64 �������� DB�� ����
        Debug.Log("[DB] Come in SaveCharactorProfile method");


        // ��������
        string update_Command;
        MySqlCommand update_sqlCmd;

        // user_info == table.list[0]
        update_Command = $"UPDATE `{table.list[0]}` SET `{userinfo_Columns[3]}` = '{dataList[3]}' " +
                         $"WHERE `{userinfo_Columns[0]}` = '{Int32.Parse(dataList[1])}' AND `{userinfo_Columns[1]}` = '{Int32.Parse(dataList[2])}';";
        Debug.Log(update_Command);
        update_sqlCmd = new MySqlCommand(update_Command, connection);
        update_sqlCmd.ExecuteNonQuery();

        // rank == table.list[1]
        update_Command = $"UPDATE `{table.list[1]}` SET `{rank_Columns[2]}` = '{dataList[3]}' " +
                         $"WHERE `{rank_Columns[0]}` = '{Int32.Parse(dataList[1])}' AND `{rank_Columns[1]}` = '{Int32.Parse(dataList[2])}';";
        update_sqlCmd = new MySqlCommand(update_Command, connection);
        update_sqlCmd.ExecuteNonQuery();

        Debug.Log("[DB] Complete save charactor profile to DB");
    }

    // DB�� ĳ���� ������ ���� (ĳ���� ���� �Ǵ� ���� �����)
    public void SaveCharactorData(List<string> dataList)
    {
        Debug.Log("[DB] Come in save charactor data");
        // dataList[0] : $"{requestName}|{clientLicenseNumber}|{clientCharactor}|"
        // dataList[1] : $"{table.list[0]}|{playerdb.playerName}|{Convert.ToBase64String(playerdb.image)}|{playerdb.BirthDay}|{playerdb.TotalAnswers}|{playerdb.TotalTime}|";
        // dataList[2~n-1] : $"{table.list[i]}|{reactionRate_List[i - 4]}|{answersCount_List[i - 4]}|{answers_List[i - 4]}|{playTime_List[i - 4]}|{totalScore_List[i - 4]}|{starCount_List[i - 4]}|}";
        // dataList[n] : $"Finish"

        for(int i = 0; i<dataList.Count; i++)
        {
            Debug.Log($"[DB] charactor dataList{i} : {dataList[i]}");
        }
        string update_Command;
        MySqlCommand update_SqlCmd = new MySqlCommand();
        update_SqlCmd.Connection = connection;
        Debug.Log($"[DB] Check dataList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[1] : {Int32.Parse(dataList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[1])}");
        Debug.Log($"[DB] Check dataList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[2] : {Int32.Parse(dataList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[2])}");
        int clientLicenseNumber = Int32.Parse(dataList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[1]);
        int clientCharactor = Int32.Parse(dataList[0].Split('|', StringSplitOptions.RemoveEmptyEntries)[2]);

        //userinfo_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "User_Name", "User_Profile", "User_Birthday", "User_TotalAnswers", "User_TotalTime", "User_Coin" };
        //game_Columns = new string[] { "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerCount", "AnswerRate", "Playtime", "TotalScore", "StarPoint" };

        for(int i = 1; i < dataList.Count; i++)
        {
            List<string> tempAllocate = dataList[i].Split('|', StringSplitOptions.RemoveEmptyEntries).ToList();
            if (tempAllocate.Contains("Finish")) break;

            if (i == 1) // userinfo table
            {
                update_Command = $"UPDATE `{tempAllocate[0]}` SET ";
                tempAllocate.RemoveAt(0);
                for(int j = 2; j < userinfo_Columns.Length-1; j++) // 0,1 - license, charactor , todo ���߿� ���� playerdata�� �߰��Ǹ� ���� ������� Length-1 -> Length
                {
                    if (j == 2) update_Command += $"`{userinfo_Columns[j]}` = '{tempAllocate[j - 2]}', "; // name, string
                    else if(j == 3) update_Command += $"`{userinfo_Columns[j]}` = '{tempAllocate[j - 2]}', "; // profile, Base64 (MediumBlob)
                    else if(j == 4) update_Command += $"`{userinfo_Columns[j]}` = '{tempAllocate[j - 2]}', "; // birthday, string
                    else if (j == 5) update_Command += $"`{userinfo_Columns[j]}` = {Int32.Parse(tempAllocate[j - 2])}, "; // totalanswers, int
                    else if (j == 6) update_Command += $"`{userinfo_Columns[j]}` = {float.Parse(tempAllocate[j - 2])} "; // totaltime, float
                }
                update_Command += $"WHERE `{userinfo_Columns[0]}` = {clientLicenseNumber} AND `{userinfo_Columns[1]}` = {clientCharactor};";
            }
            else // game table
            {
                update_Command = $"UPDATE `{tempAllocate[0]}` SET ";
                tempAllocate.RemoveAt(0);
                for (int j = 2; j < game_Columns.Length; j++) // 0,1 - license, charactor
                {   
                    if (j == 2) update_Command += $"`{game_Columns[j]}` = {float.Parse(tempAllocate[j - 2])}, "; //reactionrate, float
                    else if(j == 3) update_Command += $"`{game_Columns[j]}` = {Int32.Parse(tempAllocate[j - 2])}, "; // answercount, int
                    else if(j == 4) update_Command += $"`{game_Columns[j]}` = {Int32.Parse(tempAllocate[j - 2])}, "; // answerrate, int
                    else if(j == 5) update_Command += $"`{game_Columns[j]}` = {float.Parse(tempAllocate[j - 2])}, "; // playtime, float
                    else if(j == 6) update_Command += $"`{game_Columns[j]}` = {Int32.Parse(tempAllocate[j - 2])}, "; // totalscore, int
                    else if(j == 7) update_Command += $"`{game_Columns[j]}` = {Int32.Parse(tempAllocate[j - 2])} "; // starpoint, int
                }
                update_Command += $"WHERE `{game_Columns[0]}` = {clientLicenseNumber} AND `{game_Columns[1]}` = {clientCharactor};";
            }

            Debug.Log($"[DB] update_Command : {update_Command}");
            update_SqlCmd.CommandText = update_Command;
            update_SqlCmd.ExecuteNonQuery();
            Debug.Log("[DB] In progress save charactor data");
        }

        Debug.Log("[DB] Complete save charactor data");
    }

    // DB�� ���Ӱ�� ����
    public void SaveGameResultData(List<string> dataList)
    {
        // DB gametable column�� : User_Licensenumber/User_Charactor/ReactionRate/AnswerCount/AnswerRate/Playtime/TotalScore/StarPoint
        // dataList�� [0]�� �����ϰ� value(int)�� ����. index������ RequestName[0]/level[1]/step[2]/User_Licensenumber[3]/User_Charactor[4]/ReactionRate[5]/.../TotalScore[9]
        Debug.Log("[DB] Come in SaveGameResultData method");
        Debug.Log($"[DB] SaveGameResultData, gameName : {dataList[0]}"); // [Save]gameName
        string gameName = dataList[0].Substring("[Save]".Length); // [Save] ����
        Debug.Log($"[DB] gameName : {gameName}");

        string table_Name = $"{gameName}_level{dataList[1]}_step{dataList[2]}";
        if(gameName == "venezia_chn") table_Name = $"{gameName}_level_step{dataList[2]}";

        // gametable�� ����� totalscore �����ͼ� �� (�� ������ �ְ�����)
        int gameresult_Score = Int32.Parse(dataList[9]);
        int db_Score = 0;

        // Score�� StarPoint(0,1,2,3) �Ҵ�
        int starPoint = 0;

        // DB ��ȸ
        Debug.Log($"[DB] SelectGameTable ....");
        string selectGameTable_Command = $"SELECT `{game_Columns[0]}`, `{game_Columns[1]}`, `{game_Columns[6]}` FROM `{table_Name}`";
        Debug.Log($"[DB] selectGameTable_Command : {selectGameTable_Command}");
        MySqlCommand select_SqlCmd = new MySqlCommand(selectGameTable_Command, connection);
        MySqlDataReader reader = select_SqlCmd.ExecuteReader();
        Debug.Log($"[DB] reader is alright?");

        while(reader.Read())
        {
            Debug.Log($"[DB] reader has come While()?");
            try
            {
                // DB�� �ִ� licensenumber�� charactor�� �����Ϸ��� �������� licensenumber�� charactor�� ���ٸ� (�ش� �࿡ �ִ� �� ������)
                if (reader.GetInt32(0) == Int32.Parse(dataList[3]) && reader.GetInt32(1) == Int32.Parse(dataList[4]))
                {
                    db_Score = reader.GetInt32(game_Columns[6]);
                    Debug.Log($"[DB] db_Score = {db_Score}");
                }
            }
            catch(Exception e)
            {
                Debug.Log($"[DB] reader error : {e.Message}");
            }
        }
        reader.Close();

        // �� ���������� db �������� ��
        Debug.Log($"[DB] gameresult_Score, DB_Score : {gameresult_Score}, {db_Score}");
        if (gameresult_Score >= db_Score) // ����
        {
            // gameresult_Score�� StarPoint �Ҵ���
            if (gameresult_Score >= 25000) starPoint = 3;
            else if (gameresult_Score >= 12500) starPoint = 2;
            else if (gameresult_Score >= 6500) starPoint = 1;

            // rank table�� TotalTime�� TotalScore�� ���� ���� 
            UpdateRankTable(Int32.Parse(dataList[3]), Int32.Parse(dataList[4]), float.Parse(dataList[8]), gameresult_Score);
        }
        else // ���� X
        {
            // ������ �ʿ䰡 �����Ƿ� rank table���� TotalTime�� TotalScore�� ���� ���� �� return
            UpdateRankTable(Int32.Parse(dataList[3]), Int32.Parse(dataList[4]), float.Parse(dataList[8]), gameresult_Score);
            return;
        }

        Debug.Log($"[DB] starPoint : {starPoint}");
        // starpoint value�� list�� �߰�
        dataList.Add(starPoint.ToString());
        Debug.Log($"[DB] dataList's Count : {dataList.Count}");
        Debug.Log($"[DB] dataList[9]'s value : {dataList[9]}");
        Debug.Log($"[DB] dataList[10]'s value : {dataList[10]}");

        for (int i = 2; i < game_Columns.Length; i++) // game_Columns = [0]~[7]
        {
            string updateGameData_Command = "";
            if (i == 2 || i == 5) // float
            {
                updateGameData_Command = $"UPDATE `{table_Name}` SET `{game_Columns[i]}` = '{float.Parse(dataList[i + 3])}' WHERE `{game_Columns[0]}` = '{Int32.Parse(dataList[3])}' AND `{game_Columns[1]}` = '{Int32.Parse(dataList[4])}'";
            }
            else
            {
                updateGameData_Command = $"UPDATE `{table_Name}` SET `{game_Columns[i]}` = '{Int32.Parse(dataList[i + 3])}' WHERE `{game_Columns[0]}` = '{Int32.Parse(dataList[3])}' AND `{game_Columns[1]}` = '{Int32.Parse(dataList[4])}'";
            }
            MySqlCommand update_SqlCmd = new MySqlCommand(updateGameData_Command, connection);
            update_SqlCmd.ExecuteNonQuery();
        }
    }

    // ������ ���������� rank table�� �ð�, ���� ����
    private void UpdateRankTable(int licensenumber, int charactor, float time, int score)
    {
        // rank table -> [0]:User_LicenseNumber, [1]:User_Charactor, [2]:User_Profile, [3]:User_Name, [4]:TotalTime, [5]"TotalScore
        Debug.Log("[DB] Come in UpdateRankTable method");
        Debug.Log($"[DB] licensenumber : {licensenumber}, characator : {charactor}");
        string rankTable = "rank";
        float totalTime = 0;
        int totalScore = 0;

        string selectRankData_Command = $"SELECT * FROM {rankTable}";
        MySqlCommand select_SqlCmd = new MySqlCommand(selectRankData_Command, connection);
        MySqlDataReader reader = select_SqlCmd.ExecuteReader();

        while(reader.Read())
        {
            if ((reader.GetInt32(rank_Columns[0]) == licensenumber) && (reader.GetInt32(rank_Columns[1]) == charactor))
            {
                totalTime = reader.GetFloat(rank_Columns[4]);
                totalScore = reader.GetInt32(rank_Columns[5]);
            }
        }
        reader.Close();

        totalTime += time;
        totalScore += score;

        string updateRankData_Command = $"UPDATE {rankTable} SET {rank_Columns[4]} = {totalTime} WHERE {rank_Columns[0]} = {licensenumber} AND {rank_Columns[1]} = {charactor}";
        MySqlCommand update_SqlCmd = new MySqlCommand(updateRankData_Command, connection);
        update_SqlCmd.ExecuteNonQuery();
        update_SqlCmd.CommandText = $"UPDATE {rankTable} SET {rank_Columns[5]} = {totalScore} WHERE {rank_Columns[0]} = {licensenumber} AND {rank_Columns[1]} = {charactor}";
        update_SqlCmd.ExecuteNonQuery();
    }

    

    // �÷��̾� ������ �ҷ�����, DB���� �ҷ��ͼ� ������ Ŭ���̾�Ʈ���� ���ټ� �ְ� string���� ��� ��ȯ�� string
    public List<string> LoadCharactorData(int clientlicensenumber, int clientcharactor)
    {
        Debug.Log("[DB] Come in LoadCharactorData method");

        List<string> return_TableData = new List<string>();
        string return_TempData;

        string selectTable_Command;
        MySqlCommand select_SqlCmd = new MySqlCommand();
        select_SqlCmd.Connection = connection;


        for (int i = 0; i < table.list.Count; i++) // table_List
        {
            selectTable_Command = $"SELECT * FROM {table.list[i]}";
            select_SqlCmd.CommandText = selectTable_Command;
            reader = select_SqlCmd.ExecuteReader();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    int dbLicenseNumber = reader.GetInt32("User_LicenseNumber");
                    // int dbcharactor = reader.GetInt32("User_Charactor"); ���߿� ĳ���� ����� �ʿ��� ����. todo

                    // Ŭ���̾�Ʈ ���̼����� db�� �ִ� ���̼����� ���ٸ� �����͸� �ҷ��´�
                    if (clientlicensenumber == dbLicenseNumber)
                    {
                        Debug.Log($"[DB] tableName : {table.list[i]}, dbLicenseNumber : {dbLicenseNumber},");
                        switch (table.list[i])
                        {
                            case "user_info":
                                Debug.Log("[DB] LoadCharactorData - user_info table");
                                string user_Name = reader.GetString("User_Name");
                                // DB���� MediumBlob Ÿ�� ������(Base64 �������� �����) string���� ��������
                                string user_Profile = reader.GetString("User_Profile");
                                string user_Birthday = reader.GetString("User_Birthday");
                                string user_TotalAnswers = reader.GetInt32("User_TotalAnswers").ToString();
                                string user_TotalTime = reader.GetFloat("User_TotalTime").ToString();
                                string user_Coin = reader.GetInt32("User_Coin").ToString();
                                // table ������ ���� �� �տ� {table.list[i]} �߰�
                                return_TempData = $"{table.list[i]}|{user_Name}|{user_Profile}|{user_Birthday}|{user_TotalAnswers}|{user_TotalTime}|{user_Coin}|{separatorString}"; 
                                return_TableData.Add(return_TempData);
                                break;
                            case "rank":
                                Debug.Log("[DB] LoadCharactorData - rank table");
                                string rank_TotalTime = reader.GetString("TotalTime");
                                string rank_TotalScore = reader.GetString("TotalScore");
                                return_TempData = $"{table.list[i]}|{rank_TotalTime}|{rank_TotalScore}|{separatorString}";
                                return_TableData.Add(return_TempData);
                                break;
                            case "achievement":
                                Debug.Log("[DB] LoadCharactorData - achievement table");
                                string achievement_Something = reader.GetInt32("Something").ToString();
                                return_TempData = $"{table.list[i]}|{achievement_Something}|{separatorString}";
                                return_TableData.Add(return_TempData);
                                break;
                            case "pet":
                                Debug.Log("[DB] LoadCharactorData - pet table");
                                string pet_White = reader.GetInt32("White").ToString();
                                return_TempData = $"{table.list[i]}|{pet_White}|{separatorString}";
                                return_TableData.Add(return_TempData);
                                break;
                            default:
                                Debug.Log("[DB] LoadCharactorData - game table");
                                string game_ReactionRate = reader.GetFloat("ReactionRate").ToString();
                                string game_AnswerCount = reader.GetInt32("AnswerCount").ToString();
                                string game_AnswerRate = reader.GetInt32("AnswerRate").ToString();
                                string game_Playtime = reader.GetFloat("Playtime").ToString();
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

    // �м� ������ �ҷ����� 
    public List<string> LoadAnalyticsData(int licensenumber, int charactor)
    {
        // Ŭ���̾�Ʈ���� �޾ƾ��� List ����
        // dataList[0] = "[Load]AnalyticsData|E|"
        // dataList[1] = "day1|venezia_kor_level1_analytics|ReactionRate|AnswerRate|E|"
        // dataList[2] = "day1|venezia_kor_level2_analytics|ReactionRate|AnswerRate|E|"
        // dataList[3] = "day1|venezia_kor_level3_analytics|ReactionRate|AnswerRate|E|"
        // dataList[4] = "day1|venezia_eng_level1_analytics|ReactionRate|AnswerRate|E|"
        // dataList[5] = "day1|venezia_eng_level2_analytics|ReactionRate|AnswerRate|E|"
        // dataList[6] = "day1|venezia_eng_level3_analytics|ReactionRate|AnswerRate|E|"
        // dataList[7] = "day1|venezia_chn_analytics|ReactionRate|AnswerRate|E|"
        // dataList[8] = "day1|calculation_level1_anlaytics|ReactionRate|AnswerRate|E|"
        // dataList[9] = "day1|calculation_level2_anlaytics|ReactionRate|AnswerRate|E|"
        // dataList[10] = "day1|calculation_level3_anlaytics|ReactionRate|AnswerRate|E|"
        // dataList[11] = "day1|gugudan_level1_analytics|ReactionRate|AnswerRate|E|"
        // dataList[12] = "day1|gugudan_level2_analytics|ReactionRate|AnswerRate|E|"
        // dataList[13] = "day1|gugudan_level3_analytics|ReactionRate|AnswerRate|E|"

        // ���� �ֱ� ��¥���� ã�Ƶ��� �ϹǷ�(���� �� ����) day1 = ���� �ֱ� ��¥ (List index �ռ����� ��ġ�ǹǷ�)
        // day1 = ���� �ֱ� ��¥ ... day7 = ���� ������ ��¥
        // �ش��ϴ� ��¥�� ã���� �ϴ� ������ �����Ͱ� ���ٸ� �� �� ��¥
        // ���� ��� 24.03.05 DB�� venezia_kor ������ level 1,2�� �÷����� �����Ͱ� �ְ� level 3�� �÷����� �����Ͱ� ����
        // 24.03.04 DB�� veneiza_kor ������ level 1,2,3�� �÷����� �����Ͱ� �ִٸ�
        // ���κм�ǥ���� ������ �� 24.03.05 ���ڿ��� level 1,2 �����ְ� level 3�� �����ټ����� / 24.03.04 ���ڿ��� level 1,2,3 ��� ������
        // DB Name : "24.02.25", ... , "24.03.04", "24.03.05", "24.03.06" ...
        // DB�� ��ȸ�� �� ���� ���� ����� ��¥����(maximum 30��) ã�Ҵµ��� ����� �����Ͱ� ��� 7�ϱ��� ä��� ���ٸ� default������ 0�� �ִ´�.

        Debug.Log("[DB] Come in LoadAnalyticsData Method");

        List<string> return_List = new List<string>();

        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.Connection = connection;

        // ��ȸ�� DB Array
        string[] DBName_Array = new string[30];
        DateTime currentDate = DateTime.Now.AddDays(-1); // ���� ����
        for(int i =0; i < DBName_Array.Length; i++)
        {
            currentDate = currentDate.AddDays(-1);
            DBName_Array[i] = $"{currentDate.Year.ToString().Substring(2,2)}.{currentDate.Month:00}.{currentDate.Day:00}";
        }

        // ��ȸ�� ���̺� ����ŭ String Type�� �޴� List�� ����, �� �迭�� ���̺��� �ǹ��ϰ�, �ش� ���̺��� value�� ���ǹ� ���� �ƴ��� �Ǵ��ؼ� Add�� �߰����� ������ ����
        List<string>[] tempList_Array = new List<string>[13];

        // ��ȸ�� ���̺� Array
        string[] tableName_Array = new string[13];
        for(int i = 0; i < tableName_Array.Length; i++)
        {
            if(i < 3) // venezia_kor
            {
                tableName_Array[i] = $"venezia_kor_level{i+1}_anlaytics";
            }   
            else if(i < 6) // venezia_eng
            {
                tableName_Array[i] = $"venezia_eng_level{i-2}_anlaytics";
            }
            else if(i == 6) // venezia_chn
            {
                tableName_Array[i] = $"venezia_chn_anlaytics";
            }
            else if(i < 10) // calculation
            {
                tableName_Array[i] = $"calculation_level{i-6}_anlaytics";
            }
            else // gugudan
            {
                tableName_Array[i] = $"gugudan_level{i-9}_anlaytics";
            }

            // tempList_Array ��ü ����
            tempList_Array[i] = new List<string>();
        }

        // ������ �÷� Array
        string[] columnName_Array = new string[4] { "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerRate"};

        // 30�� ����, �ֱ� ��¥(����)���� DB��ȸ
        for(int i = 0; i < DBName_Array.Length; i++)
        {
            try
            {
                string useDB_Command = $"USE {DBName_Array[i]};";
                mySqlCommand.CommandText = useDB_Command;
                mySqlCommand.ExecuteNonQuery();

                // DB���� �ִ� ���̼����� ĳ���͸� �����ϴ� ���̺� �÷� ��ȸ
                for (int j = 0; j < tableName_Array.Length; j++)
                {
                    string selectTable_Command = $"SELECT * FROM `{tableName_Array[j]}` WHERE `{columnName_Array[0]}` = {licensenumber} AND `{columnName_Array[1]}` = {charactor};";
                    mySqlCommand.CommandText = selectTable_Command;
                    MySqlDataReader reader = mySqlCommand.ExecuteReader();
                    float reactionRate = 0;
                    int answerRate = 0;

                    while (reader.Read())
                    {
                        reactionRate = reader.GetFloat(columnName_Array[2]);
                        answerRate = reader.GetInt32(columnName_Array[3]);
                    }
                    reader.Close();

                    if (reactionRate == 0 || answerRate == 0) continue; // ����Ǿ��ִ� �����Ͱ� 0�̸�(�ش� ��¥�� ������ �÷����������ٸ�) continue

                    // �迭���� List index�� 7�� ������ ä���. -> �����͸� ������ �ִ� ��¥�߿� �ֱ� ��¥ 7�ϸ� 
                    if(tempList_Array[j].Count < 7)
                    {
                        // DB�κ��� �ҷ��� �����Ͱ� ��ȿ�ϴٸ� �ֱ� ��¥��� index ������ ������ �ɰ�
                        tempList_Array[j].Add($"{DBName_Array[i]}|{tableName_Array[j]}|{reactionRate}|{answerRate}|{separatorString}");
                    }
                }
            }
            catch(Exception e)
            {
                // ���ں��� �����ǰ� ����Ǵ� DB�� ���ٸ� �ٷ� break / � ���� DB�� ���ٸ� �ش� ������ ���� ���� DB�� ����(DB�� ���ں��� ���� �����Ǳ⶧��)
                Debug.Log($"[DB] Select DB is not efficient, Maybe there is not existed database by date : {e.Message}");
                break;
            }
        }

        // �ӽ÷� ���� List�� �迭�� ���̵��� -> �� ���̺���
        for(int i = 0; i < tempList_Array.Length; i++)
        {
            // DB�� ����� ��¥�� 7�� �̸��̶� ���� �����Ͱ� ���ٸ� �Ǵ�
            // 30�ϱ��� ��ϵ� ���ں� DB�� �ش� ������ �÷��� �����Ͱ� ���ٸ�
            // ����� ������ 7�ϰ��� �����ͱ���� ���ٸ� ������ �ϼ�(index)�� 0���� ä���� �Ǵ��Ҽ� �ְ� ä���
            while (tempList_Array[i].Count < 7)
            {
                // day|game_name|reactionRate|answerRate|E| / value ���鿡 ���� ���̱� ������ 0000���� ������ �������
                tempList_Array[i].Add($"0|0|0|0|{separatorString}");
            }
        }

        // ���ں��� return_List�� �Ű� ���
        //tempList_Array[0][0]
        //tempList_Array[1][0]
        //tempList_Array[2][0]
        for(int i = 0; i < tempList_Array[0].Count; i++) 
        {
            for (int j = 0; j < tempList_Array.Length; j++)
            {
                return_List.Add(tempList_Array[j][i]);
            }
        }

        Debug.Log("[DB] Complete Load AnalyticsData From DB");

        return return_List;
    }

    // �ӽ÷� PresentDB�� �ִ� Rank Table ������ ��� / Score, Time �� Rank 1~5�� �� 6��° �ڱ� �ڽ��� ������ 
    public List<string> RankOrderByUserData(int licensenumber, int charactor)
    {
        Debug.Log("[DB] Come in RankOrderByUserData Method");

        // rank table -> [0]:User_LicenseNumber, [1]:User_Charactor, [2]:User_Profile, [3]:User_Name, [4]:TotalTime, [5]"TotalScore
        List<string> return_List = new List<string>();

        // rank table's row count
        string rowCount_Command = $"SELECT COUNT(*) FROM `rank`";
        MySqlCommand rowCount_SqlCmd = new MySqlCommand(rowCount_Command, connection);
        // Int ĳ����
        object result = rowCount_SqlCmd.ExecuteScalar(); // ExcuteScalar() �޼���� ������ �����ϰ� ��� ������ ù ��° ���� ù ��° ���� ���� ��ȯ
        Debug.Log($"[DB] Check result: {result}");
        result = (result == DBNull.Value) ? null : result;
        int rowCountInTable = Convert.ToInt32(result); 
        Debug.Log($"[DB] Check rowCount, rowCountInTable : {rowCountInTable}");

        // Score ����
        string selectScore_Command = $"SELECT * FROM `rank`";
        MySqlCommand selectScore_SqlCmd = new MySqlCommand(selectScore_Command, connection);
        MySqlDataReader reader = selectScore_SqlCmd.ExecuteReader();
        Debug.Log("[DB] Check selectScore");

        List<List<string>> rankdata = new List<List<string>>(); 

        // table�� �ִ� data�� rankdata�� ���
        while(reader.Read())
        {
            for(int i = 0; i < rowCountInTable; i++)
            {
                List<string> valuesInColumn = new List<string>();
                
                for (int j = 0; j < rank_Columns.Length; j++)
                {
                    Debug.Log($"[DB] Check Type Conversion, j : {j}");
                    if (j == 2) // byte[]
                    {
                        // column�� �ִ� MediumBlob Type value -> byte[] type���� �޾ƿ���
                        byte[] profileData = reader[$"{rank_Columns[j]}"] as byte[];
                        // Image Data�� Base64 ���ڿ��� ��ȯ
                        string profileDataBase64 = Convert.ToBase64String(profileData);
                        // client�� ���� ���ڿ��� Base64 ���ڿ� �߰� (���߿� client���� decoding�ؾ���)
                        valuesInColumn.Add(profileDataBase64);
                        Debug.Log($"[DB] Check Type Conversion, Where: byte[] profileData, {profileData}, j : {j}");
                        Debug.Log($"[DB] Check Type Conversion, Where: string profileDataBase64, {profileDataBase64}, j : {j}");
                        Debug.Log($"[DB] Check Type Conversion, Where: string Encoding.UTF8.Getstring(profileData), {System.Text.Encoding.UTF8.GetString(profileData)}, j : {j}");
                    }
                    else if(j == 3) // Varchar
                    {
                        valuesInColumn.Add(reader.GetString(rank_Columns[j]));
                        Debug.Log($"[DB] Check Type Conversion, Where: Varchar[], j : {j}");
                    }
                    else if(j == 4) // Float
                    {
                        valuesInColumn.Add(reader.GetFloat(rank_Columns[j]).ToString());
                        Debug.Log($"[DB] Check Type Conversion, Where: Float[], j : {j}");
                        Debug.Log($"[DB] Check float type? : {reader.GetFloat(rank_Columns[j]).GetType()}");
                        Debug.Log($"[DB] Check float value : {reader.GetFloat(rank_Columns[j])}");
                    }
                    else // int
                    {
                        valuesInColumn.Add(reader.GetInt32(rank_Columns[j]).ToString());
                        Debug.Log($"[DB] Check Type Conversion, Where: Int[], j : {j}");
                    }
                }
                rankdata.Add(valuesInColumn);
            }
        }
        reader.Close();

        // �������� ������
        // Score - rankdata[i][5]
        List<Rank_Score> scoreList = new List<Rank_Score>();

        for(int i= 0; i < rankdata.Count; i++)
        {
            Rank_Score score = new Rank_Score();

            score.place = 0;
            score.userlicensenumber = Int32.Parse(rankdata[i][0]);
            score.usercharactor = Int32.Parse(rankdata[i][1]);
            score.userProfile = System.Text.Encoding.UTF8.GetBytes(rankdata[i][2]);
            score.userName = rankdata[i][3];
            score.totalScore = Int32.Parse(rankdata[i][5]);

            scoreList.Add(score);
        }

        // totalScore�� �������� ����
        scoreList.Sort((a, b) => b.totalScore.CompareTo(a.totalScore));

        // ���ĵ� List������ 1~5�� ������ ����,���� ����
        for(int i = 0; i < scoreList.Count; i++)
        {
            //���� ������(value) place/Profile/Name/total
            string return_string;

            if(i >= 0 && i < 5)
            {
                return_string = $"{i}|{scoreList[i].userProfile}|{scoreList[i].userName}|{scoreList[i].totalScore}|{separatorString}";
            }
            else
            {
                continue;
            }

            return_List.Add(return_string);
        }

        // ����ó��, rank data�� 5���� ����� ������ return_List�� index[5]���� �󳻿����� ����
        if(scoreList.Count < 5)
        {
            int user_Count = scoreList.Count;

            while(user_Count < 5)
            {
                //���� ������(value) place/Profile/Name/total
                return_List.Add($"{user_Count}|0|None|0|{separatorString}");
                user_Count++;
            }
        }

        // return_List index[5]�� ���� ����,���� ����
        for (int i = 0; i < scoreList.Count; i++)
        {
            if (scoreList[i].userlicensenumber == licensenumber && scoreList[i].usercharactor == charactor)
            {
                string return_string;
                return_string = $"{i}|{scoreList[i].userProfile}|{scoreList[i].userName}|{scoreList[i].totalScore}|{separatorString}";
                return_List.Add(return_string);
            }
        }

        // Time ����
        List<Rank_Time> timeList = new List<Rank_Time>();

        for (int i = 0; i < rankdata.Count; i++)
        {
            Rank_Time time = new Rank_Time();

            time.place = 0;
            time.userlicensenumber = Int32.Parse(rankdata[i][0]);
            time.usercharactor = Int32.Parse(rankdata[i][1]);
            time.userProfile = System.Text.Encoding.UTF8.GetBytes(rankdata[i][2]);
            time.userName = rankdata[i][3];
            time.totalTime = float.Parse(rankdata[i][4]);

            timeList.Add(time);
        }

        // totalTime���� �������� ����
        timeList.Sort((a, b) => b.totalTime.CompareTo(a.totalTime));

        // ���ĵ� List������ 1~5�� ������ ����, �ð� ����
        for (int i = 0; i < timeList.Count; i++)
        {
            //���� ������(value) place/Profile/Name/total
            string return_string;

            if (i >= 0 && i < 5)
            {
                return_string = $"{i}|{timeList[i].userProfile}|{timeList[i].userName}|{timeList[i].totalTime}|{separatorString}";
            }
            else
            {
                continue;
            }

            return_List.Add(return_string);
        }

        // ����ó��, rank data�� 5���� ����� ������ return_List�� index[5]���� �󳻿����� ����
        if (timeList.Count < 5)
        {
            int user_Count = timeList.Count;

            while (user_Count < 5)
            {
                //���� ������(value) place/Profile/Name/total
                return_List.Add($"{user_Count}|0|None|0|{separatorString}");
                user_Count++;
            }
        }

        // return_List index[11]�� ���� ����,���� ����
        for (int i = 0; i < timeList.Count; i++)
        {
            if (timeList[i].userlicensenumber == licensenumber && timeList[i].usercharactor == charactor)
            {
                string return_string;
                return_string = $"{i}|{timeList[i].userProfile}|{timeList[i].userName}|{timeList[i].totalTime}|{separatorString}";
                return_List.Add(return_string);
            }
        }

        return return_List;
    }

    // �Ϸ簡 ������ �� presentDB gamedata -> ����DB �����ϰ� gamedata ����
    public void CreateDateDB()
    {
        Debug.Log("[DB] CreateDateDB");

        // ������ DB �̸�
        string DBName = $"{DateTime.Now.Year.ToString().Substring(2,2)}.{DateTime.Now.Month:00}.{DateTime.Now.Day:00}";
        Debug.Log($"DBName : {DBName}");
        // ���� ���̺� ��, TableName�� �����Ǹ� �˾Ƽ� ���̺���� ���. ������ ������ ������ ���̺� ����
        TableName gameTable = new TableName();
        string[] deleteTable = { "user_info", "rank", "achievement", "pet" };
        for (int i = 0; i < deleteTable.Length; i++)
        {
            gameTable.list.Remove(deleteTable[i]);
        }

        // �м�ǥ ���̺� -> analyticsTable

        // DateDB����
        MySqlCommand mySqlCommand = new MySqlCommand();
        mySqlCommand.CommandText = $"CREATE DATABASE IF NOT EXISTS `{DBName}`;"; // DB���� ��ɹ�
        mySqlCommand.Connection = connection;
        mySqlCommand.ExecuteNonQuery();
        Debug.Log("[DB] Complete Create DateDB");

        // DateDB���
        mySqlCommand.CommandText = $"USE `{DBName}`;";
        mySqlCommand.ExecuteNonQuery();

        // Table���� ��ɹ�
        // �� ���̺�� �����ϰ� �÷��� ����
        // game_Columns = new string[]{ "User_LicenseNumber", "User_Charactor", "ReactionRate", "AnswerCount", "AnswerRate", "Playtime", "TotalScore", "StarPoint" };
        for (int i = 0; i < gameTable.list.Count; i++)
        {
            string createTable_Command = $"CREATE TABLE IF NOT EXISTS {gameTable.list[i]} (";
            for (int j = 0; j < game_Columns.Length; j++)
            {
                if (j == 2 || j == 5) // float -> ReactionRate, Playtime
                {
                    createTable_Command += $"{game_Columns[j]} float DEFAULT NULL,";
                }
                else // int -> ������
                {
                    createTable_Command += $"{game_Columns[j]} int(11) DEFAULT NULL,";
                }
            }
            createTable_Command = createTable_Command.TrimEnd(','); // ������ ��ǥ ����
            createTable_Command += $") ENGINE = InnoDB DEFAULT CHARSET = latin1 COLLATE = latin1_swedish_ci;";
            //ENGINE = InnoDB DEFAULT CHARSET = latin1 COLLATE = latin1_swedish_ci;

            // Table ����
            MySqlCommand createTable_SqlCmd = new MySqlCommand(createTable_Command, connection);
            createTable_SqlCmd.ExecuteNonQuery();
        }
        Debug.Log("[DB] Complete Create DateDB table");

        // PresentDB ���
        mySqlCommand.CommandText = $"USE `PresentDB`";
        mySqlCommand.ExecuteNonQuery();

        // PresentDB�� (table�� �ִ� (�� ���� Column Data�� ���� List)���� ���� ������ ���� List)���� Table������ŭ Array�� ������ ����
        List<List<string>>[] valuesInColumnsInTable_Array = new List<List<string>>[gameTable.list.Count];

        // PresentDB�� �ִ� gamedata�� �ҷ��ͼ� �� ������ ����
        for (int i = 0; i < gameTable.list.Count; i++)
        {
            // table�� �� ���� �ִ���
            string rowCount_Command = $"SELECT COUNT(*) FROM `{gameTable.list[i]}`";
            MySqlCommand rowCount_SqlCmd = new MySqlCommand(rowCount_Command, connection);
            int rowCountInTable = (int)rowCount_SqlCmd.ExecuteScalar(); // ExcuteScalar() �޼���� ������ �����ϰ� ��� ������ ù ��° ���� ù ��° ���� ���� ��ȯ

            string select_Command = $"SELECT * FROM {gameTable.list[i]}";
            MySqlCommand select_SqlCmd = new MySqlCommand(select_Command, connection);
            MySqlDataReader reader = select_SqlCmd.ExecuteReader();

            List<List<string>> column_Values_List = new List<List<string>>();

            while (reader.Read())
            {
                for (int j = 0; j < rowCountInTable; j++)
                {
                    // table�� �ִ� Column���� Data�� ���� List
                    List<string> column_Values = new List<string>();

                    for (int k = 0; k < game_Columns.Length; k++)
                    {
                        if (k == 2 || k == 5) // float
                        {
                            column_Values.Add(reader.GetFloat(game_Columns[k]).ToString());
                        }
                        else // int
                        {
                            column_Values.Add(reader.GetInt32(game_Columns[k]).ToString());
                        }
                    }

                    column_Values_List.Add(column_Values);
                }
            }
            reader.Close();

            valuesInColumnsInTable_Array[i] = column_Values_List;
        }
        Debug.Log("[DB] Complete copy presentDB");

        // ������ DateDB ���
        mySqlCommand.CommandText = $"USE {DBName};";
        mySqlCommand.ExecuteNonQuery();

        // DateDB�� presentDB���� ������ ������(���ӵ����͸�) ���� ����
        for(int i = 0; i < gameTable.list.Count; i ++) // Table
        {
            for(int j=0; j < valuesInColumnsInTable_Array[i].Count; j ++) // Columns
            {
                // 0 -> licenseNumber, 1 -> charactor
                string insert_Command = $"INSERT INTO {gameTable.list[i]} (`{game_Columns[0]}`,`{game_Columns[1]}`) " +
                                        $"VALUES ({valuesInColumnsInTable_Array[i][j][0]},{valuesInColumnsInTable_Array[i][j][1]});";
                mySqlCommand.CommandText = insert_Command;
                mySqlCommand.ExecuteNonQuery();

                for (int k = 2; k < valuesInColumnsInTable_Array[i][j].Count; k++) // Column
                {
                    string update_Command = $"UPDATE {gameTable.list[i]} SET `{game_Columns[k]}` = @value " +
                                            $"WHERE `{game_Columns[0]}` = {valuesInColumnsInTable_Array[i][j][0]} AND `{game_Columns[1]}` = {valuesInColumnsInTable_Array[i][j][1]};";
                    mySqlCommand.CommandText = update_Command;

                    if(k == 2 || k == 5) // float
                    {
                        mySqlCommand.Parameters.Add("@value", MySqlDbType.Float).Value = float.Parse(valuesInColumnsInTable_Array[i][j][k]);
                    }
                    else // int
                    {
                        mySqlCommand.Parameters.Add("@value", MySqlDbType.Int32).Value = Int32.Parse(valuesInColumnsInTable_Array[i][j][k]);
                    }
                    mySqlCommand.ExecuteNonQuery();
                }
            }
        }
        Debug.Log("[DB] Complete paste presentDB to DateDB only gamedatas");

        // �м����̺� �ִ� �����ӵ�, ������� �� ������ ������ ���ܿ� �ִ� �����ӵ�, ����� ����ͼ� ����� �� �� ����
        // ������� ����ġ�� ���� ���� 1�� 6�����߿� 3���� ���ܸ� �÷��� �ߴٸ�, 3�� ������ �����ӵ�, ����� ����� ����. �� (x1+x2+x3)/3 �ع�����.
        // (��n)/n
        // �������̺��� �� ������ ������ ����1~6�� Column�� ReactionRate�� AnswerRate�� �����Ѵ�. ���� 0�̸� ���� 
        //string selectTable_Command = $"SELECT * FROM `{tableName_Array[j]}` WHERE `{columnName_Array[0]}` = {licensenumber} AND `{columnName_Array[1]}` = {charactor};"

        // Ŭ������ ����� 
        //List<Tuple<int, int, float, int>>[][] tuple_Structure = new List<Tuple<int, int, float, int>>[analyticsTable.list.Count][];
        // 13���� anlyticsTable -> Array, ���̺��� ������ �˰� ����
        // �� ���̺��� �� ���� valuesInColumnsInTable_Array[i].Count -> Array, ������ presentDB�� �����͸� ������ �� ���� ������ �˰� ����
        // �� ���� �÷� �� -> List, List�� �� ������ �ش� ������ �÷��� ���� �ʾҴٸ�(���� 0) �����͸� ����� ������ ���� �߰��� �ʿ䰡 ����
        List<AnalyticsColumnValue>[][] valueList_ColumnArray_TableArray = new List<AnalyticsColumnValue>[analyticsTable.list.Count][];

        for (int i = 0; i < gameTable.list.Count; i++)
        {
            switch(i/6) // ����1~6
            {
                case 0: // venezia_kor_level1_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    //// �ߺ��ؼ� �ʱ�ȭ���� �ʵ��� ����ó��
                    //if (valueList_ColumnArray_TableArray[i / 6] == null)
                    //{
                    //    valueList_ColumnArray_TableArray[i / 6] = new List<AnalyticsColumnValue>[valuesInColumnsInTable_Array[i].Count];
                    //}
                    //
                    //for (int j = 0; j < valuesInColumnsInTable_Array[i].Count; i ++) // ���̺� ����� ���(������ ĳ���͵�)
                    //{
                    //    // �ߺ��ؼ� �ʱ�ȭ���� �ʵ��� ����ó��
                    //    if (valueList_ColumnArray_TableArray[i / 6][j] == null)
                    //    {
                    //        valueList_ColumnArray_TableArray[i / 6][j] = new List<AnalyticsColumnValue>();
                    //    }
                    //
                    //    // �����Ͱ� �ִٸ�(0�� �ƴ϶��) �߰�
                    //    if(float.Parse(valuesInColumnsInTable_Array[i][j][2]) != 0 && Int32.Parse(valuesInColumnsInTable_Array[i][j][4]) != 0)
                    //    {
                    //        int _licenseNumber = Int32.Parse(valuesInColumnsInTable_Array[i][j][0]);
                    //        int _charactor = Int32.Parse(valuesInColumnsInTable_Array[i][j][1]);
                    //        float _reactionRate = float.Parse(valuesInColumnsInTable_Array[i][j][2]);
                    //        int _answerRate = Int32.Parse(valuesInColumnsInTable_Array[i][j][4]);
                    //        //AnalyticsColumnValue data = new AnalyticsColumnValue(_licenseNumber, _charactor, _reactionRate, _answerRate);
                    //        valueList_ColumnArray_TableArray[i / 6][j].Add(new AnalyticsColumnValue(_licenseNumber, _charactor, _reactionRate, _answerRate));
                    //    }
                    //}
                    break;
                case 1: // venezia_kor_level2_analytics
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break;
                case 2: // venezia_kor_level3_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break;
                case 3: // venezia_eng_level1_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break;         
                case 4: // venezia_eng_level2_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break;         
                case 5: // venezia_eng_level3_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break;
                case 6: // venezia_chn_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break;
                case 7: // calculation_level1_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break; 
                case 8: // calculation_level2_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break; 
                case 9: // calculation_level3_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break;
                case 10: // gugudan_level1_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break;  
                case 11: // gugudan_level2_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break;  
                case 12: // gugudan_level3_analytics 
                    etcMethodHandler.AddAnalyticsColumnValueInDB(valueList_ColumnArray_TableArray, valuesInColumnsInTable_Array, i);
                    break;
                default:
                    Debug.Log("[DB] index i is unexpected value In CreateDB");
                    break;
            }
        }
        Debug.Log("[DB] Complete etcMethodHandler.AddAnalyticsColumnValueInDB");

        // �ݺ��� �ۿ��� �Ű����� �߰�
        mySqlCommand.Parameters.Clear();
        mySqlCommand.Parameters.Add("@licenseNumber", MySqlDbType.Int32);
        mySqlCommand.Parameters.Add("@charactor", MySqlDbType.Float);
        mySqlCommand.Parameters.Add("@reactionRate", MySqlDbType.Int32);
        mySqlCommand.Parameters.Add("@answerRate", MySqlDbType.Int32);

        // ��� �����ӵ�, ����� ���ϰ� �м� table ����
        for (int i = 0; i < valueList_ColumnArray_TableArray.Length; i++) // 13�� table
        {
            // ���̺��� �����, �÷��� ���� �� �����͸� Insert����.
            string analyticsTableName = $"{analyticsTable.list[i]}";
            string createAnalyticsTable_Command = $"CREATE TABLE IF NOT EXISTS {analyticsTable.list[i]} (";

            for(int j = 0; j < analytics_Columns.Length; j++)
            {
                if(j ==2) // float -> ReactionRate
                {
                    createAnalyticsTable_Command += $"{analytics_Columns[j]} float DEFAULT NUll,";
                }
                else
                {
                    createAnalyticsTable_Command += $"{analytics_Columns[j]} int(11) DEFAULT NULL,";
                }
            }
            createAnalyticsTable_Command = createAnalyticsTable_Command.TrimEnd(',');
            createAnalyticsTable_Command += $") ENGINE = InnoDB DEFAULT CHARSET = latin1 COLLATE = latin1_swedish_ci;";

            mySqlCommand.CommandText = createAnalyticsTable_Command;
            mySqlCommand.ExecuteNonQuery();

            // Insert Column's value in Rows
            for (int j = 0; j < valueList_ColumnArray_TableArray[i].Length; j ++) // �� table�ȿ� �����ϴ� ���� ����(�������� ������ �ִ� ĳ������ ��)
            {
                int licenseNumber = Int32.Parse(valuesInColumnsInTable_Array[i][j][0]);
                int charactor = Int32.Parse(valuesInColumnsInTable_Array[i][j][1]);
                float reactionRate;
                int answerRate;

                if (valueList_ColumnArray_TableArray[i][j].Count == 0) // �Ϸ翡 �ش� ����(����,����)�� �ѹ��� ���� �ʾҴٸ�
                {
                    // �ش� j�࿡ default �� == 0 �� �ִ´�.
                    reactionRate = 0;
                    answerRate = 0;
                }
                else
                {
                    reactionRate = valueList_ColumnArray_TableArray[i][j].Sum(item => item.reactionRate) / valueList_ColumnArray_TableArray[i][j].Count;
                    answerRate = (int)(valueList_ColumnArray_TableArray[i][j].Sum(item => item.answerRate) / valueList_ColumnArray_TableArray[i][j].Count);
                }

                //for (int k = 0; j < valueList_ColumnArray_TableArray[i][j].Count; k++)
                //{
                //
                //}

                // insertColumnsValueInAnalyticsTable_Command
                string insert_Command = $"INSERT INTO `{analyticsTable.list[i]}` ({analytics_Columns[0]}, {analytics_Columns[1]}, {analytics_Columns[2]}, {analytics_Columns[3]}) " +
                                        $"VALUES (@licenseNumber, @charactor, @reactionRate, @answerRate);";
                mySqlCommand.CommandText = insert_Command;
                // �ݺ��� �ȿ��� �Ű����� ������Ʈ
                mySqlCommand.Parameters["@licenseNumber"].Value = licenseNumber;
                mySqlCommand.Parameters["@charactor"].Value = charactor;
                mySqlCommand.Parameters["@reactionRate"].Value = reactionRate;
                mySqlCommand.Parameters["@answerRate"].Value = answerRate;
                mySqlCommand.ExecuteNonQuery();
            }

            Debug.Log($"[DB] Continue create valueList_ColumnArray_TableArray{i}");
        }

        Debug.Log("[DB] Finish create AnalayticsTable and columns");
    }

}
