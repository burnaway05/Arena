using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MultiFPS
{
    /// <summary>
    /// This class adds frequently used functionality in this package to base networkbahaviour class in mirror.
    /// It contains void "OnNewPlayerConnected" launched ON SERVER everytime someone connects to the game. Thanks to that we can
    /// update object state for that client, for example: health and equipment of other players that are already spawned when someone
    /// connected
    /// </summary>
    public class CustomNetworkBehaviour : NetworkBehaviour
    {
        bool _subscribedToNetworkManager;
        protected virtual void OnNewPlayerConnected(NetworkConnection conn) { }

        private void OnEnable()
        {
            if (!_subscribedToNetworkManager && CustomNetworkManager._instance)
            {
                CustomNetworkManager._instance.OnNewPlayerConnected += OnNewPlayerConnected;
                _subscribedToNetworkManager = true;
            }
        }
        private void OnDisable()
        {
            if (_subscribedToNetworkManager)
            {
                CustomNetworkManager._instance.OnNewPlayerConnected -= OnNewPlayerConnected;
                _subscribedToNetworkManager = false;
            }
        }
    }
}