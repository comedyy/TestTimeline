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

    public void Execute(int index)
    {
        var frameIndex = 2 * index;
        var currentFrame = tickCurrentFrame[frameIndex] + 1;

        var firstUnCheckedIndexIndex = 2 * index + 1;
        var firstUnCheckedIndex = tickCurrentFrame[firstUnCheckedIndexIndex];
        var resultCount = 0;

        var baseIndex = TimelineMgr.TIMLINE_MAX_EVENT_SIZE * index;
        var maxEvents = tickFrameActions[baseIndex];
        for(int i = firstUnCheckedIndex; i < maxEvents; i++)
        {
            var indexOfAction = i + 1;
            var frame = tickFrameActions[baseIndex + indexOfAction * 2];
            var action = tickFrameActions[baseIndex + indexOfAction * 2 + 1];
            if(frame < currentFrame)
            {
                outputResult[baseIndex + resultCount + 1] = action;
                resultCount++;
            }
        }

        tickCurrentFrame[frameIndex] = currentFrame;
        tickCurrentFrame[firstUnCheckedIndexIndex] += resultCount;
        outputResult[baseIndex] = resultCount;
    }
}
