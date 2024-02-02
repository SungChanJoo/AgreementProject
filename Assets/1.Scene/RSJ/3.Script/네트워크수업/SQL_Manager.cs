using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data;
using MySql.Data.MySqlClient;
using System;
using System.IO;
using LitJson;

public class user_info
{
    public string User_name { get; private set; }
    public string User_Password { get; private set; }

    public user_info(string name, string password)
    {
        User_name = name;
        User_Password = password;
    }
}

public class SQL_Manager : MonoBehaviour
{
    public user_info info;

    public MySqlConnection connection;
    public MySqlDataReader reader;

    public string DB_Path = string.Empty;

    public static SQL_Manager instance = null;
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

        DB_Path = Application.dataPath + "/Database";
        string serverinfo = ServerSet(DB_Path);
        try
        {
            if(serverinfo.Equals(string.Empty))
            {
                Debug.Log("SQL Server Json Error!");
                return;
            }
            connection = new MySqlConnection(serverinfo);
            connection.Open(); // 서버 오픈한것
            Debug.Log("SQL Server Open Complete!");
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }

    }

    private string ServerSet(string path)
    {
        if(!File.Exists(path)) // 그 경로에 파일이 있나요?, 경로에 파일이 없다면
        {
            Directory.CreateDirectory(path);
        }
        string Jsonstring = File.ReadAllText(path + "/config.Json");
        /*
         [{"IP" : "192.168.0.75", "TableName '''''''''''''''
         */
        JsonData itemdata = JsonMapper.ToObject(Jsonstring);
        string serverInfo = 
            $"Server={itemdata[0]["IP"]};" +
            $"Database={itemdata[0]["TableName"]};" +
            $"Uid ={itemdata[0]["ID"]};" +
            $"Pwd = {itemdata[0]["PW"]};" +
            $"Port = { itemdata[0]["PORT"]};" +
            $"CharSet = utf8;";

        return serverInfo;
    }

    private bool connection_check(MySqlConnection con)
    {
        //현재 MySqlConnectin\on open 상태가 아니라면?
        if(con.State!= System.Data.ConnectionState.Open)
        {
            con.Open();
            if(con.State!=System.Data.ConnectionState.Open) 
            {
                return false; //재오픈을 했는데도 안되면 이상함 감지
            }
        }
        return true;
    }

    public bool Login(string id, string password)
    {
        //직접적으로 DB에서 데이터를 가지고 오는 메서드
        //조회되는 데이터가 없다면 False
        //조회가 되는 데이터가 있다면 True값을 던지는데
        //위에서 선언한 Info에다가 담은 다음에 던질거야
        /*
         1.Connection open 상황인지 확인 -> 메소드화
        2. Reader 상태가 읽고 있는 상황인지 확인
         - 한 쿼리문당 하나
        3. 데이터를 다 읽었으면 Close
         */

        try
        {
            //1.Connection open 상황인지 확인->메소드화
            if (!connection_check(connection))
            {
                return false;
            }

            string SQL_command =
                string.Format(@"SELECT User_name,User_Password FROM user_info
                                WHERE User_name = '{0}' AND User_Password = '{1}';", id,password); //골뱅이는 줄바꿈이 있어도 한줄로 인식한다는 뜻, 쿼리문에 문제가 없다는 것을 확인 후 복붙하기
            MySqlCommand cmd = new MySqlCommand(SQL_command, connection);
            reader = cmd.ExecuteReader();
            //Reader 읽은 데이터가 한개이상 존재해?
            if(reader.HasRows)
            {
                //읽은 데이터를 하나씩 나열합니다.
                while (reader.Read())
                {
                    /*
                     삼항연산자
                     */
                    string name = (reader.IsDBNull(0)) ? string.Empty : (string)reader["User_Name"].ToString();
                    string Pass = (reader.IsDBNull(1)) ? string.Empty : (string)reader["User_Password"].ToString();
                    if(!name.Equals(string.Empty)|| !Pass.Equals(string.Empty))
                    {
                        //정상적으로 데이터를 불러온 상황
                        info = new user_info(name, Pass);
                        if (!reader.IsClosed) reader.Close(); // 리더가 열려있다면 닫아주세요
                        return true;
                    }
                    else // 로그인 실패
                    {
                        break;
                    }

                }//while 끝
            }// if 끝
            if (!reader.IsClosed) reader.Close(); // else문으로 나왔을때 닫아주기
            return false; //여기까지 오면 로그인에 실패했기 떄문에 false 반환

        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            if(!reader.IsClosed) reader.Close(); // 에러나면 닫고 false 반환
            return false;
        }
    }

}
