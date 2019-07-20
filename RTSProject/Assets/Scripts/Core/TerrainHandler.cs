using System;
using UnityEngine;

namespace Core
{
    public class TerrainHandler : MonoBehaviour
    {
        [HideInInspector] public Terrain MyTerrain { get; private set; }
        
        ///<summary>
        /// Called from the Init of MainHandler or Awake of the GameManager singleton
        /// There needs to be a stable order in initializations, which is why we use this instead of Start and Awake to initialize stuff.
        /// </summary>
        public void Init()
        {
            try
            {
                MyTerrain = FindObjectOfType<Terrain>();
            }
            catch (NullReferenceException e)
            {
                Debug.LogError(e);
            }
            
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