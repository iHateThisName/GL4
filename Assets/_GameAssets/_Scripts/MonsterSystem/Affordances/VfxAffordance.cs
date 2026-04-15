using MonsterSystem;
using UnityEngine;

public class VfxAffordance : StateAffordance
{
    [SerializeField] private GameObject vfxParent;
    
    public override void Trigger()
    {
        if (vfxParent != null)
            vfxParent.SetActive(true);
    }

    public override void Stop()
    {
        if (vfxParent != null)
            vfxParent.SetActive(false);
    }
}