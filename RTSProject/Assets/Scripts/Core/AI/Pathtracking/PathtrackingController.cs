using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.AI.Pathtracking
{
    public abstract class PathtrackingController : MonoBehaviour
    {
        public bool paused { get; protected set; }

        //public event Action<int, List<Vector3>, float, bool, Vector3> newPathEvent;

        public EnhancedNavPath currentPath;

        public bool GoingBackwards;

        public virtual void Unpause()
        { paused = true; }

        public virtual void Pause()
        { paused = false; }

        public virtual bool HasPath()
        {
            return false;
        }

        /*
        protected void InvokeNewPathEvent(int leaderIndex, List<Vector3> pathPoints, float distStep, bool specifiedFinalOrientation, Vector3 finalOrientation)
        {
            if(newPathEvent != null)
                newPathEvent.Invoke(leaderIndex, pathPoints, distStep, specifiedFinalOrientation, finalOrientation);
        }
        */
    }
}
