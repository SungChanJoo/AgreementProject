using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using kcp2k;
using LitJson;
using System;
using System.IO;

public enum Type
{
    Empty = 0,
    Server,
    Client
}

public class Item
{
    public string License;
    public string Server_IP;
    public string Port;

    //제이슨의 역할은 단 하나 빌드했는데 서버 값들을 계속 바꿀껀가? 외부로 빼내서 저장하기 위해 Json 파일로 만들려는것
    public Item(string L_index, string IPvalue, string port)
    {
        License = L_index;
        Server_IP = IPvalue;
        Port = port;
    }
}

public class ServerChecker : MonoBehaviour
{
    public Type type;

    private NetworkManager manager;
    private KcpTransport kcp;

    private string Path = string.Empty;
    public string Server_IP { get; private set; }
    public string Server_Port { get; private set; }

    private void Awake()
    {
        if(Path.Equals(string.Empty))
        {
            Path = Application.dataPath + "/License";
        }
        if(!File.Exists(Path)) // 폴더 검사
        {
            Directory.CreateDirectory(Path);
        }
        if(!File.Exists(Path + "/License.json")) // 파일 검사
        {
            Default_Data(Path);
        }
        manager = GetComponent<NetworkManager>();
        //kcp = (KcpTransport)manager.transport;
    }

    private void Default_Data(string path)
    {
        List<Item> item = new List<Item>();
        item.Add(new Item("1", "127.0.0.1", "7777"));

        JsonData data = JsonMapper.ToJson(item); // 읽어올떄는 ToObject 내보낼때는 ToJson
        File.WriteAllText(path + "/License.json", data.ToString());
    }

    private Type License_type()
    {
        Type type = Type.Empty;

        try
        {
            string Json_string = File.ReadAllText(Path + "/License.json");
            JsonData itemdata = JsonMapper.ToObject(Json_string);
            string string_type = itemdata[0]["License"].ToString();
            string str_serverIP = itemdata[0]["Server_IP"].ToString();
            string str_Port = itemdata[0]["Port"].ToString();
            Server_IP = str_serverIP;
            Server_Port = str_Port;
            type = (Type)Enum.Parse(typeof(Type), string_type);

            //manager.networkAddress = Server_IP;
            kcp.port = ushort.Parse(Server_Port);

            return type;
            /*
            public string License;
            public string Server_IP;
            public string Port;
            */
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return Type.Empty;
        }
    }

    private void Start()
    {
        type = License_type();

        if(type.Equals(Type.Server))
        {
            Start_Server();
        }
        else
        {
            Start_Client();
        }

    }

    public void Start_Server()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("WebGL cannot be Server");
        }
        else
        {
            //manager.StartServer();
            //Debug.Log($"{manager.networkAddress} StartServer...");
            NetworkServer.OnConnectedEvent += (NetworkConnectionToClient) =>
            {
                Debug.Log($"new client Connect : {NetworkConnectionToClient.address}");
            };
            NetworkServer.OnDisconnectedEvent += (NetworkConnectionToClient) =>
            {
                Debug.Log($"client Disconnect : {NetworkConnectionToClient.address}");
            };
        }
    }


    public void Start_Client()
    {
        //Debug.Log($"{manager.networkAddress} : StartClient..");
        //manager.StartClient();
    }

    private void OnApplicationQuit()
    {
        if(NetworkClient.isConnected)
        {
            //manager.StopClient();
        }

        if(NetworkServer.active)
        {
            //manager.StopServer();
        }
    }


}
