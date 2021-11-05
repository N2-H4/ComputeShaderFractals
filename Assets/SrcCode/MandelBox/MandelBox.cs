using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MandelBox : MonoBehaviour
{
    public ComputeShader fractalShader; //compute shader
    public InputField epsiloneField;
    public InputField scaleField;
    public InputField foldingField;
    public InputField minRadiusField;
    public InputField fixedRadiusField;

    //fractal coeffcients
    float scale=2.0f;
    float foldingLimit = 1.0f;
    float minRadius = 2.0f;
    float fixedRadius = 5.0f;
    float epsilon = 0.01f;
    int eps = 1;

    RenderTexture target; //output texture
    Camera cam;
    Light directionalLight;

    void Start()
    {
        Application.targetFrameRate = 60;
        Init();
        cam.gameObject.GetComponent<CamControl>().setMovementSpeed(1.0f);
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

    public void setScale(float value)
    {
        scale = value;
        scaleField.text = value.ToString();
    }

    public void setFolding(float value)
    {
        foldingLimit = value;
        foldingField.text = value.ToString();
    }

    public void setMinRadius(float value)
    {
        minRadius = value;
        minRadiusField.text = value.ToString();
    }

    public void setFixedRadius(float value)
    {
        fixedRadius = value;
        fixedRadiusField.text = value.ToString();
    }

    public void setSpeed(float value)
    {
        cam.GetComponent<CamControl>().setMovementSpeed(value);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //create render texture
        InitRenderTexture();
        //link variables to shader
        SetParameters();

        //run shader
        int threadGroupsX = Mathf.CeilToInt(cam.pixelWidth / 32.0f);
        int threadGroupsY = Mathf.CeilToInt(cam.pixelHeight / 32.0f);
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
        fractalShader.SetFloat("Scale", scale);
        fractalShader.SetFloat("foldingLimit", foldingLimit);
        fractalShader.SetFloat("minRadius2", minRadius);
        fractalShader.SetFloat("fixedRadius2", fixedRadius);
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
