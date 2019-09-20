using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.AI.Pathtracking
{

    public struct EnhancedWaypointStruct
    {
        public Vector3 position;
        //public bool hasOrientation;
        public Vector3 orientation;
        public bool lastInPath;
    }

    public class EnhancedNavPath
    {
        //public float halfHeight;
        //private ABPath pathObj;
        private List<Vector3> waypoints = new List<Vector3>();

        public List<Vector3> Special_GetVectorPath()
        {
            //return pathObj.vectorPath;
            return waypoints;
        }

        private int _nextActualWaypointIndex = -1;
        private int NextActualWaypointIndex
        {
            get { return _nextActualWaypointIndex; }
            set { _nextActualWaypointIndex = value; UpdateNextActualWaypoint(); }
        }

        private EnhancedWaypointStruct _nextActualWaypoint;
        public EnhancedWaypointStruct NextActualWaypoint
        {
            get
            {
                return _nextActualWaypoint;
            }
            private set
            {
                _nextActualWaypoint = value;
            }
        }

        public bool IsNextWaypointLast { get { return NextActualWaypoint.lastInPath; } }

        public bool specifiedFinalOrientation { get; private set; }
        private Vector3 lastPointOrientation;

        public int PathCount
        {
            get
            {
                if (ActualPathExists())
                {
                    //return pathObj.vectorPath.Count;
                    return waypoints.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        /*
        public List<Vector3> PathCircleIntersectionPointsOrElseClosest(Vector3 circleCenter, float radius)
        {
            List<Vector3> res = new List<Vector3>();

            if(PathCount > 0)
            {
                for(int i = 0; i < PathCount; i++)
                {

                }
            }

            return res;
        }
        */
        private float remainingDistanceFromNextActualWaypoint = -1;

        public float RemainingDistance(Vector3 startpos)
        {

            if (ActualPathExists())
            {
                return Vector3.Distance(startpos, NextActualWaypoint.position) + remainingDistanceFromNextActualWaypoint;
            }
            else
            {
                remainingDistanceFromNextActualWaypoint = -1;
                return remainingDistanceFromNextActualWaypoint;
            }
            
        }

        private void UpdateNextActualWaypoint()
        {
            if (ActualPathExists())
            {
                /*Vector3 directionToNextWaypoint =
                    (IsNextWaypointLast) ?
                    pathObj.vectorPath[NextActualWaypointIndex] - pathObj.vectorPath[NextActualWaypointIndex - 1]
                    : pathObj.vectorPath[NextActualWaypointIndex + 1] - pathObj.vectorPath[NextActualWaypointIndex];
                    */
                bool b = (NextActualWaypointIndex == PathCount - 1);
                Vector3 orient = (!b) ? waypoints[NextActualWaypointIndex + 1] - waypoints[NextActualWaypointIndex] : lastPointOrientation;
                NextActualWaypoint = new EnhancedWaypointStruct
                {
                    //position = pathObj.vectorPath[NextActualWaypointIndex]/* + 0.5f * Vector3.up * halfHeight*/,
                    position = waypoints[NextActualWaypointIndex],
                    orientation = orient,
                    lastInPath = b
                };
                if (NextActualWaypointIndex >= 1)
                {
                    //remainingDistanceFromNextActualWaypoint -= Vector3.Distance(pathObj.vectorPath[NextActualWaypointIndex], pathObj.vectorPath[NextActualWaypointIndex - 1]);
                    remainingDistanceFromNextActualWaypoint -= Vector3.Distance(waypoints[NextActualWaypointIndex], waypoints[NextActualWaypointIndex - 1]);
                }
            }
        }

        public void SetFirstWaypoint()
        {
            NextActualWaypointIndex = 0;
            //orientingAtLastWaypoint = false;
            UpdateNextActualWaypoint();
        }

        public void SetWaypointClosestToPosition(Vector3 position)
        {
            int ind = -1;

            float min = -1;
            var pc = PathCount;
            for (int i = 0; i < pc; i++)
            {
                //float f = (pathObj.vectorPath[i] - position).magnitude;
                float f = (waypoints[i] - position).magnitude;
                if (min < 0 || f < min)
                {
                    min = f;
                    ind = i;
                }
            }

            NextActualWaypointIndex = ind;
            UpdateNextActualWaypoint();
        }

        /// <summary>
        /// Updates situation when the agent arrived has reached "next" waypoint;
        /// </summary>
        /// <returns></returns>
        public void ArrivedAtWaypoint()
        {
            var pc = PathCount;
            if (NextActualWaypointIndex < pc - 1)
            {
                NextActualWaypointIndex++;
                if (IsNextWaypointLast)
                {
                    //orientingAtLastWaypoint = true;
                }
            }
            else// if (orientingAtLastWaypoint)
            {
                NextActualWaypointIndex = -1;
                //signalPathNewOrEnded = true;
                //orientingAtLastWaypoint = false;
            }
        }

        public void OnPathReturned(Path p, bool add, bool specifiedFinalOrientation, Vector3 finalOrientation)// Vector3 endpos, bool okayLookAt, Vector3 toLookAt)*/
        {
            if (!add)
            {
                //pathObj = p as ABPath;
                waypoints.Clear();

                waypoints = p.vectorPath;

                remainingDistanceFromNextActualWaypoint = p.GetTotalLength();

                SetFirstWaypoint();
            }
            else
            {
                //var l = p.vectorPath; l.RemoveAt(0);
                //pathObj.vectorPath.AddRange(l);
                //var ll = p.path; ll.RemoveAt(0);
                //pathObj.path.AddRange(ll);

                //pathObj = ABPath.FakePath(pathObj.vectorPath, pathObj.path);
                waypoints.AddRange(p.vectorPath);

                remainingDistanceFromNextActualWaypoint += p.GetTotalLength();

                SetFirstWaypoint();
            }


            this.specifiedFinalOrientation = specifiedFinalOrientation;
            if(this.specifiedFinalOrientation)
                lastPointOrientation = finalOrientation;
            else
                lastPointOrientation = waypoints[PathCount - 1] - waypoints[PathCount - 2];

            //if (pathObj != null)
            //    pathObj.Release(this);
            //pathObj = p as ABPath;
            //pathObj.Claim(this);
            //SetFirstWaypoint();
            //signalPathNewOrEnded = true;

                /*if (okayLookAt)
                {

                    //pathObj.vectorPath.Add(endpos - (toLookAt - endpos).normalized * halfHeight * 8);
                    //pathObj.path.Add(pathObj.path[pathObj.path.Count - 1]);
                    //pathObj.vectorPath.Add(endpos + (toLookAt - endpos).normalized * halfHeight * 4);
                    //pathObj.path.Add(pathObj.path[pathObj.path.Count - 1]);

                    //DesiredOrientationAtLastWaypoint = (toLookAt - endpos).normalized;
                }
                else
                {
                    var pc = PathCount;
                    //DesiredOrientationAtLastWaypoint = (pathObj.vectorPath[pc - 1] - pathObj.vectorPath[pc - 2]).normalized;
                }
                */
                //remainingDistanceFromNextActualWaypoint = pathObj.GetTotalLength();

                //}
        }
        public bool ActualPathExists()
        {
            return NextActualWaypointIndex != -1;
        }
    }
}
