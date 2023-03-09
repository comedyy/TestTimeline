using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TimelineLogClipAsset : ITimelineClipAsset
{
    public string log;
}


[Serializable]
public class TimelineVfxClipAsset : ITimelineClipAsset
{
    public string vfx;
}
