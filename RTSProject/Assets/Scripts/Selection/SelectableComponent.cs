using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

// Units that can be "selected" by the user must attach this component.
// Serves as a "tag" for selected units.

[DisallowMultipleComponent]

public class SelectableComponent : MonoBehaviour
{

    public enum Whose { MyOwn = 0, Allied = 1, Enemy = 2, Neutral = 3 };
    public Whose whoseAmI = Whose.MyOwn;

    private GameObject selectionIcon;

    public bool selected { get; private set; }
    public bool preselected { get; private set; }

    public bool mousepreselectedmarker; // HACK : only used for a small hack in mouse preselection, allowing "unit viewer" preselection to still work as intended.

    private void Start()
    {
        selected = false;
        preselected = false;
        Outline[] outlines = gameObject.GetComponentsInChildren<Outline>();
        foreach (Outline outline in outlines)
        {
            outline.enabled = false;
        }

        mousepreselectedmarker = false;
    }

    public void Preselect(Color colorToUse, GameObject selectionCirclePrefab)
    {
        if (preselected == false)
        {
            preselected = true;

            if (selectionIcon == null)
            {
                selectionIcon = Instantiate(selectionCirclePrefab);
                selectionIcon.transform.SetParent(transform, false);
            }

            MeshRenderer[] rs = transform.GetChild(1).GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer r in rs)
            {
                Material m = r.material;
                m.color = colorToUse;
                r.material = m;
            }

            MeshRenderer selectionmr = selectionIcon.GetComponentInChildren<MeshRenderer>();
            Material selectionmr_m = selectionmr.material;
            selectionmr_m.SetColor("_Color", colorToUse);
            selectionmr.material = selectionmr_m;

            LineRenderer selectionlr = selectionIcon.GetComponentInChildren<LineRenderer>();
            selectionlr.startColor = colorToUse;
            selectionlr.endColor = colorToUse;
        }
    }

    public void Select(Color colorToUse, GameObject selectionCirclePrefab)
    {
        selected = true;

        preselected = false;

        if (selectionIcon == null)
        {
            selectionIcon = Instantiate(selectionCirclePrefab);
            selectionIcon.transform.SetParent(transform, false);
        }

        MeshRenderer[] rs = transform.GetChild(1).GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer r in rs)
        {
            Material m = r.material;
            m.color = Color.white;
            r.material = m;
        }

        MeshRenderer selectionmr = selectionIcon.GetComponentInChildren<MeshRenderer>();
        Material selectionmr_m = selectionmr.material;
        selectionmr_m.SetColor("_Color", colorToUse);
        selectionmr.material = selectionmr_m;

        LineRenderer selectionlr = selectionIcon.GetComponentInChildren<LineRenderer>();
        selectionlr.startColor = colorToUse;
        selectionlr.endColor = colorToUse;

        Outline[] outlines = GetComponentsInChildren<Outline>();
        foreach (Outline outline in outlines)
        {
            outline.enabled = true;
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.OutlineColor = colorToUse;
        }
    }

    public void Deselect()
    {
        if (selected == true || preselected == true)
        {
            selected = false;
            preselected = false;// HACK : keep in mind ??

            MeshRenderer[] rs = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer r in rs)
            {
                Material m = r.material;
                m.color = Color.white;
                r.material = m;
            }

            Outline[] outlines = GetComponentsInChildren<Outline>();

            foreach (Outline outline in outlines)
            {
                outline.enabled = false;
            }

            if (selectionIcon != null)
            {
                Destroy(selectionIcon.gameObject);
                selectionIcon = null;
            }
        }
    }

    public void Depreselect()
    {
        if (preselected == true)
        {
            preselected = false;

            MeshRenderer[] rs = GetComponentsInChildren<MeshRenderer>();

            Color _c = rs[0].material.color;

            foreach (MeshRenderer r in rs)
            {
                Material m = r.material;
                m.color = Color.white;
                r.material = m;
            }

            if (selected == false)
            {
                Destroy(selectionIcon.gameObject);
                selectionIcon = null;
            }
            else
            {
                MeshRenderer selectionmr = selectionIcon.GetComponentInChildren<MeshRenderer>();
                Material selectionmr_m = selectionmr.material;
                selectionmr_m.SetColor("_Color", _c);
                selectionmr.material = selectionmr_m;

                LineRenderer selectionlr = selectionIcon.GetComponentInChildren<LineRenderer>();
                selectionlr.startColor = _c;
                selectionlr.endColor = _c;
            }
        }
    }

}