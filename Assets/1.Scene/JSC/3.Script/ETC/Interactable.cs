using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public GameObject InteractableButton;
    public GameObject Particle;
    private void Awake()
    {
        //��ư�� ���������� ����
        if (InteractableButton.activeSelf)
            InteractableButton.SetActive(false);
        if (Particle.activeSelf)
            Particle.SetActive(false);
    }
    //��ȣ�ۿ� ������Ʈ�ȿ� ������ ��
    private void OnTriggerEnter(Collider other)
    {
        //��ư�� ���������� Ű��
        if (!InteractableButton.activeSelf)
            InteractableButton.SetActive(true);

    }

    //��ȣ�ۿ� ������Ʈ�� ������ ��
    private void OnTriggerExit(Collider other)
    {
        //��ư�� ���������� ����
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
