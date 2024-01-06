using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class MathHelper
{
    public static float RandomSign()
    {
        float value = Random.Range(0, 2);

        return value == 0 ? -1 : 1;
    }

    public static float ZeroSign(float value)
    {
        return value == 0 ? 0 : Mathf.Sign(value);
    }

    public static bool GetRandomIndex(Array array, out int value)
    {
        int length = array.Length;
        if (length == 0)
        {
            value = 0;
            return false;
        }

        value = Random.Range(0, length);
        return true;
    }
}