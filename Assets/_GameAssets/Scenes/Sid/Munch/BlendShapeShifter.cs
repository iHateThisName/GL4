using UnityEngine;
using UnityEngine.InputSystem;

public class BlendShapeShifter : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedMesh;

    [Header("Blendshape Names")]
    public string doNotTouchName = "DoNotTouch";
    public string teethOutName = "TeethOut";
    public string wideOpenName = "WideOpen";

    [Header("Settings")]
    public float doNotTouchWeight = 100f;
    public float targetWeight = 100f;
    public float transitionSpeed = 5f; // higher = faster

    private int doNotTouchIndex;
    private int teethOutIndex;
    private int wideOpenIndex;

    private float currentTeethWeight = 0f;
    private float currentWideWeight = 0f;

    private float targetTeethWeight = 0f;
    private float targetWideWeight = 0f;

    void Start()
    {
        doNotTouchIndex = skinnedMesh.sharedMesh.GetBlendShapeIndex(doNotTouchName);
        teethOutIndex = skinnedMesh.sharedMesh.GetBlendShapeIndex(teethOutName);
        wideOpenIndex = skinnedMesh.sharedMesh.GetBlendShapeIndex(wideOpenName);

        // Keep DoNotTouch always at 100
        if (doNotTouchIndex != -1)
            skinnedMesh.SetBlendShapeWeight(doNotTouchIndex, doNotTouchWeight);
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // INPUT
        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            // Angry
            targetTeethWeight = targetWeight;
            targetWideWeight = 0f;
        }

        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            // Cracy
            targetTeethWeight = 0f;
            targetWideWeight = targetWeight;
        }

        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            // Normal, smiling
            targetTeethWeight = 0f;
            targetWideWeight = 0f;
        }

        // SMOOTH TRANSITION
        currentTeethWeight = Mathf.Lerp(currentTeethWeight, targetTeethWeight, Time.deltaTime * transitionSpeed);
        currentWideWeight = Mathf.Lerp(currentWideWeight, targetWideWeight, Time.deltaTime * transitionSpeed);

        // APPLY
        if (teethOutIndex != -1)
            skinnedMesh.SetBlendShapeWeight(teethOutIndex, currentTeethWeight);

        if (wideOpenIndex != -1)
            skinnedMesh.SetBlendShapeWeight(wideOpenIndex, currentWideWeight);

        // Keep DoNotTouch always max
        if (doNotTouchIndex != -1)
            skinnedMesh.SetBlendShapeWeight(doNotTouchIndex, doNotTouchWeight);
    }
}