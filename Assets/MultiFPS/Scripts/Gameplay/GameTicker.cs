using UnityEngine;

namespace MultiFPS.Gameplay
{
    [DisallowMultipleComponent]
    public class GameTicker : MonoBehaviour
    {
        public delegate void Tick();
        public static Tick Game_Tick;


        private int _tickRate;
        float _tickDuration;
        float _tickTimer;
        void Start()
        {
            _tickRate = GetComponent<CustomNetworkManager>().sendRate;

            _tickDuration = 1f / _tickRate;
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time >= _tickTimer)
            {
                _tickTimer = Time.time + _tickDuration;
                Game_Tick?.Invoke();
            }
        }
    }
}