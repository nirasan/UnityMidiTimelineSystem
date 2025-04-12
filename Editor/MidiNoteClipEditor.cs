#if UNITY_EDITOR
using UnityEditor;

// MIDIノートクリップのカスタムエディタ
[CustomEditor(typeof(MidiNoteClip))]
public class MidiNoteClipEditor : Editor
{
    private static readonly string[] NoteNames = {
        "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
    };
    
    public override void OnInspectorGUI()
    {
        MidiNoteClip clip = (MidiNoteClip)target;
        
        if (clip.note == null)
        {
            EditorGUILayout.LabelField("No note data available.");
            return;
        }
        
        // ノート情報の表示
        int octave = clip.note.NoteNumber / 12 - 1;
        string noteName = NoteNames[clip.note.NoteNumber % 12];
        
        EditorGUILayout.LabelField("Note", $"{noteName}{octave} (MIDI: {clip.note.NoteNumber})");
        EditorGUILayout.LabelField("Velocity", clip.note.Velocity.ToString());
        EditorGUILayout.LabelField("Start Time", $"{clip.note.StartTime:F3} seconds");
        EditorGUILayout.LabelField("Duration", $"{clip.note.Duration:F3} seconds");
    }
}
#endif