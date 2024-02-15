using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Resut_Data : MonoBehaviour
{
    public string playerName;
    public Dictionary<string, Dictionary<int, Dictionary<int, List<int>>>> gameData;

    private void Start()
    {
        gameData = new Dictionary<string, Dictionary<int, Dictionary<int, List<int>>>>();

        var test = new List<int>();        
    }

}

