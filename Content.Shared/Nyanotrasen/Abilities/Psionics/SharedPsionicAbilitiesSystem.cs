using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.Psionics.Glimmer;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared.Abilities.Psionics
{
    public sealed class SharedPsionicAbilitiesSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;
        [Dependency] private readonly SharedPopupSystem _popups = default!;
        [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
        [Dependency] private readonly SharedGlimmerSystem _glimmerSystem = default!;
        [Dependency] private readonly IRobustRandom _robustRandom = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PsionicsDisabledComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<PsionicsDisabledComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<PsionicComponent, PsionicPowerUsedEvent>(OnPowerUsed);

            SubscribeLocalEvent<PsionicComponent, MobStateChangedEvent>(OnMobStateChanged);
        }

        private void OnPowerUsed(EntityUid uid, PsionicComponent component, PsionicPowerUsedEvent args)
        {
            foreach (var entity in _lookup.GetEntitiesInRange(uid, 10f))
            {
                if (HasComp<MetapsionicPowerComponent>(entity) && entity != uid && !(TryComp<PsionicInsulationComponent>(entity, out var insul) && !insul.Passthrough))
                {
                    _popups.PopupEntity(Loc.GetString("metapsionic-pulse-power", ("power", args.Power)), entity, entity, PopupType.LargeCaution);
                    args.Handled = true;
                    return;
                }
            }
        }

        private void OnInit(EntityUid uid, PsionicsDisabledComponent component, ComponentInit args)
        {
            TogglePsionics(uid, false);
        }

        private void OnShutdown(EntityUid uid, PsionicsDisabledComponent component, ComponentShutdown args)
        {
            if (!HasComp<PsionicInsulationComponent>(uid))
                TogglePsionics(uid, true);
        }

        private void OnMobStateChanged(EntityUid uid, PsionicComponent component, MobStateChangedEvent args)
        {
            if (args.NewMobState == MobState.Dead)
                TogglePsionics(uid, false);
            else
                TogglePsionics(uid, true);
        }

        public void TogglePsionics(EntityUid uid, bool toggle, PsionicComponent? component = null)
        {
            if (!Resolve(uid, ref component, false))
                return;

            if (component.PsionicAbility == null)
                return;

            _actions.SetEnabled(component.PsionicAbility, toggle);
        }
        public void LogPowerUsed(EntityUid uid, string power, int minGlimmer = 8, int maxGlimmer = 12)
        {
            _adminLogger.Add(Database.LogType.Psionics, Database.LogImpact.Medium, $"{ToPrettyString(uid):player} used {power}");
            var ev = new PsionicPowerUsedEvent(uid, power);
            RaiseLocalEvent(uid, ev, false);

            _glimmerSystem.Glimmer += _robustRandom.Next(minGlimmer, maxGlimmer);
        }
    }
    
    public sealed class PsionicPowerUsedEvent : HandledEntityEventArgs 
    {
        public EntityUid User { get; }
        public string Power = string.Empty;

        public PsionicPowerUsedEvent(EntityUid user, string power)
        {
            User = user;
            Power = power;
        }
    }

    [Serializable]
    [NetSerializable]
    public sealed class PsionicsChangedEvent : EntityEventArgs
    {
        public readonly EntityUid Euid;
        public PsionicsChangedEvent(EntityUid euid)
        {
            Euid = euid;
        }
    }
}
