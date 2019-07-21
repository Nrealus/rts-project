using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Selection;
using System.Linq;
using UtilsAndExts;
using GlobalManagers;
using Core.Faction;

namespace Core
{
    public class SelectionHandler : MonoBehaviour
    {

        /// <summary>
        /// A container used to group minimal required data and to decouple Selectable code with high-level selection management.
        /// </summary>
        public class SelectableFacade
        {
            public Selectable selectableObject;
            public bool selected;
            // DIFFERENT IDEAS
            // public int groupID;
            // public SelectableGroup group
        }

        public List<SelectableFacade> knownSelectablesList;

        //-----------------------------------------------------------------------------------------------------//

        ///<summary>
        /// Called from the Init of MainHandler or Awake of the GameManager singleton
        /// There needs to be a stable order in initializations, which is why we use this instead of Start and Awake to initialize stuff.
        /// </summary>
        public void Init()
        {
            knownSelectablesList = new List<SelectableFacade>();
        }
        
        private void Start()
        {
            StartCoroutine(CollectSelectablesHashSet());
        }

        //-----------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Returns a list of all currently selected Selectables.
        /// </summary>
        public List<Selectable> GetCurrentlySelectedUnits()
        {
            return knownSelectablesList
                .Where(facade => (facade.selected == true && facade.selectableObject != null))
                .Select(facade => facade.selectableObject)
                .ToList();
        }

        public List<Selectable> GetCurrentlySelectedUnitsFromFaction(FactionData faction)
        {
            return knownSelectablesList
                .Where(facade => (facade.selected == true && facade.selectableObject != null 
                                    && facade.selectableObject.myFactionAffiliation.MyFaction == faction))
                .Select(facade => facade.selectableObject)
                .ToList();
        }

        //-----------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Every 3 seconds, instead of checking all elements of knownSelectablesList at once, it checks only *one* element at a time to see if it can be "collected".
        /// ("Collected" means that the Selectable object encapsulated by the element is null)
        /// </summary>
        /// <returns></returns>
        private IEnumerator CollectSelectablesHashSet()
        {
            for (int i = 0;  ; i++)
            {
                var c = knownSelectablesList.Count;
                if (c > 0)
                {
                    if (i >= c)
                        i = 0;

                    if (knownSelectablesList[i].selectableObject == null)
                    {
                        knownSelectablesList.RemoveAt(i);
                        i--;
                    }
                }

                yield return new WaitForSeconds(3);
            }            
        }

        //-----------------------------------------------------------------------------------------------------//

    }
}