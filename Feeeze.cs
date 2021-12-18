using Astrum.AstralCore.UI.Attributes;
using VRC.SDKBase;

namespace Astrum
{
    partial class AstralPickups
    {
        // lkicMsn-s_8
        public static class Feeeze
        {
            private static bool state = false;
            [UIProperty<bool>("Pickups", "Freeze")]
            public static bool State
            {
                get => state;
                set
                {
                    if (state == value) return;
                    state = value;

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
