using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ITimelineClipAsset
{

}

[Serializable]
public class Track
{
    public List<Clip> clips;
}

[Serializable]
public class Clip
{
    public int from;
    public int to;

    [SerializeReference, SubclassSelector]
    public ITimelineClipAsset asset;
}

[CreateAssetMenu(menuName = "RawTimelineData", fileName = "RawTimelineData")]
public class RawTimelineData : ScriptableObject
{
    public List<Track> tracks;

    void OnValidate()
    {
        foreach (var track in tracks)
        {
            foreach (var clip in track.clips)
            {
                clip.to = Mathf.Max(clip.to, clip.from);
            }
        }
    }

    /// runtime
    [NonSerialized]
    public Clip[] runTimeclips;

    [NonSerialized]
    public KeyValuePair<int, int>[] frameEvent;

    static List<KeyValuePair<int, int>>  s_tempList = new List<KeyValuePair<int, int>>();
    public void OnLoad()
    {
        runTimeclips = tracks.SelectMany(m=>m.clips).ToArray();
        Array.Sort(runTimeclips, (m,n)=>m.from - n.from);

        s_tempList.Clear();
        for(int i = 0; i < runTimeclips.Length; i++)
        {
            var clip = runTimeclips[i];
            s_tempList.Add(new KeyValuePair<int, int>(clip.from, i * 10));

            if(clip.from != clip.to)
            {
                s_tempList.Add(new KeyValuePair<int, int>(clip.to, i * 10 + 1));
            }
        }

        s_tempList.Sort((m,n)=>m.Key - n.Key);
        frameEvent = s_tempList.ToArray();
    }
}
