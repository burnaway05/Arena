using MultiFPS.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace MultiFPS
{
    public class UITeamSelector : MonoBehaviour
    {
        public static UITeamSelector Instance;

        [SerializeField] Text _rejectionReasonMessageRenderer;


        private void Awake()
        {
            Instance = this;
            WriteRejectionReason(string.Empty);
        }

        public void WriteRejectionReason(string msg)
        {
            _rejectionReasonMessageRenderer.text = msg;
        }

        public void SelectTeam(int teamID)
        {
            ClientFrontend.ClientPlayerInstance.ClientRequestJoiningTeam(teamID);
        }
    }
}
