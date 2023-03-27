using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

public class Timeline
{
    public RawTimelineData asset;
    public object context;
    public bool isEnd;
}


public class TimelineMgr
{
    public const int MAX_TIMELINE_COUNT = 64;
    public const int TIMLINE_MAX_EVENT_SIZE = 32;
    Timeline[] _allTimes;
    int currentTimelineCount;

    // 配置数据
    public NativeArray<int> tickFrameCount;
    public NativeArray<int> tickFrameActions;        // 对应RawTimelineData的FrameAction

    // 运行数据
    public NativeArray<int> tickCurrentFrame;   // 当前（frame，已经检查到的下标）
    public NativeArray<int> tickCurrentCheck;   // 当前（frame，已经检查到的下标）

    // 输出数据
    public NativeArray<int> outputResult;         //  输出结果
    public NativeArray<int> outputResultCount;         //  输出结果

    public void Init()
    {
        _allTimes = new Timeline[MAX_TIMELINE_COUNT];
        tickFrameActions = new NativeArray<int>(MAX_TIMELINE_COUNT * TIMLINE_MAX_EVENT_SIZE * 2, Allocator.Persistent);
        tickFrameCount = new NativeArray<int>(MAX_TIMELINE_COUNT, Allocator.Persistent);
        tickCurrentFrame = new NativeArray<int>(MAX_TIMELINE_COUNT, Allocator.Persistent);
        tickCurrentCheck = new NativeArray<int>(MAX_TIMELINE_COUNT, Allocator.Persistent);
        outputResult = new NativeArray<int>(MAX_TIMELINE_COUNT * TIMLINE_MAX_EVENT_SIZE, Allocator.Persistent);
        outputResultCount = new NativeArray<int>(MAX_TIMELINE_COUNT, Allocator.Persistent);
    }

    public void CleanUp()
    {
        tickFrameActions.Dispose();
        tickCurrentFrame.Dispose();
        tickCurrentCheck.Dispose();
        tickFrameCount.Dispose();
        outputResult.Dispose();
        outputResultCount.Dispose();
    }

    internal void Update()
    {
        // Job for simulate
        // ClearArray(outputResult, MAX_TIMELINE_COUNT);
        TimelineSimulateJob job = new TimelineSimulateJob(){
            tickFrameActions = tickFrameActions,
            tickFrameCount = tickFrameCount,
            tickCurrentFrame = tickCurrentFrame,
            tickCurrentCheck = tickCurrentCheck,
            outputResult = outputResult,
            outputResultCount = outputResultCount
        };

        var handle = job.Schedule(currentTimelineCount, 8);
        handle.Complete();

        // logic Excute
        for(int i = 0; i < currentTimelineCount; i++)
        {
            var timeline = _allTimes[i];
            if(timeline.isEnd) continue;

            var baseIndex = i * TIMLINE_MAX_EVENT_SIZE;
            var resultCount = outputResultCount[i];

            if(resultCount == 0) continue;

            for(int j = 0; j < resultCount; j++)
            {
                var evIndex = outputResult[baseIndex + j];
                var clipIndex = evIndex / 10;
                var evFuncIndex = evIndex % 10;

                var clip = timeline.asset.runTimeclips[clipIndex];
                Excute(timeline, clip.asset, evFuncIndex);
            }

            if(tickCurrentCheck[i] >= timeline.asset.frameAction.Length)
            {
                timeline.isEnd = true;
            }
        }

        // CleanUP
        for(int i = currentTimelineCount - 1; i >= 0; i--)
        {
            if(_allTimes[i].isEnd)
            {
                if(i != (currentTimelineCount - 1))
                {
                    var fromIndex = currentTimelineCount - 1;
                    _allTimes[i] = _allTimes[currentTimelineCount - 1];

                    NativeArray<int>.Copy(tickFrameActions, fromIndex * TIMLINE_MAX_EVENT_SIZE, tickFrameActions, i * TIMLINE_MAX_EVENT_SIZE, TIMLINE_MAX_EVENT_SIZE);
                    tickFrameActions[i] = tickCurrentFrame[currentTimelineCount - 1];
                    tickCurrentCheck[i] = tickCurrentCheck[currentTimelineCount - 1];
                }
                currentTimelineCount--;
                Debug.Log("i End");
            }
        }
    }

    private void Excute(Timeline timeline, ITimelineClipAsset asset, int isEnter)
    {
        if(asset is TimelineLogClipAsset logClipAsset)
        {
            Debug.Log($"Excuet {timeline.context} {timeline.asset} {logClipAsset.log} {isEnter} {Time.frameCount}");
        }
        else if(asset is TimelineVfxClipAsset vfxClipAsset)
        {
            Debug.Log($"Excuet {timeline.context} {timeline.asset} {vfxClipAsset.vfx} {isEnter} {Time.frameCount}");
        }
        else
        {
            Debug.LogError("none Exception");
        }
    }

    unsafe void ClearArray<T>(NativeArray<T> to_clear, int length) where T : struct
    {
        UnsafeUtility.MemClear(
            NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(to_clear),
            UnsafeUtility.SizeOf<T>() * length);
    }

    public Timeline AddTimeline(RawTimelineData asset, object context)
    {
        if(currentTimelineCount == MAX_TIMELINE_COUNT) throw new Exception("TOO MUSH TIMELINE");

        // mono
        var timeline = new Timeline(){
            asset = asset,
            context = context
        };
        _allTimes[currentTimelineCount] = timeline;

        // native
        var baseIndex = currentTimelineCount * TIMLINE_MAX_EVENT_SIZE;
        for(int index = 0; index < asset.frameAction.Length; index++)
        {
            tickFrameActions[baseIndex + index * 2] = asset.frameAction[index].Key;
            tickFrameActions[baseIndex + index * 2 + 1] = asset.frameAction[index].Value;
        }

        tickFrameCount[currentTimelineCount] = asset.frameAction.Length;
        tickCurrentCheck[currentTimelineCount] = tickCurrentFrame[currentTimelineCount] = 0;

        currentTimelineCount++;

        return timeline;
    }

    public void RemoveTimeline(Timeline timeline)
    {
        timeline.isEnd = true;
    }
}
