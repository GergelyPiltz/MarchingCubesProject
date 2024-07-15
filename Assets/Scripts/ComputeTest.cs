using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public class ComputeTest : MonoBehaviour
{
    [SerializeField] ComputeShader computeShader;
    // Start is called before the first frame update
    void Start()
    {
        Compute();
    }

    struct Triangle
    {
        public uint a;
        public uint b;
        public uint c;
    }

    private void Compute()
    {
        Texture3D tex = new(10, 10, 10, TextureFormat.RFloat, false);

        int num = 3;
        int numThreads = 3;
        
        computeShader.SetInt(0, numThreads);

        ComputeBuffer computeBuffer = new(num * num * num * numThreads * numThreads * numThreads, Marshal.SizeOf(typeof(uint)) * 3, ComputeBufferType.Append);
        computeBuffer.SetCounterValue(0);

        computeShader.SetBuffer(0, "buffer"/*Shader.PropertyToID("buffer")*/, computeBuffer);
        computeShader.Dispatch(0, num, num, num);

        ComputeBuffer triCountBuffer = new(1, sizeof(int), ComputeBufferType.Raw);

        ComputeBuffer.CopyCount(computeBuffer, triCountBuffer, 0);

        int[] count = { 0 };

        triCountBuffer.GetData(count);

        Debug.Log("count: " + computeBuffer.count);
        Debug.Log("countByCopy: " + count[0]);

        Triangle[] array = new Triangle[computeBuffer.count];
        computeBuffer.GetData(array);

        string fileName = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.txt";

        string docPath =
          Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        using (StreamWriter outputFile = new(Path.Combine(docPath, fileName)))
        {
            foreach (var item in array)
            {
                outputFile.WriteLine(item.a + ", " + item.b + ", " + item.c);
            }
        }

        computeBuffer.Release();
        
    }
}
