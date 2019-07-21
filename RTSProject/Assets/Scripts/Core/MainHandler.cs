using System;
using UnityEngine;

namespace Core
{
    namespace GameHandlers
    {
        //[RequireComponent(typeof(ParticularHandler1), typeof(ParticularHandler2), typeof(ParticularHandler3))]
        public class MainHandler : MonoBehaviour
        {
            [HideInInspector] public TerrainHandler terrainHandler;
            [HideInInspector] public FactionHandler factionHandler;
            [HideInInspector] public SelectionHandler selectionHandler;

            ///<summary>
            /// Called from the monobehaviour awake of the GameManager singleton.
            /// There needs to be a stable order in initializations, which is why we use this instead of Start and Awake to initialize stuff.
            /// </summary>
            public void Init()
            {
                /*
                handler1 = GetComponent<ParticularHandler1>();

                handler1.Init();
                */
                factionHandler = FindObjectOfType<FactionHandler>();
                terrainHandler = FindObjectOfType<TerrainHandler>();
                selectionHandler = FindObjectOfType<SelectionHandler>();

                factionHandler.Init();
                terrainHandler.Init();
                selectionHandler.Init();

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
}