using UnityEngine;
using UnityEngine.Serialization;

namespace Kigor.Networking
{
    public class ClientRecoilManager : Sirenix.OdinInspector.SerializedMonoBehaviour
    {
#if CLIENT_BUILD
        [SerializeField] private Transform head;
        [SerializeField] private bool[,] test;
        [SerializeField] private float recoilValue;
        [SerializeField] private int shootCount;
        [SerializeField] private Transform camHolder;
        private WeaponData weaponData => this.WeaponController.CurrentWeaponData;
        private PlayerWeaponController WeaponController => this.GetComponent<PlayerWeaponController>();
        [SerializeField] private bool isShooting;

        private Vector3 lastLocalEulerAngles;

        public void Awake()
        {
        }

        private void Update()
        {
            var currentRot = this.camHolder.transform.localEulerAngles;
            if (currentRot.x > 180) currentRot.x -= 360;
            if (currentRot.y > 180) currentRot.y -= 360;
            currentRot.z = 0;

            var rotationTargetDir = Vector3.zero - currentRot;
            var returnTime = this.weaponData.recoilReturnSpd * this.shootCount;
            var speedFactor =
 DL.Utils.MathUtils.LogarithInterpolator.EvaluateSpeed(rotationTargetDir.magnitude,0 , rotationTargetDir.magnitude - 0.01f, returnTime);
            
            this.camHolder.transform.localEulerAngles =
 Vector3.Lerp(currentRot, Vector3.zero, speedFactor * 2 * Time.deltaTime);
            this.recoilValue = Mathf.Lerp(this.recoilValue, 0, speedFactor * 3.5f * Time.deltaTime);
            if (this.recoilValue < 0.006f) this.recoilValue = 0;
            //var targetRotation = Quaternion.euler
        }

        public void ApplyRecoil()
        {
            this.recoilValue += 1;
            var recoilIndex = Mathf.CeilToInt(this.recoilValue) - 1;
            if (!this.isShooting) this.shootCount = recoilIndex;
            this.shootCount++;

            Debug.Log((this.shootCount, recoilIndex));
            var recoilDir =
 this.head.TransformDirection(this.weaponData.GetRecoilDirection(this.shootCount - 1)).normalized;
            if (recoilDir == Vector3.zero) return;
            //Debug.Log((recoilValue, recoilIndex, this.shootCount, recoilDir));
            var rotation = Quaternion.LookRotation(recoilDir);
            
            var currentRot = this.camHolder.transform.localEulerAngles;
            if (currentRot.x > 180) currentRot.x -= 360;
            if (currentRot.y > 180) currentRot.y -= 360;
            this.lastLocalEulerAngles = currentRot; 
            this.camHolder.transform.rotation = rotation;
            this.isShooting = true;
        }

        public void StopRecoil()
        {
            Debug.Log("recoil stop");
            this.isShooting = false;
        }
#endif
    }
}