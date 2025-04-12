// Assets/Scripts/MIDI/MidiScaleAnalyzer.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MidiScaleAnalyzer : MonoBehaviour
{
    [System.Serializable]
    public class ScaleDefinition
    {
        public string name;
        public int[] pattern; // 半音ステップのパターン（例：長音階は [0,2,4,5,7,9,11]）
    }

    // 一般的な音階の定義
    private static readonly ScaleDefinition[] commonScales = new ScaleDefinition[]
    {
        new ScaleDefinition { name = "Major (Ionian)", pattern = new int[] { 0, 2, 4, 5, 7, 9, 11 } },
        new ScaleDefinition { name = "Natural Minor (Aeolian)", pattern = new int[] { 0, 2, 3, 5, 7, 8, 10 } },
        new ScaleDefinition { name = "Harmonic Minor", pattern = new int[] { 0, 2, 3, 5, 7, 8, 11 } },
        new ScaleDefinition { name = "Melodic Minor", pattern = new int[] { 0, 2, 3, 5, 7, 9, 11 } },
        new ScaleDefinition { name = "Dorian", pattern = new int[] { 0, 2, 3, 5, 7, 9, 10 } },
        new ScaleDefinition { name = "Phrygian", pattern = new int[] { 0, 1, 3, 5, 7, 8, 10 } },
        new ScaleDefinition { name = "Lydian", pattern = new int[] { 0, 2, 4, 6, 7, 9, 11 } },
        new ScaleDefinition { name = "Mixolydian", pattern = new int[] { 0, 2, 4, 5, 7, 9, 10 } },
        new ScaleDefinition { name = "Locrian", pattern = new int[] { 0, 1, 3, 5, 6, 8, 10 } },
        new ScaleDefinition { name = "Major Pentatonic", pattern = new int[] { 0, 2, 4, 7, 9 } },
        new ScaleDefinition { name = "Minor Pentatonic", pattern = new int[] { 0, 3, 5, 7, 10 } },
        new ScaleDefinition { name = "Blues", pattern = new int[] { 0, 3, 5, 6, 7, 10 } },
        new ScaleDefinition { name = "Whole Tone", pattern = new int[] { 0, 2, 4, 6, 8, 10 } },
        new ScaleDefinition { name = "Diminished (Half-Whole)", pattern = new int[] { 0, 1, 3, 4, 6, 7, 9, 10 } },
        new ScaleDefinition { name = "Augmented", pattern = new int[] { 0, 3, 4, 7, 8, 11 } },
        new ScaleDefinition { name = "Chromatic", pattern = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 } }
    };

    public TextMesh textDisplay;
    public float displayDuration = 5.0f;
    private Dictionary<int, HashSet<int>> noteUsageByTrack = new Dictionary<int, HashSet<int>>();

    // MIDIトラックからスケール分析を行う
    public void AnalyzeScalesFromMidiTracks(List<MidiTrack> tracks)
    {
        noteUsageByTrack.Clear();
        string result = "MIDI Scale Analysis:\n";

        // 各トラックで使用されているノートを収集
        foreach (var track in tracks)
        {
            if (track.Notes.Count == 0) continue;

            HashSet<int> uniqueNotes = new HashSet<int>();
            foreach (var note in track.Notes)
            {
                // ノート番号を0-11の範囲（オクターブ内）に正規化
                int normalizedNote = note.NoteNumber % 12;
                uniqueNotes.Add(normalizedNote);
            }

            noteUsageByTrack[track.TrackNumber] = uniqueNotes;
            result += $"\nTrack {track.TrackNumber}: {track.TrackName ?? "Unnamed"}\n";
            result += $"Notes used: {uniqueNotes.Count}/12\n";
            
            // 使用されているノートの名前を表示
            result += "Notes: ";
            List<int> sortedNotes = uniqueNotes.ToList();
            sortedNotes.Sort();
            foreach (var note in sortedNotes)
            {
                result += GetNoteName(note) + " ";
            }
            result += "\n";

            // 最も一致する音階を見つける
            result += "Possible scales:\n";
            var matchingScales = FindMatchingScales(uniqueNotes);
            foreach (var match in matchingScales)
            {
                result += $"- {match.scaleName} ({match.rootNote}{match.accuracy}%)\n";
            }
        }

        // 全トラックを組み合わせた分析
        if (noteUsageByTrack.Count > 1)
        {
            HashSet<int> allNotes = new HashSet<int>();
            foreach (var trackNotes in noteUsageByTrack.Values)
            {
                allNotes.UnionWith(trackNotes);
            }

            result += "\nAll Tracks Combined:\n";
            result += $"Notes used: {allNotes.Count}/12\n";
            
            // 使用されているノートの名前を表示
            result += "Notes: ";
            List<int> sortedAllNotes = allNotes.ToList();
            sortedAllNotes.Sort();
            foreach (var note in sortedAllNotes)
            {
                result += GetNoteName(note) + " ";
            }
            result += "\n";

            // 最も一致する音階を見つける
            result += "Possible scales:\n";
            var matchingScales = FindMatchingScales(allNotes);
            foreach (var match in matchingScales)
            {
                result += $"- {match.scaleName} ({match.rootNote}{match.accuracy}%)\n";
            }
        }

        // 結果を表示
        Debug.Log(result);
        if (textDisplay != null)
        {
            textDisplay.text = result;
        }
    }

    // 使用されているノートに最も一致する音階を見つける
    private List<(string scaleName, string rootNote, float accuracy)> FindMatchingScales(HashSet<int> uniqueNotes)
    {
        List<(string scaleName, string rootNote, float accuracy)> results = new List<(string, string, float)>();

        foreach (var scale in commonScales)
        {
            // 12の異なるルート音でスケールをチェック
            for (int root = 0; root < 12; root++)
            {
                // スケールパターンを指定されたルートからのオフセットに変換
                HashSet<int> scaleNotes = new HashSet<int>();
                foreach (int offset in scale.pattern)
                {
                    int note = (root + offset) % 12;
                    scaleNotes.Add(note);
                }

                // 使用されているノートと音階がどれだけ一致するか計算
                int matchingNotes = uniqueNotes.Intersect(scaleNotes).Count();
                int totalNotesInScale = scale.pattern.Length;
                int totalNotesUsed = uniqueNotes.Count;

                // スケールに含まれるノートがどれだけ使われているか
                float scaleCompleteness = (float)matchingNotes / totalNotesInScale * 100f;
                
                // 使用されているノートのうち、スケールに含まれる割合
                float noteFit = (float)matchingNotes / totalNotesUsed * 100f;
                
                // 2つの指標を組み合わせた総合スコア
                float combinedScore = (scaleCompleteness + noteFit) / 2f;

                // しきい値を超えたマッチのみを記録（例：50%以上の一致）
                if (combinedScore >= 50f)
                {
                    string rootNoteName = GetNoteName(root);
                    results.Add((scale.name, rootNoteName, Mathf.Round(combinedScore)));
                }
            }
        }

        // スコアの高い順にソート
        return results.OrderByDescending(r => r.accuracy).Take(5).ToList();
    }

    // ノート番号から音名を取得
    private string GetNoteName(int noteIndex)
    {
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int octave = (noteIndex / 12) - 1;
        return noteNames[noteIndex % 12] + octave.ToString();
    }
}