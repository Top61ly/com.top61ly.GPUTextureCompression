using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class GPUTextureCompressConstant
{
    public const string k_BC1CompressKernel = "BC1Compress";
    public const string k_BC3CompressKernel = "BC3Compress";
    public const string k_BC5CompressKernel = "BC5Compress";
    public const int k_GroupThreadCount = 8;

    public static int k_InputTexture = Shader.PropertyToID("inputTexture");
    public static int k_OutputTexture = Shader.PropertyToID("outputTexture");
    public static int k_DstRegionSize = Shader.PropertyToID("dstRegionSize");
    public static int k_OneOverTextureWidth = Shader.PropertyToID("oneOverTextureWidth");
}
