using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.DOTween.Samples
{
    public sealed class ReusableTweenSequenceSample : MonoBehaviour
    {
        private const string AnimatedBindingKey = "Animated";

        [SerializeField] private List<Transform> _playerRoots = new List<Transform>();

        private readonly List<TweenSequencePlayer> _players = new List<TweenSequencePlayer>();
        private TweenSequenceAsset _sharedSequence;

        private void Start()
        {
            _sharedSequence = BuildSharedSequence();

            for (int index = 0; index < _playerRoots.Count; index++)
            {
                Transform playerRoot = _playerRoots[index];
                if (playerRoot == null)
                {
                    continue;
                }

                EnsureVisibleCube(playerRoot);

                TweenSequencePlayer player = playerRoot.GetComponent<TweenSequencePlayer>();
                if (player == null)
                {
                    player = playerRoot.gameObject.AddComponent<TweenSequencePlayer>();
                }

                player.Asset = _sharedSequence;
                player.IdOverride = "ReusableSample/" + playerRoot.name;
                player.Bindings.Clear();
                player.Bindings.Add(new TweenTargetBinding(AnimatedBindingKey, playerRoot));
                _players.Add(player);
            }

            Replay();
        }

        [ContextMenu("Replay")]
        public void Replay()
        {
            for (int index = 0; index < _players.Count; index++)
            {
                TweenSequencePlayer player = _players[index];
                if (player != null)
                {
                    player.Play();
                }
            }
        }

        private void OnDestroy()
        {
            if (_sharedSequence != null)
            {
                Destroy(_sharedSequence);
                _sharedSequence = null;
            }
        }

        private static TweenSequenceAsset BuildSharedSequence()
        {
            TweenSequenceAsset asset = ScriptableObject.CreateInstance<TweenSequenceAsset>();
            asset.name = "Runtime Reusable Sequence";

            TransformMoveStepDefinition moveUp = new TransformMoveStepDefinition
            {
                TargetKey = AnimatedBindingKey,
                EndValue = new Vector3(0f, 1.5f, 0f),
                Local = true,
                Relative = true,
                Duration = 0.6f
            };
            TransformScaleStepDefinition scaleUp = new TransformScaleStepDefinition
            {
                TargetKey = AnimatedBindingKey,
                EndValue = new Vector3(1.4f, 1.4f, 1.4f),
                Duration = 0.6f
            };
            scaleUp.Placement.Mode = TweenPlacementMode.Join;

            IntervalStepDefinition hold = new IntervalStepDefinition
            {
                Duration = 0.2f
            };

            TransformMoveStepDefinition moveDown = new TransformMoveStepDefinition
            {
                TargetKey = AnimatedBindingKey,
                EndValue = new Vector3(0f, -1.5f, 0f),
                Local = true,
                Relative = true,
                Duration = 0.6f
            };
            TransformScaleStepDefinition scaleDown = new TransformScaleStepDefinition
            {
                TargetKey = AnimatedBindingKey,
                EndValue = Vector3.one,
                Duration = 0.6f
            };
            scaleDown.Placement.Mode = TweenPlacementMode.Join;

            asset.Steps.Add(moveUp);
            asset.Steps.Add(scaleUp);
            asset.Steps.Add(hold);
            asset.Steps.Add(moveDown);
            asset.Steps.Add(scaleDown);
            return asset;
        }

        private static void EnsureVisibleCube(Transform parent)
        {
            if (parent.childCount > 0)
            {
                return;
            }

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Animated Cube";
            cube.transform.SetParent(parent, false);
        }
    }
}
