using HarmonyLib;
using MelonLoader;
using System;
using System.Reflection;
using VRC.SDKBase;
using VRC.Udon.Wrapper.Modules;

[assembly: MelonInfo(typeof(Astrum.AstralPickups), "AstralPickups", "0.2.0", downloadLink: "github.com/Astrum-Project/AstralPickups")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]

namespace Astrum
{
    public class AstralPickups : MelonMod
    {
        const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;

        public static HarmonyMethod hkNoOp = new HarmonyMethod(typeof(AstralPickups).GetMethod(nameof(HookNoOp), BindingFlags.NonPublic | BindingFlags.Static));//
        public static HarmonyMethod hkAwake = typeof(AstralPickups).GetMethod(nameof(HookAwake), PrivateStatic)?.ToNewHarmonyMethod();

        public override void OnApplicationStart()
        {
            HarmonyInstance.Patch(typeof(VRC_Pickup).GetMethod(nameof(VRC_Pickup.Awake)), null, hkAwake); // https://docs.unity3d.com/ScriptReference/MonoBehaviour.Awake.html
            HarmonyInstance.Patch(typeof(VRCPlayerApi).GetMethod(nameof(VRCPlayerApi.EnablePickups)), hkNoOp); // https://docs.vrchat.com/docs/players#enablepickups
            HarmonyInstance.Patch(typeof(ExternVRCSDK3ComponentsVRCPickup).GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_DisallowTheft__SystemBoolean)), hkNoOp);
            HarmonyInstance.Patch(typeof(ExternVRCSDK3ComponentsVRCPickup).GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_pickupable__SystemBoolean)), hkNoOp);
        }

        private static bool HookNoOp() => false;
        private static void HookAwake(ref VRC_Pickup __instance)
        {
            __instance.DisallowTheft = false;
            __instance.pickupable = true;
            __instance.allowManipulationWhenEquipped = true; // TODO: write a hook to prevent this from being set
            __instance.proximity = float.MaxValue; // TODO: write a hook to prevent this from being set
        }
    }
}
