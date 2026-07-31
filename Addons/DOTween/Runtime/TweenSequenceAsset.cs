using DG.Tweening;
using UnityEngine;

namespace Valkyrie.DOTween
{
    [CreateAssetMenu(fileName = "TweenSequence", menuName = "Valkyrie/DOTween/Tween Sequence")]
    public sealed class TweenSequenceAsset : ScriptableObject
    {
        [SerializeField] private TweenTimeline _timeline = new TweenTimeline();

        public TweenTimeline Timeline
        {
            get
            {
                if (_timeline == null)
                {
                    _timeline = new TweenTimeline();
                }

                return _timeline;
            }
        }

        public System.Collections.Generic.IList<TweenStepDefinition> Steps
        {
            get { return Timeline.Steps; }
        }

        public Ease Ease
        {
            get { return Timeline.Ease; }
            set { Timeline.Ease = value; }
        }

        public int Loops
        {
            get { return Timeline.Loops; }
            set { Timeline.Loops = value; }
        }

        public LoopType LoopType
        {
            get { return Timeline.LoopType; }
            set { Timeline.LoopType = value; }
        }

        public UpdateType UpdateType
        {
            get { return Timeline.UpdateType; }
            set { Timeline.UpdateType = value; }
        }

        public bool IndependentUpdate
        {
            get { return Timeline.IndependentUpdate; }
            set { Timeline.IndependentUpdate = value; }
        }

        public float TimeScale
        {
            get { return Timeline.TimeScale; }
            set { Timeline.TimeScale = value; }
        }

        public bool AutoKill
        {
            get { return Timeline.AutoKill; }
            set { Timeline.AutoKill = value; }
        }

        public bool Recyclable
        {
            get { return Timeline.Recyclable; }
            set { Timeline.Recyclable = value; }
        }

        public bool ValidateDefinitions(TweenBuildContext context)
        {
            return Timeline.ValidateDefinitions(context);
        }

        public bool TryBuildSequence(TweenBuildContext context, out Sequence sequence)
        {
            return Timeline.TryBuildSequence(context, out sequence);
        }

        internal TweenSequenceBuildSettings CreateSettings()
        {
            return Timeline.CreateSettings();
        }
    }
}
