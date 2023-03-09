using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;

public class NewBehaviourScript : MonoBehaviour
{
    public RawTimelineData timelineDataTemplate;

    public NativeArray<int> tickFrames;         // 最多32个一组， 一个frame列表， 已经排序好
    public NativeArray<int> tickActions;        // 最多32个一组， 一个逻辑行为列表
    public NativeArray<int> tickCurrentFrame;   // 当前frame列表
    public NativeArray<int> tickCurrentCheckIndex;  // 当前已经检查到的tick下标
    public NativeArray<int> outputResult;         // 最多32个一组， 输出结果
    const int count = 64;

    // Start is called before the first frame update
    void Awake()
    {
        timelineDataTemplate.OnLoad();
        
        int maxTimelineSize = 32;
        tickFrames = new NativeArray<int>(count * maxTimelineSize, Allocator.Persistent);
        tickActions = new NativeArray<int>(count * maxTimelineSize, Allocator.Persistent);
        tickCurrentFrame = new NativeArray<int>(count, Allocator.Persistent);
        tickCurrentCheckIndex = new NativeArray<int>(count, Allocator.Persistent);
        outputResult = new NativeArray<int>(count * maxTimelineSize, Allocator.Persistent);
    }

    // Update is called once per frame
    void Update()
    {
        ClearArray(outputResult, count);
        TimelineSimulateJob job = new TimelineSimulateJob(){
            tickFrames = tickFrames,
            tickActions = tickActions,
            tickCurrentFrame = tickCurrentFrame,
            tickCurrentCheckIndex = tickCurrentCheckIndex,
            outputResult = outputResult
        };

        var handle = job.Schedule(count, 8);
        handle.Complete();
    }

    unsafe void ClearArray<T>(NativeArray<T> to_clear, int length) where T : struct
    {
        UnsafeUtility.MemClear(
            NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(to_clear),
            UnsafeUtility.SizeOf<T>() * length);
    }
}
