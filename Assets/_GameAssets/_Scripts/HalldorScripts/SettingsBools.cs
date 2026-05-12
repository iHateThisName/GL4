using UnityEngine;
using Assets.Scripts.Singleton;

public class SettingsBools : PersistenSingleton<SettingsBools>
{
    //Bools for enabling certain settings. This is used by the ApplyPlayerSettings script
    public bool snapEnabled = false;
    public bool tunnelingEnabled = false;
    public bool teleportEnabled = false;

    public int currentNight = 1;
}
