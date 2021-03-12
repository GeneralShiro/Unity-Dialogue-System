using System.Collections;
using System.Collections.Generic;

using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace CustomSystem.DialogueSystem
{
    [TrackClipType(typeof(DialogueControlAsset))]
    public class DialogueControlTrack : TrackAsset { }

    public abstract class DialogueControlAsset : PlayableAsset { }
}
