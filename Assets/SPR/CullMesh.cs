using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public struct CullMesh : IJobParallelFor
{
    public static RenderObj[] allObjects;//每个需要绘制的物体的component

    private static Plane[] frustumPlanes = new Plane[6];//camera前后左右上下6个视锥面
    
    //计算物体与camera的距离
    public static Vector3 cameraPos;
    public static float cameraFarClipDistance;

    public static void UpdateFrame(Camera cam, ref Matrix4x4 invvp, Vector3 cameraPosition)
    {
        GetCullingPlanes(ref invvp);
        cameraFarClipDistance = cam.farClipPlane;
        cameraPos = cameraPosition;
    }

    public static void GetCullingPlanes(ref Matrix4x4 invVp)
    {
        //8个远近平面的平面点
        Vector3 nearLeftButtom = invVp.MultiplyPoint(new Vector3(-1, -1, 1));
        Vector3 nearLeftTop = invVp.MultiplyPoint(new Vector3(-1, 1, 1));
        Vector3 nearRightButtom = invVp.MultiplyPoint(new Vector3(1, -1, 1));
        Vector3 nearRightTop = invVp.MultiplyPoint(new Vector3(1, 1, 1));
        Vector3 farLeftButtom = invVp.MultiplyPoint(new Vector3(-1, -1, 0));
        Vector3 farLeftTop = invVp.MultiplyPoint(new Vector3(-1, 1, 0));
        Vector3 farRightButtom = invVp.MultiplyPoint(new Vector3(1, -1, 0));
        Vector3 farRightTop = invVp.MultiplyPoint(new Vector3(1, 1, 0));
        //六个裁剪面
        //Near
        frustumPlanes[0] = new Plane(nearRightTop, nearRightButtom, nearLeftButtom);
        //Up
        frustumPlanes[1] = new Plane(farLeftTop, farRightTop, nearRightTop);
        //Down
        frustumPlanes[2] = new Plane(nearRightButtom, farRightButtom, farLeftButtom);
        //Left
        frustumPlanes[3] = new Plane(farLeftButtom, farLeftTop, nearLeftTop);
        //Right
        frustumPlanes[4] = new Plane(farRightButtom, nearRightButtom, nearRightTop);
        //Far
        frustumPlanes[5] = new Plane(farLeftButtom, farRightButtom, farRightTop);
    }

    private static bool PlaneTest(ref Matrix4x4 ObjectToWorld, ref Vector3 extent, out Vector3 position)
    {
        //得到在相机空间的标准正交基和相机在世界空间中的位置
        Vector3 right = new Vector3(ObjectToWorld.m00, ObjectToWorld.m10, ObjectToWorld.m20);
        Vector3 up = new Vector3(ObjectToWorld.m01, ObjectToWorld.m11, ObjectToWorld.m21);
        Vector3 forward = new Vector3(ObjectToWorld.m02, ObjectToWorld.m12, ObjectToWorld.m22);
        position = new Vector3(ObjectToWorld.m03, ObjectToWorld.m13, ObjectToWorld.m23);

        //遍历所有的面，然后和每个面进行碰撞比对，当确保Bounding Box在所有的面之前的时候，就可以确定这个方块是应该被绘制的
        for (int i = 0; i < 6; ++i)
        {
            Plane plane = frustumPlanes[i];
            float r = Vector3.Dot(position, plane.normal);
            Vector3 absNormal = new Vector3(Mathf.Abs(Vector3.Dot(plane.normal, right)), Mathf.Abs(Vector3.Dot(plane.normal, up)), Mathf.Abs(Vector3.Dot(plane.normal, forward)));
            float f = Vector3.Dot(absNormal, extent);
            if ((r - f) >= -plane.distance)
                return false;
        }
        return true;
    }

    public void Execute(int i)
    {
        RenderObj obj = allObjects[i];
        Vector3 position;

        //计算距离，进行分层
        if (PlaneTest(ref obj.localToWorldMatrices, ref obj.extent, out position))
        {
            float distance = Vector3.Distance(position, cameraPos);
            float layer = distance / cameraFarClipDistance;
            int layerValue = (int) Mathf.Clamp(Mathf.Lerp(0, SortMesh.LayerCount, layer), 0, SortMesh.LayerCount - 1);
            SortMesh.sortObj[layerValue].Add(distance, obj);
        }
    }

    public static JobHandle Schedule()
    {
        return (new CullMesh()).Schedule(allObjects.Length, 64);
    }
}
