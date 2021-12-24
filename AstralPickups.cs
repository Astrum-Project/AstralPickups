using Astrum.AstralCore.UI.Attributes;
using HarmonyLib;
using MelonLoader;
using System;
using System.Reflection;
using VRC.SDKBase;
using VRC.Udon.Wrapper.Modules;

[assembly: MelonInfo(typeof(Astrum.AstralPickups), "AstralPickups", "0.6.1", downloadLink: "github.com/Astrum-Project/AstralPickups")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]
[assembly: MelonOptionalDependencies("AstralCore")]

namespace Astrum
{
    public partial class AstralPickups : MelonMod
    {
        const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;

        public static HarmonyMethod hkNoOp = new(typeof(AstralPickups).GetMethod(nameof(HookNoOp), PrivateStatic));
        public static HarmonyMethod hkAwake = typeof(AstralPickups).GetMethod(nameof(HookAwake), PrivateStatic)?.ToNewHarmonyMethod();

        private static VRC_Pickup[] pickups;

        public override void OnApplicationStart()
        {
            HarmonyInstance.Patch(typeof(VRC_Pickup).GetMethod(nameof(VRC_Pickup.Awake)), null, hkAwake); // https://docs.unity3d.com/ScriptReference/MonoBehaviour.Awake.html
            HarmonyInstance.Patch(typeof(VRCPlayerApi).GetMethod(nameof(VRCPlayerApi.EnablePickups)), hkNoOp); // https://docs.vrchat.com/docs/players#enablepickups
            HarmonyInstance.Patch(typeof(ExternVRCSDK3ComponentsVRCPickup).GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_DisallowTheft__SystemBoolean)), hkNoOp);
            HarmonyInstance.Patch(typeof(ExternVRCSDK3ComponentsVRCPickup).GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_pickupable__SystemBoolean)), hkNoOp);
            HarmonyInstance.Patch(typeof(ExternVRCSDK3ComponentsVRCPickup).GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_allowManipulationWhenEquipped__SystemBoolean)), hkNoOp);
            HarmonyInstance.Patch(typeof(ExternVRCSDK3ComponentsVRCPickup).GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_proximity__SystemSingle)), hkNoOp);
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
            if (index == -1) Fetch();
        }

        [UIButton("Pickups", "Fetch")]
        public static void Fetch() => pickups = UnityEngine.Object.FindObjectsOfType<VRC_Pickup>();

        [UIButton("Pickups", "Drop")]
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

        [UIButton("Pickups", "Scatter")]
        public static void Scatter()
        {
            Fetch();

            for (int i = 0; i < pickups.Length; i++) 
                Networking.SetOwner(VRCPlayerApi.AllPlayers[UnityEngine.Random.Range(0, VRCPlayerApi.AllPlayers.Count)], pickups[i].gameObject);
        }
    }
}
