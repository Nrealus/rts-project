using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalManagers;
using UtilsAndExts;
using Gamelogic.Extensions;
using Core.AI.Formation;
using UnityEditor;

namespace Core.AI.Pathtracking
{

    public class GVAIController : PathtrackingController
    {
        [System.Serializable]
        public class AxleInfo
        {
            public WheelCollider leftWheel;
            public WheelCollider rightWheel;
            public bool motor; // is this wheel attached to motor?
            public bool steering; // does this wheel apply steer angle?
        }        

        //public EnhancedNavPath currentPath;
        public FormationBaseScrapped formationModule;
        private int hasDestination; // 0 : no destination; 1 : has path; 2 : only has a destination point
        
#pragma warning disable CS0649

        public Rigidbody mainRigidBody;

        [SerializeField] private float halfHeight;

        [SerializeField] private float wheelBase;
        [SerializeField] private float vehicleWidth;

        private Terrain terrain;
        public bool onGround { get; private set; }

        private int reversing = 0;
        private bool _goingBackwards = false;
        
        /*public bool GoingBackwards;
        {
            get
            { return _goingBackwards; }
            set
            {
                if (_goingBackwards != value)
                {
                    //throttle = 0;
                    //brakeThrottle = Mathf.Min(2 * velocityMagnitude / maxVelocity, 1);
                    _goingBackwards = value;
                    //reversing = 2;
                }
            }
        }*/

        //[SerializeField] private PID angleController;
        //[SerializeField] private PID angularVelocityController;
        [SerializeField] private PID velocityLimiter;

        public List<AxleInfo> axleInfos;
        [SerializeField] private float maxMotorTorque;
        [SerializeField] private float maxBrakeTorque;
        [SerializeField] private float maxSteeringAngle;

        [SerializeField] private float _maxVelocity;
        [SerializeField] private float _maxReverseVelocity;
        private float maxVelocity { get { if (GoingBackwards ^ reversing == 1) return _maxReverseVelocity; else return _maxVelocity; } }

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
        private float brakeThrottle;

        private float motor;
        private float brake;
        private float steering;

        private Vector3 velocity;
        private float angularVelocityMagnitude;
        private float velocityMagnitude;

        private float turnRadius;

        private float dt;

        private Vector3 desiredVelocity;

        private Vector3 targetPoint;

        void Start()
        {
            maxAngularVelocity = _maxAngularVelocity;

            currentPath = new EnhancedNavPath() /*{ halfHeight = halfHeight }*/;

            terrain = GameManager.Instance.currentMainHandler.terrainHandler.MyTerrain;

            StartCoroutine(TestOnGround(1f));
        }

        void Update()
        {
            /*
            if (GameManager.Instance.IM.IsMouseButtonHeld[GameManager.Instance.IM.LMB])
            {
                GoingBackwards = true;
            }
            else
            {
                GoingBackwards = false;
            }
            */
            /*
            if (GameManager.Instance.IM.IsMouseButtonPressed[GameManager.Instance.IM.RMB])
            {
                var ray = GameManager.Instance.currentMainCamera.GetComponentsInChildren<Camera>()[0].ScreenPointToRay(GameManager.Instance.IM.MousePosition);
                RaycastHit hit = new RaycastHit();
                Physics.Raycast(ray, out hit, Mathf.Infinity);

                if (hit.collider != null)
                {
                    targetPoint = hit.point;
                }
            }
            */

            if (currentPath.ActualPathExists())
            {
                hasDestination = 1;
            }
            /*else if ()
            {

            }*/
            else
            {
                hasDestination = 0;
            }

        }

        public override void Unpause()
        {
            paused = false;
        }
        public override void Pause()
        {
            paused = true;
        }

        private Vector3 targetOrientation;
        private bool debug_back_circle;
        private Vector3 PathFollowingSteeringBehaviour()
        {
            Vector3 v = Vector3.zero;
            float distance;

            debug_back_circle = false;

            if (hasDestination > 0)
            {

                if (paused)
                {

                }
                else
                {

                    targetPoint = currentPath.NextActualWaypoint.position;

                    v = Vector3.ProjectOnPlane(targetPoint - transform.position, transform.up);
                    distance = v.magnitude;

                    if(distance > 4 * turnRadius)
                    {
                        currentPath.SetWaypointClosestToPosition(transform.position);

                        targetPoint = currentPath.NextActualWaypoint.position;

                        v = Vector3.ProjectOnPlane(targetPoint - transform.position, transform.up);
                        distance = v.magnitude;
                    }

                    targetOrientation = currentPath.NextActualWaypoint.orientation;
                    if (currentPath.IsNextWaypointLast && distance <= proximityDistance)
                    {
                        v = currentPath.NextActualWaypoint.orientation;
                        if (GoingBackwards && !currentPath.specifiedFinalOrientation)
                            targetOrientation *= -1;
                    }

                    v.Normalize();

                    if (reversing == 1 || currentPath.IsNextWaypointLast)
                        v *= maxVelocity;
                    else
                        v *= Mathf.Clamp(currentPath.RemainingDistance(transform.position), 0.2f, maxVelocity);

                    Vector3 f = (!GoingBackwards) ? transform.forward : -transform.forward;
                    float angle = Vector3.SignedAngle(f, targetPoint - transform.position, transform.up);
                    if(!currentPath.IsNextWaypointLast)
                        v *= Mathf.Clamp01(1 - 0.25f * Mathf.Clamp01(Mathf.Abs(angle) / 90));

                    if (reversing == 1)
                        v *= -1;

                    debug_back_circle = GoingBackwards;// ^ (currentPath.IsNextWaypointLast);

                    if (reversing == 0)
                    {
                        if (Mathf.Abs(angle) > maxAngleNoReverse
                            && distance > proximityDistance
                            && (PointInLeftTurnCircle(targetPoint, turnRadius * 0.95f) || PointInRightTurnCircle(targetPoint, turnRadius * 0.95f)
                            || PointInBackTurnCircle(targetPoint, turnRadius * 0.95f, GoingBackwards)))
                        {
                            SetSteeringControlState(1, 0, 1);
                        }
                    }

                    if (reversing == 1)
                    {
                        if (Mathf.Abs(angle) < maxAngleReverse
                            && distance > proximityDistance
                            && !PointInLeftTurnCircle(targetPoint, turnRadius * 1.05f) && !PointInRightTurnCircle(targetPoint, turnRadius * 1.05f)
                            && !PointInBackTurnCircle(targetPoint, turnRadius * 1.05f, GoingBackwards))
                        {
                            SetSteeringControlState(0, 0, 1);
                        }
                    }

                    if (distance <= proximityDistance)
                    {
                        if (!currentPath.IsNextWaypointLast)
                            currentPath.ArrivedAtWaypoint();
                        else if (Mathf.Abs(Vector3.SignedAngle(transform.forward, targetOrientation, Vector3.up)) < angleMarginOrientationAtLastWp)
                        {
                            v = Vector3.zero;
                            currentPath.ArrivedAtWaypoint();
                        }
                        else
                        {
                            GoingBackwards = false;
                        }

                    }

                }
            }
            else
            {
                if (velocityMagnitude / maxVelocity < 0.01f)
                    v = Vector3.zero;
                else
                    v = -0.75f * Vector3.ProjectOnPlane(velocity, transform.up);
            }

            return v;
        }

        private void UpdateControlLogic()
        {

            float c = Vector3.Cross(velocity, desiredVelocity).magnitude;
            float fullSteers = Mathf.Clamp(Vector3.SignedAngle(0.1f * transform.forward + velocity, desiredVelocity, transform.up), -maxSteeringAngle, maxSteeringAngle);
            steering = Mathf.Lerp(steering, Mathf.Sign(c) * fullSteers, 50 * dt);

            float desvelmagn = desiredVelocity.magnitude;
            if (reversing == 1 ^ GoingBackwards)
            {
                desvelmagn *= -1;
                steering *= -1;
            }

            float targetThrottle = Mathf.Clamp(desvelmagn / maxVelocity, minThrottle, maxThrottle);

            float throttleRate = 0;
            if (targetThrottle - throttle > 0)
                throttleRate = accelRate * dt;
            else if (targetThrottle - throttle < 0)
                throttleRate = reverseAccel * dt;
            throttle = Mathf.Lerp(throttle, targetThrottle, throttleRate);

            if (targetThrottle == 0 && velocityMagnitude != 0 && brakeThrottle == 0)
            {
                throttle = 0;
                brakeThrottle = 1;
            }

            brakeThrottle = Mathf.Lerp(brakeThrottle, 0, 50*dt);
            brake = maxBrakeTorque * brakeThrottle;

            if (onGround)
            {
                if (!paused && hasDestination > 0)
                {

                    float velocityError = maxVelocity - velocityMagnitude;
                    float mult = 1;
                    float velocityCorrectionForMotor = velocityLimiter.GetOutput(velocityError, dt, mult, mult, mult);
                    if (velocityError / maxVelocity < 0.1)
                    {
                        motor *= Mathf.Min(velocityCorrectionForMotor, 0.95f);
                    }
                }
                else
                {
                    throttle = 0;
                }
                motor = maxMotorTorque * throttle;
            }
            else
            {
                throttle = 0;
            }
        }

        void FixedUpdate()
        {
            velocity = mainRigidBody.velocity;
            velocityMagnitude = velocity.magnitude;
            angularVelocityMagnitude = mainRigidBody.angularVelocity.Dot(transform.up);

            //turnRadius = (steering != 0) ? wheelBase / Mathf.Sin(Mathf.Max(10, Mathf.Abs(steering)) * Mathf.Deg2Rad) : 0;
            turnRadius = wheelBase / Mathf.Sin(maxSteeringAngle * Mathf.Deg2Rad);

            dt = Time.fixedDeltaTime;

            desiredVelocity = Vector3.zero;
            desiredVelocity += PathFollowingSteeringBehaviour();

            UpdateControlLogic();

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

            var d = Vector3.Dot(velocity, mainRigidBody.transform.right);
            var slipReductionForce = -1 * d * mainRigidBody.transform.right / dt;
            slipReductionForce += -0.5f * (1+d/maxVelocity) * mainRigidBody.transform.up;
            mainRigidBody.AddForce(slipReductionForce, ForceMode.Acceleration);

        }

        private bool PointInRightTurnCircle(Vector3 point, float turnCircleRadius)
        {
            return Vector3.Distance(point, transform.position + transform.right * (Mathf.Clamp(velocityMagnitude / maxVelocity, 0.5f, 1f) * vehicleWidth / 2 + turnCircleRadius)) <= turnCircleRadius;
        }

        private bool PointInLeftTurnCircle(Vector3 point, float turnCircleRadius)
        {
            return Vector3.Distance(point, transform.position - transform.right * (Mathf.Clamp(velocityMagnitude / maxVelocity, 0.5f, 1f) * vehicleWidth / 2 + turnCircleRadius)) <= turnCircleRadius;
        }

        private bool PointInBackTurnCircle(Vector3 point, float turnCircleRadius, bool reversedOrientation)
        {
            var vv = (reversedOrientation) ? -transform.forward : transform.forward;
            return Vector3.Distance(point, transform.position - vv * (Mathf.Clamp(velocityMagnitude / maxVelocity, 0.5f, 1f) * wheelBase / 2 + turnCircleRadius)) <= turnCircleRadius;
        }

        private void SetSteeringControlState(int reversing, float throttle, float brakeThrottle)
        {
            this.reversing = reversing;
            this.throttle = throttle;
            this.brakeThrottle = brakeThrottle;
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

        public override bool HasPath()
        {
            return currentPath.ActualPathExists();
        }

        private void OnDrawGizmosSelected()
        {
            if (EditorApplication.isPlaying)
            {
                if (currentPath.ActualPathExists())
                {
                    Gizmos.DrawSphere(currentPath.NextActualWaypoint.position, 0.8f);

                    for (int i = 0; i < currentPath.PathCount - 1; i++)
                    {
                        var lv = currentPath.Special_GetVectorPath();
                        Gizmos.DrawLine(lv[i], lv[i + 1]);
                    }
                    Gizmos.DrawLine(transform.position, transform.position + targetOrientation * 30f);
                }

                Gizmos.DrawSphere(transform.position + desiredVelocity, 0.2f);
                Gizmos.DrawLine(transform.position, transform.position + desiredVelocity);
                Gizmos.DrawWireSphere(transform.position + (Mathf.Clamp(velocityMagnitude / maxVelocity, 0.5f, 1f) * vehicleWidth / 2 + turnRadius) * transform.right, turnRadius);
                Gizmos.DrawWireSphere(transform.position - (Mathf.Clamp(velocityMagnitude / maxVelocity, 0.5f, 1f) * vehicleWidth / 2 + turnRadius) * transform.right, turnRadius);
                if (!debug_back_circle)
                    Gizmos.DrawWireSphere(transform.position - (Mathf.Clamp(velocityMagnitude / maxVelocity, 0.5f, 1f) * wheelBase / 2 + turnRadius) * transform.forward, turnRadius);
                else
                    Gizmos.DrawWireSphere(transform.position + (Mathf.Clamp(velocityMagnitude / maxVelocity, 0.5f, 1f) * wheelBase / 2 + turnRadius) * transform.forward, turnRadius);
            }

        }

    }
}