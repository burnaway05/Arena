using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using MultiFPS.Gameplay.Gamemodes;

namespace MultiFPS.Gameplay {
    public class RagDollSyncer : NetworkBehaviour
    {

        RagDoll _ragDoll;

        bool serverIsSynchronizing = false;
        Quaternion[] _rigidBodyRotations = new Quaternion[7];
        float[] _limbFlexors = new float [4];
        Vector3 _hipsPosition;
        public float RagdollLerpSpeed = 5f;

        bool lerpRagdoll = false;

        

        //lerp ragdoll to state received from server
        private void Update()
        {
            if (isServer) return;

            if (!lerpRagdoll) return;

            //lerp ragdoll position
            _ragDoll.rigidBodies[0].position = Vector3.Lerp(_ragDoll.rigidBodies[0].position, _hipsPosition, RagdollLerpSpeed*Time.deltaTime);

            //lerp limbs rotations
            for (int i = 0; i < _ragDoll.rigidBodies.Length; i++)
            {
                _ragDoll.rigidBodies[i].rotation = Quaternion.Lerp(_ragDoll.rigidBodies[i].rotation, _rigidBodyRotations[i], RagdollLerpSpeed*Time.deltaTime);
            }

            //lerp joints
            for (int i = 0; i < _ragDoll.limbFlexors.Length; i++)
            {
                _ragDoll.limbFlexors[i].localRotation = Quaternion.Lerp(_ragDoll.limbFlexors[i].localRotation, Quaternion.Euler(_limbFlexors[i], 0, 0), RagdollLerpSpeed*Time.deltaTime);
            }
        }

        void SendRagdollInfo() 
        {
            for (int i = 0; i < _ragDoll.rigidBodies.Length; i++)
            {
                _rigidBodyRotations[i] = _ragDoll.rigidBodies[i].rotation;
            }
            for (int i = 0; i < _ragDoll.limbFlexors.Length; i++)
            {
                _limbFlexors[i] = _ragDoll.limbFlexors[i].localEulerAngles.x;
            }

            ReceiveRagdollInfo(_rigidBodyRotations, _ragDoll.rigidBodies[0].position, _limbFlexors);
        }

        //receive ragdoll data from server
        [ClientRpc(channel = Channels.Unreliable)]
        void ReceiveRagdollInfo(Quaternion[] rigidBodies, Vector3 hipsPosition, float[] limbFlexors) 
        {
            if (!_ragDoll) 
                return;

            if (serverIsSynchronizing) return;

            lerpRagdoll = true;

            _hipsPosition = hipsPosition;
            _rigidBodyRotations = rigidBodies;
            _limbFlexors = limbFlexors;
        }

        public void ServerStartSynchronizingRagdoll(RagDoll ragdoll) 
        {
            AssignRagdoll(ragdoll);

            serverIsSynchronizing = true;
            StartCoroutine(SendRagdollInfoCoroutine());
            IEnumerator SendRagdollInfoCoroutine() 
            {
                SendRagdollInfo();

                while (enabled) 
                {
                    //update ragdoll for clients 33 times per second
                    yield return new WaitForSeconds(0.03f);
                    SendRagdollInfo();
                }
            }
        }

        public void AssignRagdoll(RagDoll ragDoll) 
        {
            _ragDoll = ragDoll;
        }
    }
}