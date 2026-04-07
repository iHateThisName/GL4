using UnityEngine;

public static class AnimationTriggers {
    public static readonly int Flashed = Animator.StringToHash("FlashlightTrigger");
    public static class Intruder {
        public static readonly int Walk = Animator.StringToHash("WalkTrigger");
        public static readonly int ApproachWindow = Animator.StringToHash("IntruderApproachWindowTrigger");
        public static readonly int IdleWindow = Animator.StringToHash("WindowIdleTrigger");
        public static readonly int OpenWindow = Animator.StringToHash("OpenWindowTrigger");
        public static readonly int EnterWindow = Animator.StringToHash("EnterWindowTrigger");
    }

    public static class Munch {
    }

    public static class Stalker {
        public static readonly int Walk = Animator.StringToHash("WalkTrigger");
    }

    public static int GetTriggerHash(EnumAnimationStates state) {
        int selecteState = state switch {
            EnumAnimationStates.Walk => Intruder.Walk,
            EnumAnimationStates.Flashed => Flashed,
            EnumAnimationStates.IntruderApproachWindow => Intruder.ApproachWindow,
            EnumAnimationStates.IntruderIdleWindow => Intruder.IdleWindow,
            EnumAnimationStates.IntruderOpenWindow => Intruder.OpenWindow,
            _ => 0
        };

        if (selecteState == 0) Debug.LogError($"No trigger found for state {state}");
        return selecteState;
    }
}

public enum EnumAnimationStates {
    None,
    Idle,
    Walk,
    Attack,
    Death,
    Flashed,
    IntruderApproachWindow,
    IntruderIdleWindow,
    IntruderOpenWindow
}