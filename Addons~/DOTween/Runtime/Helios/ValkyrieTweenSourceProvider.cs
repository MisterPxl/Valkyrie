using System;
using System.Globalization;
using DG.Tweening;
using UnityEngine;
using Astra.Helios.Integrations.DOTween;

namespace Astra.Valkyrie.Integrations.DOTween.Helios
{
    /// <summary>
    /// Describes tweens built by <see cref="TweenPlayer"/> to the Helios Tweens tab: owning
    /// player and readable id, sequence asset, step and the player's current diagnostics.
    /// Registered automatically when both integrations are present; <see cref="Unregister"/>
    /// removes it. Without this assembly Helios simply shows no source for these tweens.
    /// </summary>
    public sealed class ValkyrieTweenSourceProvider : IDOTweenSourceProvider
    {
        private static ValkyrieTweenSourceProvider _instance;
        private static IDisposable _registration;

        public static bool IsRegistered => _registration != null && DOTweenSourceProviders.IsRegistered(_instance);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Register()
        {
            if (IsRegistered)
                return;
            _instance = _instance ?? new ValkyrieTweenSourceProvider();
            _registration = DOTweenSourceProviders.Register(_instance);
        }

        public static void Unregister()
        {
            _registration?.Dispose();
            _registration = null;
        }

        public bool TryDescribe(Tween tween, out DOTweenTweenSource source)
        {
            source = null;
            if (!TweenSequenceRuntimeRegistry.TryGet(tween, out TweenSequenceRuntimeEntry entry) || entry.Identity == null)
                return false;
            TweenPlayer player = entry.Identity.Player;
            string owner = player != null ? "TweenPlayer '" + player.name + "'" : "TweenPlayer";
            if (!string.IsNullOrEmpty(entry.Identity.ReadableId))
                owner += " (" + entry.Identity.ReadableId + ")";
            string asset = entry.Identity.Asset != null ? "asset " + entry.Identity.Asset.name : "inline sequence";
            string step = entry.IsSequence ? DescribeSequence(player) : DescribeStep(entry);
            int diagnostics = player != null && player.Diagnostics != null ? player.Diagnostics.Count : 0;
            source = new DOTweenTweenSource(owner, asset, step, diagnostics, player);
            return true;
        }

        private static string DescribeSequence(TweenPlayer player)
        {
            int steps = player != null && player.EffectiveSteps != null ? player.EffectiveSteps.Count : 0;
            return steps > 0 ? "sequence (" + steps.ToString(CultureInfo.InvariantCulture) + (steps == 1 ? " step)" : " steps)") : "sequence";
        }

        private static string DescribeStep(TweenSequenceRuntimeEntry entry)
        {
            string label = entry.StepIndex >= 0 ? "step " + entry.StepIndex.ToString(CultureInfo.InvariantCulture) : "step";
            string name = entry.Step != null && !string.IsNullOrEmpty(entry.Step.Name) ? entry.Step.Name : entry.Step != null ? entry.Step.GetType().Name : string.Empty;
            return string.IsNullOrEmpty(name) ? label : label + " '" + name + "'";
        }
    }
}
