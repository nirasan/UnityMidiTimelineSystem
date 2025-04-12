// Assets/Scripts/MIDI/MidiNoteClip.cs
using UnityEngine;
using UnityEngine.Playables;

// MIDIノートのタイムラインクリップデータ
public class MidiNoteClip : PlayableAsset
{
    public MidiNote note;
    
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<MidiNotePlayableBehaviour>.Create(graph);
        var behaviour = playable.GetBehaviour();
        behaviour.note = note;
        return playable;
    }
}