using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanging : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void toMandelbrot()
    {
        SceneManager.LoadScene("Mandelbrot");
    }

    public void toJulia()
    {
        SceneManager.LoadScene("Julia");
    }

    public void toMandelbulb()
    {
        SceneManager.LoadScene("Mandelbulb");
    }

    public void toSierpinski()
    {
        SceneManager.LoadScene("Sierpinski");
    }
}
