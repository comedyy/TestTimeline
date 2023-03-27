using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;

public class NewBehaviourScript : MonoBehaviour
{
    TimelineMgr _timelineMgr;
    RawTimelineData timelineDataTemplateA;
    RawTimelineData timelineDataTemplateB;

    // Start is called before the first frame update
    void Awake()
    {
        timelineDataTemplateA = Resources.Load<RawTimelineData>("TimelineA");
        timelineDataTemplateA.OnLoad();
        timelineDataTemplateB = Resources.Load<RawTimelineData>("TimelineB");
        timelineDataTemplateB.OnLoad();

        _timelineMgr = new TimelineMgr();
        _timelineMgr.Init();

        for(int i = 0; i < TimelineMgr.MAX_TIMELINE_COUNT; i++)
        {
            _timelineMgr.AddTimeline(timelineDataTemplateA, null);
        }
    }

    // Update is called once per frame
    void Update()
    {
        _timelineMgr.Update();
    }

    void OnDestroy()
    {
        _timelineMgr.CleanUp();
    }

    void OnGUI()
    {
        if(GUI.Button(new Rect(0, 0, 100, 100), "addTimelineA"))
        {
            _timelineMgr.AddTimeline(timelineDataTemplateA, null);
        }

        if(GUI.Button(new Rect(100, 0, 100, 100), "addTimelineB"))
        {
            _timelineMgr.AddTimeline(timelineDataTemplateB, null);
        }
    }
}
