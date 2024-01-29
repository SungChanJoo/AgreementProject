using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    WebCamTexture _camTexture;

    public RawImage CameraViewImage; //ī�޶� ������ ȭ��
    Texture2D _texture2D;
    public Text Log;
    public Image LoadImg;
    string filePath = string.Empty;
    public void CameraOn()
    {
        //���� Ȯ��
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
        if (WebCamTexture.devices.Length == 0)//ī�޶� ���ٸ�
        {
            return;
        }
        WebCamDevice[] devices = WebCamTexture.devices;//ī�޶� ��������
        int selectedCameraIndex = -1;

        //�ĸ� ī�޶� ã��
        for(int i =0; i< devices.Length; i++)
        {
            if (devices[i].isFrontFacing == false)
            {
                selectedCameraIndex = i;
                break;
            }
        }
        //ī�޶� �ѱ�
        if(selectedCameraIndex>= 0)
        {
            //�ĸ� ī�޶� ������
            _camTexture = new WebCamTexture(devices[selectedCameraIndex].name);
            _camTexture.requestedFPS = 30; //ī�޶� ������
            CameraViewImage.texture = _camTexture;
            _camTexture.Play();
        }
    }
    //ī�޶� ����
    public void CameraOff()
    {
        if(_camTexture != null)
        {
            _camTexture.Stop();
            WebCamTexture.Destroy(_camTexture);
            _camTexture = null;
        }
    }
    public void SaveImage()
    {
        if (filePath.Equals(string.Empty))
        {
            filePath = Application.persistentDataPath + "/Image.png";
        }
        Texture texture = CameraViewImage.texture;
        int width = texture.width;
        int height = texture.height;

        RenderTexture currentRenderTexture = RenderTexture.active;
        RenderTexture copiedRenderTexture = new RenderTexture(width, height, 0);

        Graphics.Blit(texture, copiedRenderTexture);

        RenderTexture.active = copiedRenderTexture;

        Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGB24, false);

        texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = currentRenderTexture;

        byte[] texturePNGBytes = texture2D.EncodeToPNG();

        if(!File.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        File.WriteAllBytes(filePath, texturePNGBytes);
        if (File.Exists(filePath + "/Image.png"))
        {
            Log.text = $"���� ����";
        }
    }

    public void LoadImage()
    {
        if (filePath.Equals(string.Empty))
        {
            Log.text = $"������ �����ϴ�.";
            return;
        }
        byte[] byteTexture = File.ReadAllBytes(filePath);
        Log.text += $"byteTexture.Length : {byteTexture.Length}";
        if (byteTexture.Length >0)
        {
            _texture2D = new Texture2D(0, 0);
            _texture2D.LoadImage(byteTexture);
            Rect rect = new Rect(0, 0, _texture2D.width, _texture2D.height);
            LoadImg.GetComponent<SpriteRenderer>().sprite = Sprite.Create(_texture2D, rect, new Vector2(0.5f, 0.5f));
            Log.text += $"�ε� ����";
        }
    }
}
