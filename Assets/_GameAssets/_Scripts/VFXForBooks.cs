using System.Diagnostics;
using UnityEngine;
using UnityEngine.VFX;

public class VFXForBooks : MonoBehaviour
{

    [SerializeField] VisualEffectAsset vfxForBooks;
 


    public void playSmoke()
    {
        vfxForBooks.Play();
    }
}
