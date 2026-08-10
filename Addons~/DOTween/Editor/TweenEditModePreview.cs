using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace Valkyrie.DOTween.Editor
{
    public static class TweenEditModePreview
    {
        private static readonly TweenStateSnapshot Snapshot = new TweenStateSnapshot();
        private static TweenPlayer _player;
        private static Sequence _sequence;
        private static double _lastEditorTime;
        private static float _time;
        private static bool _playing;

        public static bool IsPreviewing
        {
            get { return _sequence != null && _sequence.IsActive(); }
        }

        public static float Time
        {
            get { return _time; }
        }

        public static float Duration
        {
            get { return _sequence != null && _sequence.IsActive() ? _sequence.Duration(false) : 0f; }
        }

        public static void Play(TweenPlayer player)
        {
            if (Application.isPlaying)
            {
                player.Play();
                return;
            }

            EnsureSequence(player);
            _playing = true;
            _lastEditorTime = EditorApplication.timeSinceStartup;
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }

        public static void Pause()
        {
            _playing = false;
        }

        public static void Stop()
        {
            EditorApplication.update -= Tick;
            _playing = false;
            _time = 0f;
            if (_sequence != null && _sequence.IsActive())
            {
                _sequence.Kill(false);
            }

            _sequence = null;
            Snapshot.Restore();
            SceneView.RepaintAll();
        }

        public static void Scrub(TweenPlayer player, float time)
        {
            if (Application.isPlaying)
            {
                return;
            }

            EnsureSequence(player);
            _playing = false;
            _time = Mathf.Clamp(time, 0f, Duration);
            _sequence.Goto(_time, false);
            SceneView.RepaintAll();
        }

        private static void EnsureSequence(TweenPlayer player)
        {
            if (_player == player && _sequence != null && _sequence.IsActive())
            {
                return;
            }

            Stop();
            _player = player;
            if (_player == null)
            {
                return;
            }

            TweenBuildContext context = new TweenBuildContext(_player.TargetRoot, _player.Bindings);
            CaptureSnapshot(_player, context);
            Sequence sequence;
            if (!_player.TryBuildConfiguredSequence(context, out sequence) || sequence == null)
            {
                return;
            }

            _sequence = sequence;
            _sequence.SetAutoKill(false);
            _sequence.Pause();
            _time = 0f;
            _sequence.Goto(0f, false);
        }

        private static void CaptureSnapshot(TweenPlayer player, TweenBuildContext context)
        {
            List<UnityEngine.Object> targets = new List<UnityEngine.Object>();
            TweenTimeline timeline = player.EffectiveTimeline;
            IList<TweenStepDefinition> steps = timeline != null ? timeline.Steps : null;
            if (steps != null)
            {
                for (int index = 0; index < steps.Count; index++)
                {
                    TweenStepDefinition step = steps[index];
                    if (step == null) continue;
                    context.SetCurrentStep(index, step);
                    step.CollectSnapshotTargets(context, targets);
                }
            }

            context.SetCurrentStep(-1, null);
            Undo.RegisterCompleteObjectUndo(targets.ToArray(), "DOTween Preview");
            Snapshot.Capture(targets);
        }

        private static void Tick()
        {
            if (!_playing || _sequence == null || !_sequence.IsActive())
            {
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            float deltaTime = (float)(now - _lastEditorTime);
            _lastEditorTime = now;
            _time += Mathf.Max(0f, deltaTime);
            if (_time >= Duration)
            {
                _time = Duration;
                _playing = false;
            }

            _sequence.Goto(_time, false);
            SceneView.RepaintAll();
        }
    }
}
