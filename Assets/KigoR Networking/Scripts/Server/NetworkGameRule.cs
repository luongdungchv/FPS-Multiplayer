using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kigor.Networking
{
    public abstract partial class NetworkGameRule
    {
        public static Dictionary<GameRule, System.Func<NetworkGameRule>> ruleCreatorMap = new();
        protected TickScheduler tickScheduler;
        public  TickScheduler TickScheduler => this.tickScheduler;
        public void SetTickScheduler(TickScheduler scheduler) => this.tickScheduler = scheduler;
        public abstract void PlayerJoinCallback(NetworkPlayer newPlayer, int id);
        public abstract void PlayerLeaveCallback(int id);
        public abstract void Initialize(Dictionary<int, NetworkPlayer>  intialPlayers, Scene loadedScene);
        public abstract void Dispose();

        public static void InitCreatorMap()
        {
            ruleCreatorMap.Add(GameRule.FREE_FOR_ALL, () => new FreeForAllRule());
            ExtendedInitCreatorMap();
        }

        public static NetworkGameRule CreateRule(GameRule rule){
            if(!ruleCreatorMap.ContainsKey(rule)) return null;
            return ruleCreatorMap[rule]?.Invoke();
        }


        static partial void ExtendedInitCreatorMap();

        
    }

    public partial struct GameRule
    {
        private int value;
        public int Value => this.value;
        public GameRule(int value) => this.value = value;
        #region Operators_Overloading
        public static implicit operator GameRule(int value) => new(value);
        public static implicit operator GameRule(byte value) => new(value);
        public static implicit operator GameRule(short value) => new(value);
        public static explicit operator int(GameRule GameRule) => GameRule.value;

        public static bool operator ==(GameRule GameRule1, GameRule GameRule2){
            return GameRule1.value == GameRule2.value;
        }
        public static bool operator !=(GameRule GameRule1, GameRule GameRule2){
            return GameRule1.value != GameRule2.value;
        }

        public bool Equals(GameRule other)
        {
            return other.Value == this.value;
        }
        public override bool Equals(object obj)
        {
            return Equals((GameRule)obj);
        }
        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }
        #endregion

        public static GameRule FREE_FOR_ALL = 1;
        public static GameRule TEAM_DEATHMATCH = 2;
    }
}
