using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;

public class ClientLicense : MonoBehaviour
{
    public static ClientLoginStatus loginStatus;

    public static string clientLicenseNumber;

    public string licensePath = string.Empty;

    private void Awake()
    {
        licensePath = Application.dataPath + "/License";
        ClientLoginSet(licensePath);
    }

    public void ClientLoginSet(string path)
    {
        // 경로에 파일이 존재하지 않는다면 라이센스넘버가 없다는것이고, 처음 접속한다는 뜻
        if(!File.Exists(path))
        {
            Directory.CreateDirectory(path);
            loginStatus = ClientLoginStatus.First;
            // 서버에서 라이센스 넘버를 받아와야함, 그러기 위해 서버에 요청 todo
            //Client.stream 
            
        }

        // 해당 경로에 있는 파일을 읽어 클라이언트 라이센스 넘버를 불러옴
        string jsonString = File.ReadAllText(path + "/clientlicense");
        JsonData itemdata = JsonMapper.ToObject(jsonString);

    }


}
