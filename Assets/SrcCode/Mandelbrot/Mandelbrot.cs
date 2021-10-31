using UnityEngine.UI;
using UnityEngine;

//this script uses compute shader to render Mandelbrot fractal on RawImage ui element
public class Mandelbrot : MonoBehaviour
{
    double width, height; //size of image
    double IMstarting, REstarting; //starting point on complex plane
    int maxIterations, increment; //max iterations and increment by with increase max iterations while zooming
    float zoomAmount; //zoom amount during one frame of zooming

    public ComputeShader shader; //ref to compute shader
    ComputeBuffer buffer; //buffer used for passing data to shader
    RenderTexture renderTexture; //shader renders into this texture
    public RawImage image; //ui element with will display rendered frames

    //stuff for maing screenshots
    int imgNum = 1;
    bool record = false;

    //struct containing information to be passed to shader each frame
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

    //compute buffer will be made out of this array
    Data[] shaderData;
    
    void Start()
    {
        Application.targetFrameRate = 20;
        //initialize values
        width = 4.5;
        height = width * Screen.height / Screen.width;
        IMstarting = -1.25f;
        REstarting = -2.0f;
        maxIterations = 500;
        increment = 3;
        zoomAmount = 0.5f;

        //compute buffer will be containing only one element of Data type
        shaderData = new Data[1];
        shaderData[0] = new Data(width, height, IMstarting, REstarting, Screen.width, Screen.height);

        //creating ComputeBuffer object
        buffer = new ComputeBuffer(shaderData.Length, 40); //40 is the size of package 4*double + 2*int

        //creating output texture
        renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        //calling function to render the fractal
        mandelbrot();
    }

    
    void Update()
    {
        //if LMB pressed zoom in,if RMB pressed zoom out, if MMB pressed center image on mouse position
        if (Input.GetMouseButton(0)) zoomIn();
        if (Input.GetMouseButton(1)) zoomOut();
        if (Input.GetMouseButtonDown(2)) centerScreen();

        //taking input
        if (Input.GetKeyDown(KeyCode.Escape)) UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        if (Input.GetKeyDown(KeyCode.X)) saveScreenShot();
        if(Input.GetKeyDown(KeyCode.Space))
        {
            record = !record;
            zoomAmount = 0.05f;
        }
        genImageSequence();
    }

    public void saveScreenShot()
    {
        RenderTexture.active = renderTexture;
        Texture2D temp = new Texture2D(Screen.width, Screen.height);
        temp.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        byte[] bytes = temp.EncodeToPNG();
        System.IO.File.WriteAllBytes("RenderingResults\\MandelbrotSet\\Screenshots\\ss.png", bytes);
    }

    public void genImageSequence()
    {
        if(record)
        {
            RenderTexture.active = renderTexture;
            Texture2D temp = new Texture2D(Screen.width, Screen.height);
            temp.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            byte[] bytes = temp.EncodeToPNG();
            System.IO.File.WriteAllBytes("RenderingResults\\MandelbrotSet\\ImageSequence\\" + imgNum + ".png", bytes);
            imgNum++;
            zoomOut();
        }
    }

    void centerScreen()
    {
        //function will center image on mouse position
        //use mouse position as starting point of calculations on compelx plane
        REstarting += (Input.mousePosition.x - (Screen.width / 2.0)) / Screen.width * width;
        IMstarting += (Input.mousePosition.y - (Screen.height / 2.0)) / Screen.height * height;

        //put data in compute buffer
        shaderData[0].r = REstarting;
        shaderData[0].i = IMstarting;

        //render image
        mandelbrot();
    }

    void zoomIn()
    {
        //function will zoom in on image
        //increase max Iterations
        maxIterations = Mathf.Max(100, maxIterations + increment);
        //decrease width and height and change starting starting point
        double wFactor = width * zoomAmount * Time.deltaTime;
        double hFactor = height * zoomAmount * Time.deltaTime;
        width -= wFactor;
        height -= hFactor;
        REstarting += wFactor / 2.0;
        IMstarting += hFactor / 2.0;

        //put information in compute buffer
        shaderData[0].w = width;
        shaderData[0].h = height;
        shaderData[0].r = REstarting;
        shaderData[0].i = IMstarting;

        //render image
        mandelbrot();
    }

    void zoomOut()
    {
        //function will zoom out of image
        //decrease amount of iterations
        //maxIterations = Mathf.Max(100, maxIterations - increment);
        //increase image size and change starting point
        double wFactor = width * zoomAmount * Time.deltaTime;
        double hFactor = height * zoomAmount * Time.deltaTime;
        width += wFactor;
        height += hFactor;
        REstarting -= wFactor / 2.0;
        IMstarting -= hFactor / 2.0;

        //put data in compute buffer
        shaderData[0].w = width;
        shaderData[0].h = height;
        shaderData[0].r = REstarting;
        shaderData[0].i = IMstarting;

        //render image
        mandelbrot();
    }

    void mandelbrot()
    {
        //function will setup shader, run it and assigne output texture to RawImage ui element
        //get kernel id by its name (kernel is like main function)
        int kernelHandle = shader.FindKernel("CSMain");
        //assing buffer to shader
        buffer.SetData(shaderData);
        shader.SetBuffer(kernelHandle, "buffer", buffer);
        //connect maxIterations variable inside c# script with one inside shader
        shader.SetInt("maxIterations", maxIterations);
        //connect renderTexture with texture variable inside shader
        shader.SetTexture(kernelHandle, "Result", renderTexture);
        //run shader,pass thread groups sizes
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
