using System.Collections;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using UnityEngine;
using System;

public class DatabaseHandler : MonoBehaviour
{
    private string connectionString = "Server=localhost;Database=your_database;UserID=your_user;Password=your_password;Pooling=true";

    public void HandlePacket(Packet packet)
    {
        // DB에 연결
        using(MySqlConnection connection = new MySqlConnection(connectionString))
        {
            // using이 되었다면 실행됨. {}밖을 나가면 메모리 할당된거 종료
            // 즉 connection이 되었다면 db를 연다.
            try
            {
                connection.Open();

                // 예시 : SQL 쿼리를 써서 DB에 data 삽입
                string query = $"INSERT INTO your_table (playerName, playerScore) VALUES ('{packet.playerName}', '{packet.playerScore}')";
                using(MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                    Debug.Log("Data inserted into the database");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error handling packet: " + e.Message);
            }
        }
    }
}
