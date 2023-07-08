using p3ppc.outfitSelector.Configuration;
using p3ppc.outfitSelector.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Memory.Sources;
using Reloaded.Mod.Interfaces;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static p3ppc.outfitSelector.Enums;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace p3ppc.outfitSelector
{
    /// <summary>
    /// Your mod logic goes here.
    /// </summary>
    public unsafe class Mod : ModBase // <= Do not Remove.
    {
        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        private readonly IModLoader _modLoader;

        /// <summary>
        /// Provides access to the Reloaded.Hooks API.
        /// </summary>
        /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
        private readonly IReloadedHooks? _hooks;

        /// <summary>
        /// Provides access to the Reloaded logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Entry point into the mod, instance that created this class.
        /// </summary>
        private readonly IMod _owner;

        /// <summary>
        /// Provides access to this mod's configuration.
        /// </summary>
        private Config _configuration;

        /// <summary>
        /// The configuration of the currently executing mod.
        /// </summary>
        private readonly IModConfig _modConfig;

        private IHook<GetCharacterModelGMOPathDelegate> _getCharacterModelGMOPathHook;

        private IMemory _memory;

        private SelectionMenu _selectionMenu;

        private ItemTblEntry** _itemEntries;

        public Mod(ModContext context)
        {
            _modLoader = context.ModLoader;
            _hooks = context.Hooks;
            _logger = context.Logger;
            _owner = context.Owner;
            _configuration = context.Configuration;
            _modConfig = context.ModConfig;

            if (!Utils.Initialise(_logger, _configuration, _modLoader))
                return;

            _memory = Memory.Instance;

            Utils.SigScan("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 0F B7 D9 49 8B F8 48 8D 0D ?? ?? ?? ?? 0F B7 F2", "GetCharacterModelGMOPath", address =>
            {
                _getCharacterModelGMOPathHook = _hooks.CreateHook<GetCharacterModelGMOPathDelegate>(GetCharacterModelGMOPath, address).Activate();
            });

            Utils.SigScan("48 89 05 ?? ?? ?? ?? 44 89 C2", "ItemTblEntries Ptr", address =>
            {
                _itemEntries = (ItemTblEntry**)Utils.GetGlobalAddress(address + 3);
                Utils.LogDebug($"Found ItemTblEntries at 0x{(nuint)_itemEntries:X}");
            });

            _selectionMenu = new SelectionMenu();
            _selectionMenu.Hook(_hooks, _memory);
        }

        private int GetCharacterModelGMOPath(short param_1, PartyMember member, nuint outStr, nuint param_4)
        {
            // Not character outfits (idk what)
            if (param_1 != 1)
                return _getCharacterModelGMOPathHook.OriginalFunction(param_1, member, outStr, param_4);

            int outfit = GetOutfit(member);
            string path;

            // 1 is used for both Female and Male here so we need to correct that
            if (member == PartyMember.MaleProtag && *_selectionMenu.IsFemc)
                member = PartyMember.FemaleProtag;
            
            if(outfit != -1)
                path = $"model/player/bc{(int)member:x3}_c{outfit:x}.GMO";
            else
                path = $"model/player/bc{(int)member:x3}.GMO";

            Utils.LogDebug($"Setting path for {member} to {path}");
            _memory.WriteRaw(outStr, Encoding.ASCII.GetBytes($"{path}\0"));
            return 1;
        }

        private int GetOutfit(PartyMember member)
        {
            var outfitItem = _selectionMenu.GetCharacterEquipment(member, EquipmentType.Outfit);
            return (*_itemEntries)[outfitItem].OutfitId;
        }
            
        private delegate int GetCharacterModelGMOPathDelegate(short param_1, PartyMember member, nuint outStr, nuint param_4);

        [StructLayout(LayoutKind.Explicit)]
        private struct ItemTblEntry
        {
            // This is actually Attack but we're using it for outfit id
            [FieldOffset(8)]
            internal short OutfitId;

            // Just making the struct 56 long
            [FieldOffset(54)]
            short unk;
        }

        #region Standard Overrides
        public override void ConfigurationUpdated(Config configuration)
        {
            // Apply settings from configuration.
            // ... your code here.
            _configuration = configuration;
            _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
        }
        #endregion

        #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Mod() { }
#pragma warning restore CS8618
        #endregion
    }
}