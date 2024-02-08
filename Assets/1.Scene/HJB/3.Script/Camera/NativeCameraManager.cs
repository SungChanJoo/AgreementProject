using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;
using System;
using System.IO;


public class NativeCameraManager : MonoBehaviour
{
    
    public Image captureImage;    
    public Texture2D captureTexture;

    int CaptureCounter = 0;

    // Start is called before the first frame update
   

    public void NativeCameraOpen()
    {
        SettingManager.Instance.Re_AppSetPermission();
        
        //Cemearar가 실행중이라면
        if (NativeCamera.IsCameraBusy())
        {
            return;
        }

        TakePicture();
    }

    void TakePicture()
    {
        //영구 파일 경로 저장
        string SavePath = Application.persistentDataPath;        

        NativeCamera.Permission permission = NativeCamera.TakePicture((path) =>
        {            
            string galaryPath = SavePath.Substring(0, SavePath.IndexOf("Android")) + "/DCIM/UnityCamera/";
            if (false == string.IsNullOrEmpty("UnityCamera") && false == Directory.Exists("UnityCamera"))
            {
                //해당 디렉토리 없을 시 생성.
                Directory.CreateDirectory(galaryPath);
            }
            Debug.Log("Image path : " + galaryPath);
            if (path != null)
            {
                Texture2D texture = NativeCamera.LoadImageAtPath(path, 2048);
                if (texture == null)
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }

                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2.5f;
                quad.transform.forward = Camera.main.transform.forward;
                quad.transform.localScale = new Vector3(1f, texture.height / (float)texture.width, 1f);                
                Material material = quad.GetComponent<Renderer>().material;
                if (!material.shader.isSupported)
                {
                    material.shader = Shader.Find("Legacy Shaders/Diffuse");
                }
                material.mainTexture = texture;
                captureTexture = texture;
                var rect = new Rect(0, 0, captureTexture.width, captureTexture.height);
                captureImage.sprite = Sprite.Create(captureTexture, rect, new Vector2(0.5f, 0.5f));


                //texture.GetPixels(); 사용 시 texture is not readable error 발생하여
                //GetReadableTexture(texture); 로 readable한 Texture 만들어야 함.
                Texture2D readableTexture = GetReadableTexture(texture);
                Texture2D snap = new Texture2D(readableTexture.width, readableTexture.height);
                snap.SetPixels(readableTexture.GetPixels());
                snap.Apply();

                string time = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                File.WriteAllBytes(
                    galaryPath + "UnityCamera" + time + CaptureCounter.ToString() + ".png", snap.EncodeToPNG()
                    );
                ++CaptureCounter;
                Destroy(quad, 5f);                
            }            
        }, 2048, true, NativeCamera.PreferredCamera.Front);
    }

    private Texture2D GetReadableTexture(Texture2D source)
    {

        RenderTexture renderTex = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableTexture = new Texture2D(source.width, source.height);
        readableTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableTexture.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableTexture;
    }

}
