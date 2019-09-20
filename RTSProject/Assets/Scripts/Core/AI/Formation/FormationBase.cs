using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.AI.Pathtracking;
using Pathfinding;
using UnityEditor;

namespace Core.AI.Formation
{

    public class FormationBase : MonoBehaviour
    {

        private float defaultSpacingDistance = 4f;

        private List<FormationSpot> formationSpots;

        public List<FormationElement> temp_pc_list;

        private void Start()
        {
            formationSpots = new List<FormationSpot>
            {
                new FormationSpot(new Vector3(0, 0, 0), 1.5f, this),
                new FormationSpot(new Vector3(-1, 0, 0), 1.5f, this),
                new FormationSpot(new Vector3(1, 0, 0), 1.5f, this)
            };

            AssignUnitsToFormationSpots(temp_pc_list);

        }

        public int test_ref_index;
        private int last_reference_index;
        private void Update()
        {
            //if(formationSpots[test_ref_index].assignedElement.HasPath())
            //    AdjustFollowerPointsAlongPath(test_ref_index, formationSpots[test_ref_index].assignedElement.GetPath(), 4, false, Vector3.zero);
        }

        private bool AssignUnitsToFormationSpots(List<FormationElement> list)
        {
            bool ok = true;

            if (list.Count > formationSpots.Count)
            {
                ok = false;
            }

            for (int i = 0; i < Mathf.Min(formationSpots.Count, list.Count); i++)
            {
                //if (i < formationSpots.Count)
                {
                    formationSpots[i].AssignElement(list[i]);
                }
            }

            return ok;
        }

        private void AdjustRelativePositions(Vector3 anchorPoint, Vector3 orientation, int referenceIndex)
        {
            Quaternion orientationRotationQuat = Quaternion.LookRotation(orientation, Vector3.up);

            for (int i = 0; i < formationSpots.Count; i++)
            {
                Vector3 theoreticalRealPos = anchorPoint + orientationRotationQuat * (formationSpots[i].offset - formationSpots[referenceIndex].offset) * defaultSpacingDistance;

                NNInfo info = AstarPath.active.GetNearest(theoreticalRealPos);
                Vector3 closest = info.position;

                //formationSpots[i].adjustedPosition = closest;
                formationSpots[i].marksPath.Add(closest);
            }
        }

        public void AdjustFollowerPointsAlongPath(FormationSpot referenceSpot, List<Vector3> pathPoints, float distStep, bool specifiedFinalOrientation, Vector3 finalOrientation)
        {
            int j = formationSpots.IndexOf(referenceSpot);
            last_reference_index = j;
            AdjustFollowerPointsAlongPath(j, pathPoints, distStep, specifiedFinalOrientation, finalOrientation);
            for (int i = 0; i < formationSpots.Count; i++)
            {
                if(i != j)
                    formationSpots[i].AskAssignedElementForPath(true, specifiedFinalOrientation, finalOrientation);
            }
        }

        private void AdjustFollowerPointsAlongPath(int referenceIndex, List<Vector3> pathPoints, float distStep, bool specifiedFinalOrientation, Vector3 finalOrientation)
        {

            for (int j = 0; j < formationSpots.Count; j++)
            {
                formationSpots[j].ClearMarksPath();
            }

            int i = pathPoints.Count - 1;

            Vector3 orientation;
            if(specifiedFinalOrientation)
                orientation = finalOrientation;
            else
                orientation = (pathPoints[i] - pathPoints[i - 1]);

            AdjustRelativePositions(pathPoints[i], orientation, referenceIndex);
            
            while (i > 0)
            {
                float d = 0;
                while (i > 0 && (pathPoints[i] - pathPoints[i - 1]).magnitude + d <= distStep)
                {
                    d += (pathPoints[i] - pathPoints[i - 1]).magnitude;
                    if(i > 1)
                        i--;
                }

                if (i > 1 && (pathPoints[i] - pathPoints[i - 1]).magnitude + d > distStep)
                {
                    orientation = pathPoints[i] - pathPoints[i - 1];
                    AdjustRelativePositions(pathPoints[i] + (pathPoints[i - 1] - pathPoints[i]).normalized * (d - distStep), orientation, referenceIndex);
                    i--;
                }

                if(i == 1)
                {
                    for (int j = 0; j < formationSpots.Count; j++)
                    {
                        if (formationSpots[j].assignedElement != null)
                        {
                            //orientation = pathPoints[1] - formationSpots[i].assignedElement.transform.position;
                            formationSpots[j].marksPath.Add(formationSpots[j].assignedElement.transform.position);
                        }
                        else
                        {
                            orientation = pathPoints[1] - pathPoints[0];
                            AdjustRelativePositions(pathPoints[0], orientation, referenceIndex);
                        }
                    }
                    i = 0;
                }

            }
            /*
            if (formationSpots[referenceIndex].assignedElement != null)
            {
                Vector3 ppp = formationSpots[referenceIndex].assignedElement.transform.position;
                AdjustRelativePositions(ppp, pathPoints[0] - ppp, referenceIndex);
            }
            */
            for (int j = 0; j < formationSpots.Count; j++)
            {
                formationSpots[j].marksPath.Reverse();
                //formationSpots[j].PathStuff();
            }


        }

        private void OnDrawGizmosSelected()
        {
            if (EditorApplication.isPlaying)
            {
                for (int i = 0; i < formationSpots.Count; i++)
                {
                    for (int j = 0; j < formationSpots[i].marksPath.Count - 1; j++)
                    {
                        float k = (i == last_reference_index) ? 2f : 1f;
                        Gizmos.DrawCube(formationSpots[i].marksPath[j] + Vector3.up, k * Vector3.one / 2);
                        Gizmos.DrawLine(formationSpots[i].marksPath[j] + Vector3.up, formationSpots[i].marksPath[j + 1]);
                        //Gizmos.DrawWireSphere(formationPositions[i].marksPath[j], formationPositions[i].radius);
                    }
                    /*if (formationSpots[i].marksPath.Count > 0)
                    {
                        float k = (i == 0) ? 2f : 1f;
                        Gizmos.DrawCube(formationSpots[i].marksPath[formationSpots[i].marksPath.Count - 1], Vector3.one * k);
                    }*/
                }
            }
        }

    }
}