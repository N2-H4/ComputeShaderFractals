using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FF : MonoBehaviour
{
    public ComputeShader shader;
    public RawImage img;
    RenderTexture result;
    Texture2D startingPoints;
    ComputeBuffer buffer;

    int numAffine = 5;
    int numThreads = 32 * 32;
    float[] a, b, c, d, e, f;

    Data[] shaderData;

    public struct Data
    {
        float aCoff, bCoff, cCoff, dCoff, eCoff, fCoff;
        public Data(float a,float b,float c,float d,float e,float f)
        {
            aCoff = a;
            bCoff = b;
            cCoff = c;
            dCoff = d;
            eCoff = e;
            fCoff = f;
        }
    }
    void Start()
    {
        result = new RenderTexture(512, 512, 0);
        result.enableRandomWrite = true;
        result.Create();
        startingPoints = new Texture2D(512,512);
        shaderData = new Data[numThreads];
        randomizeCoefficients();
        //fill shader data
        for(int i=0;i<numThreads;i++)
        {
            int rand = UnityEngine.Random.Range(0, numAffine);
            shaderData[i] = new Data(a[rand], b[rand], c[rand], d[rand], e[rand], f[rand]);
        }

        buffer = new ComputeBuffer(shaderData.Length, 6 * sizeof(float));




        int kernelHandle = shader.FindKernel("CSMain");
        buffer.SetData(shaderData);
        shader.SetBuffer(kernelHandle, "buffer", buffer);
        shader.SetTexture(kernelHandle, "Result", result);
        shader.SetTexture(kernelHandle, "startingPoints", startingPoints);
        shader.Dispatch(kernelHandle, 512 / 32, 512 / 32, 1);
        RenderTexture.active = result;
        img.material.mainTexture = result;
    }

    void Update()
    {
        
    }

    void randomizeCoefficients()
    {
        a = new float[numAffine];
        b = new float[numAffine];
        c = new float[numAffine];
        d = new float[numAffine];
        e = new float[numAffine];
        f = new float[numAffine];
        for(int i=0;i<numAffine;i++)
        {
            a[i] = UnityEngine.Random.Range(-1.0f, 1.0f);
            b[i] = UnityEngine.Random.Range(-1.0f, 1.0f);
            c[i] = UnityEngine.Random.Range(-1.0f, 1.0f);
            d[i] = UnityEngine.Random.Range(-1.0f, 1.0f);
            e[i] = UnityEngine.Random.Range(-1.0f, 1.0f);
            f[i] = UnityEngine.Random.Range(-1.0f, 1.0f);
        }

    }

    void randomizeStartingPoints()
    {
        for(int i=0;i<512;i++)
        {
            for(int j=0;j<512;j++)
            {
                startingPoints.SetPixel(i, j, new Color(UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(-1.0f, 1.0f), 0.0f, 1.0f));
            }
        }
    }
}
