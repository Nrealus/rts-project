using System;
using UnityEngine;

namespace Core
{
    namespace GameHandlers
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
            }
            private void Update()
            {

            }

        }
    }
}