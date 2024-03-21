using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;
    public int player_num;
    
    public void InputName(int num)
    {
        playerName.text = DataBase.Instance.UserList.user[num].Name;
        
        player_num= num;
    }
}
