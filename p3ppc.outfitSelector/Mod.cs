using p3ppc.outfitSelector.Configuration;
using p3ppc.outfitSelector.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Memory.Sources;
using Reloaded.Mod.Interfaces;
using System.Diagnostics;
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

        public Mod(ModContext context)
        {
            Debugger.Launch();
            _modLoader = context.ModLoader;
            _hooks = context.Hooks;
            _logger = context.Logger;
            _owner = context.Owner;
            _configuration = context.Configuration;
            _modConfig = context.ModConfig;

            Utils.Initialise(_logger, _configuration);

            _memory = Memory.Instance;

            var startupScannerController = _modLoader.GetController<IStartupScanner>();
            if (startupScannerController == null || !startupScannerController.TryGetTarget(out var startupScanner))
            {
                Utils.LogError($"Unable to get controller for Reloaded SigScan Library, stuff won't work :(");
                return;
            }

            startupScanner.AddMainModuleScan("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 0F B7 D9 49 8B F8 48 8D 0D ?? ?? ?? ?? 0F B7 F2", result =>
            {
                if (!result.Found)
                {
                    Utils.LogError($"Unable to find GetCharacterModelGMOPath, aborting initialisation.");
                    return;
                }
                Utils.LogDebug($"Found GetCharacterModelGMOPath at 0x{result.Offset + Utils.BaseAddress:X}");

                _getCharacterModelGMOPathHook = _hooks.CreateHook<GetCharacterModelGMOPathDelegate>(GetCharacterModelGMOPath, result.Offset + Utils.BaseAddress).Activate();
            });
        }

        private int GetCharacterModelGMOPath(short param_1, PartyMember member, nuint outStr, nuint param_4)
        {
            // Not character outfits (idk what)
            if (param_1 != 1)
                return _getCharacterModelGMOPathHook.OriginalFunction(param_1, member, outStr, param_4);

            var property = typeof(Config).GetProperty($"{member}Outfit");
            if (property == null)
            {
                Utils.LogError($"Unable to get outfit for {member}");
                return _getCharacterModelGMOPathHook.OriginalFunction(param_1, member, outStr, param_4);
            }
            int outfit = (int)property.GetValue(_configuration);
            string path;
            
            if(outfit != -1)
                path = $"model/player/bc{(int)member:x3}_c{outfit:x}.GMO";
            else
                path = $"model/player/bc{(int)member:x3}.GMO";

            Utils.LogDebug($"Setting path for {member} to {path}");
            _memory.WriteRaw((nuint)outStr, Encoding.ASCII.GetBytes($"{path}\0"));
            return 1;
        }
            
        private delegate int GetCharacterModelGMOPathDelegate(short param_1, PartyMember member, nuint outStr, nuint param_4);

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