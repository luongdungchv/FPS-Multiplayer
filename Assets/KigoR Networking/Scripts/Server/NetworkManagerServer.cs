using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kigor.Networking
{
    public partial class NetworkManager : MonoBehaviour
    {
        [SerializeField] private string hostname;
        [SerializeField] private string ipAddress;
        [SerializeField] private List<NetworkPortCluster> portClusters;
#if SERVER_BUILD
        private List<Scene> gameRoomScenes;

        private void Start()
        {
            gameRoomScenes = new List<Scene>();
        }

        public void AddScene(Scene scene){
            this.gameRoomScenes.Add(scene);
        }
        public void RemoveScene(Scene scene){
            this.gameRoomScenes.Remove(scene);
        }

        private void FixedUpdate(){
            foreach(var scene in this.gameRoomScenes){
                scene.GetPhysicsScene().Simulate(Time.fixedDeltaTime);
            }
        }
#endif

    }
}
