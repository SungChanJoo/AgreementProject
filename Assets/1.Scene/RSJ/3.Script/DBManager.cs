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

    // LicenseNumber 기본
    private int clientLicenseNumber_Base = 10000;

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
        Debug.Log(str_Connection);
    }

    // DB에 연결
    private void ConnectDB()
    {
        try
        {
            connection = new MySqlConnection(str_Connection);
            connection.Open(); // DB 열음
            Debug.Log("Success connect to db!");
        }
        catch(Exception e)
        {
            Debug.Log($"Fail connect to db + {e.Message}");
        }

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

    // 계정등록 // todo 비동기화 생각해봐야함
    public int CreateLicenseNumber()
    {
        if(!CheckConnection(connection))
        {
            Debug.Log("Didn't connect to DB");
            return -1;
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

        int clientLicenseNumber = clientLicenseNumber_Base + count;
        string user_Name = "Guest";

        // 생성하는 쿼리문
        string createAccount_Command = $"INSERT INTO user_info (User_LicenseNumber, User_Name) VALUES ({clientLicenseNumber}, '{user_Name}')"; // `(따옴표), '(백틱) 구분하기
        MySqlCommand insert_SqlCmd = new MySqlCommand(createAccount_Command, connection);
        //insert_SqlCmd.ExecuteNonQuery(); // command.ExecuteNonQuery()은 데이터베이스에서 변경 작업을 수행하는 SQL 명령문을 실행하고, 영향을 받은 행의 수를 반환하는 메서드입니다.
        
        reader.Close(); // DataReader가 열려있는 동안 추가적인 명령 실행 불가
        insert_SqlCmd.ExecuteNonQuery();

        // ClientLicenseNumber를 발급할 때 모든 테이블에 그 넘버를 추가해야함, Primary Key처럼 사용할것이기 때문
        InsertLicenseNumberToAllTable(clientLicenseNumber);

        return clientLicenseNumber;
    }

    private void InsertLicenseNumberToAllTable(int licensenumber)
    {
        List<string> table_List = new List<string>();
        table_List.Add("rank");
        table_List.Add("achievement");
        table_List.Add("venezia_kor_level1");
        table_List.Add("venezia_kor_level2");
        table_List.Add("venezia_kor_level3");
        table_List.Add("venezia_eng_level1");
        table_List.Add("venezia_eng_level2");
        table_List.Add("venezia_eng_level3");
        table_List.Add("venezia_chn_level1");
        table_List.Add("venezia_chn_level2");
        table_List.Add("venezia_chn_level3");
        table_List.Add("gugudan_level1");
        table_List.Add("gugudan_level2");
        table_List.Add("gugudan_level3");
        table_List.Add("calculation_level1");
        table_List.Add("calculation_level2");
        table_List.Add("calculation_level3");

        for(int i = 0; i<table_List.Count; i++)
        {
            string insertLicenseNumber_Command = $"INSERT INTO {table_List[i]} (User_LicenseNumber) VALUES ({licensenumber})";
            MySqlCommand insert_SqlCmd = new MySqlCommand(insertLicenseNumber_Command, connection);
            insert_SqlCmd.ExecuteNonQuery();
        }
    }
}
