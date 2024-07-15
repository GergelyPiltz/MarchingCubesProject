using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptResources : MonoBehaviour
{
    [SerializeField] private ComputeShader marchingCompute;
    public ComputeShader MarchingCompute => marchingCompute;

    [SerializeField] private ComputeShader noiseCompute;
    public ComputeShader NoiseCompute => noiseCompute;
}
