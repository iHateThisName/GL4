using Assets.Scripts.Singleton;
using UnityEngine;
using UnityEngine.VFX;

public class VisualEffectManager : Singleton<VisualEffectManager>
{
    [SerializeField] private GameObject vfxPrefab;

    public VisualEffect CreateVfx(Vector3 position)
    {
        var vfxObject = Instantiate(this.vfxPrefab, position ,Quaternion.identity);
        if (vfxObject == null) return null;

        var vfx = vfxObject.GetComponent<VisualEffect>();
        if (vfx == null) return null;
        
        return vfx;
    }
}