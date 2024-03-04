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
        playerName.text = DataBase.Instance.PlayerCharacter[num].playerName;
        
        player_num= num;
    }
}
