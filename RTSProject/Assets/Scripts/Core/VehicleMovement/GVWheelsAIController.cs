using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using GlobalManagers;
using UtilsAndExts;
using System.Linq;
using Gamelogic.Extensions;
using Core.Orders;

namespace Core.VehicleMovement
{

    public class GVWheelsAIController : MonoBehaviour
    {
        [System.Serializable]
        public class AxleInfo
        {
            public WheelCollider leftWheel;
            public WheelCollider rightWheel;
            public bool motor; // is this wheel attached to motor?
            public bool steering; // does this wheel apply steer angle?
        }

        public class InternalPath
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

            public Vector3 LastActualWaypoint { get { return pathObj.endPoint + 0.5f * Vector3.up * halfHeight; } }

            public bool IsNextWaypointLast { get { return NextActualWaypoint == LastActualWaypoint; } }

            //public Vector3 DesiredOrientationAtLastWaypoint { get; private set; }
            //public bool orientingAtLastWaypoint;

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
                //orientingAtLastWaypoint = false;
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
            /// Returns true when the last 
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
                    //orientingAtLastWaypoint = false;
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
                        //DesiredOrientationAtLastWaypoint = (toLookAt - endpos).normalized;
                    }
                    else
                    {
                        var pc = PathCount;
                        //DesiredOrientationAtLastWaypoint = (pathObj.vectorPath[pc - 1] - pathObj.vectorPath[pc - 2]).normalized;
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
                            //DesiredOrientationAtLastWaypoint = (toLookAt - endpos).normalized;
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
                            //DesiredOrientationAtLastWaypoint = (toLookAt - endpos).normalized;
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
        public bool onGround { get; private set; }

        private int reversing = 0;
        private bool _goingBackwards = false;
        private bool GoingBackwards
        {
            get
            { return _goingBackwards; }
            set
            {
                if (_goingBackwards != value)
                {
                    throttle = 0;
                    targetBrkThrt = Mathf.Min(2 * velocityMagnitude / maxVelocity, 1);
                    _goingBackwards = value;
                    reversing = 2;
                }
            }
        }

        //[SerializeField] private PID angleController;
        //[SerializeField] private PID angularVelocityController;
        [SerializeField] private PID velocityController;

        public List<AxleInfo> axleInfos;
        [SerializeField] private float maxMotorTorque;
        [SerializeField] private float maxBrakeTorque;
        [SerializeField] private float maxSteeringAngle;

        [SerializeField] private float _maxVelocity;
        [SerializeField] private float _maxReverseVelocity;
        private float maxVelocity { get { if (GoingBackwards && reversing != 2) return _maxReverseVelocity; else return _maxVelocity; } } 

        [SerializeField] private float _maxAngularVelocity;
        private float maxAngularVelocity { get { return _maxAngularVelocity; } set { _maxAngularVelocity = value; mainRigidBody.maxAngularVelocity = value; } }

        [SerializeField] private float accelRate;
        [SerializeField] private float reverseAccel;
        [SerializeField] private float minThrottle;
        [SerializeField] private float maxThrottle;

        [SerializeField] private float proximityDistance;

        [SerializeField] private float reversingRadiusLowBound;
        [SerializeField] private float reversingRadiusHighBound;

        [SerializeField] private float maxDistanceBeforeResettingToClosestWp;

        [SerializeField] private float maxAngleNoReverse;
        [SerializeField] private float maxAngleReverse;

        [SerializeField] private float angleMarginOrientationAtLastWp;

        private float throttle = 0;
        private float targetThrottle = 0;
        private float targetBrkThrt;

        private float motor;
        private float brake;
        private float steering;

        private Vector3 velocity;
        private float angularVelocityMagnitude;
        private float velocityMagnitude;

        private Vector3 forward { get { /*if (!GoingBackwards)*/ return mainRigidBody.transform.forward;/* else return -mainRigidBody.transform.forward; */} }

        private float dt;

        void Start()
        {
            maxAngularVelocity = _maxAngularVelocity;

            currentPath = new InternalPath() { halfHeight = halfHeight};

            terrain = GameManager.Instance.currentMainHandler.terrainHandler.MyTerrain;

            StartCoroutine(TestOnGround(1f));
        }

        void Update()
        {
            if (GameManager.Instance.IM.IsMouseButtonHeld[GameManager.Instance.IM.LMB])
            {
                GoingBackwards = true;
            }
            else
            {
                GoingBackwards = false;
            }

            OnRightReleaseSetDestination();

            OnRightHoldChoosingLookAtDirection();
        }

        private void UpdatePhysicsLogic()
        {
            float throttleRate = 0;
            if (targetThrottle - throttle > 0)
            {
                throttleRate = accelRate * dt;
            }
            else if (targetThrottle - throttle < 0)
            {
                throttleRate = -reverseAccel * dt;
            }

            throttle = Mathf.Clamp(throttle + throttleRate, minThrottle, maxThrottle);

            if (targetThrottle == 0 && velocityMagnitude != 0)
            {
                targetBrkThrt = 1;
            }

            targetBrkThrt = Mathf.Clamp(targetBrkThrt - 1 * dt, 0, 1);
            brake = maxBrakeTorque * targetBrkThrt;

            if (GoingBackwards && reversing != 2)
                brake *= -1;

            if (onGround)
            {
                motor = maxMotorTorque * throttle;
                if (currentPath.ActualPathExists())
                {

                    targetThrottle = ((!GoingBackwards && reversing != 0) || (GoingBackwards && reversing != 2)) ? minThrottle : maxThrottle;

                    var dirProjected = Vector3.ProjectOnPlane(100 * currentPath.NextActualWaypoint - 100 * mainRigidBody.worldCenterOfMass, mainRigidBody.transform.up);
                    dirProjected.Normalize();

                    Vector3 dir = dirProjected;

                    var ff = (GoingBackwards) ? -forward : forward;
                    float angleError = Mathf.Clamp(Vector3.SignedAngle(ff, dir, mainRigidBody.transform.up), -maxSteeringAngle, maxSteeringAngle);
                    float velocityError = maxVelocity - velocityMagnitude;

                    float mult = 1;
                    float velocityCorrectionForMotor = velocityController.GetOutput(velocityError, dt, mult, mult, mult);

                    steering = angleError * Mathf.Sign(throttle);

                    if (velocityError / maxVelocity < 0.1)
                    {
                        motor *= Mathf.Min(velocityCorrectionForMotor, 0.95f);
                    }
                }
                else
                {
                    targetThrottle = 0;
                }
            }
            else
            {
                targetThrottle = 0;
            }
        }

        private void UpdateSteeringDecisionLogic()
        {
            //Debug.Log(targetThrottle);
            //Debug.Log(motor);

            if (currentPath.ActualPathExists())
            {
                //currentPath.SetFirstWaypoint();

                var dir = Vector3.ProjectOnPlane(currentPath.NextActualWaypoint - transform.position, transform.up);

                float distance = dir.magnitude;

                if (distance > maxDistanceBeforeResettingToClosestWp)
                {
                    currentPath.SetWaypointClosestToPosition(transform.position);
                    dir = Vector3.ProjectOnPlane(currentPath.NextActualWaypoint - transform.position, transform.up);
                    distance = dir.magnitude;
                }

                //float mar = (!GoingBackwards) ? maxAngleReverse : maxAngleNoReverse;
                //float manor = (!GoingBackwards) ? maxAngleNoReverse : maxAngleReverse;
                float mar = maxAngleReverse;
                float manor = maxAngleNoReverse;

                float angle;
                angle = Vector3.SignedAngle(forward, dir, transform.up);

                float abs180Angle = Mathf.Abs(angle);
                if (GoingBackwards)
                    abs180Angle = 180 - abs180Angle;

                var r = Mathf.Sin(Mathf.Deg2Rad * Mathf.Min(abs180Angle, 100/*180 - 2 * mar*/));
                float turnRadius = Mathf.Lerp(reversingRadiusLowBound, reversingRadiusHighBound, r);

                if (reversing == 0)
                {
                    if (GoingBackwards && abs180Angle > 2 * mar && distance > reversingRadiusLowBound)
                    {
                        SetSteeringLogicState(2, 0, 1);
                        currentPath.SetWaypointClosestToPosition(transform.position);
                    }
                    else if ((distance <= proximityDistance) || (distance < turnRadius/*reversingRadiusLowBound*/ && abs180Angle < 2 * manor))
                    {
                        if(currentPath.IsNextWaypointLast)
                            SetSteeringLogicState(0, 0, 1);
                        else
                            SetSteeringLogicState(0, throttle, 0.5f * (velocityMagnitude / maxVelocity) * (1 + 0.5f * (angularVelocityMagnitude / maxAngularVelocity)));
                        currentPath.ArrivedAtWaypoint();
                    }
                    else if (distance < turnRadius && abs180Angle > 2 * manor)
                    {
                        SetSteeringLogicState(1, 0, 1);
                    }
                }
                else if (reversing == 1)
                {
                    if (distance <= proximityDistance || (distance < turnRadius/*reversingRadiusLowBound*/ && (abs180Angle < 2 * mar || 180 - abs180Angle < 2 * mar)))
                    {
                        if (currentPath.IsNextWaypointLast)
                            SetSteeringLogicState(1, 0, 1);
                        else
                            SetSteeringLogicState(1, throttle, 0.5f * (velocityMagnitude / maxVelocity) * (1 + 0.5f * (angularVelocityMagnitude / maxAngularVelocity)));
                        currentPath.ArrivedAtWaypoint();
                    }
                    else if (distance > turnRadius)
                    {
                        if (abs180Angle < 2 * manor)
                        {
                            SetSteeringLogicState(0, 0, 1);
                        }
                    }
                }
                else if (reversing == 2)
                {
                    if (distance <= proximityDistance || (distance < turnRadius/*reversingRadiusLowBound*/ && (abs180Angle < 2 * mar || 180 - abs180Angle < 2 * mar)))
                    {
                        if (currentPath.IsNextWaypointLast)
                            SetSteeringLogicState(2, 0, 1);
                        else
                            SetSteeringLogicState(2, throttle, 0.5f * (velocityMagnitude / maxVelocity) * (1 + 0.5f * (angularVelocityMagnitude / maxAngularVelocity)));
                        currentPath.ArrivedAtWaypoint();
                    }
                    else if (distance > turnRadius && abs180Angle < 2 * mar)
                    {
                        SetSteeringLogicState(0, 0, 1);
                    }
                }

            }
        }

        private void SetSteeringLogicState(int reversing, float throttle, float targetBtkThrt)
        {
            this.reversing = reversing;
            this.throttle = throttle;
            this.targetBrkThrt = targetBtkThrt;
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

        private float previousVelocityMagnitude;
        void FixedUpdate()
        {
            velocity = mainRigidBody.velocity;
            velocityMagnitude = velocity.magnitude;
            angularVelocityMagnitude = mainRigidBody.angularVelocity.Dot(transform.up);

            dt = Time.fixedDeltaTime;

            UpdatePhysicsLogic();

            UpdateSteeringDecisionLogic();

            foreach (AxleInfo ainfo in axleInfos)
            {
                if (ainfo.steering)
                {
                    ainfo.leftWheel.steerAngle = steering;
                    ainfo.rightWheel.steerAngle = steering;
                    ainfo.leftWheel.brakeTorque = brake;
                    ainfo.rightWheel.brakeTorque = brake;
                }
                if (ainfo.motor)
                {
                    ainfo.leftWheel.motorTorque = motor;
                    ainfo.rightWheel.motorTorque = motor;
                    ainfo.leftWheel.brakeTorque = brake;
                    ainfo.rightWheel.brakeTorque = brake;
                }
            }

            //var d = Vector3.Dot(velocity, mainRigidBody.transform.right);
            //var slipReductionForce = -1 * d * mainRigidBody.transform.right / dt;
            //slipReductionForce += -0.5f * (1+d/maxVelocity) * mainRigidBody.transform.up;
            //mainRigidBody.AddForce(slipReductionForce, ForceMode.Acceleration);

        }

        private void AskPathTo(Vector3 startPos, Vector3 endPos, bool add, bool okayLookAt, Vector3 toLookAt)
        {
            Path pp = GetComponent<Seeker>().StartPath(startPos, endPos, (Path p) => currentPath.OnPathReturned(p, add, endPos, okayLookAt, toLookAt));
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
                //AskPathTo(startpos, endpos, add, okayLookAt, toLookAt);

                OrderContainer oc = new OrderContainer(typeof(MovementOrder), null, new List<OrderReceiver>() { GetComponentInParent<OrderReceiver>() },
                                                                            new List<object>() { endpos, add, okayLookAt, toLookAt }, -1);
                GameManager.Instance.currentMainHandler.orderHandler.DispatchOrderContainerToReceivers(oc);

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

        private void OnDrawGizmosSelected()
        {
            if(currentPath.ActualPathExists())
            {
                Gizmos.DrawSphere(currentPath.NextActualWaypoint, 0.5f);
                Gizmos.DrawSphere(currentPath.LastActualWaypoint, 1f);
            }
        }

    }
}