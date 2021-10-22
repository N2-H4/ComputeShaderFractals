using UnityEngine.UI;
using UnityEngine;
using System;
using System.Collections.Generic;

public class FractalFlame : MonoBehaviour
{
    public RawImage img;
    int sizex = 1920;
    int sizey = 1080;
    float[] a, b, c, d, e, f;
    List<Vector2> plot;
    Texture2D tex;
    float maxx = float.MinValue, minx = float.MaxValue, maxy = float.MinValue, miny = float.MaxValue;
    int[,] density;

    void Start()
    {
        setCoefficients(-1.0f,1.0f,10);
        plot = new List<Vector2>();
        density = new int[sizex, sizey];
        tex = new Texture2D(sizex, sizey, TextureFormat.ARGB32, false);
        
        
        float x = UnityEngine.Random.Range(-1.0f,1.0f);
        float y = UnityEngine.Random.Range(-1.0f, 1.0f);
        for (int ii = 0; ii < 5000000; ii++)
        {
            int rand = UnityEngine.Random.Range(0, 5);
            Vector2 temp,temp2;
            temp = affine(x, y, a[rand], b[rand], c[rand], d[rand], e[rand], f[rand]);
            x = temp.x;
            y = temp.y;

            temp = diamond(x, y,Mathf.PI/2);
            temp2 = popcorn(x, y,c[rand],f[rand]);

            x = temp.x+temp2.x;
            y = temp.y+temp2.y;


            temp = variation(x, y);
            x = temp.x;
            y = temp.y;


            if (x > maxx) maxx = x;
            if (x < minx) minx = x;
            if (y > maxy) maxy = y;
            if (y < miny) miny = y;

            if (ii>20)
            {
                plot.Add(new Vector2(x, y));
            }
        }


        Debug.Log(minx + " " + maxx);
        Debug.Log(miny + " " + maxy);

        calculateDensity();

        renderFractalFlame();
        tex.Apply();
        img.material.mainTexture = tex;
    }

    

    Vector2 affine(float x,float y,float a,float b,float c,float d,float e,float f)
    {
        return new Vector2(x * a + b * y + c, x * d + y * e + f);
    }

    Vector2 variation(float x,float y)
    {
        return new Vector2(Mathf.Sin(x), Mathf.Sin(y));
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

    Vector2 diamond(float x,float y,float angle)
    {
        float r = Mathf.Sqrt(x * x + y * y);
        return new Vector2(Mathf.Sin(angle) * Mathf.Cos(r), Mathf.Cos(angle * Mathf.Sin(r)));
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



    }

    float sigmoid(float x)
    {
        return 1 / (1 + Mathf.Pow((float)Math.E, -x));
    }

    void calculateDensity()
    {
        for (int i = 0; i < plot.Count; i++)
        {
            float _x = plot[i].x.Remap(minx, maxx, 0, sizex-1);
            float _y = plot[i].y.Remap(miny, maxy, 0, sizey-1);
            density[(int)_x,(int) _y]++;
        }
    }

    void renderFractalFlame()
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
                float c = Mathf.Log(density[i, j]);
                tex.SetPixel(i, j, new Color(c,c,c,1));
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
