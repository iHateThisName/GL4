using MonsterSystem;
using UnityEngine;
using UnityEngine.VFX;

public class VfxAffordance : StateAffordance
{
    [SerializeField] private VisualEffectAsset vfxAsset;
    
    private VisualEffect visualEffect;
    
    public override void Trigger()
    {
        this.visualEffect = VisualEffectManager.Instance.CreateVfx(this.controller.transform.position);
        if (vfxAsset != null)
        {
            this.visualEffect.visualEffectAsset = this.vfxAsset;
            this.visualEffect.gameObject.SetActive(true);
        }
    }

    public override void Stop()
    {
        this.visualEffect.Stop();
    }
}