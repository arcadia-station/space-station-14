using Content.Shared.Inventory.Events;
using Content.Shared.Clothing.Components;
using Content.Shared.StatusEffect;

namespace Content.Shared.Abilities.Psionics
{
    public sealed class PsionicItemsSystem : EntitySystem
    {
        [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
        [Dependency] private readonly IComponentFactory _componentFactory = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psiAbilities = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<TinfoilHatComponent, GotEquippedEvent>(OnEquipped);
            SubscribeLocalEvent<TinfoilHatComponent, GotUnequippedEvent>(OnUnequipped);
            SubscribeLocalEvent<ClothingGrantPsionicPowerComponent, GotEquippedEvent>(OnGranterEquipped);
            SubscribeLocalEvent<ClothingGrantPsionicPowerComponent, GotUnequippedEvent>(OnGranterUnequipped);
        }
        private void OnEquipped(EntityUid uid, TinfoilHatComponent component, GotEquippedEvent args)
        {
            // This only works on clothing
            if (!TryComp<SharedClothingComponent>(uid, out var clothing))
                return;
            // Is the clothing in its actual slot?
            if (!clothing.Slots.HasFlag(args.SlotFlags))
                return;
            
            _psiAbilities.TogglePsionics(args.Equipee, false);
            var insul = EnsureComp<PsionicInsulationComponent>(args.Equipee);
            insul.Passthrough = component.Passthrough;
            component.IsActive = true;
        }

        private void OnUnequipped(EntityUid uid, TinfoilHatComponent component, GotUnequippedEvent args)
        {
            if (!component.IsActive)
                return;

            if (!_statusEffects.HasStatusEffect(uid, "PsionicallyInsulated"))
                RemComp<PsionicInsulationComponent>(args.Equipee);

            component.IsActive = false;

            if (!HasComp<PsionicsDisabledComponent>(args.Equipee))
                _psiAbilities.TogglePsionics(args.Equipee, true);
        }

        private void OnGranterEquipped(EntityUid uid, ClothingGrantPsionicPowerComponent component, GotEquippedEvent args)
        {
            // This only works on clothing
            if (!TryComp<SharedClothingComponent>(uid, out var clothing))
                return;
            // Is the clothing in its actual slot?
            if (!clothing.Slots.HasFlag(args.SlotFlags))
                return;
            // does the user already has this power?
            var componentType = _componentFactory.GetRegistration(component.Power).Type;
            if (EntityManager.HasComponent(args.Equipee, componentType)) return;


            var newComponent = (Component) _componentFactory.GetComponent(componentType);
            newComponent.Owner = args.Equipee;

            EntityManager.AddComponent(args.Equipee, newComponent);

            component.IsActive = true;
        }

        private void OnGranterUnequipped(EntityUid uid, ClothingGrantPsionicPowerComponent component, GotUnequippedEvent args)
        {
            if (!component.IsActive)
                return;

            component.IsActive = false;
            var componentType = _componentFactory.GetRegistration(component.Power).Type;
            if (EntityManager.HasComponent(args.Equipee, componentType))
            {
                EntityManager.RemoveComponent(args.Equipee, componentType);
            }
        }
    }
}