using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Valkyrie.DOTween.Editor
{
    public static class ValkyrieDOTweenAddonPlayground
    {
        private const string PlaygroundFolder = "Assets/__ValkyrieDOTweenAddonPlayground";
        private const string PlaygroundRootName = "__Valkyrie DOTween Addon Playground";
        private const string AllFeaturesAssetPath = PlaygroundFolder + "/AllFeaturesTweenSequence.asset";
        private const string InvalidAssetPath = PlaygroundFolder + "/InvalidTweenSequence.asset";
        private const string MissingBindingAssetPath = PlaygroundFolder + "/MissingBindingTweenSequence.asset";

        [MenuItem("Tools/Valkyrie/DOTween/Create Addon Playground")]
        public static void CreatePlayground()
        {
            bool shouldCreate = EditorUtility.DisplayDialog(
                "Create Valkyrie DOTween playground?",
                "This will recreate temporary demo assets under " + PlaygroundFolder + " and a demo root in the current scene.",
                "Create",
                "Cancel");
            if (!shouldCreate)
            {
                return;
            }

            DeleteGeneratedContent(false);
            EnsureFolder();

            TweenSequenceAsset allFeaturesAsset = CreateAllFeaturesAsset();
            TweenSequenceAsset invalidAsset = CreateInvalidAsset();
            TweenSequenceAsset missingBindingAsset = CreateMissingBindingAsset();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            GameObject root = new GameObject(PlaygroundRootName);
            GameObject validPlayerObject = CreateInlinePlayer(root.transform);
            CreateMissingBindingPlayer(root.transform, missingBindingAsset);
            CreateDuplicateBindingPlayer(root.transform, allFeaturesAsset);
            CreateInspectorNotes(root.transform);

            EditorSceneManager.MarkSceneDirty(root.scene);
            Selection.objects = new Object[] { validPlayerObject, allFeaturesAsset, invalidAsset, missingBindingAsset };
            EditorGUIUtility.PingObject(allFeaturesAsset);

            Debug.Log(BuildCreatedMessage());
        }

        [MenuItem("Tools/Valkyrie/DOTween/Delete Addon Playground")]
        public static void DeletePlayground()
        {
            DeleteGeneratedContent(true);
        }

        private static TweenSequenceAsset CreateAllFeaturesAsset()
        {
            TweenSequenceAsset asset = ScriptableObject.CreateInstance<TweenSequenceAsset>();
            asset.name = "AllFeaturesTweenSequence";
            asset.Ease = Ease.Linear;
            asset.Loops = 1;
            asset.UpdateType = UpdateType.Normal;
            asset.TimeScale = 1f;
            asset.AutoKill = false;
            asset.Recyclable = false;

            AddAllFeatureSteps(asset.Steps);

            AssetDatabase.CreateAsset(asset, AllFeaturesAssetPath);
            return asset;
        }

        private static TweenSequenceAsset CreateInvalidAsset()
        {
            TweenSequenceAsset asset = ScriptableObject.CreateInstance<TweenSequenceAsset>();
            asset.name = "InvalidTweenSequence";
            asset.TimeScale = 0f;

            IntervalStepDefinition invalidInterval = new IntervalStepDefinition
            {
                Duration = 0f
            };
            CanvasGroupFadeStepDefinition invalidFade = new CanvasGroupFadeStepDefinition
            {
                TargetKey = "Panel",
                EndAlpha = 1.5f,
                Duration = 0.3f
            };

            asset.Steps.Add(invalidInterval);
            asset.Steps.Add(invalidFade);

            AssetDatabase.CreateAsset(asset, InvalidAssetPath);
            return asset;
        }

        private static TweenSequenceAsset CreateMissingBindingAsset()
        {
            TweenSequenceAsset asset = ScriptableObject.CreateInstance<TweenSequenceAsset>();
            asset.name = "MissingBindingTweenSequence";

            asset.Steps.Add(new TransformMoveStepDefinition
            {
                TargetKey = "MissingMoveTarget",
                EndValue = new Vector3(1f, 0f, 0f),
                Local = true,
                Duration = 0.5f,
                Ease = Ease.Linear
            });

            AssetDatabase.CreateAsset(asset, MissingBindingAssetPath);
            return asset;
        }

        private static GameObject CreateInlinePlayer(Transform root)
        {
            GameObject playerObject = CreateChild(root, "Player - inline all features", new Vector3(0f, 0f, 0f));
            GameObject selfTarget = CreateChild(playerObject.transform, "Self target root", new Vector3(0f, 0f, 0f));
            GameObject moveTarget = CreateChild(playerObject.transform, "Move target", new Vector3(-3f, 0f, 0f));
            GameObject rotationTarget = CreateChild(playerObject.transform, "Rotation target", new Vector3(0f, 0f, 0f));
            GameObject scaleTarget = CreateChild(playerObject.transform, "Scale target", new Vector3(3f, 0f, 0f));
            GameObject panelTarget = CreateChild(playerObject.transform, "Fade panel target", new Vector3(0f, -1.5f, 0f));
            GameObject overrideTarget = CreateChild(playerObject.transform, "Optional target override", new Vector3(0f, 1.5f, 0f));
            CanvasGroup canvasGroup = panelTarget.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;

            TweenPlayer player = playerObject.AddComponent<TweenPlayer>();
            player.SourceMode = TweenPlayerSourceMode.Sequence;
            player.Timeline.Ease = Ease.Linear;
            player.Timeline.Loops = 1;
            player.Timeline.UpdateType = UpdateType.Normal;
            player.Timeline.TimeScale = 1f;
            AddAllFeatureSteps(player.Timeline.Steps);
            player.TargetRoot = selfTarget.transform;
            player.IdOverride = "Playground/InlineAllFeatures";
            player.TargetOverride = overrideTarget;
            player.DisableCleanup = TweenCleanupMode.Kill;
            player.DestroyCleanup = TweenCleanupMode.Kill;
            player.Bindings.Add(new TweenTargetBinding("Move", moveTarget));
            player.Bindings.Add(new TweenTargetBinding("Rotation", rotationTarget.transform));
            player.Bindings.Add(new TweenTargetBinding("Scale", scaleTarget));
            player.Bindings.Add(new TweenTargetBinding("Panel", panelTarget));

            return playerObject;
        }

        private static void AddAllFeatureSteps(IList<TweenStepDefinition> steps)
        {
            TransformMoveStepDefinition selfLift = new TransformMoveStepDefinition
            {
                TargetKey = TweenTargetBinding.SelfKey,
                EndValue = new Vector3(0f, 1.5f, 0f),
                Local = true,
                Duration = 0.6f,
                Ease = Ease.OutQuad
            };
            selfLift.Placement.Mode = TweenPlacementMode.Insert;
            selfLift.Placement.InsertAt = 0.1f;

            TransformMoveStepDefinition move = new TransformMoveStepDefinition
            {
                TargetKey = "Move",
                EndValue = new Vector3(2f, 0f, 0f),
                Local = true,
                Duration = 0.8f,
                Ease = Ease.OutBack
            };

            TransformRotationStepDefinition rotation = new TransformRotationStepDefinition
            {
                TargetKey = "Rotation",
                EndValue = new Vector3(0f, 0f, 180f),
                Local = true,
                Duration = 0.8f,
                Ease = Ease.InOutSine
            };
            rotation.Placement.Mode = TweenPlacementMode.Join;

            TransformScaleStepDefinition scale = new TransformScaleStepDefinition
            {
                TargetKey = "Scale",
                EndValue = new Vector3(1.8f, 1.8f, 1.8f),
                Duration = 0.7f,
                Ease = Ease.OutElastic
            };
            scale.Placement.Mode = TweenPlacementMode.Insert;
            scale.Placement.InsertAt = 0.25f;

            CanvasGroupFadeStepDefinition fade = new CanvasGroupFadeStepDefinition
            {
                TargetKey = "Panel",
                EndAlpha = 0.25f,
                Duration = 0.5f,
                Delay = 0.1f,
                Ease = Ease.Linear
            };

            IntervalStepDefinition interval = new IntervalStepDefinition
            {
                Duration = 0.35f
            };

            steps.Add(move);
            steps.Add(rotation);
            steps.Add(scale);
            steps.Add(fade);
            steps.Add(interval);
            steps.Add(selfLift);
        }

        private static void CreateMissingBindingPlayer(Transform root, TweenSequenceAsset asset)
        {
            GameObject playerObject = CreateChild(root, "Player - missing binding diagnostics", new Vector3(-4f, 2f, 0f));
            TweenPlayer player = playerObject.AddComponent<TweenPlayer>();
            player.SourceMode = TweenPlayerSourceMode.Asset;
            player.Asset = asset;
            player.IdOverride = "Playground/MissingBinding";
        }

        private static void CreateDuplicateBindingPlayer(Transform root, TweenSequenceAsset asset)
        {
            GameObject playerObject = CreateChild(root, "Player - duplicate binding diagnostics", new Vector3(4f, 2f, 0f));
            GameObject firstTarget = CreateChild(playerObject.transform, "First Move binding", new Vector3(0f, 0f, 0f));
            GameObject secondTarget = CreateChild(playerObject.transform, "Second Move binding", new Vector3(1f, 0f, 0f));
            GameObject rotationTarget = CreateChild(playerObject.transform, "Rotation binding", new Vector3(0f, 1f, 0f));
            GameObject scaleTarget = CreateChild(playerObject.transform, "Scale binding", new Vector3(0f, 2f, 0f));
            GameObject panelTarget = CreateChild(playerObject.transform, "Panel binding", new Vector3(0f, 3f, 0f));
            panelTarget.AddComponent<CanvasGroup>();

            TweenPlayer player = playerObject.AddComponent<TweenPlayer>();
            player.SourceMode = TweenPlayerSourceMode.Asset;
            player.Asset = asset;
            player.IdOverride = "Playground/DuplicateBinding";
            player.DestroyCleanup = TweenCleanupMode.None;
            player.Bindings.Add(new TweenTargetBinding("Move", firstTarget));
            player.Bindings.Add(new TweenTargetBinding("Move", secondTarget));
            player.Bindings.Add(new TweenTargetBinding("Rotation", rotationTarget));
            player.Bindings.Add(new TweenTargetBinding("Scale", scaleTarget));
            player.Bindings.Add(new TweenTargetBinding("Panel", panelTarget));
        }

        private static void CreateInspectorNotes(Transform root)
        {
            GameObject notes = CreateChild(root, "How to use", new Vector3(0f, 3.5f, 0f));
            TextMesh textMesh = notes.AddComponent<TextMesh>();
            textMesh.text =
                "Temporary Valkyrie DOTween addon playground\n\n" +
                "1. Select AllFeaturesTweenSequence to inspect the managed-reference step list and timeline summary.\n" +
                "2. Select Player - inline all features to inspect inline steps, bindings, target root, id override, target override and cleanup.\n" +
                "3. Enable Play On Enable on the valid player, then enter Play Mode to see it run.\n" +
                "4. Select the invalid/missing/duplicate examples to inspect validation diagnostics.\n" +
                "5. Use Tools > Valkyrie > DOTween > Delete Addon Playground when finished.";
            textMesh.characterSize = 0.18f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
        }

        private static GameObject CreateChild(Transform parent, string name, Vector3 localPosition)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPosition;
            return child;
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(PlaygroundFolder))
            {
                AssetDatabase.CreateFolder("Assets", "__ValkyrieDOTweenAddonPlayground");
            }
        }

        private static void DeleteGeneratedContent(bool logResult)
        {
            GameObject existingRoot = GameObject.Find(PlaygroundRootName);
            if (existingRoot != null)
            {
                Object.DestroyImmediate(existingRoot);
            }

            AssetDatabase.DeleteAsset(PlaygroundFolder);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (logResult)
            {
                Debug.Log("[Valkyrie DOTween Playground] Deleted generated scene objects and assets.");
            }
        }

        private static string BuildCreatedMessage()
        {
            List<string> lines = new List<string>
            {
                "[Valkyrie DOTween Playground] Created.",
                "Generated assets:",
                "- " + AllFeaturesAssetPath + " shows move, rotation, scale, CanvasGroup fade, interval, Self key, delay, ease, Join and Insert placement.",
                "- " + InvalidAssetPath + " shows invalid asset diagnostics in the TweenSequenceAsset inspector.",
                "- " + MissingBindingAssetPath + " shows player-side missing binding diagnostics.",
                "Generated scene objects:",
                "- Player - inline all features: inspect inline steps, bindings, target root, id override, target override and cleanup settings.",
                "- Player - missing binding diagnostics: inspect missing target errors.",
                "- Player - duplicate binding diagnostics: inspect duplicate binding and destroy-cleanup warning."
            };
            return string.Join("\n", lines);
        }
    }

}
