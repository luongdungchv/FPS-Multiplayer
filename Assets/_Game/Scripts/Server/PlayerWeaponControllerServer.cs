using Kigor.Networking;

namespace Kigor.Networking
{
    public partial class PlayerWeaponController
    {
#if SERVER_BUILD
        public partial void HandleInput(FPSInputPacket packet)
        {
            
        }

        public partial void ChangeWeapon(WeaponEnum weapon)
        {
            
        }
#endif
    }
}