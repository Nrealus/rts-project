using GlobalManagers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilsAndExts;
using Core.Helpers;

namespace Core.Selects
{
    public class Selector : MonoBehaviour
    {

        private GameManager gm;
        private Camera mainCamera;
        private UnitsRoot unitsRoot;
        private Vector3 myPreviousMousePosition, myCurrentMousePosition;

        [HideInInspector] public enum SelectionModes { Simple, Additive, Complementary };
        private bool isSelecting;

        //-----------------------------------------------------------------------------------------------------//

        private void Start()
        {
            gm = GameManager.Instance;
            mainCamera = gm.currentMainCamera.transform.GetChild(0).GetComponent<Camera>();
            unitsRoot = FindObjectOfType<UnitsRoot>();
        }

        private void Update()
        {
            myCurrentMousePosition = gm.IM.MousePosition;

            // debug
            if (Input.GetKeyDown(KeyCode.X))
            {
                FindObjectOfType<Selectable>().Select();
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                FindObjectOfType<Selectable>().Deselect();
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                Destroy(FindObjectOfType<Selectable>().gameObject);

            }

            // will be done with the new InputSystem - the global idea will remain the same (as in no overly complicated stuff involved)
            // but the great thing is that these will be done with anonymous methods subscribing to events
            if (Input.GetMouseButtonDown(gm.IM.LMB) && !isSelecting)
            {
                isSelecting = true;
            }
            if (Input.GetMouseButtonUp(gm.IM.LMB) && isSelecting)
            {
                isSelecting = false;
                ConfirmShapeSelecting();
            }
            if (isSelecting && Input.GetMouseButtonDown(gm.IM.RMB))
            {
                isSelecting = false;
                CancelShapeSelecting();
            }
        }

        private void FixedUpdate()
        {
            if (isSelecting)
            {
                ShapeSelecting();
            }
        }

        private void LateUpdate()
        {
            if (!isSelecting)
                myPreviousMousePosition = myCurrentMousePosition;
        }

        private void OnGUI()
        {
            if (isSelecting)
            {
                // Create a rect from both mouse positions
                var rect = DrawUtils.GetScreenRect(myPreviousMousePosition, myCurrentMousePosition);
                DrawUtils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
                DrawUtils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
            }
        }

        //-----------------------------------------------------------------------------------------------------//

        private void ShapeSelecting()
        {
            for (int i = 0; i < unitsRoot.transform.childCount; i++)
            {
                Selectable s = unitsRoot.transform.GetChild(i).GetComponentInChildren<Selectable>();
                if (IsSelectable(s))
                {
                    s.Preselect();
                }
                else
                {
                    s.Depreselect();
                }
            }
        }
        private void ConfirmShapeSelecting()
        {
            for (int i = 0; i < unitsRoot.transform.childCount; i++)
            {
                Selectable s = unitsRoot.transform.GetChild(i).GetComponentInChildren<Selectable>();
                s.Depreselect();
                if (IsSelectable(s))
                {
                    s.Select();
                }
                else
                {
                    s.Deselect();
                }
            }
        }

        private void CancelShapeSelecting()
        {
            for (int i = 0; i < unitsRoot.transform.childCount; i++)
            {
                unitsRoot.transform.GetChild(i).GetComponentInChildren<Selectable>().Depreselect();
            }
        }

        //-----------------------------------------------------------------------------------------------------//

        private bool IsSelectable(Selectable selectableObject)
        {
            // TODO : clumsy
            var viewportBounds = UIUtils.GetViewportBounds(mainCamera, myPreviousMousePosition, myCurrentMousePosition);

            Collider selectionCollider = selectableObject.GetComponent<Collider>();

            Vector3 center = selectionCollider.bounds.center;
            Vector3 extents = selectionCollider.bounds.extents;

            return IsPointed(selectableObject)
                    || (viewportBounds.Contains(mainCamera.WorldToViewportPoint(center))
                        && (viewportBounds.Contains(mainCamera.WorldToViewportPoint(center + extents))
                            || viewportBounds.Contains(mainCamera.WorldToViewportPoint(center - extents))));
        }

        private bool IsPointed(Selectable selectableObject)
        {

            Collider selectionCollider = selectableObject.GetComponent<Collider>();

            RaycastHit hit = new RaycastHit();
            Physics.Raycast(mainCamera.ScreenPointToRay(myCurrentMousePosition), out hit, Mathf.Infinity, LayerMask.GetMask("Selection"));

            bool pointed;
            pointed = (hit.collider == null) ? false : (hit.collider == selectionCollider);

            return pointed;
        }
    }
}