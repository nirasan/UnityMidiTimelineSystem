// Assets/Scripts/MIDI/MidiNote.cs
using System;
using UnityEngine;

[Serializable]
public class MidiNote
{
    public int NoteNumber; // MIDIノート番号 (0-127)
    public int Velocity;   // ノートの強さ (0-127)
    public float StartTime; // 開始時間（秒）
    public float Duration;  // 持続時間（秒）
}