// Assets/Scripts/MIDI/MidiTrackAsset.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// MIDIトラック用のカスタムトラックアセット
[TrackColor(0.7f, 0.3f, 0.8f)]
[TrackClipType(typeof(MidiNoteClip))]
[TrackBindingType(typeof(MidiNoteReceiver))]
public class MidiTrackAsset : TrackAsset
{
    public MidiTrack midiTrack;
    public string midiFilePath;
    public int selectedTrackIndex;
    
    // トラックを作成/初期化
    public void InitializeFromMidiTrack(MidiTrack track)
    {
        midiTrack = track;
        name = track.TrackName ?? $"MIDI Track {track.TrackNumber}";
        
        // 既存のクリップをクリア
        DeleteRecordedClips();
        
        // 各ノートに対してクリップを作成
        foreach (var note in track.Notes)
        {
            CreateClipForNote(note);
        }
    }
    
    private void CreateClipForNote(MidiNote note)
    {
        TimelineClip timelineClip = CreateClip<MidiNoteClip>();
        MidiNoteClip clip = timelineClip.asset as MidiNoteClip;
        
        if (clip != null)
        {
            clip.note = note;
            
            // クリップの名前設定
            clip.name = $"Note {note.NoteNumber} (Vel: {note.Velocity})";
        }
        
        // タイムラインにクリップを配置
        timelineClip.start = note.StartTime;
        timelineClip.duration = Mathf.Max(0.1f, note.Duration); // 最小の長さを確保
        timelineClip.displayName = $"Note {note.NoteNumber} (Vel: {note.Velocity})";
    }
    
    private void DeleteRecordedClips()
    {
        if (GetClips() == null || !GetClips().Any()) return;
        
        // クリップのリストを取得して削除
        List<TimelineClip> clipsToDelete = new List<TimelineClip>(GetClips());
        foreach (var clip in clipsToDelete)
        {
            DeleteClip(clip);
        }
    }
}