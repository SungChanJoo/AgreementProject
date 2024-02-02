using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public GameObject InteractableButton;
    public GameObject Particle;
    private void Awake()
    {
        //버튼이 켜져있으면 끄기
        if (InteractableButton.activeSelf)
            InteractableButton.SetActive(false);
        if (Particle.activeSelf)
            Particle.SetActive(false);
    }
    //상호작용 오브젝트안에 들어왔을 때
    private void OnTriggerEnter(Collider other)
    {
        //버튼이 꺼져있으면 키기
        if (!InteractableButton.activeSelf)
            InteractableButton.SetActive(true);

    }

    //상호작용 오브젝트를 나갔을 때
    private void OnTriggerExit(Collider other)
    {
        //버튼이 켜져있으면 끄기
        if (InteractableButton.activeSelf)
            InteractableButton.SetActive(false);
    }

    public void OnClickInteract()
    {
        StartCoroutine(PlayEffect());
    }
    IEnumerator PlayEffect()
    {
        Particle.SetActive(true);
        while(Particle.GetComponent<ParticleSystem>().isPlaying)
        {
            yield return null;
        }
        Particle.SetActive(false);
    }
    
}
