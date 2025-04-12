// Assets/Scripts/Editor/MidiClipEditor.cs
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

// MIDIノートクリップのカスタムタイムラインエディタ
[CustomTimelineEditor(typeof(MidiNoteClip))]
public class MidiClipEditor : ClipEditor
{
    private static readonly Color[] NoteColors = {
        Color.red, Color.yellow, Color.green, Color.cyan, 
        Color.blue, Color.magenta, Color.white, Color.gray
    };
    
    private static readonly string[] NoteNames = {
        "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
    };
    
    // カスタムクリップの描画
    public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
    {
        base.DrawBackground(clip, region);
        
        MidiNoteClip midiClip = clip.asset as MidiNoteClip;
        if (midiClip == null || midiClip.note == null) return;
        
        Rect clipRect = region.position;
        
        // ノート番号をカラーインデックスに変換
        int colorIndex = midiClip.note.NoteNumber % NoteColors.Length;
        Color noteColor = NoteColors[colorIndex];
        
        // ベロシティをアルファ値に変換（強いほど不透明）
        float alpha = midiClip.note.Velocity / 127f;
        noteColor.a = 0.3f + alpha * 0.7f; // 最低透明度を確保
        
        // ノートの表示
        Rect noteRect = clipRect;
        noteRect.height *= 0.8f; // 高さ調整
        
        // ノート番号に応じて縦位置を調整
        float notePosition = 1.0f - ((midiClip.note.NoteNumber % 24) / 24.0f);
        noteRect.y = clipRect.y + (clipRect.height - noteRect.height) * notePosition;
        
        // 背景矩形の描画
        EditorGUI.DrawRect(noteRect, noteColor);
        
        // ノート名の表示
        int octave = midiClip.note.NoteNumber / 12 - 1;
        string noteName = NoteNames[midiClip.note.NoteNumber % 12];
        string noteText = $"{noteName}{octave}";
        
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.black;
        style.alignment = TextAnchor.MiddleCenter;
        style.fontStyle = FontStyle.Bold;
        
        EditorGUI.LabelField(noteRect, noteText, style);
    }
}

// MIDIトラックアセット用のカスタムエディタ（必要な設定のみ）
[CustomTimelineEditor(typeof(MidiTrackAsset))]
public class MidiTrackEditor : TrackEditor
{
    // トラック色の指定など、必要な場合はここでカスタマイズできます
    public override TrackDrawOptions GetTrackOptions(TrackAsset track, Object binding)
    {
        TrackDrawOptions options = base.GetTrackOptions(track, binding);
        
        // トラックのカスタム色を指定
        options.trackColor = new Color(0.7f, 0.3f, 0.8f);
        
        return options;
    }
}