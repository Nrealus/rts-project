using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.AI.Pathtracking;
using Pathfinding;
using UnityEditor;

namespace Core.AI.Formation
{

    public class FormationBaseScrapped : MonoBehaviour
    {

        private float spacingScale = 4f;

        private FormationSpot[] formationSpots;

        public Vector3? anchorPoint;

        public List<PathtrackingController> unitsInThisFormation;
        private int referenceIndex;

        private void Start()
        {
            /*formationSpots = new FormationSpot[] 
            {
                //new FormationSpot() { desiredTheoriticalRelativePosition = new Vector3(-2f, 0, 0), radius = 1.5f, active = true },
                new FormationSpot() { offset = new Vector3(0, 0, 0), radius = 1.5f, active = true },
                new FormationSpot() { offset = new Vector3(-1, 0, 0), radius = 1.5f, active = true },
                new FormationSpot() { offset = new Vector3(1, 0, 0), radius = 1.5f, active = true },
                //new FormationSpot() { desiredTheoriticalRelativePosition = new Vector3(2f, 0, 0), radius = 1.5f, active = true }
            };
            */
            SetUnitsToSuitableFormSpotIndices();

        }

        /*
        public void UpdateAdjustedPositions()
        {
            for (int i = 0; i < formationPositions.Length; i++)
            {
                formationPositions[i].neighbourCount = 0;
                formationPositions[i].compensateColl = Vector3.zero;
                formationPositions[i].compensateSep = Vector3.zero;

                Vector3 theoreticalPos = anchorPoint + formationPositions[i].desiredTheoriticalRelativePosition * spacingScale;

                NNInfo info = AstarPath.active.GetNearest(theoreticalPos);
                Vector3 closest = info.position;
                formationPositions[i].compensateColl = (closest - theoreticalPos);

                formationPositions[i].collAdjustedPosition = theoreticalPos + formationPositions[i].compensateColl;

                formationPositions[i].adjustedPosition = formationPositions[i].collAdjustedPosition;
            }

            for (int i = 0; i < formationPositions.Length; i++)
            {
                for (int j = 0; j < formationPositions.Length; j++)
                {
                    Vector3 v = formationPositions[j].adjustedPosition - formationPositions[i].adjustedPosition;
                    float f = formationPositions[i].radius + formationPositions[j].radius - v.magnitude;
                    if (f > 0)
                    {
                        if (formationPositions[i].compensateColl.magnitude - formationPositions[j].compensateColl.magnitude > 0)
                        {
                            formationPositions[j].compensateSep = -v.normalized * f;
                        }
                        formationPositions[j].adjustedPosition += formationPositions[j].compensateSep;

                    }

                }
            }

        }
        */

        public void SetUnitsToSuitableFormSpotIndices()
        {
            referenceIndex = 0;

            if (unitsInThisFormation.Count > 0)
            {
                /*for (int i = 0; i < formationSpots.Length; i++)
                {
                    formationSpots[i].desiredTheoriticalRelativePosition 
                }*/
                for (int i = 0; i < formationSpots.Length; i++)
                {
                    /*if (formationSpots[i].active)
                    {
                        formationSpots[i].associatedUnit = unitsInThisFormation[i];
                        formationSpots[i].assignedUnit.newPathEvent += AdjustFollowerPointsAlongPath;
                    }*/
                }
            }

            /*
            for (int i = 0; i < formationSpots.Length; i++)
            {
                for (int j = 0; j < unitsInThisFormation.Count; j++)
                {
                    
                }
            }
            */
        }

        public void AdjustRelativePositions(Vector3 anchorPoint, Vector3 orientation)
        {
            Quaternion orientationRotationQuat = Quaternion.LookRotation(orientation, Vector3.up);

            for (int i = 0; i < formationSpots.Length; i++)
            {
                Vector3 theoreticalRealPos = anchorPoint + orientationRotationQuat * formationSpots[i].offset * spacingScale;

                NNInfo info = AstarPath.active.GetNearest(theoreticalRealPos);
                Vector3 closest = info.position;

                //formationSpots[i].adjustedPosition = closest;
                formationSpots[i].marksPath.Add(closest);
            }
        }

        public void AdjustFollowerPointsAlongPath(int leaderIndex, List<Vector3> pathPoints, float distStep, bool specifiedFinalOrientation, Vector3 finalOrientation)
        {

            //SetUnitsToSuitableFormSpotIndices();
            /*
            for (int j = 0; j < formationSpots.Length; j++)
            {
                formationSpots[j].ClearMarksPath();
            }
            */
            int i = pathPoints.Count - 1;
            Vector3 orientation;
            /*if(specifiedFinalOrientation)
                orientation = finalOrientation;
            else
                */orientation = (pathPoints[i] - pathPoints[i - 1]);
            AdjustRelativePositions(pathPoints[i], orientation/*Vector3.forward*/);
            //Vector3 orientationRightNormal = Quaternion.AngleAxis(-90, Vector3.up) * orientation;
            /*while (i > 0)
            {
                float d = 0;
                while (i > 0 && (pathPoints[i] - pathPoints[i - 1]).magnitude + d <= distStep)
                {
                    d += (pathPoints[i] - pathPoints[i - 1]).magnitude;
                    i--;
                }

                if (i > 0 && (pathPoints[i] - pathPoints[i - 1]).magnitude + d > distStep)
                {
                    orientation = pathPoints[i] - pathPoints[i - 1];
                    AdjustRelativePositions(pathPoints[i] + (pathPoints[i - 1] - pathPoints[i]).normalized * (d - distStep), orientation);
                    i--;
                }
            }
            */
            for (int j = 0; j < formationSpots.Length; j++)
            {
                formationSpots[j].marksPath.Reverse();
                //formationSpots[j].PathStuff();
            }

            

        }

        public void OnDrawGizmosSelected()
        {
            if (EditorApplication.isPlaying)
            {
                for (int i = 0; i < formationSpots.Length; i++)
                {
                    for (int j = 0; j < formationSpots[i].marksPath.Count - 1; j++)
                    {
                        Gizmos.DrawCube(formationSpots[i].marksPath[j] + Vector3.up, Vector3.one / 2);
                        Gizmos.DrawLine(formationSpots[i].marksPath[j] + Vector3.up, formationSpots[i].marksPath[j + 1]);
                        //Gizmos.DrawWireSphere(formationPositions[i].marksPath[j], formationPositions[i].radius);
                    }

                    /*if (formationSpots[i].marksPath.Count > 0)
                    {
                        Gizmos.DrawCube(formationSpots[i].marksPath[formationSpots[i].marksPath.Count - 1], Vector3.one);
                    }*/
                }
            }
        }
    }
}