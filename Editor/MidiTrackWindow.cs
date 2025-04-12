// Assets/Scripts/Editor/MidiTrackWindow.cs
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// エディタ拡張用のカスタムエディタウィンドウ
public class MidiTrackWindow : EditorWindow
{
    private string midiFilePath = "";
    private MidiParser parser;
    private List<MidiTrack> tracks;
    private int selectedTrackIndex = 0;
    private PlayableDirector playableDirector;
    private TimelineAsset timelineAsset;
    private Vector2 scrollPosition;
    
    [MenuItem("Window/Audio/MIDI Track Editor")]
    public static void ShowWindow()
    {
        GetWindow<MidiTrackWindow>("MIDI Track Editor");
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUILayout.Label("MIDI Track Editor", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        midiFilePath = EditorGUILayout.TextField("MIDI File", midiFilePath);
        if (GUILayout.Button("Browse", GUILayout.Width(70)))
        {
            string path = EditorUtility.OpenFilePanel("Select MIDI File", "", "mid,midi");
            if (!string.IsNullOrEmpty(path))
            {
                midiFilePath = path;
                LoadMidiFile();
            }
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Load MIDI File"))
        {
            LoadMidiFile();
        }
        
        EditorGUILayout.Space();
        
        if (tracks != null && tracks.Count > 0)
        {
            string[] trackNames = new string[tracks.Count];
            for (int i = 0; i < tracks.Count; i++)
            {
                trackNames[i] = string.IsNullOrEmpty(tracks[i].TrackName) ? 
                    $"Track {i}" : $"{i}: {tracks[i].TrackName}";
            }
            
            selectedTrackIndex = EditorGUILayout.Popup("Select Track", selectedTrackIndex, trackNames);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Add to Timeline"))
            {
                AddTrackToTimeline();
            }
            
            ShowScaleAnalysisGUI();
            
            EditorGUILayout.Space();
            
            // トラック情報の表示
            if (selectedTrackIndex < tracks.Count)
            {
                MidiTrack track = tracks[selectedTrackIndex];
                EditorGUILayout.LabelField($"Track: {track.TrackName ?? $"Track {track.TrackNumber}"}");
                EditorGUILayout.LabelField($"Notes: {track.Notes.Count}");
                
                // ノート情報のプレビュー表示
                EditorGUILayout.LabelField("Notes Preview:", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                for (int i = 0; i < Mathf.Min(10, track.Notes.Count); i++)
                {
                    MidiNote note = track.Notes[i];
                    EditorGUILayout.LabelField($"Note {note.NoteNumber} (Vel: {note.Velocity}), " +
                        $"Start: {note.StartTime:F2}s, Duration: {note.Duration:F2}s");
                }
                
                if (track.Notes.Count > 10)
                {
                    EditorGUILayout.LabelField("...");
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void LoadMidiFile()
    {
        if (string.IsNullOrEmpty(midiFilePath))
        {
            EditorUtility.DisplayDialog("Error", "Please select a MIDI file.", "OK");
            return;
        }
        
        parser = new MidiParser();
        if (parser.ParseMidiFile(midiFilePath))
        {
            tracks = parser.Tracks;
            selectedTrackIndex = 0;
            EditorUtility.DisplayDialog("Success", $"Loaded MIDI file with {tracks.Count} tracks.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Failed to parse MIDI file.", "OK");
        }
    }
    
    private void AddTrackToTimeline()
    {
        if (tracks == null || selectedTrackIndex >= tracks.Count)
        {
            EditorUtility.DisplayDialog("Error", "No track selected.", "OK");
            return;
        }
        
        // 現在のシーンから選択されたPlayableDirectorを取得
        playableDirector = Selection.activeGameObject?.GetComponent<PlayableDirector>();
        if (playableDirector == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a GameObject with PlayableDirector component.", "OK");
            return;
        }
        
        // タイムラインアセットの取得
        timelineAsset = playableDirector.playableAsset as TimelineAsset;
        if (timelineAsset == null)
        {
            EditorUtility.DisplayDialog("Error", "Selected PlayableDirector does not have a TimelineAsset.", "OK");
            return;
        }
        
        // MIDIトラックの作成
        MidiTrackAsset midiTrackAsset = timelineAsset.CreateTrack<MidiTrackAsset>(null, $"MIDI {tracks[selectedTrackIndex].TrackName ?? $"Track {selectedTrackIndex}"}");
        midiTrackAsset.InitializeFromMidiTrack(tracks[selectedTrackIndex]);
        midiTrackAsset.midiFilePath = midiFilePath;
        midiTrackAsset.selectedTrackIndex = selectedTrackIndex;
        
        // バインディングオブジェクトの作成
        GameObject receiverObject = new GameObject($"MIDI Receiver ({tracks[selectedTrackIndex].TrackName ?? $"Track {selectedTrackIndex}"})");
        MidiNoteReceiver receiver = receiverObject.AddComponent<MidiNoteReceiver>();
        
        // PlayableDirectorへのバインディング
        playableDirector.SetGenericBinding(midiTrackAsset, receiver);
        
        EditorUtility.DisplayDialog("Success", "Added MIDI track to timeline.", "OK");
    }
    
    private void ShowScaleAnalysisGUI()
    {
        if (tracks != null && tracks.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scale Analysis", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Analyze Scales"))
            {
                AnalyzeScales();
            }
        }
    }
    
    private void AnalyzeScales()
    {
        if (tracks == null || tracks.Count == 0) return;
        
        // ログウィンドウに分析結果を表示
        Debug.Log("=== MIDI Scale Analysis ===");
        
        // 各トラックの使用ノートを収集
        Dictionary<int, HashSet<int>> noteUsageByTrack = new Dictionary<int, HashSet<int>>();
        HashSet<int> allNotesUsed = new HashSet<int>();
        
        foreach (var track in tracks)
        {
            if (track.Notes.Count == 0) continue;
            
            Debug.Log($"Track {track.TrackNumber}: {track.TrackName ?? "Unnamed"} ({track.Notes.Count} notes)");
            
            HashSet<int> notesInTrack = new HashSet<int>();
            foreach (var note in track.Notes)
            {
                // // オクターブ内の音階位置（0-11）に正規化
                // int pitchClass = note.NoteNumber % 12;
                // notesInTrack.Add(pitchClass);
                // allNotesUsed.Add(pitchClass);
                notesInTrack.Add(note.NoteNumber);
                allNotesUsed.Add(note.NoteNumber);
            }
            
            noteUsageByTrack[track.TrackNumber] = notesInTrack;
            
            // このトラックで使用されている音の名前を表示
            List<int> sortedNotes = notesInTrack.ToList();
            sortedNotes.Sort();
            string notesStr = "Notes used: ";
            foreach (int n in sortedNotes)
            {
                notesStr += GetNoteName(n) + " ";
            }
            Debug.Log(notesStr);
        }
        
        // 全体のノート使用状況
        Debug.Log("\nOverall Note Usage:");
        List<int> sortedAllNotes = allNotesUsed.ToList();
        sortedAllNotes.Sort();
        string allNotesStr = "All notes used: ";
        foreach (int n in sortedAllNotes)
        {
            allNotesStr += GetNoteName(n) + " ";
        }
        Debug.Log(allNotesStr);
        
        // 結果をダイアログで表示
        EditorUtility.DisplayDialog("Scale Analysis", 
            $"Analysis complete. Check the console for detailed results.\n\nDetected {allNotesUsed.Count} unique notes across all tracks.", 
            "OK");
    }
    
    private string GetNoteName(int noteIndex)
    {
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int octave = (noteIndex / 12) - 1;
        return noteNames[noteIndex % 12] + octave.ToString();
    }
}