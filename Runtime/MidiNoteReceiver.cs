// Assets/Scripts/MIDI/MidiNoteReceiver.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// トラックからノート情報を受け取るバインディングコンポーネント
public class MidiNoteReceiver : MonoBehaviour, ITimeControl
{
    public List<GameObject> boundObjects = new List<GameObject>();
    
    // 現在アクティブなノート
    private HashSet<int> activeNotes = new HashSet<int>();
    
    public void TriggerNote(MidiNote note)
    {
        // ノート情報をバインドされたオブジェクトに送信
        foreach (var obj in boundObjects)
        {
            IMidiNoteReceiver receiver = obj.GetComponent<IMidiNoteReceiver>();
            if (receiver != null)
            {
                receiver.OnNoteTriggered(note);
            }
        }
        
        // アクティブノートとして追加
        activeNotes.Add(note.NoteNumber);
        
        // ノートの終了タイミングを設定
        StartCoroutine(ReleaseNoteAfterDuration(note));
    }
    
    private IEnumerator ReleaseNoteAfterDuration(MidiNote note)
    {
        yield return new WaitForSeconds(note.Duration);
        activeNotes.Remove(note.NoteNumber);
    }
    
    // ITimeControlインターフェースの実装
    public void OnControlTimeStart()
    {
        // 再生開始時の処理
        activeNotes.Clear();
    }
    
    public void OnControlTimeStop()
    {
        // 再生停止時の処理
        activeNotes.Clear();
    }
    
    public void SetTime(double time)
    {
        // タイムラインの時間が変更された時の処理
    }
}