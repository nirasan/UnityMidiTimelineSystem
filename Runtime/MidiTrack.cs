// Assets/Scripts/MIDI/MidiTrack.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MidiTrack
{
    public int TrackNumber;
    public string TrackName;
    public List<MidiNote> Notes = new List<MidiNote>();
}