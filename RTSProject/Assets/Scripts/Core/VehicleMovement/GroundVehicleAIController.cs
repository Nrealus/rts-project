using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using GlobalManagers;
using UtilsAndExts;
using System.Linq;
using Gamelogic.Extensions;

namespace Core.VehicleMovement
{

    public class GroundVehicleAIController : MonoBehaviour
    {

        public struct InternalPath
        {
            public float halfHeight;
            private ABPath pathObj;

            private int _nextActualWaypointIndex;
            private int NextActualWaypointIndex
            {
                get { return _nextActualWaypointIndex - 1; }
                set { _nextActualWaypointIndex = value + 1; UpdateNextActualWaypoint(); }
            }

            private Vector3 _nextActualWaypoint;
            public Vector3 NextActualWaypoint
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

            public Vector3 LastActualWaypoint { get { return pathObj.endPoint +0.5f * Vector3.up * halfHeight; } }

            public bool IsNextWaypointLast { get { return NextActualWaypoint == LastActualWaypoint; } }

            public Vector3 DesiredOrientationAtLastWaypoint { get; private set; }
            public bool orientingAtLastWaypoint;

            public int PathCount
            {
                get
                {
                    if (ActualPathExists())
                    {
                        return pathObj.vectorPath.Count;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            private void UpdateNextActualWaypoint()
            {
                if (ActualPathExists())
                    NextActualWaypoint = pathObj.vectorPath[NextActualWaypointIndex] + 0.5f * Vector3.up * halfHeight;
            }

            public void SetFirstWaypoint()
            {
                NextActualWaypointIndex = 0;
                orientingAtLastWaypoint = false;
            }

            public void SetWaypointClosestToPosition(Vector3 position)
            {
                int ind = -1;

                float min = -1;
                var pc = PathCount;
                for (int i = 0; i < pc; i++)
                {
                    float f = (pathObj.vectorPath[i] - position).magnitude;
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
            /// returns if the "previous" waypoint was the first or last one in the path
            /// </summary>
            /// <returns></returns>
            public bool ArrivedAtWaypoint()
            {
                var pc = PathCount;
                if (NextActualWaypointIndex <= pc - 2)
                {
                    NextActualWaypointIndex++;
                    return (NextActualWaypointIndex - 1 == 0) || (NextActualWaypointIndex == pc - 2);
                }
                else
                {
                    //pathObj.Release(this);
                    NextActualWaypointIndex = -1;
                    return false;
                }
            }

            public void OnPathReturned(Path p, bool add, Vector3 endpos, bool okayLookAt, Vector3 toLookAt)
            {
                if (!add)
                {
                    //if (pathObj != null)
                    //    pathObj.Release(this);
                    pathObj = p as ABPath;
                    //pathObj.Claim(this);
                    SetFirstWaypoint();

                    if (okayLookAt)
                    {
                        /*
                        pathObj.vectorPath.Add(endpos - (toLookAt - endpos).normalized * halfHeight * 8);
                        pathObj.path.Add(pathObj.path[pathObj.path.Count - 1]);
                        pathObj.vectorPath.Add(endpos + (toLookAt - endpos).normalized * halfHeight * 4);
                        pathObj.path.Add(pathObj.path[pathObj.path.Count - 1]);
                        */
                        DesiredOrientationAtLastWaypoint = (toLookAt - endpos).normalized;
                    }
                    else
                    {
                        var pc = PathCount;
                        DesiredOrientationAtLastWaypoint = (pathObj.vectorPath[pc - 1] - pathObj.vectorPath[pc - 2]).normalized;
                    }

                }
                else
                {
                    if (!ActualPathExists())
                    {
                        //if(pathObj != null)
                        //    pathObj.Release(this);
                        pathObj = p as ABPath;
                        //pathObj.Claim(this);
                        SetFirstWaypoint();

                        if (okayLookAt)
                        {
                            /*
                            pathObj.vectorPath.Add(endpos - (toLookAt - endpos).normalized * halfHeight * 8);
                            pathObj.path.Add(pathObj.path[pathObj.path.Count - 1]);
                            pathObj.vectorPath.Add(endpos + (toLookAt - endpos).normalized * halfHeight * 4);
                            pathObj.path.Add(pathObj.path[pathObj.path.Count - 1]);
                            */
                            DesiredOrientationAtLastWaypoint = (toLookAt - endpos).normalized;
                        }

                    }
                    else
                    {
                        var newVectorPath = pathObj.vectorPath.Concat(p.vectorPath.Skip(1)).ToList();
                        var newNodePath = pathObj.path.Concat(p.path.Skip(1)).ToList();
                        if (okayLookAt)
                        {
                            /*
                            newVectorPath.Add(endpos - (toLookAt - endpos).normalized * halfHeight * 8);
                            newNodePath.Add(newNodePath[newNodePath.Count - 1]);
                            pathObj.vectorPath.Add(endpos + (toLookAt - endpos).normalized * halfHeight * 4);
                            pathObj.path.Add(pathObj.path[pathObj.path.Count - 1]);
                            */
                            DesiredOrientationAtLastWaypoint = (toLookAt - endpos).normalized;
                        }

                        pathObj = ABPath.FakePath(newVectorPath, newNodePath);
                    }
                }
            }
            public bool ActualPathExists()
            {
                return NextActualWaypointIndex != -1;
            }
            /*
            private Vector3 ComputeDesiredOrientationAtWp(int wpIndex, Vector3 transformPosition, Vector3 transformUp)
            {
                if (wpIndex + 1 < actualWaypoints.Count)
                    return Vector3.ProjectOnPlane(actualWaypoints[wpIndex + 1] - actualWaypoints[wpIndex], transformUp);
                else
                    return Vector3.ProjectOnPlane(actualWaypoints[wpIndex] - transformPosition, transformUp);
            }
            */
        }

        public InternalPath currentPath;

#pragma warning disable CS0649

        public Rigidbody mainRigidBody;
        [SerializeField] private float halfHeight;
        [SerializeField] private float halfWidth;

        private Terrain terrain;
        public bool onGround;

        private bool reversing = false;
        private bool goingBackwards = false;

        [SerializeField] private PID angleController;
        [SerializeField] private float angleErrorClampValue;
        [SerializeField] private AnimationCurve anglePIDKmultFromThrottleRatio;

        [SerializeField] private PID angularVelocityController;

        [SerializeField] private PID velocityController;

        [SerializeField] private float maxVelocity;

        [SerializeField] private float _maxAngularVelocity;
        [SerializeField] private float maxAngularVelocity { get { return _maxAngularVelocity; } set { _maxAngularVelocity = value; mainRigidBody.maxAngularVelocity = value; } }
        [SerializeField] private float torqueFactor;

        [SerializeField] private float accelRate;
        [SerializeField] private float brakeRate;
        [SerializeField] private float minThrottle;
        [SerializeField] private float maxThrottle;

        [SerializeField] private float proximityDistance;

        [SerializeField] private float turnRadiusLowBound;
        [SerializeField] private float turnRadiusHighBound;

        [SerializeField] private float maxDistanceBeforeResettingToClosestWp;

        [SerializeField] private float maxAngleNoReverse;
        [SerializeField] private float maxAngleReverse;

        [SerializeField] private float angleMarginOrientationAtLastWp;

        [SerializeField] private float slipReductionFactor;

        private float throttle = 0;
        private float targetThrottle = 0;

        private Vector3 velocity;
        private Vector3 angularVelocity;
        private float velocityMagnitude;

        private float dt;

        void Start()
        {
            currentPath = new InternalPath() { halfHeight = halfHeight};

            terrain = GameManager.Instance.currentMainHandler.terrainHandler.MyTerrain;

            StartCoroutine(TestOnGround(1f));
        }

        void Update()
        {
            if (GameManager.Instance.IM.IsMouseButtonHeld[GameManager.Instance.IM.LMB])
            {
                goingBackwards = true;
            }
            else
            {
                goingBackwards = false;
            }

            OnRightReleaseSetDestination();

            OnRightHoldChoosingLookAtDirection();
        }

        private void UpdatePathtrackingPhysics()
        {
            float throttleRate = 0;
            if (targetThrottle - throttle > 0)
            {
                throttleRate = accelRate * dt;
            }
            else if (targetThrottle - throttle < 0)
            {
                throttleRate = -brakeRate * dt;
            }

            throttle = Mathf.Clamp(throttle + throttleRate, minThrottle - throttleRate, targetThrottle);

            /*
            if (throttle < 0)
                Debug.Log("brakkkkeee : " + throttle);
            */

            if (onGround)
            {
                mainRigidBody.AddRelativeForce(-1 * mainRigidBody.transform.up, ForceMode.Acceleration);

                if (currentPath.ActualPathExists())
                {

                    targetThrottle = (!reversing) ? maxThrottle : minThrottle;

                    var dirProjected = Vector3.ProjectOnPlane(100 * currentPath.NextActualWaypoint - 100 * mainRigidBody.worldCenterOfMass, mainRigidBody.transform.up);
                    dirProjected.Normalize();

                    Vector3 dir = (currentPath.orientingAtLastWaypoint) ? currentPath.DesiredOrientationAtLastWaypoint : dirProjected;

                    Vector3 forward = (!goingBackwards) ? mainRigidBody.transform.forward : -mainRigidBody.transform.forward;

                    //float angleError = Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle);
                    float angleError = Mathf.Clamp(-angleErrorClampValue, Vector3.SignedAngle(forward, dir, mainRigidBody.transform.up), angleErrorClampValue);
                    var tratio = (throttle > 0) ? throttle / maxThrottle : -throttle / minThrottle;
                    var mult = anglePIDKmultFromThrottleRatio.Evaluate(tratio);
                    float torqueCorrectionForAngle = angleController.GetOutput(angleError, dt, mult, mult, mult);

                    float angularVelocityError = Mathf.Clamp(Vector3.Dot(angularVelocity, mainRigidBody.transform.up), - maxAngularVelocity/2, maxAngularVelocity/2);
                    float torqueCorrectionForAngularVelocity = angularVelocityController.GetOutput(angularVelocityError, dt, mult, mult, mult);

                    float velocityError = maxVelocity - velocityMagnitude;
                    float forceCorrectionForVelocity = velocityController.GetOutput(velocityError, dt, mult, mult, mult);

                    Vector3 torque = mainRigidBody.transform.up;

                    torque *= torqueFactor;
                    torque *= torqueCorrectionForAngle + torqueCorrectionForAngularVelocity;

                    var force = mainRigidBody.transform.forward;
                    force.y = 0;
                    force.Normalize();

                    force *= throttle;
                    if (velocityError / maxVelocity < 0.08)
                    {
                        force *= Mathf.Min(forceCorrectionForVelocity, 0.99f);
                    }
                    /*           
                    if (torque.magnitude > 10000)
                    {
                        torque = Vector3.zero;
                        mainRigidBody.velocity = Vector3.zero;
                    }
                    if (force.magnitude > 10000)
                    {
                        force = Vector3.zero;
                        mainRigidBody.velocity = Vector3.zero;
                    }
                    */
                    mainRigidBody.AddTorque(torque, ForceMode.Acceleration);

                    mainRigidBody.AddForce(force, ForceMode.Acceleration);

                }
                else
                {
                    targetThrottle = 0;
                }
                var slipReductionForce = -slipReductionFactor * Vector3.Project(velocity, mainRigidBody.transform.right) / dt; // slip / traction reduction
                /*
                if (slipReductionForce.magnitude > 10000)
                {
                    slipReductionForce = Vector3.zero;
                    mainRigidBody.velocity = Vector3.zero;
                }
                */
                mainRigidBody.AddForce(slipReductionForce, ForceMode.Acceleration);
            }
            else
            {
                targetThrottle = 0;
            }
        }

        private void UpdatePathtrackingLogic()
        {
            if (currentPath.ActualPathExists())
            {
                                
                //currentPath.SetFirstWaypoint();

                //float turnRadius = Mathf.Clamp(mainRigidBody.velocity.magnitude/maxAngularVelocity, halfWidth*2, maxVelocity/maxAngularVelocity);
                var tratio = (throttle > 0) ? throttle / maxThrottle : -throttle / minThrottle;
                float turnRadius = Mathf.Lerp(turnRadiusLowBound, turnRadiusHighBound, tratio);

                var dir = Vector3.ProjectOnPlane(currentPath.NextActualWaypoint - transform.position, transform.up);

                float distance = dir.magnitude;

                if (distance > maxDistanceBeforeResettingToClosestWp)
                {
                    currentPath.SetWaypointClosestToPosition(transform.position);
                    dir = Vector3.ProjectOnPlane(currentPath.NextActualWaypoint - transform.position, transform.up);
                    distance = dir.magnitude;
                }

                float angle;

                bool isWpLast = currentPath.IsNextWaypointLast;
                if (!isWpLast)
                    angle = Vector3.SignedAngle(transform.forward, dir, transform.up);
                else
                {
                    angle = Vector3.SignedAngle(transform.forward, currentPath.DesiredOrientationAtLastWaypoint, transform.up);
                    goingBackwards = false;
                }
               
                if (!reversing)
                {

                    if ((distance <= proximityDistance) || (distance < turnRadiusLowBound && Mathf.Abs(angle) < 2 * maxAngleNoReverse))
                    {
                        if (!isWpLast)
                        {
                            currentPath.orientingAtLastWaypoint = false;
                            if (currentPath.ArrivedAtWaypoint())
                                throttle = 0; //if the waypoint is the first or second last
                        }
                        else if (distance <= proximityDistance * 2 && Mathf.Abs(angle) < angleMarginOrientationAtLastWp)
                        {
                            currentPath.orientingAtLastWaypoint = false;
                            currentPath.ArrivedAtWaypoint();
                            throttle = 0;
                        }
                        else
                        {
                            currentPath.orientingAtLastWaypoint = true;
                            reversing = true;
                            throttle = 0;
                        }
                    }
                    else if (distance < turnRadius && Mathf.Abs(angle) > 2 * maxAngleNoReverse)
                    {
                        currentPath.orientingAtLastWaypoint = isWpLast;
                        reversing = true;
                        throttle = 0;
                    }
                }
                else
                {
                    if (distance <= proximityDistance || (distance < turnRadiusLowBound && (Mathf.Abs(angle) < maxAngleReverse || 180 - Mathf.Abs(angle) < 2 * maxAngleReverse)))
                    {
                        if (!isWpLast)
                        {
                            currentPath.orientingAtLastWaypoint = false;
                            if (currentPath.ArrivedAtWaypoint())
                                throttle = 0; //if the waypoint is the first or second last
                        }
                        else if (distance <= proximityDistance * 2 && Mathf.Abs(angle) < angleMarginOrientationAtLastWp * (maxAngleReverse/maxAngleNoReverse))
                        {
                            currentPath.orientingAtLastWaypoint = false;
                            currentPath.ArrivedAtWaypoint();
                            throttle = 0;
                        }
                        else
                        {
                            currentPath.orientingAtLastWaypoint = isWpLast;
                            reversing = false;
                            throttle = 0;
                        }
                    }
                    else if (distance > turnRadius && Mathf.Abs(angle) < 2 * maxAngleReverse)
                    {
                        currentPath.orientingAtLastWaypoint = false;
                        reversing = false;
                        throttle = 0;
                    }
                }
            }
        }

        private IEnumerator TestOnGround(float secswait)
        {
            while (true)
            {
                var ray = new Ray(mainRigidBody.worldCenterOfMass, -halfHeight * mainRigidBody.transform.up);
                RaycastHit hit = new RaycastHit();
                Physics.Raycast(ray, out hit, 1f);

                onGround = (hit.collider != null);

                yield return new WaitForSeconds(secswait);
            }
        }

        void FixedUpdate()
        {
            velocity = mainRigidBody.velocity;
            velocityMagnitude = velocity.magnitude;
            angularVelocity = mainRigidBody.angularVelocity;

            dt = Time.fixedDeltaTime;

            UpdatePathtrackingPhysics();

            UpdatePathtrackingLogic();
        }

        private void AskPathTo(Vector3 startPos, Vector3 endPos, bool add, Vector3 endpos, bool okayLookAt, Vector3 toLookAt)
        {
            Path pp = GetComponent<Seeker>().StartPath(startPos, endPos, (Path p) => currentPath.OnPathReturned(p, add, endpos, okayLookAt, toLookAt));
            pp.BlockUntilCalculated();
        }

        private Vector3 endpos;
        private bool okayEndpos;
        private Vector3 toLookAt;
        private bool okayLookAt;
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
                if (add && currentPath.ActualPathExists())
                {
                    startpos = currentPath.LastActualWaypoint;
                }
                AskPathTo(startpos, endpos, add, endpos, okayLookAt, toLookAt);

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

                if (hit.collider != null && (hit.point-endpos).magnitude > 1)
                {
                    okayLookAt = true;
                    toLookAt = hit.point;
                }
                else
                {
                    okayLookAt = false;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if(currentPath.ActualPathExists())
            {
                Gizmos.DrawSphere(currentPath.NextActualWaypoint, 0.5f);
                Gizmos.DrawSphere(currentPath.LastActualWaypoint, 1f);
            }
        }

    }
}