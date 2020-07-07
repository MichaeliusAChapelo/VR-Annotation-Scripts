using System.Collections;
using System.Collections.Generic;
using System.Threading;
using OVR.OpenVR;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

public class AnnotationController : MonoBehaviour
{

    #region Transformations

    private OVRCustomGrabbable CustomGrabbable;
    private Rigidbody Rigidbody;

    private Vector3 DefaultPosition;
    private Quaternion DefaultRotation;

    private Transform Origin;

    public float MovementExponent = 0.2f;
    public float RotationExponent = 0.1f;

    #endregion

    #region Raycast Annotation

    public GameObject AnnotationDotPrefab;
    private GameObject AnnotationObject;

    public Transform LeftGunTip, RightGunTip;
    private const float RaycastRange = 10f;
    private LineRenderer LaserLine;

    private GameObject MovingDot;

    #endregion

    [HideInInspector]
    internal readonly List<GameObject> AnnotationPoints = new List<GameObject>();

    public bool ImportDataSet = false;
    public string DataSet = "FI-C10_D_D_2.csv";
    public int[] LandmarksToImport;

    public GameObject[] AnnotationObjects;
    private int AnnotationObjectIndex = 0;

    // Start is called before the first frame update
    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        CustomGrabbable = GetComponent<OVRCustomGrabbable>();
        DefaultPosition = transform.position;
        DefaultRotation = transform.rotation;
        Origin = transform.parent;

        //AnnotationObject = GameObject.FindWithTag("AnnotationObject");
        AnnotationObject = AnnotationObjects[0];

        LaserLine = GetComponent<LineRenderer>();

        if (ImportDataSet)
            ImportAnnotations();
        else
            ImportEmptyAnnotationPage();
    }

    // Update is called once per frame
    private void Update()
    {
        OVRInput.Update();
        InputMapping.UpdateThisFrame();

        //if (InputMapping.PressedThisFrame(InputMapping.AnnotationInput.Save))
        //    ExportAnnotations();

        if (MovingDot != null)
            MovingDot.SetActive(false);

        if (CustomGrabbable.isGrabbed)
            LaserLine.enabled = false;// Disallow transformations
        else
        {
            LaserLine.enabled = true;
            TransformAnnotationObject();
            HandleAnnotationLine();
        }

        if (InputMapping.PressedThisFrame(InputMapping.AnnotationInput.ExportAndClear))
        {
            ExportAnnotations();
            ClearAnnotations();
            ++AnnotationObjectIndex;
            AnnotationObject.SetActive(false);
            if (AnnotationObjectIndex >= AnnotationObjects.Length)
                Application.Quit();
            else
            {
                AnnotationObject = AnnotationObjects[AnnotationObjectIndex];
                AnnotationObject.SetActive(true);
            }
        }

        InputMapping.UpdatePreviousFrame();


        void HandleAnnotationLine()
        {
            Transform gunTip = InputMapping.IsRightHanded ? RightGunTip : LeftGunTip;
            LaserLine.SetPosition(0, gunTip.position);
            Vector3 direction = gunTip.forward;

            if (Physics.Raycast(gunTip.position, direction, out RaycastHit hit, RaycastRange * Origin.localScale.x))
            {
                LaserLine.SetPosition(1, hit.point);

                if (SelectedDotExists())
                    HandleSelectedDot();
                else if (ObjectHit().Equals(AnnotationObject) && ButtonPressed(InputMapping.AnnotationInput.Annotate))
                    CreateNewDot();
                else if (ObjectHit().CompareTag("AnnotationDot"))
                {
                    var obj = ObjectHit();
                    if (ButtonPressed(InputMapping.AnnotationInput.Annotate))
                        SelectExistingDot(obj);
                    if (ButtonPressed(InputMapping.AnnotationInput.DestroyAnnotation))
                        DestroyDot(obj);
                }
            }
            else
            {
                LaserLine.SetPosition(1, direction * RaycastRange * Origin.localScale.x);
                if (SelectedDotExists())
                    DisableDotRender();
            }




            void CreateNewDot() { AnnotationPoints.Add(Instantiate(AnnotationDotPrefab, hit.point, Quaternion.LookRotation(hit.normal), AnnotationObject.transform)); }
            void SelectExistingDot(GameObject obj) { (MovingDot = obj).layer = 2; }
            void HandleSelectedDot()
            {
                if (ButtonPressed(InputMapping.AnnotationInput.DestroyAnnotation))
                {
                    DestroyDot(MovingDot);
                    MovingDot = null;
                    return;
                }

                // Only display annotation dot if hit object (Disallows annotating annotation dots...)
                if (!ObjectHit().Equals(AnnotationObject)) return;

                MovingDot.SetActive(true);
                MovingDot.transform.position = hit.point;
                MovingDot.transform.rotation = Quaternion.LookRotation(hit.normal);

                // Places the annotation point on button press.
                if (!ButtonPressed(InputMapping.AnnotationInput.Annotate)) return;
                MovingDot.layer = 0;
                MovingDot = null;
            }
            void DisableDotRender()
            {
                MovingDot.SetActive(false);
                if (!ButtonPressed(InputMapping.AnnotationInput.DestroyAnnotation)) return;
                Destroy(MovingDot);
                MovingDot = null;
            }
            void DestroyDot(GameObject obj) { AnnotationPoints.Remove(obj); Destroy(obj); }  // Create particle effect for feedback
            bool SelectedDotExists() { return MovingDot != null; }
            bool ButtonPressed(InputMapping.AnnotationInput input) { return InputMapping.PressedThisFrame(input); }
            GameObject ObjectHit() { return hit.collider.transform.gameObject; }
        }

        void TransformAnnotationObject()
        {
            if (InputMapping.HeldThisFrame(InputMapping.AnnotationInput.SizeUp))
                Origin.localScale *= 1.03f;

            if (InputMapping.HeldThisFrame(InputMapping.AnnotationInput.SizeDown) && Origin.localScale.x > 1)
                Origin.localScale /= 1.03f;

            Vector2 lAxis = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick); // Left thumb-stick
            Vector2 rAxis = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick); // Right thumb-stick

            Translate(lAxis);
            Rotate(-rAxis, Origin);

            if (InputMapping.PressedThisFrame(InputMapping.AnnotationInput.ResetTransform))
                ResetTransformations();

            if (InputMapping.PressedThisFrame(InputMapping.AnnotationInput.SwitchDominantHand))
                SwitchHand();


            void Rotate(Vector2 axis, Transform t)
            {
                t.Rotate(Vector3.down, axis.x * 0.7f * Mathf.Pow(Origin.localScale.x, RotationExponent), Space.World);
                t.Rotate(Vector3.right, axis.y * 0.7f * Mathf.Pow(Origin.localScale.x, RotationExponent), Space.World);
            }

            void Translate(Vector2 axis) { transform.Translate(new Vector3(axis.x, 0, axis.y) * 0.006f * Mathf.Pow(Origin.localScale.x, MovementExponent), Space.World); }

            void SwitchHand()
            {
                var RightController = RightGunTip.transform.parent.parent.gameObject;
                var LeftController = LeftGunTip.transform.parent.parent.gameObject;
                RightController.SetActive(!RightController.activeSelf);
                LeftController.SetActive(!LeftController.activeSelf);

                InputMapping.SwitchDominantHand();
            }
        }

    }

    private void FixedUpdate()
    {
        OVRInput.FixedUpdate();

        if (Rigidbody.velocity == Vector3.zero && Rigidbody.angularVelocity == Vector3.zero) return;
        Rigidbody.velocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
    }

    //private void OnApplicationQuit() { ExportAnnotations(); }

    private void ResetTransformations()
    {
        Origin.rotation = Quaternion.identity; // Thumb-stick rotation
        Origin.localScale = Vector3.one;
        transform.position = DefaultPosition;
        transform.rotation = DefaultRotation; // Hand-grab rotations
    }

    public void ClearAnnotations()
    {
        foreach (GameObject point in AnnotationPoints)
            Destroy(point);
        AnnotationPoints.Clear();
    }

    public void ImportAnnotations()
    {
        DataHandler.Import(@"C:\Meine Items\Coding Ambitions\10th Semester\Data\Landmarks 2\" + DataSet, LandmarksToImport);
        SpawnImportedAnnotationDots();

        // Spawns annotation points from data. The annotation object's transform must be reset for the data to load properly.
        void SpawnImportedAnnotationDots()
        {
            ResetTransformations();
            var (position, rotation, scale) = SaveTransformValues(AnnotationObject.transform);
            ResetToIdentityTransform(AnnotationObject.transform);

            // Spawn in world space but as child objects.
            for (int i = 0; i < DataHandler.Positions.Count; ++i)
                if (!DataHandler.Positions[i].Equals(Vector3.zero))
                    AnnotationPoints.Add(
                        Instantiate(AnnotationDotPrefab, DataHandler.Positions[i], Quaternion.LookRotation(DataHandler.Normals[i]), AnnotationObject.transform));

            ResetModelCalibration(position, rotation, scale);
        }
    }

    public void ExportAnnotations()
    {
        if (AnnotationPoints.Count == 0) return; // Don't export empty data sets.

        ResetTransformations();
        var (position, rotation, scale) = SaveTransformValues(AnnotationObject.transform);
        ResetToIdentityTransform(AnnotationObject.transform);

        string fileName = @"C:\Meine Items\Coding Ambitions\10th Semester\Exported Data\" +
            DataSet.Split('_')[0] +
            "__" +
            System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        DataHandler.Export(fileName + ".csv", AnnotationPoints);

        ResetModelCalibration(position, rotation, scale);

        System.IO.File.WriteAllText(fileName + ".txt", Time.realtimeSinceStartup.ToString());
    }

    public void ImportEmptyAnnotationPage() { DataHandler.Import(@"C:\Meine Items\Coding Ambitions\10th Semester\Data\empty10.csv"); }

    private (Vector3 position, Quaternion rotation, Vector3 scale) SaveTransformValues(Transform transform)
    {
        return (transform.position, transform.rotation, transform.localScale);
    }

    private void ResetToIdentityTransform(Transform transform)
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = 10 * Vector3.one;
    }

    private void ResetModelCalibration(Vector3 savedPosition, Quaternion savedRotation, Vector3 savedScale)
    {
        AnnotationObject.transform.position = savedPosition;
        AnnotationObject.transform.rotation = savedRotation;
        AnnotationObject.transform.localScale = savedScale;
    }

}
