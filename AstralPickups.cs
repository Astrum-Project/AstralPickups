using Astrum.AstralCore.Managers;
using HarmonyLib;
using MelonLoader;
using System;
using System.Linq;
using System.Reflection;
using VRC.SDKBase;
using VRC.Udon.Wrapper.Modules;

[assembly: MelonInfo(typeof(Astrum.AstralPickups), "AstralPickups", "0.5.0", downloadLink: "github.com/Astrum-Project/AstralPickups")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonOptionalDependencies("AstralCore")]

namespace Astrum
{
    public partial class AstralPickups : MelonMod
    {
        const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;

        public static HarmonyMethod hkNoOp = new HarmonyMethod(typeof(AstralPickups).GetMethod(nameof(HookNoOp), BindingFlags.NonPublic | BindingFlags.Static));
        public static HarmonyMethod hkAwake = typeof(AstralPickups).GetMethod(nameof(HookAwake), PrivateStatic)?.ToNewHarmonyMethod();

        public static bool hasCore = false;

        private static VRC_Pickup[] pickups;

        public override void OnApplicationStart()
        {
            HarmonyInstance.Patch(typeof(VRC_Pickup).GetMethod(nameof(VRC_Pickup.Awake)), null, hkAwake); // https://docs.unity3d.com/ScriptReference/MonoBehaviour.Awake.html
            HarmonyInstance.Patch(typeof(VRCPlayerApi).GetMethod(nameof(VRCPlayerApi.EnablePickups)), hkNoOp); // https://docs.vrchat.com/docs/players#enablepickups
            HarmonyInstance.Patch(typeof(ExternVRCSDK3ComponentsVRCPickup).GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_DisallowTheft__SystemBoolean)), hkNoOp);
            HarmonyInstance.Patch(typeof(ExternVRCSDK3ComponentsVRCPickup).GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_pickupable__SystemBoolean)), hkNoOp);
            HarmonyInstance.Patch(typeof(ExternVRCSDK3ComponentsVRCPickup).GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_allowManipulationWhenEquipped__SystemBoolean)), hkNoOp);
            HarmonyInstance.Patch(typeof(ExternVRCSDK3ComponentsVRCPickup).GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_proximity__SystemSingle)), hkNoOp);

            if (hasCore = AppDomain.CurrentDomain.GetAssemblies().Any(f => f.GetName().Name == "AstralCore"))
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

        public override void OnSceneWasLoaded(int index, string _)
        {
            if (!hasCore || index != -1) return;

            Fetch();
        }

        public static void Fetch() => pickups = UnityEngine.Object.FindObjectsOfType<VRC_Pickup>();

        public static void Drop()
        {
            Fetch();

            foreach (VRC_Pickup pickup in pickups)
            {
                if (!pickup.IsHeld) continue;
                if (Networking.GetOwner(pickup.gameObject) != Networking.LocalPlayer)
                    Networking.SetOwner(Networking.LocalPlayer, pickup.gameObject);
                pickup.Drop(); // may be unneeded
                // owner can possibly be restored
            }
        }

        public static void Scatter()
        {
            Fetch();

            int max = VRCPlayerApi.AllPlayers.Count - 1;
            for (int i = 0; i < pickups.Length; i++)
                Networking.SetOwner(VRCPlayerApi.AllPlayers[UnityEngine.Random.Range(0, max)], pickups[i].gameObject);
        }

        private static class Extern
        {
            public static void SetupCommands()
            {
                ModuleManager.Module module = new ModuleManager.Module("Pickups");
                module.Register(new CommandManager.Button(new Action(() => Fetch())), nameof(Fetch));
                module.Register(new CommandManager.Button(new Action(() => Drop())), nameof(Drop));
                module.Register(new CommandManager.Button(new Action(() => Scatter())), nameof(Scatter));

                module.Register(Feeeze.cState, "Freeze");

                module.Register(Orbit.cState, "Orbit");
                module.Register(Orbit.cSpeed, "Orbit.Speed");
                module.Register(Orbit.cDistance, "Orbit.Distance");
            }
        }
    }
}
