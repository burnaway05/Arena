using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace MultiFPS
{
    public class WebRequestManager : MonoBehaviour
    {
        public delegate void MethodDelegate(string downloadHandler = "", int responseCode = 0);

        public static WebRequestManager Instance;

        List<Coroutine> _webRequests = new List<Coroutine>();

        [SerializeField] public string _domain = "localhost:5000";

        private void Awake()
        {
            if (Instance)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        private void OnDestroy()
        {
           // Instance = null;
            for (int i = 0; i < _webRequests.Count; i++)
            {
                Coroutine coroutine = _webRequests[i];
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                    _webRequests[i] = null;
                }
            }
        }

        public void SetDomain(string domain)
        {
            _domain = domain;
        }

        public void Post(string endPoint, WWWForm data, MethodDelegate receivedMessageMethod = null, MethodDelegate errorMethod = null, bool showLoadingScreen = false) //timeout time and method for timeout
        {
            _webRequests.Add(StartCoroutine(C_post($"{_domain}{endPoint}", data, receivedMessageMethod, errorMethod, showLoadingScreen)));
        }
        public void Get(string endPoint, MethodDelegate receivedMessageMethod = null, MethodDelegate errorMethod = null, bool showLoadingScreen = false)
        {
            _webRequests.Add(StartCoroutine(C_get($"{_domain}{endPoint}", receivedMessageMethod, errorMethod, showLoadingScreen)));
        }

        IEnumerator C_post(string endPoint, WWWForm data, MethodDelegate receivedMessageMethod, MethodDelegate errorMethod, bool showLoadingScreen = false)
        {

            using (UnityWebRequest www = UnityWebRequest.Post(endPoint, data))
            {
                yield return www.SendWebRequest();

                if (errorMethod != null && (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError))
                {
                    errorMethod();
                }
                else if (receivedMessageMethod != null && www.result == UnityWebRequest.Result.Success)
                {
                    receivedMessageMethod(www.downloadHandler.text, (int)www.responseCode);
                }
            }
        }

        IEnumerator C_get(string endPoint, MethodDelegate receivedMessageMethod, MethodDelegate errorMethod, bool showLoadingScreen = false)
        {

            using (UnityWebRequest www = UnityWebRequest.Get(endPoint))
            {
                yield return www.SendWebRequest();

                if (errorMethod != null && (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError))
                {
                    errorMethod();
                }
                else if (receivedMessageMethod != null)
                {
                    receivedMessageMethod(www.downloadHandler.text, (int)www.responseCode);
                }
            }
        }

        public static T Deserialize<T>(string json)
        {
            T obj = Activator.CreateInstance<T>();
            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            obj = (T)serializer.ReadObject(ms);
            ms.Close();
            return obj;
        }
    }
}