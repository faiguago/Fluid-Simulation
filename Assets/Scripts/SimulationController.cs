using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FI
{

    public class SimulationController : MonoBehaviour
    {

        private FluidSimulation fluidSim;

        [SerializeField]
        private bool bIsMouseControlled = false;

        [SerializeField]
        private LayerMask layerMask;

        private Vector3 oldPos;

        private bool bIsPressed;

        // Start is called before the first frame update
        private void Start()
        {
            fluidSim = FindObjectOfType<FluidSimulation>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (!fluidSim)
                return;

            if (bIsMouseControlled)
                DetectMouseInput();
            else
                DetectWorldInput();
        }

        // ----------------------------
        private void DetectWorldInput()
        {
            Vector3 pos = transform.position;
            pos.y = 0f;

            Ray ray = new Ray(pos + Vector3.up, Vector3.down);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1.1f, layerMask))
            {
                Vector3 dir = pos - oldPos;
                if (dir.magnitude >= 0.0001f)
                {
                    fluidSim.ProcessHit(hit, new Vector2(dir.x, dir.z) / Time.deltaTime, true);
                }
                else
                {
                    fluidSim.ProcessHit(hit, new Vector2(pos.x, pos.z), false);
                }
            }

            oldPos = pos;
        }

        // ----------------------------
        private void DetectMouseInput()
        {
            if (bIsPressed)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    Vector3 newPos = Input.mousePosition;

                    if ((newPos - oldPos).magnitude >= 0.0001f)
                    {
                        Vector3 dir = newPos - oldPos;
                        oldPos = newPos;

                        fluidSim.ProcessHit(hit, dir / Time.deltaTime, true);
                    }
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                bIsPressed = true;
                oldPos = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                bIsPressed = false;
                oldPos = Vector3.zero;
            }
        }
    }

}