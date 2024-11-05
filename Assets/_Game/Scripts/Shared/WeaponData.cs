using UnityEngine;
using UnityEngine.Serialization;

namespace Kigor.Networking
{
    [CreateAssetMenu(fileName = "New Weapon Data", menuName = "Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        public int damage;
        public float range;
        public float shootInterval;
        public ShootMode shootMode;
        public float reloadDuration;
        public int magazineSize;
    }

    public enum ShootMode
    {
        Auto, SemiAuto, Burst
    }
}