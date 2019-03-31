using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public class Test : MonoBehaviour
{
    private static int _DepthTexture = Shader.PropertyToID("_DepthTexture");
    private static int _InvVP = Shader.PropertyToID("_InvVP");//从裁剪空间反算回世界空间
    private RenderTexture cameraTarget;
    private RenderBuffer[] GBuffers;
    private RenderTexture[] GBufferTextures;
    private int[] gbufferIDs;
    public Material deferredMaterial;
    public DrawSkyBox skyDraw;
    public DeferredLighting lighting;
    private RenderTexture depthTexture;
    public RenderObj[] allRenderObjs;//所有要渲染的物体
    [Range(0, 4f)] public float superSample = 1;
    private int screenWidth;
    private int screenHeight;

    void Start()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        //定制了3块RT，第一块是CameraTarget，也就是光照计算后的结果
        //第二块是四个贴图，GBuffer，在Shader中输出这四个值
        //第三块是depthTexture用来处理深度问题
        cameraTarget = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
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
        //单例初始化
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
        //调整屏幕大小以满足超采样
        if (screenHeight != cam.pixelHeight * superSample || screenWidth != cam.pixelWidth * superSample)
        {
            screenHeight = (int)(cam.pixelHeight * superSample);
            screenWidth = (int)(cam.pixelWidth * superSample);
            ReSize(cameraTarget, screenWidth, screenHeight);
            ReSize(depthTexture, screenWidth, screenHeight);
            foreach (var i in GBufferTextures)
            {
                ReSize(i, screenWidth, screenHeight);
            }
    
            for (int i = 0; i < GBuffers.Length; i++)
            {
                GBuffers[i] = GBufferTextures[i].colorBuffer;
            }
        }
        Shader.SetGlobalTexture(_DepthTexture, depthTexture);
    
        //计算每帧的投影矩阵，依靠投影矩阵计算视锥裁面
        Matrix4x4 proj = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);
        Matrix4x4 vp = proj * cam.worldToCameraMatrix;
        Matrix4x4 invvp = vp.inverse;
        Shader.SetGlobalMatrix(_InvVP, invvp);
    
        CullMesh.UpdateFrame(cam, ref invvp, transform.position);
        SortMesh.UpdateFrame();
    
        JobHandle cullHandle = CullMesh.Schedule();
        JobHandle sortHandle = SortMesh.Schedule(cullHandle);
        JobHandle.ScheduleBatchedJobs();//使用Job System的调用在分线程中完成剔除和排序
    
        for (int i = 0; i < gbufferIDs.Length; i++)
        {
            Shader.SetGlobalTexture(gbufferIDs[i], GBufferTextures[i]);
        }
    
        Graphics.SetRenderTarget(GBuffers, depthTexture.depthBuffer);
        GL.Clear(true, true, Color.black);
        //start draw call
        deferredMaterial.SetPass(0);
        sortHandle.Complete();
        for (int i = 0; i < SortMesh.sortObj.Length; i++)//遍历每个obj的Mesh进行输出
        {
            DrawElements(ref SortMesh.sortObj[i]);
        }        
        lighting.DrawLight(GBufferTextures, gbufferIDs, cameraTarget, cam);
        skyDraw.SkyBoxDraw(cam, cameraTarget.colorBuffer, depthTexture.depthBuffer);
        //end draw call
        Graphics.Blit(cameraTarget, cam.targetTexture);
    }

    //将绘制过程进行抽象出来
    public static void DrawElements(ref BinarySort<RenderObj> binarySort)
    {
        RenderObj[] objs = binarySort.meshes;
        for (int j = 0; j < binarySort.count; ++j)
        {
            Graphics.DrawMeshNow(objs[j].targetMesh, objs[j].localToWorldMatrices);
        }
    }
}
