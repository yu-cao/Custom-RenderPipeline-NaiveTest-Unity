using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//也就是使用Light对gBuffer进行光照，保存到target中进行渲染
[System.Serializable]
public class DeferredLighting
{
    public Material DeferredMaterial;//传入用于延迟渲染的material
    public Light directionalLight;//传入光照
    private static int _InvVP = Shader.PropertyToID("_InvVP");
    private static int _CurrentLightDir = Shader.PropertyToID("_CurrentLightDir");
    private static int _LightFinalColor = Shader.PropertyToID("_LightFinalColor");

    public void DrawLight(RenderTexture[] gbuffers, int[] gbufferIDs, RenderTexture target, Camera cam)
    {
        Matrix4x4 proj = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);
        Matrix4x4 vp = proj * cam.worldToCameraMatrix;
        DeferredMaterial.SetMatrix(_InvVP, vp.inverse);
        DeferredMaterial.SetVector(_CurrentLightDir, -directionalLight.transform.forward);
        DeferredMaterial.SetVector(_LightFinalColor, directionalLight.color * directionalLight.intensity);

        for (int i = 0; i < gbufferIDs.Length; ++i)
        {
            DeferredMaterial.SetTexture(gbufferIDs[i], gbuffers[i]);
        }
        Graphics.Blit(null, target, DeferredMaterial, 0);
    }
}
