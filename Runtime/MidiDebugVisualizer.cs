// Assets/Scripts/MIDI/MidiDebugVisualizer.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// MIDI情報をデバッグ表示するサンプルコンポーネント
public class MidiDebugVisualizer : MonoBehaviour, IMidiNoteReceiver
{
    public TextMesh textDisplay;
    public float displayDuration = 2.0f;
    
    private Dictionary<int, GameObject> activeNoteObjects = new Dictionary<int, GameObject>();
    
    void Start()
    {
        if (textDisplay == null)
        {
            textDisplay = GetComponentInChildren<TextMesh>();
        }
        
        if (textDisplay == null)
        {
            GameObject textObj = new GameObject("DebugText");
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = Vector3.zero;
            textDisplay = textObj.AddComponent<TextMesh>();
            textDisplay.fontSize = 24;
            textDisplay.alignment = TextAlignment.Center;
            textDisplay.anchor = TextAnchor.MiddleCenter;
        }
    }
    
    public void OnNoteTriggered(MidiNote note)
    {
        // ノート情報の表示
        int octave = note.NoteNumber / 12 - 1;
        string noteName = GetNoteName(note.NoteNumber % 12);
        string noteText = $"{noteName}{octave} (Vel: {note.Velocity})";
        
        textDisplay.text = noteText;
        
        // 一定時間後に表示をクリア
        StartCoroutine(ClearTextAfterDelay());
        
        // ノートに対応するビジュアル要素を作成
        CreateNoteVisual(note);
    }
    
    private string GetNoteName(int noteIndex)
    {
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        return noteNames[noteIndex];
    }
    
    private IEnumerator ClearTextAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        textDisplay.text = "";
    }
    
    private void CreateNoteVisual(MidiNote note)
    {
        // 既存の表示オブジェクトがあれば削除
        if (activeNoteObjects.ContainsKey(note.NoteNumber))
        {
            Destroy(activeNoteObjects[note.NoteNumber]);
        }
        
        // 新しい表示オブジェクトを作成
        GameObject noteObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        noteObj.transform.SetParent(transform);
        
        // ノート番号に応じて位置を変える
        float xPos = (note.NoteNumber % 12) * 0.5f - 3.0f;
        float yPos = (note.NoteNumber / 12) * 0.3f - 1.0f;
        noteObj.transform.localPosition = new Vector3(xPos, yPos, 0);
        
        // ベロシティに応じてサイズを変える
        float scale = 0.2f + (note.Velocity / 127f) * 0.3f;
        noteObj.transform.localScale = new Vector3(scale, scale, scale);
        
        // マテリアルの色を設定
        Renderer renderer = noteObj.GetComponent<Renderer>();
        
        // 音階に応じて色を変える
        Color noteColor = Color.HSVToRGB((note.NoteNumber % 12) / 12f, 0.8f, 0.8f);
        renderer.material.color = noteColor;
        
        // アクティブなノートとして記録
        activeNoteObjects[note.NoteNumber] = noteObj;
        
        // ノートの持続時間後に削除
        StartCoroutine(DestroyNoteVisualAfterDuration(note.NoteNumber, note.Duration));
    }
    
    private IEnumerator DestroyNoteVisualAfterDuration(int noteNumber, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        if (activeNoteObjects.ContainsKey(noteNumber))
        {
            Destroy(activeNoteObjects[noteNumber]);
            activeNoteObjects.Remove(noteNumber);
        }
    }
}