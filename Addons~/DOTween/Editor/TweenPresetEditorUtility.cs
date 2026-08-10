using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Valkyrie.DOTween.Editor
{
    public sealed class TweenPresetOption
    {
        public string DisplayName { get; }
        public TweenTimeline Timeline { get; }

        public TweenPresetOption(string displayName, TweenTimeline timeline)
        {
            DisplayName = displayName;
            Timeline = timeline;
        }
    }

    public static class TweenPresetEditorUtility
    {
        public static List<TweenPresetOption> CollectPresets()
        {
            List<TweenPresetOption> options = new List<TweenPresetOption>();
            AddBuiltInPresets(options);

            string[] guids = AssetDatabase.FindAssets("t:TweenPresetLibrary");
            for (int index = 0; index < guids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[index]);
                TweenPresetLibrary library = AssetDatabase.LoadAssetAtPath<TweenPresetLibrary>(path);
                if (library == null) continue;

                IList<TweenPreset> presets = library.Presets;
                for (int presetIndex = 0; presetIndex < presets.Count; presetIndex++)
                {
                    TweenPreset preset = presets[presetIndex];
                    if (preset == null) continue;
                    string category = string.IsNullOrWhiteSpace(preset.Category) ? "Project" : preset.Category.Trim();
                    string name = string.IsNullOrWhiteSpace(preset.Name) ? "Preset" : preset.Name.Trim();
                    options.Add(new TweenPresetOption(category + "/" + name, preset.Timeline));
                }
            }

            return options;
        }

        public static void ApplyPreset(TweenPlayer player, TweenPresetOption option)
        {
            if (player == null || option == null || option.Timeline == null)
            {
                return;
            }

            Undo.RecordObject(player, "Apply Tween Preset");
            player.SetTimeline(TweenTimelineCloneUtility.Clone(option.Timeline));
            EditorUtility.SetDirty(player);
        }

        private static void AddBuiltInPresets(List<TweenPresetOption> options)
        {
            options.Add(new TweenPresetOption("Built-in/Fade In", CreateFadePreset(1f)));
            options.Add(new TweenPresetOption("Built-in/Fade Out", CreateFadePreset(0f)));
            options.Add(new TweenPresetOption("Built-in/Pop In", CreateScaleFromPreset(Vector3.zero, Vector3.one)));
            options.Add(new TweenPresetOption("Built-in/Pulse", CreateScaleByPreset(Vector3.one * 0.15f)));
            options.Add(new TweenPresetOption("Built-in/Slide In Left", CreateMoveByPreset(new Vector3(-100f, 0f, 0f), TweenValueMode.From)));
            options.Add(new TweenPresetOption("Built-in/Shake", CreateShakePreset()));
        }

        private static TweenTimeline CreateFadePreset(float alpha)
        {
            TweenTimeline timeline = new TweenTimeline();
            CanvasGroupFadeStepDefinition step = new CanvasGroupFadeStepDefinition { EndAlpha = alpha };
            step.Duration = 0.25f;
            timeline.Steps.Add(step);
            return timeline;
        }

        private static TweenTimeline CreateScaleFromPreset(Vector3 from, Vector3 to)
        {
            TweenTimeline timeline = new TweenTimeline();
            TransformScaleStepDefinition step = new TransformScaleStepDefinition { EndValue = from };
            step.ValueMode = TweenValueMode.From;
            step.Duration = 0.25f;
            timeline.Steps.Add(step);
            return timeline;
        }

        private static TweenTimeline CreateScaleByPreset(Vector3 by)
        {
            TweenTimeline timeline = new TweenTimeline();
            TransformScaleStepDefinition up = new TransformScaleStepDefinition { EndValue = by };
            up.ValueMode = TweenValueMode.By;
            up.Duration = 0.15f;
            TransformScaleStepDefinition down = new TransformScaleStepDefinition { EndValue = -by };
            down.ValueMode = TweenValueMode.By;
            down.Duration = 0.15f;
            timeline.Steps.Add(up);
            timeline.Steps.Add(down);
            return timeline;
        }

        private static TweenTimeline CreateMoveByPreset(Vector3 value, TweenValueMode mode)
        {
            TweenTimeline timeline = new TweenTimeline();
            TransformMoveStepDefinition step = new TransformMoveStepDefinition { EndValue = value };
            step.ValueMode = mode;
            step.Local = true;
            step.Duration = 0.3f;
            timeline.Steps.Add(step);
            return timeline;
        }

        private static TweenTimeline CreateShakePreset()
        {
            TweenTimeline timeline = new TweenTimeline();
            timeline.Steps.Add(new TransformShakePositionStepDefinition());
            return timeline;
        }
    }
}
