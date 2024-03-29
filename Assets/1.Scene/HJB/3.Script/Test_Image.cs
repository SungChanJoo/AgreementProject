using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Test_Image : MonoBehaviour
{

    public string base64image;
    public string asd;

    [SerializeField] private Image load_img;

    public void SaveImage()
    {        
        //byte[] image = Convert.FromBase64String(base64image);

        Game_Type game_Type = Game_Type.A;
        int level = 1;
        int step = 2;
        Player_DB result_DB = new Player_DB();
        Data_value data_Value = new Data_value(234.21f, 23, 12, 22.44f, 18000,3);
        //Reult_DB가 Null일 때 처리
        if (!result_DB.Data.ContainsKey((game_Type, level, step)))
        {
            result_DB.Data.Add((game_Type, level, step), data_Value);
        }
        else
        {
            //만약 totalScore가 DB에 있는 점수보다 크다면 다시 할당
            //if (result_DB.Data[(game_Type, level, step)].TotalScore < 20000)
            //{
            //    result_DB.Data[(game_Type, level, step)] = data_Value;
            //}
        }

        Client.Instance.AppGame_SaveResultDataToDB(result_DB, game_Type, level, step);

    }
    
    public void ImageDecoding()
    {
        
        
        //Player_DB db = Client.instance.AppStart_LoadCharactorDataFromDB();
        //byte[] image = db.image;
            
        byte[] image = Convert.FromBase64String(asd);
        

        Texture2D tex = new Texture2D(1, 1);
        tex.LoadImage(image);
        
        Sprite LoadSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        load_img.sprite = LoadSprite;
        
        //Debug.Log(b.Data[(Game_Type.A, 1, 2)].Answers);
        
    }
}
