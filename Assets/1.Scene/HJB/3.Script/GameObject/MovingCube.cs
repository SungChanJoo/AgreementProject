using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MovingCube : MonoBehaviour
{
    [SerializeField] private float timeSet;    
    
    [SerializeField]private TextMeshPro cube_text;

    private float result = 0;
    
    public void Start_Obj(int _first, string _operator, int _second)
    {
        AOP_Manager.Instance.Answer_Calculator(_first,0, _second);   
        cube_text.text = $"{_first} {_operator} {_second}";
    }    
    
}
