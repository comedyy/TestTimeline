using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct TimelineSimulateJob : IJobParallelFor
{
    [ReadOnly] 
    public NativeArray<int> tickFrames;         // 最多32个一组， 一个frame列表， 已经排序好
    [ReadOnly] 
    public NativeArray<int> tickActions;        // 最多32个一组， 一个逻辑行为列表
    public NativeArray<int> tickCurrentFrame;   // 当前frame列表
    public NativeArray<int> tickCurrentCheckIndex;  // 当前已经检查到的tick下标
    [WriteOnly] 
    public NativeArray<int> outputResult;         // 最多32个一组， 输出结果

    public void Execute(int index)
    {
        var currentFrame = tickCurrentFrame[index] + 1;
        var resultCount = 0;

        var fromIndex = 32 * index;
        for(int i = fromIndex + tickCurrentCheckIndex[index]; i < fromIndex + 32; i++)
        {
            var currentTick = tickFrames[i];
            if(currentTick < 0)
            {
                break;
            }

            if(tickFrames[i] <= currentFrame)
            {
                outputResult[fromIndex + resultCount + 1] = tickActions[i];
                resultCount++;
            }
        }

        tickCurrentFrame[index]++;
        tickCurrentCheckIndex[index] += resultCount;
        outputResult[fromIndex] = resultCount;
    }
}
