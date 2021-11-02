using UnityEngine;
using UnityEngine.UI;

public class Mandelbulb : MonoBehaviour
{

    public ComputeShader fractalShader; //Compute shader
    //UI fields
    public InputField powerField;
    public InputField epsilonField;

    //fractal coefficients
    public float fractalPower = 10;
    public float darkness = 70;
    public int eps = 1;
    const float baseEpsilone = 0.0001f;
    float epsilon = 0.0001f;

    //variables for coloring fractal
    public float blackAndWhite;
    public float redA;
    public float greenA;
    public float blueA = 1;
    public float redB = 1;
    public float greenB;
    public float blueB;

    RenderTexture target; //texture containing result of rendering
    Camera cam;
    CamControl cc;
    Light directionalLight;

    void Start()
    {
        Application.targetFrameRate = 60;
        Init();
    }

    void Init()
    {
        //initialize game objects
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        cc = GameObject.Find("Main Camera").GetComponent<CamControl>();
        directionalLight = FindObjectOfType<Light>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //go back to menu when esc pressed
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
        //change camera movement speed based on distance from fractal
        float val = SceneInfo(cam.transform.position).x;
        if(val!=0)
        {
            cc.setMovementSpeed(1 / (val*val*val));
            epsilon = baseEpsilone / (Mathf.Pow(2, eps));
        }
    }

    public void setPower(float value)
    {
        //set power coefficient
        fractalPower = value;
        powerField.text = value.ToString();
    }

    public void setEpsilone(float value)
    {
        //set epsilone coefficient
        eps = (int)value;
        epsilonField.text = value.ToString();
    }

    Vector2 SceneInfo(Vector3 position)
    {
        //calculate distance form fractal
        Vector3 z = position;
        float dr = 1.0f;
        float r = 0.0f;
        int iterations = 0;

        for (int i = 0; i < 50; i++)
        {
            iterations = i;
            r = z.magnitude;

            if (r > 2)
            {
                break;
            }

            // convert to polar coordinates
            float theta = Mathf.Acos(z.z / r);
            float phi = Mathf.Atan2(z.y, z.x);
            dr = Mathf.Pow(r, fractalPower - 1.0f) * fractalPower * dr + 1.0f;

            // scale and rotate the point
            float zr = Mathf.Pow(r, fractalPower);
            theta = theta * fractalPower;
            phi = phi * fractalPower;

            // convert back to cartesian coordinates
            z = zr * new Vector3(Mathf.Sin(theta) * Mathf.Cos(phi), Mathf.Sin(phi) * Mathf.Sin(theta), Mathf.Cos(theta));
            z += position;
        }
        float dst = 0.5f * Mathf.Log(r) * r / dr;
        return new Vector2(iterations, dst);
    }


    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        InitRenderTexture(); //create output texture
        SetParameters(); //link variables to shader

        //run shader
        int threadGroupsX = Mathf.CeilToInt(cam.pixelWidth / 16.0f);
        int threadGroupsY = Mathf.CeilToInt(cam.pixelHeight / 16.0f);
        fractalShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        //show output texture on screen
        Graphics.Blit(target, destination);
    }

    void SetParameters()
    {
        //link varables to shader
        fractalShader.SetTexture(0, "Destination", target);
        fractalShader.SetFloat("power", Mathf.Max(fractalPower, 1.01f));
        fractalShader.SetFloat("darkness", darkness);
        fractalShader.SetFloat("blackAndWhite", blackAndWhite);
        fractalShader.SetVector("colourAMix", new Vector3(redA, greenA, blueA));
        fractalShader.SetVector("colourBMix", new Vector3(redB, greenB, blueB));
        fractalShader.SetFloat("epsilon", epsilon);
        fractalShader.SetMatrix("_CameraToWorld", cam.cameraToWorldMatrix);
        fractalShader.SetMatrix("_CameraInverseProjection", cam.projectionMatrix.inverse);
        fractalShader.SetVector("_LightDirection", directionalLight.transform.forward);

    }

    void InitRenderTexture()
    {
        //create output texture
        if (target == null || target.width != cam.pixelWidth || target.height != cam.pixelHeight)
        {
            if (target != null)
            {
                target.Release();
            }
            target = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();
        }
    }
}
