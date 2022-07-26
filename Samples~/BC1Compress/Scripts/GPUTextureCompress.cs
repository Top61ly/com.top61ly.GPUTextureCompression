using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;


public class GPUTextureCompress : MonoBehaviour
{
    [SerializeField] private ComputeShader m_CompressComputeShader;
    [SerializeField] private Texture2D m_SrcTexture;
    [SerializeField] private int m_SrcTextureSize = 1024;

    [SerializeField] private Renderer m_DstRenderer;

    private RenderTexture m_CompressedTexture;
    private Texture2D m_DstTexture;

    private CommandBuffer m_Cmd;

    private int m_CompressedTextureSize = -1;
    private int m_ComputeShaderKernel = -1;

    private void OnEnable()
    {
        if (m_CompressedTexture == null)
        {
            m_CompressedTexture = new RenderTexture(m_SrcTextureSize / 4, m_SrcTextureSize / 4, 0, GraphicsFormat.R16G16B16A16_UInt, 0);
            m_CompressedTexture.enableRandomWrite = true;
            m_CompressedTexture.Create();

            m_CompressedTextureSize = m_SrcTextureSize / 4;
        }

        if (m_DstTexture == null)
        {
            m_DstTexture = new Texture2D(m_SrcTextureSize, m_SrcTextureSize, GraphicsFormat.RGBA_DXT1_UNorm, 0);

            m_DstRenderer.sharedMaterial.mainTexture = m_DstTexture;
        }

        if (m_Cmd == null)
        {
            m_Cmd = CommandBufferPool.Get("GPU Texture Compress");
        }

        if (m_CompressComputeShader != null)
        {
            m_ComputeShaderKernel = m_CompressComputeShader.FindKernel(GPUTextureCompressConstant.k_BC1CompressKernel);
        }
    }

    private void Update()
    {
        Debug.Assert(m_CompressedTexture != null);
        Debug.Assert(m_DstTexture != null);
        Debug.Assert(m_CompressComputeShader != null);
        Debug.Assert(m_Cmd != null);

        m_Cmd.Clear();

        CompressTexture(m_Cmd, m_ComputeShaderKernel, m_CompressComputeShader, m_SrcTexture, m_SrcTextureSize, m_CompressedTexture, m_CompressedTextureSize);

        Graphics.ExecuteCommandBuffer(m_Cmd);

        Graphics.CopyTexture(m_CompressedTexture, 0, 0, 0, 0, 256, 256, m_DstTexture, 0, 0, 0, 0);
    }

    public static void CompressTexture(CommandBuffer cmd, int kernel, ComputeShader computeShader, Texture2D src, int srcSize, RenderTexture dst, int dstSize)
    {
        cmd.BeginSample("GPUTexture Compress");
        cmd.SetComputeTextureParam(computeShader, kernel, GPUTextureCompressConstant.k_InputTexture, src);
        cmd.SetComputeTextureParam(computeShader, kernel, GPUTextureCompressConstant.k_OutputTexture, dst);
        cmd.SetComputeIntParam(computeShader, GPUTextureCompressConstant.k_DstRegionSize, dstSize);
        cmd.SetComputeFloatParam(computeShader, GPUTextureCompressConstant.k_OneOverTextureWidth, 1.0f / srcSize);

        cmd.DispatchCompute(computeShader, kernel,
            (dstSize + GPUTextureCompressConstant.k_GroupThreadCount - 1) / GPUTextureCompressConstant.k_GroupThreadCount,
            (dstSize + GPUTextureCompressConstant.k_GroupThreadCount - 1) / GPUTextureCompressConstant.k_GroupThreadCount,
            1);
        cmd.EndSample("GPUTexture Compress");
    }

    private void OnDisable()
    {
        m_CompressedTexture?.Release();
        if (m_DstTexture != null)
        {
            Destroy(m_DstTexture);
        }
    }
}
