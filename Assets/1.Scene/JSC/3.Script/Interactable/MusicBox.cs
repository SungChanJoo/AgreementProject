using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
//뮤직박스 클래스
public class MusicBox : ObjInteractable
{
    public AudioSource audioSource;
    //음악 실행
    public override IEnumerator PlayInterctable()
    {
        if (Player != null)
        {
            Player.InteractablePlayMusicBox(audioSource, PlayTime); //실행시간만큼 재생
            yield return new WaitForSeconds(PlayTime + LimitTime); //음악 재생 쿨타임
            currentPlayInterctable = null;
        }
    }
}
