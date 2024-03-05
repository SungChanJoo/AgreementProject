using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Android;
using System;



public class LoadImage : MonoBehaviour
{
    [SerializeField] private GameObject content_obj;
    [SerializeField] private GameObject image_prefeb;
    [SerializeField] private Image profile_Img;
    private GameObject[] image_obj; 
    private Texture2D[] texture2Ds;

    public void ImageFileLoad()
    {
        SettingManager.Instance.Re_AppSetPermission();
        string pathFolder = Application.persistentDataPath;
        string galaryPath = pathFolder.Substring(0, pathFolder.IndexOf("Android")) + "/DCIM/UnityCamera/";
        string[] pngFiles = Directory.GetFiles(galaryPath,"*.png");
        Texture2D[] textures = new Texture2D[pngFiles.Length];
        Debug.Log(textures.Length);
        image_obj = new GameObject[pngFiles.Length];
        for (int i = 0; i < pngFiles.Length; i++)
        {
            image_obj[i] = Instantiate(image_prefeb);
            Button button = image_obj[i].GetComponent<Button>();
            int index = i;
            button.onClick.AddListener(() => SelectProfile_Image(index));
            image_obj[i].transform.SetParent(content_obj.transform);
        }

        for (int i = 0; i < pngFiles.Length; i++)
        {
            byte[] fileData = File.ReadAllBytes(pngFiles[i]);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            Rect rect = new Rect(0, 0, texture.width, texture.height);                        
            textures[i] = texture;
            Image img = image_obj[i].GetComponent<Image>();
            img.sprite = Sprite.Create(textures[i], rect, new Vector2(0.5f, 0.5f));
        }        
        texture2Ds = new Texture2D[pngFiles.Length];
        texture2Ds = textures;

    }    
    public void SelectProfile_Image(int index)
    {
        Rect rect = new Rect(0, 0, texture2Ds[index].width, texture2Ds[index].height);

        profile_Img.sprite = Sprite.Create(texture2Ds[index], rect, new Vector2(0.5f, 0.5f));


    }

    
}
