using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Faction;
using Gamelogic.Extensions;

namespace Core
{
    public class FactionHandler : MonoBehaviour
    {
        /// <summary>
        /// List of faction data for factions that currently exist.
        /// </summary>
        [ReadOnly] public List<FactionData> existingFactions;

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