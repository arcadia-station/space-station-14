using Robust.Client.GameObjects;
using static Robust.Client.GameObjects.SpriteComponent;
using Content.Client.Kitchen.Components;
using Content.Shared.Clothing;
using Content.Shared.Hands;
using Content.Shared.Kitchen.Components;

namespace Content.Client.Kitchen.Visualizers
{
    public sealed class DeepFriedVisualizerSystem : VisualizerSystem<DeepFriedComponent>
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<DeepFriedComponent, HeldVisualsUpdatedEvent>(OnHeldVisualsUpdated);
            SubscribeLocalEvent<DeepFriedComponent, EquipmentVisualsUpdatedEvent>(OnEquipmentVisualsUpdated);
        }

        protected override void OnAppearanceChange(EntityUid uid, DeepFriedComponent component, ref AppearanceChangeEvent args)
        {
            if (args.Sprite == null)
                return;

            if (!args.Component.TryGetData(DeepFriedVisuals.Fried, out bool isFried))
                return;

            args.Sprite.LayerSetShader(0, "Crispy");
        }

        private void OnHeldVisualsUpdated(EntityUid uid, DeepFriedComponent component, HeldVisualsUpdatedEvent args)
        {
            if (args.RevealedLayers.Count == 0)
            {
                return;
            }

            if (!TryComp(args.User, out SpriteComponent? sprite))
                return;

            foreach (var key in args.RevealedLayers)
            {
                if (!sprite.LayerMapTryGet(key, out var index) || sprite[index] is not Layer layer)
                    continue;

                sprite.LayerSetShader(index, "Crispy");

            }
        }

        private void OnEquipmentVisualsUpdated(EntityUid uid, DeepFriedComponent component, EquipmentVisualsUpdatedEvent args)
        {
            if (args.RevealedLayers.Count == 0)
            {
                return;
            }

            if (!TryComp(args.Equipee, out SpriteComponent? sprite))
                return;

            foreach (var key in args.RevealedLayers)
            {
                if (!sprite.LayerMapTryGet(key, out var index) || sprite[index] is not Layer layer)
                    continue;

                sprite.LayerSetShader(index, "Crispy");
            }
        }
    }
}
