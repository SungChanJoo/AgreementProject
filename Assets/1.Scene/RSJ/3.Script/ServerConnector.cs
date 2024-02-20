using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;

public class ServerConnector : MonoBehaviour
{
    private TcpClient client;

    private string awsIPv4Address = "15.165.159.141";
    private int awsPort = 3389;
    private string awsIPv4DNS = "ec2-15-165-159-141.ap-northeast-2.compute.amazonaws.com";


    private void Start()
    {
        Debug.Log("ServerConnector Start..");

        try
        {
            client = new TcpClient();
            client.Connect(awsIPv4Address, awsPort);
            Debug.Log("Connected to AWS server!");
        }
        catch(SocketException e)
        {
            Debug.Log($"Failed connect to AWS server. Error : {e.Message}");
        }
        
    }

    public void OnButtonClick_CheckConnected()
    {
        if(client.Connected)
        {
            Debug.Log("Connected on server!");
        }
        else
        {
            Debug.Log("Connected fail..");
        }
    }
}
