# AstralPickups
Ignore `VRC_Pickup::DisallowTheft`, `VRC_Pickup::pickupable` via hooking

# Features:
- Allows pickup theft from other players
- Makes pickups unconditionally enabled

# Advantages:
- Doesn't run OnUpdate
- Less codes gets ran by NOPing an existing function

# Issues:
- May not work perfectly on SDK2

# Todo:
- [ ] Unpatching
- [ ] MelonPreferences
- [ ] Wall Reach
- [ ] Infinite Reach
