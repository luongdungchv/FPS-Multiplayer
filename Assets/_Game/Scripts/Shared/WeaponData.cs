﻿using UnityEngine;
using UnityEngine.Serialization;

namespace Kigor.Networking
{
    public class WeaponData : ScriptableObject
    {
        public float damage;
        public float range;
        public float shootInterval;
        public ShootMode shootMode;
    }

    public enum ShootMode
    {
        Auto, SemiAuto, Burst
    }
}