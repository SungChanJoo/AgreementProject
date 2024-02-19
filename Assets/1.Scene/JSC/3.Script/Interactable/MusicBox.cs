using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MusicBox : ObjInteractable
{
    public AudioSource audioSource;
    //���� ����
    public override IEnumerator PlayInterctable()
    {
        if (player != null)
        {
            player.InteractablePlayMusicBox(audioSource, PlayTime);
            yield return new WaitForSeconds(PlayTime + LimitTime);
            currentPlayInterctable = null;
        }
    }
}
