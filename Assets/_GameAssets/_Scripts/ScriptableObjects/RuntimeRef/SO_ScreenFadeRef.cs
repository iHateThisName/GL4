using UnityEngine;

/// <summary>
/// Runtime reference to the active ScreenFade component.
/// ScreenFade writes Value in Awake, callers pass this to SceneTransition.
/// No static Instance — explicitly wired through the SO asset.
/// </summary>
[CreateAssetMenu(menuName = "Runtime/ScreenFade Reference")]
public class SO_ScreenFadeRef : SO_RuntimeRef<ScreenFade> { }
