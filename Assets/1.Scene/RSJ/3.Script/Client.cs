using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    public Text clientLog;

    // 플레이어가 앱을 실행시킬 때 DB에서 데이터를 가져와야함
    // 가져와야 하는 데이터. 많음. 
    // 서버에 접속하기 위해 필요한 IP는 License로 관리해? 스크립트?
    // 어쨋든 빌드해도 스크립트상에 남길 수 있으니. 보안 신경써야하나?
    // need Fix

    public void StartClient()
    {
        client = new TcpClient();
        client.BeginConnect("127.0.0.1", 8888, HandleClientConnect, null);
    }

    private void HandleClientConnect(IAsyncResult result)
    {
        client.EndConnect(result);
        stream = client.GetStream();

        Debug.Log("Connected to server!");
        clientLog.text += "\nConnected to server!";

        // 서버로부터 데이터 받기 시작
        byte[] receiveBuffer = new byte[1024];
        stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, HandleDataReceived, receiveBuffer);

        // 예시 : 서버에 패킷 보내기
        Packet packetToSend = new Packet("Player2", 150);
        SendPacket(packetToSend);
    }

    private void HandleDataReceived(IAsyncResult result)
    {
        int bytesRead = stream.EndRead(result);

        if(bytesRead > 0)
        {
            byte[] receivedData = new byte[bytesRead];
            Array.Copy((result.AsyncState as byte[]), receivedData, bytesRead);

            // 예시 : 받은 패킷 역직렬화
            Packet receivedPacket = PacketFromBytes(receivedData);
            Debug.Log("Received data from server: " + receivedPacket.playerName + " - " + receivedPacket.playerScore);

            // Handle the received data as needed

            // Continue listening for more data
            byte[] newBuffer = new byte[1024];
            stream.BeginRead(newBuffer, 0, newBuffer.Length, HandleDataReceived, newBuffer);
        }
    }

    private void SendPacket(Packet packet)
    {
        byte[] data = PacketToBytes(packet);
        stream.Write(data, 0, data.Length);
    }

    private byte[] PacketToBytes(Packet packet)
    {
        string jsonData = JsonUtility.ToJson(packet);
        return Encoding.ASCII.GetBytes(jsonData);
    }

    private Packet PacketFromBytes(byte[] data)
    {
        string jsonData = Encoding.ASCII.GetString(data);
        return JsonUtility.FromJson<Packet>(jsonData);
    }

    // need Fix
    private void OnDestroy()
    {
        if(client != null)
        {
            client.Close();
        }
    }
}
