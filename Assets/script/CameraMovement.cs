using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class CameraMovement : MonoBehaviour
{
    public RenderFile r;

    // Normal camera
    public bool canMove = false;
    public GameObject cameraObject;
    public GameObject Arrows;
    public Transform Pointer;
    public Camera mainCamera;
    private float CameraSpeed = 100;
    private Vector3 camRot = new Vector3(0, 0, 0);
    public float SensitivityMouse = 200f;

    // Ortographic camera
    public Camera PointCamera;
    Vector3 selectionPointA = new Vector3();
    Vector3 selectionPointB = new Vector3();
    public bool mode = false;
    int ortographicMode = 0;

    // Box for selection
    public GameObject box;

    void Start()
    {
        r = this.GetComponent<RenderFile>();

        if (File.Exists(UnityEngine.Application.dataPath + "\\cfg_sensitivity.cfg"))
        {
            SensitivityMouse = int.Parse(File.ReadAllText(UnityEngine.Application.dataPath + "\\cfg_sensitivity.cfg"));
        }

        // Initialize selection box
        box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.AddComponent<Rigidbody>();
        box.AddComponent<SelectOrtographic>();
        box.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
        box.GetComponent<Rigidbody>().useGravity = false;
        box.GetComponent<Rigidbody>().freezeRotation = true;
    }

    void Update()
    {
        if (r.CommandInput.isFocused == true || r.fileOpen == false || Input.GetMouseButton(0))
        {
            canMove = false;
        }
        else
        {
            canMove = true;
        }

        if (Input.GetKeyDown(KeyCode.E) && r.fileOpen && r.WindowOpen == false && r.CommandInput.text == "" && mode == false)
        {
            if (PointCamera.gameObject.activeSelf == false)
            {
                mainCamera.gameObject.SetActive(false);
                PointCamera.gameObject.SetActive(true);
            }
            else
            {
                mainCamera.gameObject.SetActive(true);
                PointCamera.gameObject.SetActive(false);
            }
        }

        if (PointCamera.gameObject.activeSelf && r.fileOpen && r.CommandInput.text == "" && r.WindowOpen == false)
        {
            if (PointCamera.gameObject.activeSelf == true)
            {
                PointCamera.orthographicSize += Input.GetAxis("Mouse ScrollWheel") * 25;
                if (PointCamera.orthographicSize < 10) PointCamera.orthographicSize = 10;
                PointCamera.transform.GetChild(0).GetComponent<Camera>().orthographicSize = PointCamera.orthographicSize;
            }

            if (ortographicMode == 0)
            {
                if (Input.GetMouseButtonDown(1) && mode == false)
                {
                    ortographicMode = 1;
                    GameObject.Find("OrtographicCameraCenter").transform.rotation = Quaternion.Euler(0, ortographicMode * 90, 0);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    mode = true;
                    selectionPointA = PointCamera.ScreenToWorldPoint(Input.mousePosition);
                    selectionPointA.z = 0;
                    box.GetComponent<Renderer>().material.shader = Shader.Find("Legacy Shaders/Transparent/Bumped Diffuse");
                    Color a = new Color();
                    ColorUtility.TryParseHtmlString("#FFD80021", out a);
                    box.GetComponent<Renderer>().material.color = a;

                    box.transform.localScale = Vector3.one * 50;
                }

                if (mode && box != null)
                {
                    selectionPointB = PointCamera.ScreenToWorldPoint(Input.mousePosition);
                    selectionPointA.z = 0;
                    box.transform.position = (selectionPointA + selectionPointB) / 2;
                    box.transform.position = new Vector3(box.transform.position.x, box.transform.position.y, 0);
                    box.transform.Rotate(0, 0, 0.000000001f);
                    float hor = Math.Abs(selectionPointA.x - selectionPointB.x);
                    float ver = Math.Abs(selectionPointA.y - selectionPointB.y);
                    box.transform.localScale = new Vector3(hor, ver, 700);
                }

                if (Input.GetMouseButtonUp(0) && mode == true || r.WindowOpen && mode == true)
                {
                    mode = false;
                    box.transform.localScale = box.transform.localScale + new Vector3(0, 0, 700);
                    selectionPointA = Vector3.zero;
                    selectionPointB = Vector3.zero;
                    box.transform.localScale = Vector3.zero;

                    MovementAxis();
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(1) && mode == false)
                {
                    ortographicMode = 0;
                    GameObject.Find("OrtographicCameraCenter").transform.rotation = Quaternion.Euler(0, ortographicMode * 90, 0);
                }

                if (!r.WindowOpen && Input.GetMouseButtonDown(0))
                {
                    mode = true;
                    selectionPointA = PointCamera.ScreenToWorldPoint(Input.mousePosition);
                    selectionPointA.x = 0;
                    box.GetComponent<Renderer>().material.shader = Shader.Find("Legacy Shaders/Transparent/Bumped Diffuse");
                    Color a = new Color();
                    ColorUtility.TryParseHtmlString("#FFD80021", out a);
                    box.GetComponent<Renderer>().material.color = a;

                    box.transform.localScale = Vector3.one * 50;
                }

                if (mode && box != null)
                {
                    selectionPointB = PointCamera.ScreenToWorldPoint(Input.mousePosition);
                    selectionPointA.x = 0;
                    box.transform.position = (selectionPointA + selectionPointB) / 2;
                    box.transform.position = new Vector3(0, box.transform.position.y, box.transform.position.z);
                    box.transform.Rotate(0.00000001f, 0, 0);
                    float ver = Math.Abs(selectionPointA.y - selectionPointB.y);
                    float dep = Math.Abs(selectionPointA.z - selectionPointB.z);
                    box.transform.localScale = new Vector3(700, ver, dep);
                }

                if (Input.GetMouseButtonUp(0) && mode == true || r.WindowOpen && mode == true)
                {
                    mode = false;
                    box.transform.localScale = box.transform.localScale + new Vector3(700, 0, 0);
                    selectionPointA = Vector3.zero;
                    selectionPointB = Vector3.zero;
                    box.transform.localScale = Vector3.zero;

                    MovementAxis();
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (!(canMove && r.inCommand == false && r.WindowOpen == false)) return;

        if (mainCamera.gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                CameraSpeed = 200f;
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                CameraSpeed = 100f;
            }
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                cameraObject.transform.position = cameraObject.transform.position + mainCamera.transform.forward * 100 * Time.deltaTime;
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                cameraObject.transform.position = cameraObject.transform.position - mainCamera.transform.forward * 100 * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.W))
            {
                cameraObject.transform.position = cameraObject.transform.position + mainCamera.transform.forward * CameraSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.S))
            {
                cameraObject.transform.position = cameraObject.transform.position - mainCamera.transform.forward * CameraSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.A))
            {
                cameraObject.transform.position = cameraObject.transform.position - mainCamera.transform.right * CameraSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.D))
            {
                cameraObject.transform.position = cameraObject.transform.position + mainCamera.transform.right * CameraSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.X))
            {
                cameraObject.transform.position = cameraObject.transform.position + Vector3.up * CameraSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.C))
            {
                cameraObject.transform.position = cameraObject.transform.position - Vector3.up * CameraSpeed * Time.deltaTime;
            }

            if (camRot.x > 90)
            {
                camRot.x = 90;
            }
            if (camRot.x < -90)
            {
                camRot.x = -90;
            }

            // PRESSING RIGHT CLICK TO ROTATE THE CAMERA
            if (Input.GetMouseButton(1))
            {
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                UnityEngine.Cursor.lockState = CursorLockMode.None;
            }

            if (Input.GetAxisRaw("Mouse Y") > 0 && Input.GetMouseButton(1))
            {
                camRot.x = camRot.x - SensitivityMouse * Time.deltaTime * Input.GetAxisRaw("Mouse Y");
            }
            if (Input.GetAxisRaw("Mouse Y") < 0 && Input.GetMouseButton(1))
            {
                camRot.x = camRot.x - SensitivityMouse * Time.deltaTime * Input.GetAxisRaw("Mouse Y");
            }
            if (Input.GetAxisRaw("Mouse X") < 0 && Input.GetMouseButton(1))
            {
                camRot.y = camRot.y + SensitivityMouse * Time.deltaTime * Input.GetAxisRaw("Mouse X");
            }
            if (Input.GetAxisRaw("Mouse X") > 0 && Input.GetMouseButton(1))
            {
                camRot.y = camRot.y + SensitivityMouse * Time.deltaTime * Input.GetAxisRaw("Mouse X");
            }

            mainCamera.transform.rotation = Quaternion.Euler(mainCamera.transform.rotation.x + camRot.x, mainCamera.transform.rotation.y + camRot.y, mainCamera.transform.rotation.z + camRot.z);
        }
        else
        {
            if (Input.GetKey(KeyCode.W))
            {
                PointCamera.transform.position += PointCamera.transform.up * CameraSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.S))
            {
                PointCamera.transform.position -= PointCamera.transform.up * CameraSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.A))
            {
                PointCamera.transform.position -= PointCamera.transform.right * CameraSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.D))
            {
                PointCamera.transform.position += PointCamera.transform.right * CameraSpeed * Time.deltaTime;
            }
        }

        Arrows.transform.rotation = Quaternion.Euler(Vector3.forward);
    }

    void MovementAxis()
    {
        if (r.selectedVertex.Count > 0)
        {
            Vector3 pos = Vector3.zero;
            for (int x = 0; x < r.selectedVertex.Count; x++)
            {
                pos = pos + r.selectedVertex[x].transform.position;
            }
            pos = pos / r.selectedVertex.Count;
            GameObject.Find("Movement Axis").transform.position = pos;
        }
        else
        {
            GameObject.Find("Movement Axis").transform.position = new Vector3(65535, 65535, 65535);
        }
    }
}
