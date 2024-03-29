using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BubbleObject : MonoBehaviour
{
    [SerializeField] private float timeSet;

    [SerializeField] private TextMeshPro cubeText;

    public float result = 0;

    public float reactionRate = 0;

    public int index;

    Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }    
    private void Update()
    {
        reactionRate += Time.deltaTime;
        
    }
    
    public void ObjectTextPrint(int _first, char _operator, int _second)
    {        
        cubeText.text = $"{_first} {_operator} {_second}";
    }    
    public void ExplosionAni()
    {
        animator.SetTrigger("Explosion");
        cubeText.text = "";
    }
    public void DefaultAni()
    {
        animator.SetTrigger("Default");
    }
    public void UpScale()
    {
        StartCoroutine(UpScale_Co());
    }
    private IEnumerator UpScale_Co()
    {
        yield return new WaitForSeconds(0.5f);        
        float duration = 2f;
        float startTime = Time.time;

        Vector3 targetScale = new Vector3(30f, 30f, transform.localScale.z);

        while (Time.time - startTime < duration && transform.localScale.x < 30f)
        {
            float fraction = (Time.time - startTime) / duration;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, fraction);
            transform.LookAt(Camera.main.transform);
            yield return null;
        }
    }
    
}
