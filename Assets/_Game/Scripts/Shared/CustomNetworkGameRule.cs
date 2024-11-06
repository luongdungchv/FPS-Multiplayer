using UnityEngine;

namespace Kigor.Networking
{
    public partial class NetworkGameRule
    {
        static partial void ExtendedInitCreatorMap()
        {
            ruleCreatorMap.Add(GameRule.TEAM_DEATHMATCH, () => new TeamDMRule());
        }
    }
}