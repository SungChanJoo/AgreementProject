using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Firecracker : ObjInteractable
{
    public List<GameObject> ParticleList;


    //이펙트 실행
    public override IEnumerator PlayInterctable()
    {
        //파티클 리스트 만들어서 랜덤 파티클 해줘 todo 02.16
        var rand = Random.Range(0, ParticleList.Count);
        InteractableParticle.SetActive(false);
        ParticleList[rand].SetActive(true);
        yield return new WaitForSeconds(PlayTime);
        ParticleList[rand].SetActive(false);
        yield return new WaitForSeconds(LimitTime);
        if(player != null)
            InteractableParticle.SetActive(true);
        currentPlayInterctable = null;
    }
}
