using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalManagers;

namespace Core
{
    public class SelectionCircle : MonoBehaviour
    {
        private Terrain terrain;

        //private float toBottom;
        private LineRenderer myLineRenderer;
        private float t = 0;

        private void Start()
        {
            terrain = GameManager.Instance.currentMainHandler.terrainHandler.MyTerrain;

            if (transform.parent != null)
            {
                MeshFilter parentMeshFilter = transform.parent.GetComponentInChildren<MeshFilter>();

                Vector3 correctedExt = Vector3.Scale(parentMeshFilter.transform.localScale, parentMeshFilter.mesh.bounds.extents);
                float h = Mathf.Max(3f, 8 * Mathf.Max(correctedExt.x, correctedExt.z));
                //toBottom = correctedExt.y;

                Transform childtransform = transform.GetComponentInChildren<Transform>();
                childtransform.localScale = new Vector3(h, h, h);

                myLineRenderer = transform.GetComponentInChildren<LineRenderer>();

                myLineRenderer.useWorldSpace = true;
                myLineRenderer.SetPosition(0, transform.parent.position);
                myLineRenderer.SetPosition(1, transform.parent.position);

            }

        }

        private Ray downRay = new Ray();
        private void FixedUpdate()
        {
            if (transform.parent != null)
            {

                myLineRenderer.SetPosition(0, transform.parent.position);

                var collider = terrain.GetComponent<Collider>();
                downRay.origin = myLineRenderer.GetPosition(0);
                downRay.direction = Vector3.down;
                RaycastHit hit = new RaycastHit();
                Vector3 pos;
                if (collider.Raycast(downRay, out hit, Mathf.Infinity))
                {
                    pos = hit.point;
                    myLineRenderer.SetPosition(1, pos);
                    if (Mathf.Abs(pos.y - transform.parent.position.y) <= 5f)
                    {
                        myLineRenderer.enabled = false;
                    }
                    else
                    {
                        myLineRenderer.enabled = true;
                    }
                }

                /* terrain.SampleHeight ? */

            }
        }

        private void Update()
        {
            if (transform.parent != null)
            {
                t += 30 * Time.deltaTime;
                if (t > 360) { t -= 360; }
                transform.SetPositionAndRotation(transform.parent.position, Quaternion.Euler(0, t, 0));
            }
        }
    }
}