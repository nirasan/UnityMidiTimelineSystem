// Assets/Scripts/MIDI/MidiNotePlayableBehaviour.cs
using UnityEngine;
using UnityEngine.Playables;

// MIDIノートのプレイアブル動作
public class MidiNotePlayableBehaviour : PlayableBehaviour
{
    public MidiNote note;
    private bool hasTriggered = false;
    private float previousTime = 0f;
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (playerData == null) return;
        
        // 現在の再生時間を取得
        double currentTime = playable.GetTime();
        
        // クリップの先頭を通過したタイミングでトリガー
        if (!hasTriggered && currentTime >= 0 && previousTime <= 0)
        {
            MidiNoteReceiver receiver = playerData as MidiNoteReceiver;
            if (receiver != null)
            {
                receiver.TriggerNote(note);
            }
            hasTriggered = true;
        }
        
        // 前フレームの時間を記録
        previousTime = (float)currentTime;
    }
    
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        hasTriggered = false;
        previousTime = 0f;
    }
    
    public override void OnGraphStart(Playable playable)
    {
        hasTriggered = false;
        previousTime = 0f;
    }
    
    public override void OnPlayableCreate(Playable playable)
    {
        hasTriggered = false;
        previousTime = 0f;
    }
}