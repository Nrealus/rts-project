using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Core.Faction;
using Core.Helpers;
using OutlineGraphical;

namespace Core.Selection
{
    /// <summary>
    /// Allows to an object to be selected, typically (or even only ?) a unit.
    /// </summary>
    [RequireComponent(typeof(FactionAffiliation))]
    [DisallowMultipleComponent]
    public class Selectable : MonoBehaviour
    {

        [HideInInspector] public Transform RootObject { get { return transform.parent; } }

        private MeshesRoot meshesRoot;
        public FactionAffiliation myFactionAffiliation { get; private set; }
        private SelectionCircle selectionIcon;
        private SelectableCallbacks myCallbacks = new SelectableCallbacks();

        // This field indicates whether the object is selected or not.
        private bool selected;
        private bool Selected
        {
            get => selected;
            set
            {
                if (selected != value)
                { selected = value; myCallbacks.CallbackSelectionHandler(this, selected); }
            }
        }
        //Preselection indicates whether the object "is being selected", but isn't necessarily really selected yet.
        private bool preselected;
        private bool Preselected
        {
            get => preselected;
            set => preselected = value;
        }

        public bool mousepreselectedmarker; // HACK : only used for a small hack in mouse preselection, allowing "unit viewer" preselection to still work as intended.

        private void OnDestroy()
        {
            Selected = false;
            Preselected = false;
        }

        private void Start()
        {
            meshesRoot = transform.parent.GetComponentInChildren<MeshesRoot>();

            myFactionAffiliation = GetComponent<FactionAffiliation>();

            Selected = false;
            Preselected = false;
            Outline[] outlines = meshesRoot.GetComponentsInChildren<Outline>();
            foreach (Outline outline in outlines)
            {
                outline.enabled = false;
            }

            mousepreselectedmarker = false;
        }

        internal void Preselect()
        {
            Preselect(myFactionAffiliation.MyFaction.baseColor, myFactionAffiliation.MyFaction.selectionCirclePrefab);
        }
        internal void Select()
        {
            Select(myFactionAffiliation.MyFaction.baseColor, myFactionAffiliation.MyFaction.selectionCirclePrefab);
        }
            
        private void Preselect(Color colorToUse, SelectionCircle selectionCirclePrefab)
        {
            if (Preselected == false)
            {
                Preselected = true;

                if (selectionIcon == null)
                {
                    selectionIcon = Instantiate(selectionCirclePrefab);
                    selectionIcon.transform.SetParent(transform, false);
                }
                else
                {
                    selectionIcon.gameObject.SetActive(true);
                }

                MeshRenderer[] rs = meshesRoot.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer r in rs)
                {
                    Material m = r.material;
                    m.color = colorToUse;
                    r.material = m;
                }

                selectionIcon.MaterialsAndColorUpdate(colorToUse);
            }
        }

        private void Select(Color colorToUse, SelectionCircle selectionCirclePrefab)
        {
            Selected = true;

            Preselected = false;

            if (selectionIcon == null)
            {
                selectionIcon = Instantiate(selectionCirclePrefab);
                selectionIcon.transform.SetParent(transform, false);
            }
            else
            {
                selectionIcon.gameObject.SetActive(true);
            }

            MeshRenderer[] rs = meshesRoot.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer r in rs)
            {
                Material m = r.material;
                m.color = Color.white; // HACK : watch out ?
                r.material = m;
            }

            selectionIcon.MaterialsAndColorUpdate(colorToUse);

            Outline[] outlines = meshesRoot.GetComponentsInChildren<Outline>();
            foreach (Outline outline in outlines)
            {
                outline.enabled = true;
                outline.OutlineMode = Outline.Mode.OutlineAll;
                outline.OutlineColor = colorToUse;
            }
        }

        internal void Deselect()
        {
            if (Selected == true || Preselected == true)
            {
                Selected = false;
                Preselected = false;// HACK : keep in mind ??

                MeshRenderer[] rs = meshesRoot.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer r in rs)
                {
                    Material m = r.material;
                    m.color = Color.white; // HACK : watch out ?
                    r.material = m;
                }

                Outline[] outlines = meshesRoot.GetComponentsInChildren<Outline>();

                foreach (Outline outline in outlines)
                {
                    outline.enabled = false;
                }

                selectionIcon.gameObject.SetActive(false);
            }
        }

        internal void Depreselect()
        {
            if (Preselected == true)
            {
                Preselected = false;

                MeshRenderer[] rs = meshesRoot.GetComponentsInChildren<MeshRenderer>();

                Color _c = rs[0].material.color;

                foreach (MeshRenderer r in rs)
                {
                    Material m = r.material;
                    m.color = Color.white; // HACK : watch out ?
                    r.material = m;
                }

                if (Selected == false)
                {
                    selectionIcon.gameObject.SetActive(false);
                }
                else
                {
                    selectionIcon.MaterialsAndColorUpdate(_c);
                }
            }
        }

    }
}
