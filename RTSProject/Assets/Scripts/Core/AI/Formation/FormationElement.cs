using Core.AI.Pathtracking;
using GlobalManagers;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.AI.Formation
{
    public class FormationElement : MonoBehaviour
    {
        public bool debugCanAsk;

        private FormationSpot myOccupiedFormationSpot;
        public FormationSpot MyOccupiedFormationSpot
        {
            get
            {
                return myOccupiedFormationSpot;
            }
            set
            {
                if (myOccupiedFormationSpot != null)
                {
                    myOccupiedFormationSpot.UnassignElement();
                    //myOccupiedFormationSpot = null;
                }
                myOccupiedFormationSpot = value;
            }
        }
        private PathtrackingController MyPathtrackingController;

        private void Start()
        {
            UpdatePathtrackingController();
        }

        private void Update()
        {
            if (debugCanAsk)
            {
                if(Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    MyPathtrackingController.GoingBackwards = ! MyPathtrackingController.GoingBackwards;
                }

                OnRightReleaseSetDestination();

                OnRightHoldChoosingLookAtDirection();
            }
        }

        private void UpdatePathtrackingController()
        {
            MyPathtrackingController = GetComponentInChildren<PathtrackingController>();           
        }

        internal bool HasPath()
        {
            return MyPathtrackingController.HasPath();
        }

        internal List<Vector3> GetPath()
        {
            return MyPathtrackingController.currentPath.Special_GetVectorPath();
        }

        internal void AskOthersForTheirPath(float distStep, bool specifiedFinalOrientation, Vector3 finalOrientation)
        {
            if (myOccupiedFormationSpot != null)
            {
                myOccupiedFormationSpot.MyFormation.AdjustFollowerPointsAlongPath(myOccupiedFormationSpot, GetPath(), distStep, specifiedFinalOrientation, finalOrientation);
            }
        }

        /*internal void AskForPath(Vector3 startpos, Vector3 endpos, bool followFormationPath)
        {
            AskPathTo(startpos, endpos, false, Vector3.zero, followFormationPath);
        }*/

        internal void AskForPath(List<Vector3> waypoints, bool followFormationPath, bool specifiedFinalOrientation, Vector3 finalOrientation)
        {
            if(followFormationPath)
            {
                Path pp = MyPathtrackingController.GetComponent<Seeker>().StartPath(waypoints[0], waypoints[1],
                    (Path p) =>
                    {
                        MyPathtrackingController.currentPath.OnPathReturned(p, false, specifiedFinalOrientation, finalOrientation);
                    });
                pp.BlockUntilCalculated();
                for (int i = 1; i < waypoints.Count - 1; i++)
                {
                    pp = MyPathtrackingController.GetComponent<Seeker>().StartPath(waypoints[i], waypoints[i+1],
                    (Path p) =>
                    {
                        MyPathtrackingController.currentPath.OnPathReturned(p, true, specifiedFinalOrientation, finalOrientation);
                    });
                    pp.BlockUntilCalculated();
                }
                //pp.BlockUntilCalculated();
            }
        }

        private void AskPathTo(Vector3 startPos, Vector3 endPos, /*bool add,*/ bool specifiedFinalOrientation, Vector3 finalOrientation, bool followFormationPath)
        {
            if (!followFormationPath)
            {
                Path pp = MyPathtrackingController.GetComponent<Seeker>().StartPath(startPos, endPos,
                    (Path p) =>
                    {
                        MyPathtrackingController.currentPath.OnPathReturned(p, false, specifiedFinalOrientation, finalOrientation/*, add, endPos, okayLookAt, toLookAt*/);
                        AskOthersForTheirPath(7, specifiedFinalOrientation, Vector3.zero);
                    });
                pp.BlockUntilCalculated();
                //pp.BlockUntilCalculated();
            }
            else
            {
                Path pp = MyPathtrackingController.GetComponent<Seeker>().StartPath(startPos, endPos,
                    (Path p) =>
                    {
                        MyPathtrackingController.currentPath.OnPathReturned(p, false, specifiedFinalOrientation, finalOrientation/*, add, endPos, okayLookAt, toLookAt*/);
                    });
                pp.BlockUntilCalculated();
                //pp.BlockUntilCalculated();
            }
        }

        private Vector3 endpos;
        private bool okayEndpos;
        private Vector3 finalOrientation;
        private bool specifiedFinalOrientation;
        private void OnRightReleaseSetDestination()
        {
            if (GameManager.Instance.IM.IsMouseButtonPressed[GameManager.Instance.IM.RMB])
            {
                var ray = GameManager.Instance.currentMainCamera.GetComponentsInChildren<Camera>()[0].ScreenPointToRay(GameManager.Instance.IM.MousePosition);
                RaycastHit hit = new RaycastHit();
                Physics.Raycast(ray, out hit, Mathf.Infinity);

                if (hit.collider != null)
                {
                    okayEndpos = true;
                    endpos = hit.point;
                }
                else
                {
                    okayEndpos = false;
                }

            }

            if (okayEndpos && GameManager.Instance.IM.IsMouseButtonReleased[GameManager.Instance.IM.RMB])
            {
                bool add = GameManager.Instance.IM.IsKeyHeld[GameManager.Instance.IM.LShift_Key];

                Vector3 startpos = transform.position;
                /*if (add && currentPath.ActualPathExists())
                {
                    startpos = currentPath.LastActualWaypoint.position;
                }*/
                AskPathTo(startpos, endpos, /*add,*/ specifiedFinalOrientation, finalOrientation, false);
                /*
                OrderPlacementTypes pl = (!add) ? OrderPlacementTypes.First : OrderPlacementTypes.Last;
                List<Vector3> wp = new List<Vector3>() { endpos };
                OrderContainer oc = new OrderContainer(OrderTypes.Movement, pl, -1, new List<OrderReceiver>() { GetComponentInParent<OrderReceiver>() },
                                                                            new MovementOrderArgs(wp, okayLookAt, toLookAt), null);
                GameManager.Instance.currentMainHandler.orderHandler.DispatchOrderContainerToReceivers(oc);
                */
                okayEndpos = false;

            }
        }
        private void OnRightHoldChoosingLookAtDirection()
        {
            if (okayEndpos && GameManager.Instance.IM.IsMouseButtonHeld[GameManager.Instance.IM.RMB])
            {
                var ray = GameManager.Instance.currentMainCamera.GetComponentsInChildren<Camera>()[0].ScreenPointToRay(GameManager.Instance.IM.MousePosition);
                RaycastHit hit = new RaycastHit();
                Physics.Raycast(ray, out hit, Mathf.Infinity);

                if (hit.collider != null && (hit.point - endpos).magnitude > 1)
                {
                    specifiedFinalOrientation = true;
                    finalOrientation = hit.point - endpos;
                }
                else
                {
                    specifiedFinalOrientation = false;
                }
            }
        }

    }
}