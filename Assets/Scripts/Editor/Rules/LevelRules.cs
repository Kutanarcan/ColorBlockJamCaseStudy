using System;
using System.Collections.Generic;
using UnityEditor;

namespace Game.LevelEditor
{
    /// <summary>
    /// Runs every design rule. Discovery uses reflection (TypeCache), which is allowed here: this assembly is
    /// Editor-only and never reaches a player build (D51).
    /// </summary>
    public sealed class LevelRules
    {
        private readonly ILevelRule[] rules;

        public IReadOnlyList<ILevelRule> Rules => rules;

        public LevelRules(params ILevelRule[] rules) => this.rules = rules;

        public static LevelRules Discover()
        {
            var found = new List<ILevelRule>();

            foreach (Type type in TypeCache.GetTypesDerivedFrom<ILevelRule>())
            {
                if (!type.IsAbstract && !type.IsInterface && type.GetConstructor(Type.EmptyTypes) != null)
                    found.Add((ILevelRule)Activator.CreateInstance(type));
            }

            found.Sort((a, b) => string.CompareOrdinal(a.GetType().FullName, b.GetType().FullName));

            return new LevelRules(found.ToArray());
        }

        public List<RuleViolation> Check(LevelModel level)
        {
            var violations = new List<RuleViolation>();

            foreach (ILevelRule rule in rules)
            {
                rule.Check(level, violations);
            }

            return violations;
        }
    }
}
