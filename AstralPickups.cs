using Astrum.AstralCore.Managers;
using HarmonyLib;
using MelonLoader;
using System;
using System.Linq;
using System.Reflection;
using VRC.SDKBase;
using VRC.Udon.Wrapper.Modules;

[assembly: MelonInfo(typeof(Astrum.AstralPickups), "AstralPickups", "0.4.0", downloadLink: "github.com/Astrum-Project/AstralPickups")]
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
                Extern.SetupCommands();
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

        public static string Drop()
        {
            int count = 0;
            foreach (VRC_Pickup pickup in UnityEngine.Object.FindObjectsOfType<VRC_Pickup>())
            {
                if (!pickup.IsHeld) continue;
                if (Networking.GetOwner(pickup.gameObject) != Networking.LocalPlayer)
                    Networking.SetOwner(Networking.LocalPlayer, pickup.gameObject);
                pickup.Drop(); // may be unneeded
                count++;
                // owner can possibly be restored
            }
            return count.ToString();
        }

        private static bool frozen = false;
        private static VRC_Pickup[] pickups = new VRC_Pickup[0];
        private static void FreezePickups()
        {
            if (Networking.LocalPlayer is null)
            {
                Extern.Freeze(false);
                return;
            }

            foreach (VRC_Pickup pickup in pickups)
                if (pickup != null && Networking.GetOwner(pickup.gameObject) != Networking.LocalPlayer)
                    Networking.SetOwner(Networking.LocalPlayer, pickup.gameObject);
        }

        private static class Extern
        {
            public static void SetupCommands()
            {
                ModuleManager.Module module = new ModuleManager.Module("Pickups");
                module.Register(new CommandManager.Command(new Func<string[], string>(_ => Drop())), "Drop");
                module.Register(new CommandManager.ConVar<bool>(new Action<bool>(state => Freeze(state))), "Freeze");
            }

            public static void Freeze(bool state)
            {
                if (frozen == state) return;
                frozen = state;

                if (state)
                {
                    pickups = UnityEngine.Object.FindObjectsOfType<VRC_Pickup>();
                    AstralCore.Events.OnUpdate += FreezePickups;
                }
                else AstralCore.Events.OnUpdate -= FreezePickups;
            }
        }
    }
}
