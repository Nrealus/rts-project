using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;
using GlobalManagers;
using System.Linq;

namespace Core
{
    namespace Selection
    {
        public class SelectionCallbacks
        {

            private SelectionHandler sh;

            internal void CallbackSelectionHandler(Selectable entry, bool selected)
            {
                UpdateSelectableEntry(entry, selected);
            }

            // TODO: Initially this belonged to SelectionHandler. Should it still do ?
            private void UpdateSelectableEntry(Selectable entry, bool updatedSelected)
            {
                if (sh == null)
                    sh = GameManager.Instance.currentMainHandler.selectionHandler;

                var temp = sh.knownSelectablesList.Where(facade => (facade.selectableObject == entry));
                bool entryExistsAlready = temp.Any();

                if (entryExistsAlready)
                {
                    temp.FirstOrDefault().selected = updatedSelected;
                }
                else
                {
                    sh.knownSelectablesList.Add(new SelectionHandler.SelectableFacade { selectableObject = entry, selected = updatedSelected });
                }
            }

        }
    }
}