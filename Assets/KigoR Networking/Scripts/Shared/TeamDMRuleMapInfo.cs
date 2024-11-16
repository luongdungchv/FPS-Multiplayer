using System.Linq;
using UnityEngine;

namespace Kigor.Networking
{
    public class TeamDMRuleMapInfo : MonoBehaviour
    {
        public static TeamDMRuleMapInfo Instance;

        private void Awake()
        {
            Instance = this;
        }
        [SerializeField] private Transform[] firstTeamSpawnPoints, secondTeamSpawnPoints;

        public Vector3[] GetFirstTeamSpawnPositions()
        {
            return this.firstTeamSpawnPoints.Select(x => x.position).ToArray();
        }
        public Vector3[] GetSecondTeamSpawnPositions()
        {
            return this.secondTeamSpawnPoints.Select(x => x.position).ToArray();
        }
    }
}