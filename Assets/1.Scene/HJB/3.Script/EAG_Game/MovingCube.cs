using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MovingCube : MonoBehaviour
{
    [SerializeField] private float timeSet;

    [SerializeField] private TextMeshPro cube_text;

    public float result = 0;

    public float reactionRate = 0;

    public int index;

    Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {        
        transform.LookAt(Camera.main.transform);        
    }
    private void Update()
    {
        reactionRate += Time.deltaTime;
    }

    public void Start_Obj(int _first, char _operator, int _second)
    {        
        cube_text.text = $"{_first} {_operator} {_second}";
    }    
    public void ExplosionAni()
    {
        animator.SetTrigger("Explosion");
        cube_text.text = "";


    }
    public void DefaultAni()
    {
        animator.SetTrigger("Default");

    }
    
}
