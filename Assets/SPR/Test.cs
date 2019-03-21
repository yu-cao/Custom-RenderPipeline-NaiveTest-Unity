using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private RenderTexture rt;

    public Transform cubeTransform;

    public Mesh cubeMesh;

    public Material pureColorMaterial;

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
        pureColorMaterial.color = new Color(0,0.5f,0.8f);
        pureColorMaterial.SetPass(0);
        Graphics.DrawMeshNow(cubeMesh,cubeTransform.localToWorldMatrix);
        //end draw call
        Graphics.Blit(rt,cam.targetTexture);
    }
}
