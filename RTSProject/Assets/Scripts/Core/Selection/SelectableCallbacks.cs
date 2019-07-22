using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;
using GlobalManagers;
using System.Linq;
using Core;

namespace Core.Selection
{
    public class SelectableCallbacks
    {
        public void CallbackSelectionHandler(Selectable entry, bool selected)
        {
            SelectionHandler.SelectionCallback(entry, selected);
        }

    }
}