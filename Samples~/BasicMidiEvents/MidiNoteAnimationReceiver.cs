using System.Collections.Generic;
using UnityEngine;

public class MidiNoteAnimationReceiver: MonoBehaviour, IMidiNoteReceiver
{
    public List<Animator> animators;
    
    public void OnNoteTriggered(MidiNote note)
    {
        // ノート情報の表示
        int octave = note.NoteNumber / 12 - 1;
        string noteName = GetNoteName(note.NoteNumber % 12);
        string noteText = $"{noteName}{octave} (Vel: {note.Velocity})";
        
        // Debug.Log(noteText);

        if (animators.Count > 0 && animators[0] != null)
        {
            // animators[0].SetTrigger(noteName);
        }
    }
    
    private string GetNoteName(int noteIndex)
    {
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        return noteNames[noteIndex];
    }
}