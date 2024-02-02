using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    WebCamTexture _camTexture;

    public RawImage CameraViewImage; //카메라가 보여질 화면
    Texture2D _texture2D;
    public Text Log;
    public Image LoadImg;
    string filePath = string.Empty;
    public void CameraOn()
    {
        //권한 확인
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
        if (WebCamTexture.devices.Length == 0)//카메라가 없다면
        {
            return;
        }
        WebCamDevice[] devices = WebCamTexture.devices;//카메라 가져오기
        int selectedCameraIndex = -1;

        //후면 카메라 찾기
        for(int i =0; i< devices.Length; i++)
        {
            if (devices[i].isFrontFacing == false)
            {
                selectedCameraIndex = i;
                break;
            }
        }
        //카메라 켜기
        if(selectedCameraIndex>= 0)
        {
            //후면 카메라 가져옴
            _camTexture = new WebCamTexture(devices[selectedCameraIndex].name);
            _camTexture.requestedFPS = 30; //카메라 프레임
            CameraViewImage.texture = _camTexture;
            _camTexture.Play();
        }
    }
    //카메라 끄기
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
            Log.text = $"저장 성공";
        }
    }

    public void LoadImage()
    {
        if (filePath.Equals(string.Empty))
        {
            Log.text = $"파일이 없습니다.";
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
            Log.text += $"로드 성공";
        }
    }
}
