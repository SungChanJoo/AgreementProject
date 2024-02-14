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
        // ��ο� ������ �������� �ʴ´ٸ� ���̼����ѹ��� ���ٴ°��̰�, ó�� �����Ѵٴ� ��
        if(!File.Exists(path))
        {
            Directory.CreateDirectory(path);
            loginStatus = ClientLoginStatus.First;
            // �������� ���̼��� �ѹ��� �޾ƿ;���, �׷��� ���� ������ ��û todo
            //Client.stream 
            
        }

        // �ش� ��ο� �ִ� ������ �о� Ŭ���̾�Ʈ ���̼��� �ѹ��� �ҷ���
        string jsonString = File.ReadAllText(path + "/clientlicense");
        JsonData itemdata = JsonMapper.ToObject(jsonString);

    }


}
