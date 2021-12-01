using VRC.SDKBase;
using static Astrum.AstralCore.Managers.CommandManager;

namespace Astrum
{
    partial class AstralPickups
    {
        // lkicMsn-s_8
        public static class Feeeze
        {
            public static ConVar<bool> cState = new ConVar<bool>(val => State = val, state);
            private static bool state = false;
            public static bool State
            {
                get => state;
                set
                {
                    if (state == value) return;
                    state = value;
                    cState.Value = value;

                    if (value)
                        AstralCore.Events.OnUpdate += Update;
                    else AstralCore.Events.OnUpdate -= Update;
                }
            }

            private static void Update()
            {
                if (Networking.LocalPlayer is null)
                {
                    State = false;
                    return;
                }

                foreach (VRC_Pickup pickup in pickups)
                {
                    if (pickup is null)
                    {
                        Fetch();
                        return;
                    }

                    if (Networking.GetOwner(pickup.gameObject) != Networking.LocalPlayer)
                        Networking.SetOwner(Networking.LocalPlayer, pickup.gameObject);
                }
            }
        }
    }
}
