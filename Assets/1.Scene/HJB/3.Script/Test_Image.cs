using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Test_Image : MonoBehaviour
{

    public string base64image;

    [SerializeField] private Image load_img;

    public void SaveImage()
    {        
        //byte[] image = Convert.FromBase64String(base64image);

        Game_Type game_Type = Game_Type.A;
        int level = 1;
        int step = 2;
        Result_DB result_DB = new Result_DB();
        Data_value data_Value = new Data_value(234.21f, 23, 12, 22.44f, 18000);
        //Reult_DB�� Null�� �� ó��
        if (!result_DB.Data.ContainsKey((game_Type, level, step)))
        {
            result_DB.Data.Add((game_Type, level, step), data_Value);
        }
        else
        {
            //���� totalScore�� DB�� �ִ� �������� ũ�ٸ� �ٽ� �Ҵ�
            //if (result_DB.Data[(game_Type, level, step)].TotalScore < 20000)
            //{
            //    result_DB.Data[(game_Type, level, step)] = data_Value;
            //}
        }

        Client.instance.AppGame_SaveResultDataToDB(result_DB, game_Type, level, step);

    }
    
    public void ImageDecoding()
    {
        
        
        Result_DB db = Client.instance.AppStart_LoadAllDataFromDB();
        byte[] image = db.image;
            
        //byte[] image = Convert.FromBase64String(base64image);
        

        Texture2D tex = new Texture2D(1, 1);
        tex.LoadImage(image);
        
        Sprite LoadSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        load_img.sprite = LoadSprite;
        
        Debug.Log(db.Data[(Game_Type.A, 1, 2)].Answers);
        
    }
}