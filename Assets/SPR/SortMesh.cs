using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

//这里需要是struct方式使得Schedule函数能够被继承
public struct SortMesh : IJobParallelFor
{
    public const int LayerCount = 20;//指定一个分层常量LayerCount，后续物体应该根据距离被均匀准确地分配入这些层中
    public static BinarySort<RenderObj>[] sortObj = new BinarySort<RenderObj>[LayerCount];
    private static bool init = false;
    public static void InitSortMesh(int maximumDrawCall)
    {
        if (init) return;
        init = true;
        for (int i = 0; i < LayerCount; ++i)
            sortObj[i] = new BinarySort<RenderObj>(maximumDrawCall);
    }
    public void Execute(int i)
    {
        sortObj[i].Sort();//进行排序操作
        sortObj[i].GetSorted();//完成二叉树的后续遍历
    }

    public static JobHandle Schedule(JobHandle cull)
    {
        SortMesh instance = new SortMesh();
        return instance.Schedule(LayerCount, 1, cull);
    }

    public static void UpdateFrame()
    {
        for (int i = 0; i < sortObj.Length; ++i)
        {
            sortObj[i].Clear();
        }
    }

}
