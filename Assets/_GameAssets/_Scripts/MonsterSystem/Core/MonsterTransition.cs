using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterSystem
{
    [Serializable]
    public class MonsterTransition
    {
        [Tooltip("State this transition applies from. Null = any state.")]
        public MonsterState fromState;

        [Tooltip("State to transition to when all conditions are met.")]
        public MonsterState toState;

        [Tooltip("All conditions must be true (respecting invert flags) for this transition to fire.")]
        public ConditionEntry[] conditions;

        [Serializable]
        public struct ConditionEntry
        {
            [SerializeReference] public TransitionCondition condition;
            public bool invert;
        }

        public bool Evaluate(MonsterController controller)
        {
            for (int i = 0; i < conditions.Length; i++)
            {
                if (conditions[i].condition == null) return false;
                bool result = conditions[i].condition.Evaluate(controller);
                if (conditions[i].invert) result = !result;
                if (!result) return false;
            }
            return true;
        }

        public int CleanupNullConditions()
        {
            if (conditions == null || conditions.Length == 0) return 0;
            int removed = 0;
            var valid = new List<ConditionEntry>();
            for (int i = 0; i < conditions.Length; i++)
            {
                if (conditions[i].condition != null)
                    valid.Add(conditions[i]);
                else
                    removed++;
            }
            if (removed > 0) conditions = valid.ToArray();
            return removed;
        }
    }
}
