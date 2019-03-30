using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

public class SortMesh : IJobParallelFor
{
    public const int LayerCount = 20;
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
        sortObj[i].Sort();
        sortObj[i].GetSorted();
    }

    public static JobHandle Schedule(JobHandle cull)
    {
        SortMesh instance = new SortMesh();
        //TODO
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
