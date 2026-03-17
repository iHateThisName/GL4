using UnityEngine;

namespace MonsterSystem
{
    public static class MonsterAnimation
    {
        public static void SetTrigger(Animator animator, string param)
        {
            if (animator == null || string.IsNullOrEmpty(param)) return;

            animator.SetTrigger(param);
        }

        public static void SetBool(Animator animator, string param, bool value)
        {
            if (animator == null || string.IsNullOrEmpty(param)) return;

            animator.SetBool(param, value);
        }

        public static void SetFloat(Animator animator, string param, float value)
        {
            if (animator == null || string.IsNullOrEmpty(param)) return;

            animator.SetFloat(param, value);
        }

        public static void SetInt(Animator animator, string param, int value)
        {
            if (animator == null || string.IsNullOrEmpty(param)) return;

            animator.SetInteger(param, value);
        }

        public static void Play(Animator animator, string stateName, int layer = 0)
        {
            if (animator == null || string.IsNullOrEmpty(stateName)) return;

            animator.Play(stateName, layer);
        }
    }
}
