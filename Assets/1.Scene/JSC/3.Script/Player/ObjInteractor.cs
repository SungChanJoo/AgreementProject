using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
//개인에게만 상호작용 가능하게 만들기 위한 크랠스
public class ObjInteractor : NetworkBehaviour
{
    [Header("MusicKeyboard")]
    public AudioSource audioSource;
    public List<AudioClip> audioClips;
    public int _clipIndex = 0;
    public float time = 0;

    private void OnTriggerStay(Collider other)
    {
        if (!isLocalPlayer) return;
        if(other.gameObject.CompareTag("MusicKeyboard"))
        {
            audioClips = other.GetComponent<MusicKeyboard>().audioClips;
            time += Time.deltaTime;
            OnClickObj();
        }

    }
    //상호작용 가능한 UI show or hide 
    public void InteractableUI(GameObject UI, bool value)
    {
        if (!isLocalPlayer) return;

        UI.SetActive(value);
    }
    public void InteractablePlayMusicBox(AudioSource audioSource, int playTime)
    {
        if (!isLocalPlayer) return;
        StartCoroutine(PlayMusic_co(audioSource, playTime));
    }
    IEnumerator PlayMusic_co(AudioSource audioSource, int playTime)
    {
        audioSource.Play();
        yield return new WaitForSeconds(playTime);
        audioSource.Stop();
    }

    public void OnClickObj()
    {
        Ray ray;

        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                Click(ray);
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Click(ray);
            }
        }

    }
    public void Click(Ray ray)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.CompareTag("MusicKeyboard"))
            {
                Debug.Log("touch");

                if (_clipIndex >= audioClips.Count || time > 3) _clipIndex = 0;
                audioSource.clip = audioClips[_clipIndex];
                audioSource.Play();
                time = 0;
                _clipIndex++;
            }
        }
    }
}
