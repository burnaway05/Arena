using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiFPS
{
    public class RoomConnector : MonoBehaviour
    {
        [Tooltip("Drag and drop here input field for typing address")]
        public InputField Address;

        [Tooltip("Drag and drop here input field for typing port")]
        public InputField Port;

        [Tooltip("Drag and drop here button for connecting to the existing game")]
        public Button ConnectBtn;

        private void Start()
        {
            Address.onValueChanged.AddListener(OnSetValues);
            Port.onValueChanged.AddListener(OnSetValues);
            ConnectBtn.onClick.AddListener(Btn_Connect);

            OnSetValues();
        }
        public void OnSetValues(string s = "")
        {

            Port.text = Port.text.Replace("-", string.Empty);

            if (Port.text == string.Empty) return;

            int port = System.Convert.ToInt32(Port.text);
            port = Mathf.Clamp(port, 0, ushort.MaxValue);

            Port.text = port.ToString();

            CustomNetworkManager._instance.SetAddressAndPort(Address.text, Port.text);
        }
        public void Btn_Connect()
        {
            CustomNetworkManager._instance.StartClient();
        }

    }
}