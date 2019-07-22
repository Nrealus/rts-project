using Core.Faction;
using Core.Selection;
using Gamelogic.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Core
{
    public class SelectionHandler : MonoBehaviour
    {

        /// <summary>
        /// A container used to group minimal required data and to decouple Selectable code with high-level selection management.
        /// </summary>
        private class SelectableFacade
        {
            public Selectable selectableObject;
            public ObservedValue<bool> selected;
            // DIFFERENT IDEAS
            // public int groupID;
            // public SelectableGroup group

            public void Init(Selectable selectableObjectValue, bool selectedValue)
            {
                selectableObject = selectableObjectValue;

                selected = new ObservedValue<bool>(!selectedValue);
                selected.OnValueChange += () =>
                {
                    SelectionHandler.UpdateCurrentlySelectedUnits();
                };
                selected.Value = selectedValue;

            }
        }

        private readonly static List<SelectableFacade> _knownSelectablesList = new List<SelectableFacade>();

        private static List<Selectable> _currentlySelected;
        public static ReadOnlyCollection<Selectable> CurrentlySelectedReadonly { get; private set; }
        
        //-----------------------------------------------------------------------------------------------------//

        ///<summary>
        /// Called from the Init of MainHandler or Awake of the GameManager singleton
        /// There needs to be a stable order in initializations, which is why we use this instead of Start and Awake to initialize stuff.
        /// </summary>
        public void Init()
        {
            _currentlySelected = new List<Selectable>();
            CurrentlySelectedReadonly = _currentlySelected.AsReadOnly();
        }

        private void Start()
        {
            StartCoroutine(CollectSelectablesHashSet());
        }

        public void Update()
        {
            Debug.Log(CurrentlySelectedReadonly.Count);
        }

        //-----------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Updates the list of all currently selected Selectables.
        /// </summary>
        public static void UpdateCurrentlySelectedUnits()
        {
            _currentlySelected = _knownSelectablesList
                .Where(facade => (facade.selected.Value == true && facade.selectableObject != null))
                .Select(facade => facade.selectableObject)
                .ToList();

            CurrentlySelectedReadonly = _currentlySelected.AsReadOnly();
        }

        public static List<Selectable> GetCurrentlySelectedUnitsFromFaction(FactionData faction)
        {
            return _knownSelectablesList
                .Where(facade => (facade.selected.Value == true && facade.selectableObject != null
                                    && facade.selectableObject.myFactionAffiliation.MyFaction == faction))
                .Select(facade => facade.selectableObject)
                .ToList();
        }

        public static void SelectionCallback(Selectable entry, bool updatedSelected)
        {
            var temp = _knownSelectablesList.Where(facade => (facade.selectableObject == entry));
            bool entryExistsAlready = temp.Any();

            if (entryExistsAlready)
            {
                temp.FirstOrDefault().selected.Value = updatedSelected;
            }
            else
            {
                // this order is important !!
                var facade = new SelectableFacade();
                _knownSelectablesList.Add(facade);
                facade.Init(entry, updatedSelected);
            }
        }

        //-----------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Every 3 seconds, instead of checking all elements of knownSelectablesList at once, it checks only *one* element at a time to see if it can be "collected".
        /// ("Collected" means that the Selectable object encapsulated by the element is null)
        /// </summary>
        /// <returns></returns>
        private IEnumerator CollectSelectablesHashSet()
        {
            for (int i = 0; ; i++)
            {
                var c = _knownSelectablesList.Count;
                if (c > 0)
                {
                    if (i >= c)
                        i = 0;

                    if (_knownSelectablesList[i].selectableObject == null)
                    {
                        _knownSelectablesList[i].selectableObject = null; // needed ?
                        _knownSelectablesList[i].selected.Clear();
                        _knownSelectablesList.RemoveAt(i);
                        i--;
                    }
                }

                yield return new WaitForSeconds(3);
            }
        }

        //-----------------------------------------------------------------------------------------------------//

    }
}