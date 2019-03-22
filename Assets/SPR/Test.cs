using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private RenderTexture rt;

    public Transform[] cubeTransform;

    public Mesh cubeMesh;

    public Material pureColorMaterial;

    public DrawSkyBox skybox;
    // Start is called before the first frame update
    void Start()
    {
        rt = new RenderTexture(Screen.width,Screen.height,0);
    }

    private void OnPostRender()
    {
        Camera cam = Camera.current;
        Graphics.SetRenderTarget(rt);
        GL.Clear(true,true,Color.gray);
        //start draw call
        skybox.SkyBoxDraw(cam);
        pureColorMaterial.color = new Color(0,0.5f,0.8f);
        pureColorMaterial.SetPass(0);
        foreach(var i in cubeTransform)
        {
            Graphics.DrawMeshNow(cubeMesh, i.localToWorldMatrix);
        }
        //end draw call
        Graphics.Blit(rt,cam.targetTexture);
    }
}
