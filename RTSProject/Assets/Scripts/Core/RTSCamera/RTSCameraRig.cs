using UnityEngine;
using System.Collections;
using GlobalManagers;
using VariousUtilsExtensions;
// This components must be attached on a camera. The camera will be then controllable by the player as an "RTS camera".

namespace Core
{
    public class RTSCameraRig : MonoBehaviour
    {
        private InputManager im;

        #region Various things defined.
        // Public fields
        public float normalPanSpeed = 15.0f;
        public float zoomSpeed = 100.0f;
        public float rotationSpeed = 50.0f;

        public float fastPanMultiplier = 1.5f;
        public float slowPanMultiplier = 0.8f;
        public float mousePanMultiplier = 0.1f;
        public float mouseRotationMultiplier = 0.2f;
        public float mouseZoomMultiplier = 5.0f;

        public float minZoomDistance = 20.0f;
        public float maxZoomDistance = 200.0f;
        public float smoothingFactor = 0.1f;
        public float goToSpeed = 0.1f;

        public bool useKeyboardInput = true;
        public bool useMouseInput = true;
        public bool adaptToTerrainHeight = true;
        public bool increaseSpeedWhenZoomedOut = true;
        public bool correctZoomingOutRatio = true;
        public bool smoothing = true;
        public bool allowDoubleClickMovement = false;
        public bool minimalZoomForced = false;

        public bool allowScreenEdgeMovement = true;
        public int screenEdgeSize = 10;
        public float screenEdgeSpeed = 1.0f;

        public GameObject objectToFollow;
        public Vector3 cameraTarget;

        // private fields
        private Terrain terrain;
        private float currentCameraDistance;
        private Vector3 lastPanSpeed = Vector3.zero;
        private Vector3 goingToCameraTarget = Vector3.zero;
        private float wantedCameraDistance;
        private bool doingAutoMovement = false;

        private Vector3 lastMousePos;

        // class containing the handlers for "control conditions" (by the player) for this object.
        private class CameraControls
        {
            public Vector3 myMousePos;
            public bool forwardControl;
            public bool backControl;
            public bool leftControl;
            public bool rightControl;
            public bool leftShiftControl;
            public bool leftAltControl;
            public bool leftRotControl;
            public bool rightRotControl;
            public bool forwardZoomControl;
            public bool backZoomControl;
            public bool moveToDoubleClickedControl;
            public bool mousePanControl;
            public bool mouseRotControl;
            public float mouseScrollControl;
            public bool screenEdgeMovLeftControl;
            public bool screenEdgeMovRightControl;
            public bool screenEdgeMovForwardControl;
            public bool screenEdgeMovBackControl;
            public bool fastPanControl;
            public bool slowPanControl;

            public CameraControls()
            {
                myMousePos = GameManager.Instance.IM.MousePosition;
                forwardControl = false;
                backControl = false;
                leftControl = false;
                rightControl = false;
                leftShiftControl = false;
                leftAltControl = false;
                leftRotControl = false;
                rightRotControl = false;
                forwardZoomControl = false;
                backZoomControl = false;
                moveToDoubleClickedControl = false;
                mousePanControl = false;
                mouseRotControl = false;
                mouseScrollControl = 0;
                screenEdgeMovBackControl = false;
                screenEdgeMovForwardControl = false;
                screenEdgeMovLeftControl = false;
                screenEdgeMovRightControl = false;
                fastPanControl = false;
                slowPanControl = false;
            }
        }

        private CameraControls myControls;

        #endregion

        public void Start()
        {
            im = GameManager.Instance.IM;

            myControls = new CameraControls();

            lastMousePos = Vector3.zero;
            currentCameraDistance = minZoomDistance + ((maxZoomDistance - minZoomDistance) / 2.0f);
            wantedCameraDistance = currentCameraDistance;

            terrain = GameManager.Instance.currentMainHandler.terrainHandler.MyTerrain;

            if (terrain != null)
            {
                cameraTarget = transform.position;
            }
            else
            {
                cameraTarget = transform.position;
            }

        }

        // Sets local control/handling conditions. Called at the beginning of every Update (for this object).
        private void UpdateMyControls()
        {

            myControls.forwardControl = useKeyboardInput
                                        && im.IsKeyHeld[im.Z_Key];
            myControls.backControl = useKeyboardInput
                                    && im.IsKeyHeld[im.S_Key];
            myControls.leftControl = useKeyboardInput
                                    && im.IsKeyHeld[im.Q_Key];
            myControls.rightControl = useKeyboardInput
                                    && im.IsKeyHeld[im.D_Key];
            myControls.leftRotControl = useKeyboardInput
                                        && im.IsKeyHeld[im.A_Key];
            myControls.rightRotControl = useKeyboardInput
                                        && im.IsKeyHeld[im.E_Key];
            myControls.forwardZoomControl = useKeyboardInput
                                        && im.IsKeyHeld[im.F_Key];
            myControls.backZoomControl = useKeyboardInput
                                        && im.IsKeyHeld[im.R_Key];

            myControls.leftShiftControl = im.IsKeyHeld[im.LShift_Key];
            myControls.leftAltControl = im.IsKeyHeld[im.LAlt_Key];

            myControls.myMousePos = im.MousePosition;
            myControls.moveToDoubleClickedControl = allowDoubleClickMovement
                                                && im.IsMouseButtonDoublePressed[im.LMB]
                                                && im.PointerNotOnUIElement
                                                && terrain
                                                && terrain.GetComponent<Collider>();
            myControls.mousePanControl = useMouseInput
                                        && im.IsMouseButtonHeld[im.MMB]
                                        && im.PointerNotOnUIElement
                                        && myControls.leftShiftControl;
            myControls.mouseRotControl = useMouseInput
                                        && im.IsMouseButtonHeld[im.MMB]
                                        && im.PointerNotOnUIElement
                                        && !myControls.leftShiftControl;

            myControls.mouseScrollControl = (useMouseInput && im.PointerNotOnUIElement) ? im.MouseScroll : 0;

            myControls.screenEdgeMovLeftControl = allowScreenEdgeMovement
                                                && (myControls.myMousePos.x < screenEdgeSize);
            myControls.screenEdgeMovRightControl = allowScreenEdgeMovement
                                                && (myControls.myMousePos.x > Screen.width - screenEdgeSize);
            myControls.screenEdgeMovBackControl = allowScreenEdgeMovement
                                                && (myControls.myMousePos.y < screenEdgeSize);
            myControls.screenEdgeMovForwardControl = allowScreenEdgeMovement
                                                && (myControls.myMousePos.y > Screen.height - screenEdgeSize);

            myControls.fastPanControl = myControls.leftShiftControl
                                        && !(im.IsMouseButtonHeld[im.MMB]
                                        && useMouseInput);
            myControls.slowPanControl = myControls.leftAltControl
                                    && !(im.IsMouseButtonHeld[im.MMB]
                                    && useMouseInput);

        }

        private void Update()
        {
            UpdateMyControls();

            if (myControls.moveToDoubleClickedControl == true)
            {
                MoveCameraToDoubleClickedPoint();
            }

            UpdatePanning();
            UpdateRotation();
            UpdateZooming();
            UpdatePosition();
            UpdateAutoMovement();
        }

        private void LateUpdate()
        {
            lastMousePos = myControls.myMousePos;
        }

        public void GoTo(Vector3 position)
        {
            doingAutoMovement = true;
            goingToCameraTarget = position;
            objectToFollow = null;
        }

        public void Follow(GameObject gameObjectToFollow)
        {
            objectToFollow = gameObjectToFollow;
        }

        private void MoveCameraToDoubleClickedPoint()
        {
            var cameraTargetY = cameraTarget.y;

            var collider = terrain.GetComponent<Collider>();

            var ray = GetComponentsInChildren<Camera>()[0].ScreenPointToRay(myControls.myMousePos);
            RaycastHit hit = new RaycastHit();

            Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Selection Colliders"));

            if (hit.collider == null && collider.Raycast(ray, out hit, Mathf.Infinity))
            {
                Vector3 pos = hit.point;
                pos.y = cameraTargetY;
                GoTo(pos);
            }
        }

        private Vector3 moveVector = Vector3.zero;
        private void UpdatePanning()
        {
            moveVector = Vector3.zero;
            if (myControls.forwardControl)
            {
                moveVector.z += 1;
            }
            if (myControls.backControl)
            {
                moveVector.z -= 1;
            }
            if (myControls.leftControl)
            {
                moveVector.x -= 1;
            }
            if (myControls.rightControl)
            {
                moveVector.x += 1;
            }

            if (myControls.screenEdgeMovLeftControl)
            {
                moveVector.x -= screenEdgeSpeed;
            }
            else if (myControls.screenEdgeMovRightControl)
            {
                moveVector.x += screenEdgeSpeed;
            }
            if (myControls.screenEdgeMovBackControl)
            {
                moveVector.z -= screenEdgeSpeed;
            }
            else if (myControls.screenEdgeMovForwardControl)
            {
                moveVector.z += screenEdgeSpeed;
            }

            if (myControls.mousePanControl)
            {
                Vector3 deltaMousePos = (myControls.myMousePos - lastMousePos);
                moveVector += new Vector3(-deltaMousePos.x, 0, -deltaMousePos.y) * mousePanMultiplier;
            }

            if (moveVector != Vector3.zero)
            {
                objectToFollow = null;
                doingAutoMovement = false;
            }

            var effectivePanSpeed = moveVector;
            if (smoothing)
            {
                effectivePanSpeed = Vector3.Lerp(lastPanSpeed, moveVector, smoothingFactor);
                lastPanSpeed = effectivePanSpeed;
            }

            float panMultiplier1;
            if (myControls.fastPanControl)
            {
                panMultiplier1 = fastPanMultiplier;
            }
            else if (myControls.slowPanControl)
            {
                panMultiplier1 = slowPanMultiplier;
            }
            else
            {
                panMultiplier1 = 1f;
            }

            effectivePanSpeed *= panMultiplier1;
            lastPanSpeed *= panMultiplier1;

            var oldXRotation = transform.localEulerAngles.x;

            // Set the local X rotation to 0;
            transform.SetLocalEulerAngles(0.0f);

            // faster if zoomed out;
            float panMultiplier2 = increaseSpeedWhenZoomedOut ? (Mathf.Sqrt(currentCameraDistance)) : 1.0f;
            cameraTarget = cameraTarget + transform.TransformDirection(effectivePanSpeed) * normalPanSpeed * panMultiplier2 * Time.deltaTime;

            effectivePanSpeed /= panMultiplier1;
            lastPanSpeed /= panMultiplier1;

            // Set the old x rotation.
            transform.SetLocalEulerAngles(oldXRotation);
        }

        private void UpdateRotation()
        {
            float deltaAngleH = 0.0f;
            float deltaAngleV = 0.0f;

            if (myControls.leftRotControl)
            {
                deltaAngleH -= 3.0f;
            }
            if (myControls.rightRotControl)
            {
                deltaAngleH += 3.0f;
            }

            if (myControls.mouseRotControl)
            {
                var deltaMousePos = (myControls.myMousePos - lastMousePos);
                deltaAngleH += deltaMousePos.x * mouseRotationMultiplier;
                deltaAngleV -= deltaMousePos.y * mouseRotationMultiplier;
            }

            transform.SetLocalEulerAngles(
                Mathf.Min(80.0f, Mathf.Max(5.0f, transform.localEulerAngles.x + deltaAngleV * Time.deltaTime * rotationSpeed)),
                transform.localEulerAngles.y + deltaAngleH * Time.deltaTime * rotationSpeed
            );
        }

        private void UpdateZooming()
        {
            float deltaZoom = 0.0f;
            if (myControls.forwardZoomControl)
            {
                deltaZoom = 1.0f;
            }
            if (myControls.backZoomControl)
            {
                deltaZoom = -1.0f;
            }
            deltaZoom -= myControls.mouseScrollControl * mouseZoomMultiplier;
            var zoomedOutRatio = correctZoomingOutRatio ? (currentCameraDistance - minZoomDistance) / (maxZoomDistance - minZoomDistance) : 0.0f;
            currentCameraDistance = Mathf.Max(minZoomDistance, Mathf.Min(maxZoomDistance, currentCameraDistance + deltaZoom * Time.deltaTime * zoomSpeed * (zoomedOutRatio * 2.0f + 1.0f)));
            //Debug.Log(currentCameraDistance);
        }

        private void UpdatePosition()
        {
            if (objectToFollow != null)
            {
                cameraTarget = Vector3.Lerp(cameraTarget, objectToFollow.transform.position, goToSpeed);
            }

            transform.position = cameraTarget;
            transform.Translate(Vector3.back * currentCameraDistance);

            if (adaptToTerrainHeight && terrain != null)
            {
                transform.SetPosition(
                    null,
                    Mathf.Max(terrain.SampleHeight(transform.position) + terrain.transform.position.y + 3.0f, transform.position.y)
                );
            }
        }

        private void UpdateAutoMovement()
        {
            if (doingAutoMovement)
            {
                cameraTarget = Vector3.Lerp(cameraTarget, goingToCameraTarget, goToSpeed);
                if (Vector3.Distance(goingToCameraTarget, cameraTarget) < 1.0f)
                {
                    doingAutoMovement = false;
                }
            }

            if (!minimalZoomForced)
            {
                wantedCameraDistance = currentCameraDistance;
            }
            else
            {
                wantedCameraDistance = maxZoomDistance;
                //currentCameraDistance = Mathf.Max(minZoomDistance, Mathf.Min(maxZoomDistance, currentCameraDistance - *Time.deltaTime * zoomSpeed * (zoomedOutRatio * 2.0f + 1.0f))); -
            }
            currentCameraDistance = Mathf.Lerp(currentCameraDistance, wantedCameraDistance, 20.0f * Time.deltaTime);

        }
    }
}