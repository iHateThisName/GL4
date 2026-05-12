using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class TheMunchHeadController : MonoBehaviour {
    [Header("Blendshape Names")]
    private const string doNotTouchName = "DoNotTouch";
    private const string teethOutName = "TeethOut";
    private const string wideOpenName = "WideOpen";

    private int doNotTouchIndex;
    private int teethOutIndex;
    private int wideOpenIndex;

    [Header("Settings")]
    [SerializeField] private SkinnedMeshRenderer skinnedMesh;
    //[SerializeField] private float doNotTouchWeight = 100f; // should always be 100
    [SerializeField] private float transitionSpeed = 5f; // higher = faster

    private void Start() {
        this.doNotTouchIndex = this.skinnedMesh.sharedMesh.GetBlendShapeIndex(doNotTouchName);
        this.teethOutIndex = this.skinnedMesh.sharedMesh.GetBlendShapeIndex(teethOutName);
        this.wideOpenIndex = this.skinnedMesh.sharedMesh.GetBlendShapeIndex(wideOpenName);

        this.skinnedMesh.SetBlendShapeWeight(doNotTouchIndex, 100f);
    }

    public void Smile() { // Normal
        StopAllCoroutines();
        StartCoroutine(TransitionBlendShape(teethOutIndex, 0f));
        StartCoroutine(TransitionBlendShape(wideOpenIndex, 0f));
    }

    public void Angry() {
        StopAllCoroutines();
        StartCoroutine(TransitionBlendShape(teethOutIndex, 100f));
        StartCoroutine(TransitionBlendShape(wideOpenIndex, 0f));
    }

    public void OpenWide() {
        StopAllCoroutines();
        StartCoroutine(TransitionBlendShape(teethOutIndex, 0f));
        StartCoroutine(TransitionBlendShape(wideOpenIndex, 100f));
    }

    private IEnumerator TransitionBlendShape(int blendShapeIndex, float targetWeight) {
        float currentWeight = skinnedMesh.GetBlendShapeWeight(blendShapeIndex);
        while (!Mathf.Approximately(currentWeight, targetWeight)) {
            currentWeight = Mathf.MoveTowards(currentWeight, targetWeight, transitionSpeed * Time.deltaTime);
            skinnedMesh.SetBlendShapeWeight(blendShapeIndex, currentWeight);
            yield return null;
        }
    }
}
