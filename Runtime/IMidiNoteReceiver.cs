// Assets/Scripts/MIDI/IMidiNoteReceiver.cs
using UnityEngine;

// MIDI再生時にノート情報を受け取るインターフェース
public interface IMidiNoteReceiver
{
    void OnNoteTriggered(MidiNote note);
}