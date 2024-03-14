using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Android;
using System;



public class LoadImage : MonoBehaviour
{
    [SerializeField] private ProfileText_M profileText_;
    [SerializeField] private GameObject content_obj;
    [SerializeField] private GameObject image_prefeb;
    [SerializeField] private Image profile_Img;
    [SerializeField] private Image select_img;
    public Sprite SelectImageSprite
    {
        get { return select_img.sprite; }
        set
        {
            select_img.sprite = value;
            // 이미지가 변경될 때마다 수행할 작업을 여기에 추가할 수 있습니다.
            profileText_.PlayerSprite = value;            
            Debug.Log("이미지가 변경되었습니다.");
        }
    }



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
        try
        {
            Texture2D texture_save = profile_Img.sprite.texture;
            byte[] saveImage = texture_save.EncodeToPNG();
            DataBase.Instance.PlayerCharacter[0].image = saveImage;
            Client.instance.RegisterCharactorProfile_SaveDataToDB(saveImage);
        }
        catch (Exception)
        {
            Debug.Log("사진이 안됩니다. 하하하");            
        }

        SelectImageSprite = profile_Img.sprite;         
    }

    public void ProfileImage_Set()
    {
        byte[] fileData = DataBase.Instance.PlayerCharacter[0].image;
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Texture2D texture2Dload = texture;

        SelectImageSprite = Sprite.Create(texture2Dload, rect, new Vector2(0.5f, 0.5f));
    }
}
