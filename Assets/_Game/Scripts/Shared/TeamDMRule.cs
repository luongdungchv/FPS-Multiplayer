using System;
using System.Collections;
using System.Collections.Generic;
using Kigor.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;
using NetworkPlayer = Kigor.Networking.NetworkPlayer;

namespace Kigor.Networking
{
    public partial class TeamDMRule : NetworkGameRule, IPlayersHaveState
    {
        [SerializeField] private Dictionary<int, NetworkPlayer> players;
        [SerializeField] private int maxAllowed;
        
    }

}