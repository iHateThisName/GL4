using UnityEngine;

public static class AnimationTriggers {
    public static class Intruder {
        public static readonly int Walk = Animator.StringToHash("WalkTrigger");
        public static readonly int OpenWindow = Animator.StringToHash("IntruderOpenWindowTrigger");
    }

    public static class Munch {
    }

    public static class Stalker {
        public static readonly int Walk = Animator.StringToHash("WalkTrigger");
    }

    //public static int GetTriggerHash(BaseNavAIMonster.EnumMonsterType monsterType, EnumAnimationStates state) {
    //    switch (monsterType) {
    //        case BaseNavAIMonster.EnumMonsterType.Stalker:
    //            switch (state) {
    //                case EnumAnimationStates.Walk:
    //                    return Stalker.Walk;
    //                default:
    //                    return 0;
    //            }
    //        case BaseNavAIMonster.EnumMonsterType.Munch:
    //            switch (state) {
    //                default:
    //                    return 0;
    //            }
    //        case BaseNavAIMonster.EnumMonsterType.Intruder:
    //            switch (state) {
    //                case EnumAnimationStates.Walk:
    //                    return Intruder.Walk;
    //                default:
    //                    return 0;
    //            }
    //        default:
    //            return 0;
    //    }
    //}

    //public static int GetTriggerHash(BaseNavAIMonster.EnumMonsterType monsterType, EnumAnimationStates state) {
    //    return (monsterType, state) switch {
    //        (BaseNavAIMonster.EnumMonsterType.Intruder, EnumAnimationStates.Walk) => GetTriggerHash(state),
    //        (BaseNavAIMonster.EnumMonsterType.Stalker, EnumAnimationStates.Walk) => GetTriggerHash(state),

    //        _ => 0
    //    };
    //}
    public static int GetTriggerHash(EnumAnimationStates state) {
        int selecteState = state switch {
            EnumAnimationStates.Walk => Intruder.Walk,
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
    IntruderOpenWindow,
    Death
}