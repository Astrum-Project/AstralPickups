using HarmonyLib;
using MelonLoader;
using System;
using System.Linq;
using System.Reflection;
using VRC.SDKBase;
using VRC.Udon.Wrapper.Modules;

[assembly: MelonInfo(typeof(Astrum.AstralPickups), "AstralPickups", "0.3.0", downloadLink: "github.com/Astrum-Project/AstralPickups")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonOptionalDependencies("AstralCore")]

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
            HarmonyInstance.Patch(typeof(ExternVRCSDK3ComponentsVRCPickup).GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_allowManipulationWhenEquipped__SystemBoolean)), hkNoOp);
            HarmonyInstance.Patch(typeof(ExternVRCSDK3ComponentsVRCPickup).GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_proximity__SystemSingle)), hkNoOp);

            if (AppDomain.CurrentDomain.GetAssemblies().Any(f => f.GetName().Name == "AstralCore"))
            {
                if (UnityEngine.Application.version.Contains("1134"))
                    External.RemoveWallCheck(External.FindWallCheck());
            }
            else MelonLogger.Warning("AstralCore is missing, running at reduced functionality");
        }

        private static bool HookNoOp() => false;
        private static void HookAwake(ref VRC_Pickup __instance)
        {
            __instance.DisallowTheft = false;
            __instance.pickupable = true;
            __instance.allowManipulationWhenEquipped = true;
            __instance.proximity = float.MaxValue;
        }

        private static class External
        {
            public static IntPtr FindWallCheck()
            {
                return AstralCore.Utils.PatternScanner.Scan(
                    "GameAssembly.dll", 
                    "0F 85 C9 00 00 00" + //    jne GameAssembly.dll + AE40E0
                    "48 8B 0D ????????" + //    mov rcx, [GameAssembly.dll + 7200EC0]
                    "F6 81 2F 01 00 00 02" + // test byte ptr [rcx+12F], 2
                    "74 0E" + //                je GameAssembly.dll + AE4035
                    "44 39 B1 E0 00 00 00" + // cmp [rcx + E0],r14d
                    "75 05" + //                jne GameAssembly.dll + AE4035??
                    "E8 ????????" + //          call GameAssembly.il2cpp_runtime_class_init
                    "33 D2" //                  xor edx, edx
                );
            }

            public static void RemoveWallCheck(IntPtr address) => AstralCore.Utils.MemoryUtils.WriteBytes(address, new byte[6] { 0xE9, 0xCA, 0x00, 0x00, 0x00, 0x90 });
            public static void RepairWallCheck(IntPtr address) => AstralCore.Utils.MemoryUtils.WriteBytes(address, new byte[6] { 0x0F, 0x85, 0xC9, 0x00, 0x00, 0x00 });
        }
    }
}
