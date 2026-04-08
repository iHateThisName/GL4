using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MonsterSystem.EditorTools
{
    /// <summary>
    /// Custom inspector for <see cref="AnimationStateChange"/> that adds a live scrub
    /// preview of the underlying AnimationClip driven by the fireAt slider.
    /// Drag a rigged GameObject (typically the monster prefab instance in the scene)
    /// into the Preview Target field, toggle Live Preview on, and drag the slider
    /// to see the character pose at that point of the animation.
    /// </summary>
    [CustomEditor(typeof(AnimationStateChange))]
    public class AnimationStateChangeEditor : Editor
    {
        private const string PreviewTargetPrefKeyPrefix = "MonsterSystem.AnimationStateChange.PreviewTarget.";

        private GameObject previewTarget;
        private bool livePreview;
        private AnimationClip cachedClip;
        private AnimatorState cachedState;
        private bool clipLookupDone;

        private void OnEnable()
        {
            string instanceId = target.GetInstanceID().ToString();
            int savedId = EditorPrefs.GetInt(PreviewTargetPrefKeyPrefix + instanceId, 0);
            if (savedId != 0)
                this.previewTarget = EditorUtility.InstanceIDToObject(savedId) as GameObject;
        }

        private void OnDisable()
        {
            StopPreview();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animation Preview", EditorStyles.boldLabel);

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Preview is disabled in play mode (the live Animator owns the rig).", MessageType.Info);
                StopPreview();
                return;
            }

            if (!this.clipLookupDone)
            {
                ResolveClip();
                this.clipLookupDone = true;
            }

            if (this.cachedClip == null)
            {
                EditorGUILayout.HelpBox(
                    "Couldn't resolve an AnimationClip for this state. Either the state's Motion is " +
                    "a BlendTree / sub-state machine, or this SMB isn't on an AnimatorController asset.",
                    MessageType.Warning);
                if (GUILayout.Button("Retry clip lookup"))
                {
                    this.clipLookupDone = false;
                    Repaint();
                }
                return;
            }

            EditorGUILayout.ObjectField("Clip", this.cachedClip, typeof(AnimationClip), false);

            EditorGUI.BeginChangeCheck();
            var newTarget = (GameObject)EditorGUILayout.ObjectField(
                "Preview Target", this.previewTarget, typeof(GameObject), true);
            if (EditorGUI.EndChangeCheck())
            {
                this.previewTarget = newTarget;
                string instanceId = target.GetInstanceID().ToString();
                EditorPrefs.SetInt(
                    PreviewTargetPrefKeyPrefix + instanceId,
                    this.previewTarget != null ? this.previewTarget.GetInstanceID() : 0);
            }

            using (new EditorGUI.DisabledScope(this.previewTarget == null))
            {
                bool wasLive = this.livePreview;
                this.livePreview = EditorGUILayout.Toggle("Live Preview", this.livePreview);
                if (wasLive != this.livePreview)
                {
                    if (this.livePreview) StartPreview();
                    else StopPreview();
                }
            }

            if (this.previewTarget == null)
            {
                EditorGUILayout.HelpBox("Drag a rigged GameObject from the scene to preview the pose.", MessageType.Info);
            }

            if (this.livePreview && this.previewTarget != null)
            {
                SamplePreview();
                // Repaint constantly while live so dragging the slider in the default
                // inspector immediately updates the sampled pose.
                Repaint();
            }
        }

        private void StartPreview()
        {
            if (!AnimationMode.InAnimationMode())
                AnimationMode.StartAnimationMode();
        }

        private void StopPreview()
        {
            this.livePreview = false;
            if (AnimationMode.InAnimationMode())
                AnimationMode.StopAnimationMode();
        }

        private void SamplePreview()
        {
            if (this.cachedClip == null || this.previewTarget == null) return;
            if (!AnimationMode.InAnimationMode()) AnimationMode.StartAnimationMode();

            var smb = (AnimationStateChange)target;
            float time = smb.FireAt * this.cachedClip.length;

            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(this.previewTarget, this.cachedClip, time);
            AnimationMode.EndSampling();

            SceneView.RepaintAll();
        }

        /// <summary>
        /// Walks every AnimatorController asset to find the AnimatorState whose behaviours
        /// list contains the inspected SMB, then caches that state's clip.
        /// </summary>
        private void ResolveClip()
        {
            this.cachedClip = null;
            this.cachedState = null;

            string assetPath = AssetDatabase.GetAssetPath(target);
            if (string.IsNullOrEmpty(assetPath)) return;

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
            if (controller == null) return;

            foreach (var layer in controller.layers)
            {
                if (FindStateInMachine(layer.stateMachine, out var state))
                {
                    this.cachedState = state;
                    this.cachedClip = state.motion as AnimationClip;
                    return;
                }
            }
        }

        private bool FindStateInMachine(AnimatorStateMachine machine, out AnimatorState found)
        {
            foreach (var child in machine.states)
            {
                var behaviours = child.state.behaviours;
                for (int i = 0; i < behaviours.Length; i++)
                {
                    if (behaviours[i] == target)
                    {
                        found = child.state;
                        return true;
                    }
                }
            }
            foreach (var sub in machine.stateMachines)
            {
                if (FindStateInMachine(sub.stateMachine, out found))
                    return true;
            }
            found = null;
            return false;
        }
    }
}
