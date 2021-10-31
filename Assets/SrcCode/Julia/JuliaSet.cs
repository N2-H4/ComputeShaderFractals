using UnityEngine.UI;
using UnityEngine;

public class JuliaSet : MonoBehaviour
{
    double width, height; //size of rendered image
    double IMstarting, REstarting; //starting points on complex plane
    public double cReal, cImag; //coefficient c
    int maxIterations, increment; //max iterations and amount of iteration increment per zoom in frame
    float zoomAmount; //amount of zoom per frame
    float timePassed=0; //time used for animation
    bool animate = false;

    public ComputeShader shader; //ref to compute shader
    ComputeBuffer buffer; //used for passing data to shader
    RenderTexture renderTexture; //texture in which shader will output image
    public RawImage image; //ui element that will display rendered image

    //Data struct contains data to be passed in compute buffer
    public struct Data
    {
        public double w, h, i, r;
        public double cReal, cImag;
        public int screenWidth, screenHeight;
        public Data(double w, double h, double i, double r, int sw, int sh,double cReal,double cImag)
        {
            this.w = w;
            this.h = h;
            this.i = i;
            this.r = r;
            this.screenHeight = sh;
            this.screenWidth = sw;
            this.cReal = cReal;
            this.cImag = cImag;
        }
    }

    //compute buffer will be constructed from this array
    Data[] shaderData;

    void Start()
    {
        //initializing values
        Application.targetFrameRate = 60;
        width = 4.5;
        height = width * Screen.height / Screen.width;
        IMstarting = -1.25f;
        REstarting = -2.0f;
        cReal = 0.7885*Mathf.Cos(0);
        cImag = 0.7885 * Mathf.Sin(0);
        maxIterations = 500;
        increment = 3;
        zoomAmount = 0.5f;
        
        //buffer will contain only one data element
        shaderData = new Data[1];
        shaderData[0] = new Data(width, height, IMstarting, REstarting, Screen.width, Screen.height,cReal,cImag);

        //creating ComputeBuffer object
        buffer = new ComputeBuffer(shaderData.Length, 6*sizeof(double)+2*sizeof(int)); //40 is the size of package 6*double + 2*int

        //creating render texture
        renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        //rendering julia fractal
        julia();
    }


    void Update()
    {
        //animating coefficent c
        if(animate) animateValues();
        //if LMB pressed zoom in
        if (Input.GetMouseButton(0)) zoomIn();
        //if RMB pressef zoom out
        if (Input.GetMouseButton(1)) zoomOut();
        //if MMB pressed center on mouse position
        if (Input.GetMouseButtonDown(2)) centerScreen();

        //taking input
        if (Input.GetKeyDown(KeyCode.Escape)) UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        if (Input.GetKeyDown(KeyCode.X)) saveScreenShot();

        //rendering fractal
        julia();
    }

    public void saveScreenShot()
    {
        RenderTexture.active = renderTexture;
        Texture2D temp = new Texture2D(Screen.width, Screen.height);
        temp.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        byte[] bytes = temp.EncodeToPNG();
        System.IO.File.WriteAllBytes("RenderingResults\\JuliaSet\\Screenshots\\ss.png", bytes);
    }

    void animateValues()
    {
        //calculate coefficient c depending on time passed
        timePassed += Time.deltaTime;
        cReal = 0.7885 * Mathf.Cos(Mathf.LerpAngle(0, 2 * Mathf.PI, 0.05f * timePassed));
        cImag = 0.7885 * Mathf.Sin(Mathf.LerpAngle(0, 2 * Mathf.PI, 0.05f * timePassed));
        //put data in buffer
        shaderData[0].cReal = cReal;
        shaderData[0].cImag = cImag;
    }

    public void setAnimate(bool value)
    {
        animate = value;
    }

    void centerScreen()
    {
        //calculate new starting point based on mouse position
        REstarting += (Input.mousePosition.x - (Screen.width / 2.0)) / Screen.width * width;
        IMstarting += (Input.mousePosition.y - (Screen.height / 2.0)) / Screen.height * height;

        //put data in buffer
        shaderData[0].r = REstarting;
        shaderData[0].i = IMstarting;

        //render fractal
        julia();
    }

    void zoomIn()
    {
        //increase max number of iterations
        maxIterations = Mathf.Max(100, maxIterations + increment);
        //calculate new image size andstarting pos
        double wFactor = width * zoomAmount * Time.deltaTime;
        double hFactor = height * zoomAmount * Time.deltaTime;
        width -= wFactor;
        height -= hFactor;
        REstarting += wFactor / 2.0;
        IMstarting += hFactor / 2.0;

        //put data in buffer
        shaderData[0].w = width;
        shaderData[0].h = height;
        shaderData[0].r = REstarting;
        shaderData[0].i = IMstarting;

        //render fractal
        julia();
    }

    void zoomOut()
    {
        //maxIterations = Mathf.Max(100, maxIterations + increment);
        //calculate new image size and starting pos
        double wFactor = width * zoomAmount * Time.deltaTime;
        double hFactor = height * zoomAmount * Time.deltaTime;
        width += wFactor;
        height += hFactor;
        REstarting -= wFactor / 2.0;
        IMstarting -= hFactor / 2.0;

        //put data in buffer
        shaderData[0].w = width;
        shaderData[0].h = height;
        shaderData[0].r = REstarting;
        shaderData[0].i = IMstarting;

        //render fractal
        julia();
    }

    void julia()
    {
        //get id of shader kernel by name
        int kernelHandle = shader.FindKernel("CSMain");
        //connect buffer to shader
        buffer.SetData(shaderData);
        shader.SetBuffer(kernelHandle, "buffer", buffer);
        //connect var maxIterations to shader
        shader.SetInt("maxIterations", maxIterations);
        //connect result texture to shader
        shader.SetTexture(kernelHandle, "Result", renderTexture);
        //run shader and use given number of thread groups
        shader.Dispatch(kernelHandle, Screen.width / 32, Screen.height / 32, 1);
        //assign now filled render texture to image
        RenderTexture.active = renderTexture;
        image.material.mainTexture = renderTexture;
    }

    private void OnDestroy()
    {
        //dispose buffer when script stops running
        if(buffer!=null) buffer.Dispose();
    }
}
