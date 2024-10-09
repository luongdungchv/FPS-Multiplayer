using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kigor.Networking
{
    public partial class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance;
        [SerializeField] private NetworkPlayer networkPlayerPrefab;
        [SerializeField] private TickScheduler tickSchedulerPrefab;
        
        private ThreadManager threadManager;
        
        private void Awake(){
            if(Instance != null) Destroy(Instance.gameObject);
            Instance = this;

            NetworkGameRule.InitCreatorMap();

            Application.runInBackground = true;
            Application.targetFrameRate = 60;
        }
        
        private void Update(){
            ThreadManager.Update();
        }
        
        public NetworkPlayer SpawnPlayer(){
            var player = Instantiate(networkPlayerPrefab);
            return player;
        }
        public TickScheduler CreateTickScheduler(){
            return Instantiate(this.tickSchedulerPrefab);
        }
#if CLIENT_BUILD
        private NetworkTransport transport;
        private NetworkHandleClient handle;
        
        private void OnEnable(){
            this.handle = new NetworkHandleClient();
            this.transport = new NetworkTransport();
        }
#endif
    }
}
