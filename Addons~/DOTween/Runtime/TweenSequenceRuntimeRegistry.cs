using System.Collections.Generic;
using DG.Tweening;

namespace Astra.Valkyrie.Integrations.DOTween
{
    /// <summary>
    /// One live tween produced by a <see cref="TweenPlayer"/>: either the sequence itself
    /// (<see cref="StepIndex"/> is -1) or the tween built for one step.
    /// </summary>
    public sealed class TweenSequenceRuntimeEntry
    {
        public TweenSequenceRuntimeEntry(TweenSequenceRuntimeIdentity identity, TweenStepDefinition step, int stepIndex)
        {
            Identity = identity;
            Step = step;
            StepIndex = stepIndex;
        }

        public TweenSequenceRuntimeIdentity Identity { get; }
        public TweenStepDefinition Step { get; }
        public int StepIndex { get; }
        public bool IsSequence => Step == null;
    }

    /// <summary>
    /// Maps live DOTween tweens back to the <see cref="TweenPlayer"/>, asset and step that
    /// built them. Entries are added when a player configures its sequence identity and removed
    /// when that sequence is released or killed. Main thread only; tools (for example the Helios
    /// Tweens tab) query it read-only.
    /// </summary>
    public static class TweenSequenceRuntimeRegistry
    {
        private static readonly Dictionary<Tween, TweenSequenceRuntimeEntry> Entries = new Dictionary<Tween, TweenSequenceRuntimeEntry>();
        private static readonly Dictionary<Sequence, List<Tween>> Owned = new Dictionary<Sequence, List<Tween>>();

        public static int Count => Entries.Count;

        /// <summary>Snapshot of every registered tween and its entry, sequences included.</summary>
        public static IEnumerable<KeyValuePair<Tween, TweenSequenceRuntimeEntry>> Enumerate()
        {
            return new List<KeyValuePair<Tween, TweenSequenceRuntimeEntry>>(Entries);
        }

        public static bool TryGet(Tween tween, out TweenSequenceRuntimeEntry entry)
        {
            if (tween != null && Entries.TryGetValue(tween, out entry))
                return true;
            entry = null;
            return false;
        }

        internal static void Register(TweenSequenceRuntimeIdentity identity, Sequence sequence, IList<(TweenStepDefinition Step, Tween Tween)> builtTweens, IList<TweenStepDefinition> steps)
        {
            if (identity == null || sequence == null)
                return;
            Unregister(sequence);
            var owned = new List<Tween> { sequence };
            Entries[sequence] = new TweenSequenceRuntimeEntry(identity, null, -1);
            if (builtTweens != null)
            {
                for (int i = 0; i < builtTweens.Count; i++)
                {
                    Tween tween = builtTweens[i].Tween;
                    if (tween == null || ReferenceEquals(tween, sequence))
                        continue;
                    int stepIndex = steps != null ? steps.IndexOf(builtTweens[i].Step) : -1;
                    Entries[tween] = new TweenSequenceRuntimeEntry(identity, builtTweens[i].Step, stepIndex);
                    owned.Add(tween);
                }
            }
            Owned[sequence] = owned;
        }

        internal static void Unregister(Sequence sequence)
        {
            if (sequence == null || !Owned.TryGetValue(sequence, out List<Tween> owned))
                return;
            for (int i = 0; i < owned.Count; i++)
                Entries.Remove(owned[i]);
            Owned.Remove(sequence);
        }

        internal static void Clear()
        {
            Entries.Clear();
            Owned.Clear();
        }
    }
}
