using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kigor.Rigging
{
    public class ConstraintScheduler : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField] private RiggingConstraint[] constraints;

        private void Awake()
        {
            this.constraints = new RiggingConstraint[this.transform.childCount];
            for (int i = 0; i < this.transform.childCount; i++)
            {
                this.constraints[i] = this.transform.GetChild(i).GetComponent<RiggingConstraint>();
            }
        }

        private void LateUpdate()
        {
            foreach (var x in this.constraints)
            {
                x.SolveIK();
            }
        }
    }
}