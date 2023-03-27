using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct TimelineSimulateJob : IJobParallelFor
{
    [ReadOnly] 
    [NativeDisableParallelForRestriction]
    public NativeArray<int> tickFrameActions;        // 最多32个一组， 一个逻辑行为列表
    [NativeDisableParallelForRestriction]
    public NativeArray<int> tickCurrentFrame;       // 当前frame列表
    [WriteOnly] 
    [NativeDisableParallelForRestriction]
    public NativeArray<int> outputResult;         // 最多32个一组， 输出结果
    internal NativeArray<int> tickFrameCount;
    internal NativeArray<int> tickCurrentCheck;
    internal NativeArray<int> outputResultCount;

    public void Execute(int index)
    {
        var currentFrame = tickCurrentFrame[index] + 1;
        var firstUnCheckedIndex = tickCurrentCheck[index];
        var resultCount = 0;

        var baseIndex = TimelineMgr.TIMLINE_MAX_EVENT_SIZE * index;
        var maxEvents = tickFrameCount[index];
        for(int i = firstUnCheckedIndex; i < maxEvents; i++)
        {
            var frame = tickFrameActions[baseIndex + i * 2];
            var action = tickFrameActions[baseIndex + i * 2 + 1];
            if(frame < currentFrame)
            {
                outputResult[baseIndex + resultCount] = action;
                resultCount++;
            }
        }
        outputResultCount[index] = resultCount;

        tickCurrentFrame[index] = currentFrame;
        tickCurrentCheck[index] += resultCount;
    }
}
