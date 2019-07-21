using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Faction;

namespace Core
{
    namespace GameHandlers
    {
        public class FactionHandler : MonoBehaviour
        {
            /// <summary>
            /// List of faction data for factions that currently exist.
            /// </summary>
            public List<FactionData> existingFactions;

            ///<summary>
            /// Called from the Init of MainHandler or Awake of the GameManager singleton
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