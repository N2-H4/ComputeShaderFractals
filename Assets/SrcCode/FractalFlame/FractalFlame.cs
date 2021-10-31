using UnityEngine.UI;
using UnityEngine;
using System;
using System.Collections.Generic;

public class FractalFlame : MonoBehaviour
{
    public RawImage img;
    int sizex = 1000;
    int sizey = 1000;
    float[] a, b, c, d, e, f;
    Color[] colors;
    List<Vector2> plot;
    List<Color> plotColor;
    Texture2D tex;
    float maxx = float.MinValue, minx = float.MaxValue, maxy = float.MinValue, miny = float.MaxValue;
    int[,] density;
    Color[,] densityColor;
    int maxDensity;

    void Start()
    {
        setCoefficients(-1.0f,1.0f,10);
        
        plot = new List<Vector2>();
        plotColor = new List<Color>();
        density = new int[sizex, sizey];
        densityColor = new Color[sizex, sizey];
        tex = new Texture2D(sizex, sizey, TextureFormat.ARGB32, false);

        renderFractal();


        Debug.Log(minx + " " + maxx);
        Debug.Log(miny + " " + maxy);

        calculateDensity();

        drawTexture();
        tex.Apply();
        img.material.mainTexture = tex;
    }

    void renderFractal()
    {
        Color randColor = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
        float x = UnityEngine.Random.Range(-1.0f, 1.0f);
        float y = UnityEngine.Random.Range(-1.0f, 1.0f);
        for (int ii = 0; ii < 5000000; ii++)
        {
            int rand = UnityEngine.Random.Range(0, 10);
            randColor = (randColor + colors[rand]) / 2;
            Vector2 temp;
            temp = affine(x, y, a[rand], b[rand], c[rand], d[rand], e[rand], f[rand]);
            x = temp.x;
            y = temp.y;


            //temp2 = cross(x, y);

            //x = temp.x + temp2.x;
            //y = temp.y + temp2.y;



            if (x > maxx) maxx = x;
            if (x < minx) minx = x;
            if (y > maxy) maxy = y;
            if (y < miny) miny = y;

            if (ii > 20)
            {
                if (float.IsNaN(x) == false && float.IsInfinity(x) == false)
                {
                    if (float.IsNaN(y) == false && float.IsInfinity(y) == false)
                    {
                        plot.Add(new Vector2(x, y));
                        plotColor.Add(new Color(randColor.r, randColor.g, randColor.b));
                    }
                }

            }
        }
    }

    Vector2 fern(float x,float y,int n)
    {
        if (n >= 0 && n <= 84) return new Vector2(0.85f * x + 0.04f * y, -0.04f * x + 0.85f * y + 1.6f);
        if (n >= 84 && n <= 91) return new Vector2(-0.15f * x + 0.28f * y, 0.26f * x + 0.24f * y + 0.44f);
        if (n >= 92 && n <= 99) return new Vector2(0.2f * x - 0.26f * y, 0.23f * x + 0.22f * y + 1.6f);
        return new Vector2(0.0f, 0.16f * y);
    }

    Vector2 func(float x,float y,int n)
    {
        if (n == 0) return new Vector2(0.5f*x-0.5f*y,0.5f*x+0.5f*y);
        return new Vector2(-0.5f*x-0.5f*y+1,0.5f*x-0.5f*y);
    }

    Vector2 affine(float x,float y,float a,float b,float c,float d,float e,float f)
    {
        return new Vector2(x * a + b * y + c, x * d + y * e + f);
    }

    Vector2 sinusoidal(float x,float y)
    {
        return new Vector2(Mathf.Sin(x), Mathf.Sin(y));
    }

    Vector2 tangent(float x,float y)
    {
        return new Vector2(Mathf.Sin(x) / Mathf.Cos(y), Mathf.Tan(y));
    }

    Vector2 spherical(float x,float y)
    {
        float r = Mathf.Sqrt(x * x + y * y);
        return new Vector2((1 / (r * r)) * x, (1 / (r * r)) * y);
    }

    Vector2 swirl(float x,float y)
    {
        float r = Mathf.Sqrt(x * x + y * y);
        return new Vector2(x * Mathf.Sin(r * r) - y * Mathf.Cos(r * r), x * Mathf.Cos(r * r) + y * Mathf.Sin(r * r));
    }

    Vector2 fisheye(float x,float y)
    {
        float r = Mathf.Sqrt(x * x + y * y);
        return new Vector2((2 / (r + 1)) * y, (2 / (r + 1)) * x);
    }

    Vector2 bubble(float x,float y)
    {
        float r = Mathf.Sqrt(x * x + y * y);
        return new Vector2((4 / ((r * r) + 4)) * x, (4 / ((r * r) + 4)) * y);
    }

    Vector2 popcorn(float x,float y,float c,float f)
    {
        return new Vector2(x + c * Mathf.Sin(Mathf.Tan(3 * y)), y + f * Mathf.Sin(Mathf.Tan(3 * x)));
    }

    Vector2 cross(float x,float y)
    {
        float val = Mathf.Sqrt(1 / ( ((x * x) - (y * y)) * ((x * x) - (y * y)) ));
        return new Vector2(val * x, val * y);
    }

    Vector2 diamond(float x,float y)
    {
        float r = Mathf.Sqrt(x * x + y * y);
        float angle = Mathf.Atan(x / y);
        return new Vector2(Mathf.Sin(angle) * Mathf.Cos(r), Mathf.Cos(angle * Mathf.Sin(r)));
    }

    Vector2 polar(float x,float y)
    {
        return new Vector2(Mathf.Atan(x / y) / Mathf.PI, Mathf.Sqrt(x * x + y * y) - 1);
    }

    Vector2 handkerchief(float x,float y)
    {
        float r = Mathf.Sqrt(x * x + y * y);
        float angle= Mathf.Atan(x / y);
        return new Vector2(r * Mathf.Sin(angle + r), r * Mathf.Cos(angle - r));
    }

    Vector2 julia(float x,float y)
    {
        float r = Mathf.Sqrt(x * x + y * y);
        float angle = Mathf.Atan(x / y);
        return new Vector2(Mathf.Sqrt(r) * Mathf.Cos(angle / 2), Mathf.Sin(angle / 2));
    }

    Vector2 juliaScope(float x,float y)
    {
        float p1 = 0.5f, p2 = 0.3f;
        float p3 = (float)Math.Truncate(Math.Abs(p1) * UnityEngine.Random.Range(0, 1.0f));
        float t = (-Mathf.Atan(y / x) + 2 * Mathf.PI * p3) / p1;
        float r = Mathf.Sqrt(x * x + y * y);
        return new Vector2(Mathf.Pow(r, p2 / p1) * Mathf.Cos(t), Mathf.Pow(r, p2 / p1) * Mathf.Sin(t));
    }

    void setCoefficients(float rl,float rr,int numOfFunc)
    {
        a = new float[numOfFunc];
        for(int i=0;i<numOfFunc;i++)
        {
            a[i] = UnityEngine.Random.Range(rl, rr);
        }

        b = new float[numOfFunc];
        for (int i = 0; i < numOfFunc; i++)
        {
            b[i] = UnityEngine.Random.Range(rl, rr);
        }

        c = new float[numOfFunc];
        for (int i = 0; i < numOfFunc; i++)
        {
            c[i] = UnityEngine.Random.Range(rl, rr);
        }

        d = new float[numOfFunc];
        for (int i = 0; i < numOfFunc; i++)
        {
            d[i] = UnityEngine.Random.Range(rl, rr);
        }

        e = new float[numOfFunc];
        for (int i = 0; i < numOfFunc; i++)
        {
            e[i] = UnityEngine.Random.Range(rl, rr);
        }

        f = new float[numOfFunc];
        for (int i = 0; i < numOfFunc; i++)
        {
            f[i] = UnityEngine.Random.Range(rl, rr);
        }

        colors = new Color[numOfFunc];
        for (int i = 0; i < numOfFunc; i++)
        {
            colors[i] = new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f));
        }

    }

    float sigmoid(float x)
    {
        return 1 / (1 + Mathf.Pow((float)Math.E, -x));
    }

    void calculateDensity()
    {
        maxDensity = 0;
        for (int i = 0; i < plot.Count; i++)
        {
            float _x = plot[i].x.Remap(minx, maxx, 0, sizex-1);
            float _y = plot[i].y.Remap(miny, maxy, 0, sizey-1);
            density[(int)_x,(int) _y]++;
            densityColor[(int)_x, (int)_y] = plotColor[i];
            if (density[(int)_x, (int)_y] > maxDensity) maxDensity = density[(int)_x, (int)_y];
        }


    }

    void drawTexture()
    {
        for (int i = 0; i < sizex; i++)
        {
            for (int j = 0; j < sizey; j++)
            {
                tex.SetPixel(i, j, Color.black);
            }
        }

        for (int i = 0; i < sizex; i++)
        {
            for (int j = 0; j < sizey; j++)
            {
                float c = Mathf.Log(1+density[i,j])/Mathf.Log(1+maxDensity);
                densityColor[i, j] *= c;
                densityColor[i, j].a = 1;
                tex.SetPixel(i, j,densityColor[i,j] );
            }
        }

    }

    
    void Update()
    {
        
    }
}

public static class ExtensionMethods
{

    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

}
