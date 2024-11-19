using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kigor.Rigging
{
    public abstract class RiggingConstraint : MonoBehaviour
    {
        [SerializeField] protected bool autoUpdate;
        public bool AutoUpdate => this.autoUpdate;
        public abstract void SolveIK();
    }
}
