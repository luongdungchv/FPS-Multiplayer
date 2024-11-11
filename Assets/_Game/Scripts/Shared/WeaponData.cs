using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kigor.Networking
{
    [CreateAssetMenu(fileName = "New Weapon Data", menuName = "Weapon Data")]
    public class WeaponData : Sirenix.OdinInspector.SerializedScriptableObject
    {
        public int damage;
        public float range;
        public float shootInterval;
        public ShootMode shootMode;
        public float reloadDuration;
        public int magazineSize;

        [SerializeField] private int[,] recoilPattern;
        [SerializeField] private float recoilCalcDist;
        public float recoilIncrement;
        public float recoilReturnSpd;
        private Vector3[] recoilDirections, recoilShootDirections;
        

        public Vector3 GetRecoilDirection(int index)
        {
            if (this.recoilDirections == null || this.recoilDirections.Length == 0)
            {
                this.ConstructRecoilDirections();
            }

            // Debug.Log((index, this.recoilDirections.Length, this.recoilDirections[index]));
            return this.recoilDirections[index];
        }

        public void ConstructRecoilDirections()
        {
            this.recoilDirections = new Vector3[this.magazineSize];
            var patternWidth = this.recoilPattern.GetLength(0);
            var patternHeight = this.recoilPattern.GetLength(1);
            var shootOrigin = new Vector3((float)patternWidth / 2, patternHeight, 0);
            for (int x = 0; x < patternWidth; x++)
            {
                for (int y = 0; y < patternHeight; y++)
                {
                    var indexVal = this.recoilPattern[x, y];
                    if(indexVal == 0) continue;
                    var direction = (new Vector3(x, y, 0) - shootOrigin) + Vector3.forward * this.recoilCalcDist;
                    direction.y = -direction.y;
                    this.recoilDirections[indexVal - 1] = direction;
                }
            }
        }
    }

    public enum ShootMode
    {
        Auto, SemiAuto, Burst
    }
}