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
        // DB�� ����
        using(MySqlConnection connection = new MySqlConnection(connectionString))
        {
            // using�� �Ǿ��ٸ� �����. {}���� ������ �޸� �Ҵ�Ȱ� ����
            // �� connection�� �Ǿ��ٸ� db�� ����.
            try
            {
                connection.Open();

                // ���� : SQL ������ �Ἥ DB�� data ����
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
