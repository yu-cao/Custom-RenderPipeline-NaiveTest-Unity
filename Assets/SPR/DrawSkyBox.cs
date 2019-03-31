﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DrawSkyBox
{
    private static int _Corner = Shader.PropertyToID("_Corner");
    public Material skyboxMaterial;

    private static Mesh m_mesh;
    private static Vector4[] corners = new Vector4[4];

    //手动生成铺满屏幕的mesh，这里使用的是OpenGL的NDC，-1是左和下，1是右和上，0是远裁面，1是进裁面
    public static Mesh fullScreenMesh
    {
        get
        {
            if (m_mesh != null)
                return m_mesh;
            m_mesh = new Mesh();
            m_mesh.vertices = new Vector3[]
            {
                new Vector4(-1,-1,0, 1),
                new Vector4(-1,1,0, 1),
                new Vector4(1,1,0, 1),
                new Vector4(1,-1,0, 1)
            };
            //我们使用DX平台，UV的y轴要颠倒
            m_mesh.uv = new Vector2[]
            {
                new Vector2(0,1),
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(1,1)
            };

            m_mesh.SetIndices(new[] { 0, 1, 2, 3 }, MeshTopology.Quads, 0);
            return m_mesh;
        }
    }

    //将四个远裁面传入Shader中
    public void SkyBoxDraw(Camera cam, RenderBuffer cameraTarget, RenderBuffer depth)
    {
        corners[0] = cam.ViewportToScreenPoint(new Vector3(0, 0, cam.farClipPlane));
        corners[1] = cam.ViewportToScreenPoint(new Vector3(1, 0, cam.farClipPlane));
        corners[2] = cam.ViewportToScreenPoint(new Vector3(0, 1, cam.farClipPlane));
        corners[3] = cam.ViewportToScreenPoint(new Vector3(1, 1, cam.farClipPlane));
        skyboxMaterial.SetVectorArray(_Corner, corners);
        skyboxMaterial.SetPass(0);
        Graphics.SetRenderTarget(cameraTarget, depth);
        Graphics.DrawMeshNow(fullScreenMesh, Matrix4x4.identity);
    }
}
