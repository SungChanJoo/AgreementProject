using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
//ÆøÁ× Å¬·¡½º
public class Firecracker : ObjInteractable
{
    public List<GameObject> ParticleList;
    //ÀÌÆåÆ® ½ÇÇà
    public override IEnumerator PlayInterctable()
    {
        var rand = Random.Range(0, ParticleList.Count); //ParticleList¿¡¼­ ·£´ıÇÑ ÆøÁ× ÀÌÆåÆ® Àç»ı
        InteractableParticle.SetActive(false);
        ParticleList[rand].SetActive(true);
        yield return new WaitForSeconds(PlayTime); //ÆøÁ× ÀÌÆåÆ® Àç»ı½Ã°£
        ParticleList[rand].SetActive(false);
        yield return new WaitForSeconds(LimitTime);//ÆøÁ× ÀÌÆåÆ® ÄğÅ¸ÀÓ
        if (Player != null)
            InteractableParticle.SetActive(true); // »óÈ£ÀÛ¿ë °¡´ÉÇÑ °ÍÀ» ¾Ë¸®±â À§ÇÑ ÀÌÆåÆ®
        currentPlayInterctable = null;
    }
}
