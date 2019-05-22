using System.Net;
using System;
using UnityEngine;
using System.Text;
using Unity.Networking.Transport;
using Unity.Collections;
using System.Collections.Generic;
using System.Collections;


public static class Conversions
{

    #region ToBytes
    public static byte[] StringToBytes(string x) {
        byte[] bytes = Encoding.ASCII.GetBytes(x);
        return bytes;
    }

    public static byte[] FloatToBytes(float x) {
        byte[] bytes = BitConverter.GetBytes(x);
        return bytes;
    }
    #endregion



    #region FromBytes
    public static char[] BytesToCharArray(byte[] bytes) {
        char[] x = Encoding.ASCII.GetString(bytes).ToCharArray();
        return x;
    }

    public static string BytesToString(byte[] bytes) {
        string x = Encoding.Default.GetString(bytes);
        return x;
    }

    public static double BytesToDouble(byte[] bytes) {
        double x = System.BitConverter.ToDouble(bytes, 0);
        return x;
    }

    public static Vector2 BytesToVector2(byte[] bytesX, byte[] bytesZ) {

        double x = System.BitConverter.ToDouble(bytesX, 0);
        double z = System.BitConverter.ToDouble(bytesZ, 0);

        Vector2 vector = new Vector2((float)x, (float)z);
        return vector;
    }
    #endregion
}
