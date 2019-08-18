using System;
using UnityEngine;

namespace Core
{
    public class TerrainHandler : MonoBehaviour
    {
        [HideInInspector] public Terrain MyTerrain { get; private set; }

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