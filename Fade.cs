using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    private Material _fadeMat;
    private static readonly int Property = Shader.PropertyToID("_Fade");

    private void Awake()
    {
        _fadeMat = GetComponent<Image>().material;
    }

    public void SetFade(float fade)
    {
        _fadeMat.SetFloat(Property, fade);
    }

    public float GetFade() => _fadeMat.GetFloat(Property);
}
