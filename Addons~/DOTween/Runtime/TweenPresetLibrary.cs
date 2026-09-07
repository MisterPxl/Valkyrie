using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astra.Valkyrie.Integrations.DOTween
{
    [CreateAssetMenu(fileName = "TweenPresetLibrary", menuName = "Astra/Valkyrie/DOTween/Tween Preset Library")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.Runtime")]
    public sealed class TweenPresetLibrary : ScriptableObject
    {
        [SerializeField] private List<TweenPreset> _presets = new List<TweenPreset>();

        public IList<TweenPreset> Presets
        {
            get
            {
                if (_presets == null)
                {
                    _presets = new List<TweenPreset>();
                }

                return _presets;
            }
        }
    }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.DOTween", "Valkyrie.DOTween.Runtime")]
    public sealed class TweenPreset
    {
        [SerializeField] private string _name = "Preset";
        [SerializeField] private string _category = "General";
        [SerializeField] private TweenTimeline _timeline = new TweenTimeline();

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Category
        {
            get { return _category; }
            set { _category = value; }
        }

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
    }
}
