using UnityEngine;
using UnityEngine.UI;

public class Sierpinski3D : MonoBehaviour
{

    public ComputeShader fractalShader; //compute shader
    public InputField epsiloneField; //ui field

    //fractal coeffcients
    public float darkness = 70;
    public float epsilon=0.01f;
    int eps=1;
    public float blackAndWhite;
    public float redA;
    public float greenA;
    public float blueA = 1;
    public float redB = 1;
    public float greenB;
    public float blueB;

    RenderTexture target; //output texture
    Camera cam;
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
    }

    public void setEpsilone(float value)
    {
        //set epsilone value form ui
        eps = (int)value;
        epsilon = 1 / (Mathf.Pow(10, eps));
        epsiloneField.text = value.ToString();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //create render texture
        InitRenderTexture();
        //link variables to shader
        SetParameters();

        //run shader
        int threadGroupsX = Mathf.CeilToInt(cam.pixelWidth / 16.0f);
        int threadGroupsY = Mathf.CeilToInt(cam.pixelHeight / 16.0f);
        fractalShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        //show rendered texture on screen
        Graphics.Blit(target, destination);
    }

    void SetParameters()
    {
        //link variables to shader
        fractalShader.SetMatrix("_CameraToWorld", cam.cameraToWorldMatrix);
        fractalShader.SetMatrix("_CameraInverseProjection", cam.projectionMatrix.inverse);
        fractalShader.SetVector("_LightDirection", directionalLight.transform.forward);
        fractalShader.SetTexture(0, "Destination", target);
        fractalShader.SetFloat("darkness", darkness);
        fractalShader.SetFloat("blackAndWhite", blackAndWhite);
        fractalShader.SetVector("colourAMix", new Vector3(redA, greenA, blueA));
        fractalShader.SetVector("colourBMix", new Vector3(redB, greenB, blueB));
        fractalShader.SetFloat("epsilon", epsilon);
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