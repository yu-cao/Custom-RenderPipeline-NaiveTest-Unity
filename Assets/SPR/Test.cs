using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private static int _DepthTexture = Shader.PropertyToID("_DepthTexture");
    private RenderTexture cameraTarget;
    private RenderBuffer[] GBuffers;
    private RenderTexture[] GBufferTextures;
    private int[] gbufferIDs;
//    public Transform[] cubeTransforms;
//    public Mesh cubeMesh;
    public Material deferredMaterial;
    public DrawSkyBox skyDraw;
    public DeferredLighting lighting;
    private RenderTexture depthTexture;
    public RenderObj[] allRenderObjs;//所有要渲染的物体
    [Range(0, 4f)] public float superSample = 1;

    void Start()
    {
        //定制了3块RT，第一块是CameraTarget，也就是光照计算后的结果
        //第二块是四个贴图，GBuffer，在Shader中输出这四个值
        //第三块是depthTexture用来处理深度问题
        cameraTarget = new RenderTexture(Screen.width, Screen.height, 0);
        GBufferTextures = new[]
        {
            new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBHalf,RenderTextureReadWrite.Linear),
            new RenderTexture(Screen.width, Screen.height,0,RenderTextureFormat.ARGBHalf,RenderTextureReadWrite.Linear),
            new RenderTexture(Screen.width, Screen.height,0,RenderTextureFormat.ARGBHalf,RenderTextureReadWrite.Linear),
            new RenderTexture(Screen.width, Screen.height,0,RenderTextureFormat.ARGBHalf,RenderTextureReadWrite.Linear),
        };
        depthTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);

        GBuffers = new RenderBuffer[GBufferTextures.Length];
        for (int i = 0; i < GBuffers.Length; i++)
        {
            GBuffers[i] = GBufferTextures[i].colorBuffer;
        }
        gbufferIDs = new[]
        {
            Shader.PropertyToID("_GBuffer0"),
            Shader.PropertyToID("_GBuffer1"),
            Shader.PropertyToID("_GBuffer2"),
            Shader.PropertyToID("_GBuffer3"),
        };

        //把所有的Obj进行排序和剔除
        SortMesh.InitSortMesh(allRenderObjs.Length);
        CullMesh.allObjects = allRenderObjs;
        foreach (var i in allRenderObjs)
        {
            i.Init();
        }
    }

    //对渲染纹理的大小进行重新大小调整
    private static void ReSize(RenderTexture rt, int width, int height)
    {
        rt.Release();
        rt.width = width;
        rt.height = height;
        rt.Create();
    }

    private void OnPostRender()
    {
        Camera cam = Camera.current;
        Shader.SetGlobalTexture(_DepthTexture, depthTexture);
        Graphics.SetRenderTarget(GBuffers, depthTexture.depthBuffer);
        GL.Clear(true, true, Color.gray);
        //start draw call
        deferredMaterial.SetPass(0);
        foreach (var i in cubeTransforms)//遍历每个obj的Mesh进行输出
        {
            Graphics.DrawMeshNow(cubeMesh, i.localToWorldMatrix);
        }
        lighting.DrawLight(GBufferTextures, gbufferIDs, cameraTarget, cam);
        skyDraw.SkyBoxDraw(cam, cameraTarget.colorBuffer, depthTexture.depthBuffer);
        //end draw call
        Graphics.Blit(cameraTarget, cam.targetTexture);
    }
}
