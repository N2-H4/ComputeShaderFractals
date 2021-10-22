using UnityEngine.UI;
using UnityEngine;

public class Mandelbrot : MonoBehaviour
{
    double width, height;
    double IMstarting, REstarting;
    int maxIterations, increment;
    float zoomAmount;

    public ComputeShader shader;
    ComputeBuffer buffer; //used for passing data to shader
    RenderTexture renderTexture;
    public RawImage image;

    public struct Data
    {
        public double w, h, i, r;
        public int screenWidth, screenHeight;
        public Data(double w,double h,double i,double r,int sw,int sh)
        {
            this.w = w;
            this.h = h;
            this.i = i;
            this.r = r;
            this.screenHeight = sh;
            this.screenWidth = sw;
        }
    }

    Data[] shaderData;
    
    void Start()
    {
        //width = 4.5;
        //height = width * Screen.height / Screen.width;
        //IMstarting = -1.25f;
        //REstarting = -2.0f;
        //maxIterations = 500;
        //increment = 3;
        //zoomAmount = 0.5f;

        //shaderData = new Data[1];
        //shaderData[0] = new Data(width, height, IMstarting, REstarting, Screen.width, Screen.height);

        //buffer = new ComputeBuffer(shaderData.Length, 40); //40 is the size of package 4*double + 2*int

        //renderTexture = new RenderTexture(Screen.width, Screen.height,0);
        //renderTexture.enableRandomWrite = true;
        //renderTexture.Create();

        //mandelbrot();
    }

    
    void Update()
    {
        //if (Input.GetMouseButton(0)) zoomIn();
        //if (Input.GetMouseButton(1)) zoomOut();
        //if (Input.GetMouseButtonDown(2)) centerScreen();
    }

    void centerScreen()
    {
        REstarting += (Input.mousePosition.x - (Screen.width / 2.0)) / Screen.width * width;
        IMstarting += (Input.mousePosition.y - (Screen.height / 2.0)) / Screen.height * height;

        shaderData[0].r = REstarting;
        shaderData[0].i = IMstarting;

        mandelbrot();
    }

    void zoomIn()
    {
        maxIterations = Mathf.Max(100, maxIterations + increment);
        double wFactor = width * zoomAmount * Time.deltaTime;
        double hFactor = height * zoomAmount * Time.deltaTime;
        width -= wFactor;
        height -= hFactor;
        REstarting += wFactor / 2.0;
        IMstarting += hFactor / 2.0;

        shaderData[0].w = width;
        shaderData[0].h = height;
        shaderData[0].r = REstarting;
        shaderData[0].i = IMstarting;

        mandelbrot();
    }

    void zoomOut()
    {
        maxIterations = Mathf.Max(100, maxIterations + increment);
        double wFactor = width * zoomAmount * Time.deltaTime;
        double hFactor = height * zoomAmount * Time.deltaTime;
        width += wFactor;
        height += hFactor;
        REstarting -= wFactor / 2.0;
        IMstarting -= hFactor / 2.0;

        shaderData[0].w = width;
        shaderData[0].h = height;
        shaderData[0].r = REstarting;
        shaderData[0].i = IMstarting;

        mandelbrot();
    }

    void mandelbrot()
    {
        int kernelHandle = shader.FindKernel("CSMain");
        buffer.SetData(shaderData);
        shader.SetBuffer(kernelHandle, "buffer", buffer);
        shader.SetInt("maxIterations", maxIterations);
        shader.SetTexture(kernelHandle, "Result", renderTexture);
        shader.Dispatch(kernelHandle, Screen.width / 32, Screen.height / 32, 1);
        RenderTexture.active = renderTexture;
        image.material.mainTexture = renderTexture;
    }

    private void OnDestroy()
    {
       // buffer.Dispose();
    }
}
