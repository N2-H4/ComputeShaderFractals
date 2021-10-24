using UnityEngine.UI;
using UnityEngine;

public class JuliaSet : MonoBehaviour
{
    double width, height;
    double IMstarting, REstarting;
    public double cReal, cImag;
    int maxIterations, increment;
    float zoomAmount;
    float timePassed=0;

    public ComputeShader shader;
    ComputeBuffer buffer; //used for passing data to shader
    RenderTexture renderTexture;
    public RawImage image;

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

    Data[] shaderData;

    void Start()
    {
        width = 4.5;
        height = width * Screen.height / Screen.width;
        IMstarting = -1.25f;
        REstarting = -2.0f;
        cReal = 0.7885*Mathf.Cos(0);
        cImag = 0.7885 * Mathf.Sin(0);
        maxIterations = 500;
        increment = 3;
        zoomAmount = 0.5f;

        shaderData = new Data[1];
        shaderData[0] = new Data(width, height, IMstarting, REstarting, Screen.width, Screen.height,cReal,cImag);

        buffer = new ComputeBuffer(shaderData.Length, 6*sizeof(double)+2*sizeof(int)); //40 is the size of package 6*double + 2*int

        renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        julia();
    }


    void Update()
    {
        timePassed += Time.deltaTime;
        cReal = 0.7885 * Mathf.Cos(Mathf.LerpAngle(0, 2 * Mathf.PI,0.05f*timePassed));
        cImag = 0.7885 * Mathf.Sin(Mathf.LerpAngle(0, 2 * Mathf.PI, 0.05f*timePassed));
        shaderData[0].cReal = cReal;
        shaderData[0].cImag = cImag;
        julia();
        if (Input.GetMouseButton(0)) zoomIn();
        if (Input.GetMouseButton(1)) zoomOut();
        if (Input.GetMouseButtonDown(2)) centerScreen();
    }

    void centerScreen()
    {
        REstarting += (Input.mousePosition.x - (Screen.width / 2.0)) / Screen.width * width;
        IMstarting += (Input.mousePosition.y - (Screen.height / 2.0)) / Screen.height * height;

        shaderData[0].r = REstarting;
        shaderData[0].i = IMstarting;

        julia();
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

        julia();
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

        julia();
    }

    void julia()
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
        if(buffer!=null) buffer.Dispose();
    }
}
