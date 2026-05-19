using UnityEngine;

public static class AnimationTriggers {
    public static readonly int Idle = Animator.StringToHash("IdleTrigger");
    public static readonly int Flashed = Animator.StringToHash("FlashlightTrigger");
    public static readonly int Chasing = Animator.StringToHash("ChasingTrigger");
    public static readonly int Walk = Animator.StringToHash("WalkTrigger");
    public static readonly int Attack = Animator.StringToHash("AttackTrigger");
    public static class Intruder {
        public static readonly int ApproachWindow = Animator.StringToHash("IntruderApproachWindowTrigger");
        public static readonly int IdleWindow = Animator.StringToHash("WindowIdleTrigger");
        public static readonly int OpenWindow = Animator.StringToHash("OpenWindowTrigger");
        public static readonly int EnterWindow = Animator.StringToHash("EnterWindowTrigger");
    }

    public static class Munch {
        public static readonly int Eating = Animator.StringToHash("EatingTrigger");
        public static readonly int Enter = Animator.StringToHash("EnterTrigger");
        public static readonly int Exit = Animator.StringToHash("ExitTrigger");
        public static readonly int RejectFood = Animator.StringToHash("RejectFoodTrigger");
        public static readonly int AcceptFood = Animator.StringToHash("AcceptFoodTrigger");
        public static readonly int Hungry = Animator.StringToHash("HungryTrigger");

    }

    public static class Stalker {
        public static readonly int ArmsCover = Animator.StringToHash("ArmsCoverTrigger");
        public static readonly int ArmsUncover = Animator.StringToHash("ArmsUncoverTrigger");
    }

    public static int GetTriggerHash(EnumAnimationStates state) {
        int selecteState = state switch {
            EnumAnimationStates.Walk => AnimationTriggers.Walk,
            EnumAnimationStates.Chasing => AnimationTriggers.Chasing,
            EnumAnimationStates.Flashed => AnimationTriggers.Flashed,
            EnumAnimationStates.Attack => AnimationTriggers.Attack,
            EnumAnimationStates.Idle => AnimationTriggers.Idle,

            EnumAnimationStates.IntruderApproachWindow => Intruder.ApproachWindow,
            EnumAnimationStates.IntruderIdleWindow => Intruder.IdleWindow,
            EnumAnimationStates.IntruderOpenWindow => Intruder.OpenWindow,

            EnumAnimationStates.StalkerArmsCover => Stalker.ArmsCover,
            EnumAnimationStates.StalkerArmsUncover => Stalker.ArmsUncover,


            EnumAnimationStates.MunchEating => Munch.Eating,
            EnumAnimationStates.MunchEnter => Munch.Enter,
            EnumAnimationStates.MunchExit => Munch.Exit,
            EnumAnimationStates.MunchRejectFood => Munch.RejectFood,
            EnumAnimationStates.MunchAcceptFood => Munch.AcceptFood,
            EnumAnimationStates.MunchHungry => Munch.Hungry,
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
    Chasing,
    Attack,
    Death,
    Flashed,

    IntruderApproachWindow,
    IntruderIdleWindow,
    IntruderOpenWindow,

    StalkerArmsCover,
    StalkerArmsUncover,

    MunchEating,
    MunchEnter,
    MunchExit,
    MunchRejectFood,
    MunchAcceptFood,
    MunchHungry,

}