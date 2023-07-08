﻿using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Memory.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static p3ppc.outfitSelector.Enums;

namespace p3ppc.outfitSelector;
internal unsafe class SelectionMenu
{
    private List<IAsmHook> _hooks = new();
    private IHook<GetCharacterEquipmentDelegate> _getEquipmentHook;
    private IHook<SetCharacterEquipmentDelegate> _setEquipmentHook;
    private IHook<GetEquipmentTypeDelegate> _getEquipmentTypeHook;
    private SetItemNumDelegate _setItemNum;
    private PartyMemberInfo* _partyInfo;
    internal bool* IsFemc;

    internal void Hook(IReloadedHooks hooks, IMemory memory)
    {
        Utils.SigScan("48 8B 8C ?? ?? ?? ?? ?? 44 89 84 24 ?? ?? ?? ??", "RenderEquipTypeName", address =>
        {
            string[] function =
            {
                "use64",
                "cmp rcx, 4",
                "jne endHook",
                "mov rcx, 1", // TODO Actually change the texture to say Outfit instead of just using Body 
                "label endHook"
            };
            _hooks.Add(hooks.CreateAsmHook(function, address, AsmHookBehaviour.ExecuteFirst).Activate());
        });

        Utils.SigScan("66 83 FB 04 7C ?? 44 39 25 ?? ?? ?? ??", "DetailsDisplay5", address =>
        {
            memory.SafeWrite(address + 3, (byte)5); // Change it to loop 5 times and hence display the outfit in equip details
        });

        // Stop a stat (like At or Df) from displaying next to the outfit (like accessories)
        Utils.SigScan("0F 84 ?? ?? ?? ?? F3 44 0F 10 35 ?? ?? ?? ??", "OutfitNoStat", address =>
        {
            memory.SafeWrite(address + 1, (byte)0x8D); // Change je to jge so outfits are also included
        });

        // Let you scroll down to outfits in the equip details menu (not mouse)
        // TODO make it selectable with mouse as well (it'll be a different function somewhere)
        Utils.SigScan("E8 ?? ?? ?? ?? B8 00 08 00 00 41 8D 56 ??", "DetailsScroll5", address =>
        {
            string[] function =
            {
                "use64",
                "mov r8d, 5",
                "mov edx, 5",
            };
            _hooks.Add(hooks.CreateAsmHook(function, address, AsmHookBehaviour.ExecuteFirst).Activate());
        });

        Utils.SigScan("48 89 5C 24 ?? 57 48 83 EC 20 0F B7 F9 48 0F BF DA", "GetCharacterEquipment", address =>
        {
            _getEquipmentHook = hooks.CreateHook<GetCharacterEquipmentDelegate>(GetCharacterEquipment, address).Activate();
        });

        Utils.SigScan("E8 ?? ?? ?? ?? 0F B7 CE E8 ?? ?? ?? ?? FE C8", "SetCharacterEquipment Ptr", address =>
        {
            var funcAddress = Utils.GetGlobalAddress(address + 1);
            Utils.LogDebug($"Found SetCharacterEquipment at 0x{funcAddress:X}");
            _setEquipmentHook = hooks.CreateHook<SetCharacterEquipmentDelegate>(SetCharacterEquipment, (long)funcAddress).Activate();
        });

        Utils.SigScan("40 53 48 83 EC 20 0F B7 D9 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? B8 00 02 00 00", "GetCharacterEquipment", address =>
        {
            _getEquipmentTypeHook = hooks.CreateHook<GetEquipmentTypeDelegate>(GetEquipmentType, address).Activate();
        });

        Utils.SigScan("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 0F B7 F9 0F B6 DA 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? BA 63 00 00 00", "SetItemNum", address =>
        {
            _setItemNum = hooks.CreateWrapper<SetItemNumDelegate>(address, out _);
        });

        Utils.SigScan("83 F8 01 0F 85 ?? ?? ?? ?? 0F BF CD", "ChangeCharacterOutfit", address =>
        {
            // Make it check for a change in outfit instead of body
            memory.SafeWrite(address + 2, (byte)EquipmentType.Outfit);

            // Compare current and new outfit ids instead of ids got from GetArmorOutfit (as it's wrong)
            memory.SafeWriteRaw((nuint)address + 29, new byte[] { 0x39, 0xF5 });
        });

        Utils.SigScan("48 8D 05 ?? ?? ?? ?? 48 01 C1 31 C0", "PartyInfo Ptr", address =>
        {
            _partyInfo = (PartyMemberInfo*)(Utils.GetGlobalAddress(address + 3) - 56);
            Utils.LogDebug($"Found PartyInfo at 0x{(nuint)_partyInfo:X}");
        });

        Utils.SigScan("48 8D 35 ?? ?? ?? ?? 0F 28 05 ?? ?? ?? ??", "IsFemc Ptr", address =>
        {
            IsFemc = (bool*)Utils.GetGlobalAddress(address - 4);
            Utils.LogDebug($"Found IsFemc at 0x{(nuint)IsFemc:X}");
        });
    }

    internal short GetCharacterEquipment(PartyMember character, EquipmentType type)
    {
        short equipment;
        if (type != EquipmentType.Outfit || character != PartyMember.MaleProtag)
            equipment = _getEquipmentHook.OriginalFunction(character, type);
        // Store the protag's equipment next to Yukari's since it's probably unused memory
        else 
            equipment = _partyInfo[0].Equipment[5];

        if (equipment == 0)
            equipment = GiveDefaultOutfit(character);

        return equipment;
    }

    private void SetCharacterEquipment(PartyMember character, EquipmentType type, short item)
    {
        if (type != EquipmentType.Outfit || character != PartyMember.MaleProtag)
            _setEquipmentHook.OriginalFunction(character, type, item);
        else
            // Store the protag's equipment next to Yukari's since it's probably unused memory
            _partyInfo[0].Equipment[5] = item;
    }


    private byte GetEquipmentType(short item)
    {
        // Boots from index 125 to the end are all outfits (they were unused, I hope no one needs that many new boots...)
        // Set attack to the id of the GMO since it won't be used
        if (item > 892 && item < 1024)
            return (byte)EquipmentType.Outfit;
        return _getEquipmentTypeHook.OriginalFunction(item);
    }

    private short GiveDefaultOutfit(PartyMember character)
    {
        int index = (int)character - 1;
        if (character == PartyMember.MaleProtag && *IsFemc)
            index = 10;
        var item = _defaultItems[index];

        //_setItemNum(item, 1);
        SetCharacterEquipment(character, EquipmentType.Outfit, item);
        return item;
    }

    private readonly short[] _defaultItems = new short[]
    {
        893, // MC
        899, // Yukari
        906, // Aigis
        911, // Mitsuru
        918, // Junpei
        1, // Fuuka (not used)
        924, // Akihiko
        930, // Ken
        936, // Shinjiro
        939, // Koromaru
        941, // FeMC
    };

    [StructLayout(LayoutKind.Explicit)]
    private struct PartyMemberInfo
    {
        [FieldOffset(0)]
        internal fixed short Equipment[6];

        [FieldOffset(278)]
        short unk; // Make it the right size
    }

    [Function(CallingConventions.Microsoft)]
    private delegate byte SetItemNumDelegate(short item, byte num);

    [Function(CallingConventions.Microsoft)]
    private delegate byte GetEquipmentTypeDelegate(short item);

    [Function(CallingConventions.Microsoft)]
    private delegate short GetCharacterEquipmentDelegate(PartyMember character, EquipmentType type);

    [Function(CallingConventions.Microsoft)]
    private delegate void SetCharacterEquipmentDelegate(PartyMember character, EquipmentType type, short item);
}
