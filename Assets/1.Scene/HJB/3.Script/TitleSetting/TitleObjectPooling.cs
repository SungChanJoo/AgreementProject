using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleObjectPooling : MonoBehaviour
{
    [SerializeField] private GameObject Character_Obj;
    [SerializeField] private GameObject[] poolPosition;
    private List<GameObject> CharacterPool = new List<GameObject>();


    private void Start()
    {
        for (int i = 0; i < poolPosition.Length; i++)
        {
            GameObject character = Instantiate(Character_Obj);
            character.transform.position = poolPosition[i].transform.position;
            character.SetActive(false);
            CharacterPool.Add(character);
        }
    }

    private void RandomSpawn()
    {
        
    }
}
