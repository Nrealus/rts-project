using System;
using UnityEngine;

namespace Core
{
    //[RequireComponent(typeof(ParticularHandler1), typeof(ParticularHandler2), typeof(ParticularHandler3))]
    public class MainHandler : MonoBehaviour
    {
        [HideInInspector] public TerrainHandler terrainHandler;
        
        ///<summary>
        /// Called from the monobehaviour awake of the GameManager singleton.
        /// There needs to be a stable order in initializations, which is why we use this instead of Start and Awake to initialize stuff.
        /// </summary>
        public void Init()
        {
            /*
            handler1 = GetComponent<ParticularHandler1>();
            handler2 = GetComponent<ParticularHandler2>();
            handler3 = GetComponentInChildren<ParticularHandler3>();
            
            handler4 = FindObjectOfType<ParticularHandler4>();
            
            handler1.Init();
            handler2.Init();
            handler3.Init();
            handler4.Init();
            */
            terrainHandler = FindObjectOfType<TerrainHandler>();

            terrainHandler.Init();
        }
        private void Update()
        {
            /*
            handler1.UpdateMe();
            handler2.UpdateMe();
            */
        }

    }
}