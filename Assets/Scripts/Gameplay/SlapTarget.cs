using UnityEngine;

public class SlapTarget : MonoBehaviour
{
    [SerializeField] private Transform face;
    [SerializeField] private Transform head;
    [SerializeField] private Transform body;

    private SlapLabRuntime runtime;
    private Material skinMaterial;
    private Material shirtMaterial;
    private Material faceMaterial;

    public Transform Face => face;
    public Transform Head => head;
    public Transform Body => body;

    public void Initialize(SlapLabRuntime owner)
    {
        runtime = owner;
        BuildTarget();
    }

    private void BuildTarget()
    {
        if (body != null)
            return;

        skinMaterial = runtime != null
            ? runtime.Mat(new Color(0.55f, 0.20f, 0.16f), 0.0f)
            : MakeMaterial(new Color(0.55f, 0.20f, 0.16f));

        shirtMaterial = runtime != null
            ? runtime.Mat(new Color(0.06f, 0.10f, 0.18f), 0.25f)
            : MakeMaterial(new Color(0.06f, 0.10f, 0.18f));

        faceMaterial = runtime != null
            ? runtime.Mat(new Color(0.72f, 0.30f, 0.22f), 0.0f)
            : MakeMaterial(new Color(0.72f, 0.30f, 0.22f));

        GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        torso.name = "Body";
        torso.transform.SetParent(transform, false);
        torso.transform.localPosition = new Vector3(0f, -0.55f, 0f);
        torso.transform.localScale = new Vector3(0.62f, 0.85f, 0.42f);
        torso.GetComponent<Renderer>().material = shirtMaterial;
        body = torso.transform;

        GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        neck.name = "Neck";
        neck.transform.SetParent(transform, false);
        neck.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        neck.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
        neck.GetComponent<Renderer>().material = skinMaterial;

        GameObject headObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        headObject.name = "Head";
        headObject.transform.SetParent(transform, false);
        headObject.transform.localPosition = new Vector3(0f, 0.62f, 0f);
        headObject.transform.localScale = new Vector3(0.68f, 0.78f, 0.58f);
        headObject.GetComponent<Renderer>().material = skinMaterial;
        head = headObject.transform;

        GameObject faceObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        faceObject.name = "Face";
        faceObject.transform.SetParent(head, false);
        faceObject.transform.localPosition = new Vector3(0f, -0.02f, -0.48f);
        faceObject.transform.localScale = new Vector3(0.72f, 0.62f, 0.20f);
        faceObject.GetComponent<Renderer>().material = faceMaterial;
        face = faceObject.transform;

        CreateEye("LeftEye", new Vector3(-0.15f, 0.08f, -0.66f));
        CreateEye("RightEye", new Vector3(0.15f, 0.08f, -0.66f));
        CreateMouth();

        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void CreateEye(string eyeName, Vector3 localPosition)
    {
        GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eye.name = eyeName;
        eye.transform.SetParent(head, false);
        eye.transform.localPosition = localPosition;
        eye.transform.localScale = Vector3.one * 0.075f;
        eye.GetComponent<Renderer>().material =
            runtime != null ? runtime.Mat(Color.white, 0f) : MakeMaterial(Color.white);
    }

    private void CreateMouth()
    {
        GameObject mouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mouth.name = "Mouth";
        mouth.transform.SetParent(head, false);
        mouth.transform.localPosition = new Vector3(0f, -0.17f, -0.66f);
        mouth.transform.localScale = new Vector3(0.25f, 0.035f, 0.025f);
        mouth.GetComponent<Renderer>().material =
            runtime != null ? runtime.Mat(new Color(0.08f, 0.01f, 0.01f), 0f)
                            : MakeMaterial(new Color(0.08f, 0.01f, 0.01f));
    }

    public void RegisterHit(float power, Vector3 point, Vector3 normal)
    {
        if (runtime != null)
            runtime.RegisterHit(power, point, normal);
    }

    private Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        Material material = new Material(shader);
        material.color = color;
        return material;
    }
}
