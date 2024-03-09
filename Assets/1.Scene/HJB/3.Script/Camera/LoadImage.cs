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
    [SerializeField] private Image select_img;
    private GameObject[] image_obj; 
    private Texture2D[] texture2Ds;
    private bool loading = false;

    public void ImageFileLoad()
    {
        if (!loading)
        {
            loading = true;
            StartCoroutine(ImageFileLoad_Co());
        }
    }    
    private IEnumerator ImageFileLoad_Co()
    {
        if (content_obj.transform.childCount>0)
        {
            GameObject[] children = new GameObject[content_obj.transform.childCount];
            for (int i = 0; i < content_obj.transform.childCount; i++)
            {
                children[i] = content_obj.transform.GetChild(i).gameObject;
            }
            foreach (GameObject child in children)
            {
                Destroy(child);
            }
        }
        SettingManager.Instance.Re_AppSetPermission();
        string pathFolder = Application.persistentDataPath;
        string galleryPath = $"{pathFolder}/";
        string[] imageFiles = Directory.GetFiles(galleryPath, "*.jpg");

        texture2Ds = new Texture2D[imageFiles.Length];
        image_obj = new GameObject[imageFiles.Length];

        for (int i = 0; i < imageFiles.Length; i++)
        {
            image_obj[i] = Instantiate(image_prefeb);
            Button button = image_obj[i].GetComponent<Button>();
            int index = i;
            button.onClick.AddListener(() => SelectProfile_Image(index));
            image_obj[i].transform.SetParent(content_obj.transform);

            byte[] fileData = File.ReadAllBytes(imageFiles[i]);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            texture2Ds[i] = texture;
            Image img = image_obj[i].GetComponent<Image>();
            img.sprite = Sprite.Create(texture2Ds[i], rect, new Vector2(0.5f, 0.5f));
        }
        loading = false;
        yield return new WaitForSeconds(0.1f);
    }

    public void SelectProfile_Image(int index)
    {
        Rect rect = new Rect(0, 0, texture2Ds[index].width, texture2Ds[index].height);

        profile_Img.sprite = Sprite.Create(texture2Ds[index], rect, new Vector2(0.5f, 0.5f));
    }    
    public void Selectprofile_Change()
    {
        select_img.sprite = profile_Img.sprite;
    }

    
}
