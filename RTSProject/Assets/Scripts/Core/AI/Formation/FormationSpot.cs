using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.AI.Pathtracking;

namespace Core.AI.Formation
{
    public class FormationSpot
    {
        public Vector3 offset { get; private set; }
        public float radius { get; private set; }

        public List<Vector3> marksPath;
        public int nextMarkIndex;

        public FormationElement assignedElement { get; private set; }

        public FormationBase MyFormation { get; private set; }

        public FormationSpot(Vector3 offset, float radius, FormationBase formation)
        {
            this.offset = offset;
            this.radius = radius;
            MyFormation = formation;

            marksPath = new List<Vector3>();
        }
        
        internal void ClearMarksPath()
        {
            nextMarkIndex = 0;
            marksPath.Clear();
        }
        /*
        public void AskPathtrackingForPath(Vector3 startPos, Vector3 endPos)
        {
            assignedUnit.AskPathTo(startPos, endPos, false, Vector3.zero);
        }
        */
        internal void AskAssignedElementForPath(bool followFormationPath, bool specifiedFinalOrientation, Vector3 finalOrientation)
        {
            if (assignedElement != null)
            {
                //assignedElement.AskForPath(/*assignedElement.transform.position*/marksPath[0], marksPath[/*nextMarkIndex*/marksPath.Count - 1], followFormationPath);
                assignedElement.AskForPath(marksPath, followFormationPath, specifiedFinalOrientation, finalOrientation);
            }
        }

        internal void AssignElement(FormationElement element)
        {
            if (element != null)
            {
                assignedElement = element;
                assignedElement.MyOccupiedFormationSpot = this;
            }
        }

        internal void UnassignElement()
        {
            if (assignedElement != null)
            {
                assignedElement = null;

            }
        }

    }
}