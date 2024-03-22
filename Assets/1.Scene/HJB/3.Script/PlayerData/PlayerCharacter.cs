using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private Image img;
    public int player_num;
    
    public void SetProfile(int num)
    {
        playerName.text = DataBase.Instance.UserList.user[num].Name;
        img.sprite = ProfileImage_Set(DataBase.Instance.UserList.user[num].Profile);
        player_num = num;
    }
    public Sprite ProfileImage_Set(byte[] img)
    {
        byte[] fileData = img;
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Texture2D texture2Dload = texture;

        return Sprite.Create(texture2Dload, rect, new Vector2(0.5f, 0.5f));
    }

}
