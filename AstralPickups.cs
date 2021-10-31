using HarmonyLib;
using MelonLoader;
using System;
using System.Reflection;
using VRC.SDKBase;
using VRC.Udon.Wrapper.Modules;

[assembly: MelonInfo(typeof(Astrum.AstralPickups), "AstralPickups", "0.1.0", downloadLink: "github.com/Astrum-Project/AstralPickups")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]

namespace Astrum
{
    public class AstralPickups : MelonMod
    {
        public override void OnApplicationStart()
        {
            var nop = new HarmonyMethod(typeof(AstralPickups).GetMethod(nameof(HookNoOp), BindingFlags.NonPublic | BindingFlags.Static));

            HarmonyInstance.Patch(
                typeof(VRC_Pickup).GetMethod(nameof(VRC_Pickup.Awake)),
                null,
                new HarmonyMethod(typeof(AstralPickups).GetMethod(nameof(HookAwake), BindingFlags.NonPublic | BindingFlags.Static))
            );

            HarmonyInstance.Patch(typeof(ExternVRCSDK3ComponentsVRCPickup).GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_DisallowTheft__SystemBoolean)), nop);
            HarmonyInstance.Patch(typeof(ExternVRCSDK3ComponentsVRCPickup).GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_pickupable__SystemBoolean)), nop);
            HarmonyInstance.Patch(typeof(VRCPlayerApi).GetMethod(nameof(VRCPlayerApi.EnablePickups)), postfix: typeof(AstralPickups).GetMethod(nameof(EnablePickupsPatch), BindingFlags.NonPublic | BindingFlags.Static)
                ?.ToNewHarmonyMethod());
        }

        private static void EnablePickupsPatch(ref bool enable)
        {
            enable = true;
        }

        private static void HookAwake(ref VRC_Pickup __instance)
        {
            __instance.DisallowTheft = false;
            __instance.pickupable = true;
            __instance.allowManipulationWhenEquipped = true;
            __instance.proximity = float.MaxValue;
        }

        private static bool HookNoOp() => false;
    }
}
