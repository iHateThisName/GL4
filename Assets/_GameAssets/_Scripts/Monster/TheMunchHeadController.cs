using System.Collections;
using System.Linq.Expressions;
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

    public enum EnumExpression {
        Normal,
        Angry,
        WideOpen
    }
    private void Start() {
        this.doNotTouchIndex = this.skinnedMesh.sharedMesh.GetBlendShapeIndex(doNotTouchName);
        this.teethOutIndex = this.skinnedMesh.sharedMesh.GetBlendShapeIndex(teethOutName);
        this.wideOpenIndex = this.skinnedMesh.sharedMesh.GetBlendShapeIndex(wideOpenName);

        this.skinnedMesh.SetBlendShapeWeight(doNotTouchIndex, 100f);
    }

    [ContextMenu("Smile")]
    public void Smile() { // Normal
        StopAllCoroutines();
        StartCoroutine(TransitionBlendShape(this.teethOutIndex, 0f, this.transitionSpeed * 1.5f));
        StartCoroutine(TransitionBlendShape(this.wideOpenIndex, 0f, this.transitionSpeed));
    }

    [ContextMenu("Angry")]
    public void Angry() {
        StopAllCoroutines();
        StartCoroutine(TransitionBlendShape(this.teethOutIndex, 100f, this.transitionSpeed * 1.5f));
        StartCoroutine(TransitionBlendShape(this.wideOpenIndex, 0f, this.transitionSpeed));
    }

    [ContextMenu("Wide Open")]
    public void OpenWide() {
        StopAllCoroutines();
        StartCoroutine(TransitionBlendShape(this.teethOutIndex, 0f, this.transitionSpeed * 1.5f));
        StartCoroutine(TransitionBlendShape(this.wideOpenIndex, 100f, this.transitionSpeed));
    }

    public void PlayExpression(EnumExpression expression) {
        switch (expression) {
            case TheMunchHeadController.EnumExpression.Normal:
                Smile();
                break;
            case TheMunchHeadController.EnumExpression.Angry:
                Angry();
                break;
            case TheMunchHeadController.EnumExpression.WideOpen:
                OpenWide();
                break;
        }
    }

    private IEnumerator TransitionBlendShape(int blendShapeIndex, float targetWeight, float speed) {
        float currentWeight = skinnedMesh.GetBlendShapeWeight(blendShapeIndex);
        while (!Mathf.Approximately(currentWeight, targetWeight)) {
            currentWeight = Mathf.MoveTowards(currentWeight, targetWeight, speed * Time.deltaTime);
            skinnedMesh.SetBlendShapeWeight(blendShapeIndex, currentWeight);
            yield return null;
        }
    }
}
