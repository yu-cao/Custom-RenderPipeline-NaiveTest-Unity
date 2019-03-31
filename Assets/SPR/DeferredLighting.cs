using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//也就是使用Light对gBuffer进行光照，保存到target中进行渲染
[System.Serializable]
public class DeferredLighting
{
    //    public Material DeferredMaterial;//传入用于延迟渲染的material
    public Light directionalLight;//传入光照
    public Cubemap cubeMap;//天空盒贴图
    public Material lightingMat;

    //    private static int _InvVP = Shader.PropertyToID("_InvVP");
    private static int _CurrentLightDir = Shader.PropertyToID("_CurrentLightDir");
    private static int _LightFinalColor = Shader.PropertyToID("_LightFinalColor");
    private static int _CubeMap = Shader.PropertyToID("_CubeMap");

    public void DrawLight(RenderTexture[] gbuffers, int[] gbufferIDs, RenderTexture target, Camera cam)
    {
        lightingMat.SetVector(_CurrentLightDir, -directionalLight.transform.forward);
        lightingMat.SetVector(_LightFinalColor, directionalLight.color * directionalLight.intensity);
        lightingMat.SetTexture(_CubeMap, cubeMap);
        Graphics.Blit(null, target, lightingMat, 0);
    }
}