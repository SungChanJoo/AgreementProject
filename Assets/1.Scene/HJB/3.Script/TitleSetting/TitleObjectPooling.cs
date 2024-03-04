using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleObjectPooling : MonoBehaviour
{
    [SerializeField] private GameObject[] Character_Obj;
    [SerializeField] private Transform[] poolPosition;
    private List<GameObject> CharacterPool = new List<GameObject>();


    private void Start()
    {
        int randomPosition = Random.Range(0, 4);
        for (int i = 0; i < Character_Obj.Length; i++)
        {
            //상하좌우 포지션에 상속
            GameObject character = Instantiate(Character_Obj[i],poolPosition[randomPosition]);
            character.transform.position= poolPosition[randomPosition].position;
            
            character.SetActive(false);
            CharacterPool.Add(character);
        }
    }

    private void RandomSpawn()
    {
        
    }
}
