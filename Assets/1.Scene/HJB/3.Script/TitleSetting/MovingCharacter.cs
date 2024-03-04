using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingCharacter : MonoBehaviour
{
    [SerializeField]float speed = 10f;       
    [SerializeField] bool hold;

    RectTransform rect;    
    Vector2 startPoint;
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }
    private void Start()
    {
        startPoint = rect.anchoredPosition;
    }
    private void OnDisable()
    {
        if (!hold)
        {
            rect.anchoredPosition = startPoint + new Vector2(0, Random.Range(-300, 300));        
        }
        else
        {
            rect.anchoredPosition = startPoint;
        }
    }

    private void FixedUpdate()
    {        
        transform.position += Vector3.right *Time.fixedDeltaTime*speed;
    }    

    
}
