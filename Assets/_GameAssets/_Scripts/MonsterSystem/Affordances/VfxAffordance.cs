using MonsterSystem;
using UnityEngine;
using UnityEngine.VFX;

public class VfxAffordance : StateAffordance {
    [SerializeField] private VisualEffectAsset vfxAsset;
    [SerializeField] private bool isChild = false;

    [SerializeField, Gaskellgames.ReadOnly] private GameObject vfxInstance;
    private VisualEffect visualEffect;

    public override void OnTrigger() {
        if (this.vfxAsset == null) {
            Debug.LogWarning($"No VFX asset assigned for {this.name} on {this.controller.name}");
            return;
        }

        if (this.vfxInstance == null) {
            this.vfxInstance = new GameObject("VFX_" + vfxAsset.name);
            this.visualEffect = this.vfxInstance.AddComponent<VisualEffect>();
        }

        if (this.isChild) {
            this.vfxInstance.transform.SetParent(this.controller.transform);
        } else {
            this.vfxInstance.transform.position = this.controller.transform.position;
        }

        this.visualEffect.visualEffectAsset = this.vfxAsset;
        this.vfxInstance.SetActive(true);
        this.visualEffect.Play();
    }

    public override void OnStop() 
    {
        this.visualEffect.Stop();
        this.vfxInstance.SetActive(false);
    }

    private void OnDestroy() {
        if (this.vfxInstance != null) {
            Destroy(this.vfxInstance);
        }
    }
}