// Assets/Scripts/MIDI/MidiParser.cs
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MidiParser
{
    // MIDIファイルのフォーマット情報
    private int format;
    private int trackCount;
    private int timeDivision;
    
    // 解析したMIDIデータを保持
    public List<MidiTrack> Tracks { get; private set; } = new List<MidiTrack>();

    // MIDIファイルを解析する
    public bool ParseMidiFile(string filePath)
    {
        try
        {
            byte[] data = File.ReadAllBytes(filePath);
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                // MIDIヘッダーチャンク読み込み
                string headerChunkId = new string(reader.ReadChars(4));
                if (headerChunkId != "MThd")
                {
                    Debug.LogError("Invalid MIDI file: MThd chunk not found");
                    return false;
                }

                int headerLength = SwapEndian(reader.ReadInt32());
                format = SwapEndian(reader.ReadInt16());
                trackCount = SwapEndian(reader.ReadInt16());
                timeDivision = SwapEndian(reader.ReadInt16());

                Debug.Log($"MIDI Format: {format}, Track Count: {trackCount}, Time Division: {timeDivision}");

                // 各トラックチャンクを読み込む
                for (int i = 0; i < trackCount; i++)
                {
                    MidiTrack track = new MidiTrack { TrackNumber = i };
                    
                    string trackChunkId = new string(reader.ReadChars(4));
                    if (trackChunkId != "MTrk")
                    {
                        Debug.LogError($"Invalid track chunk ID: {trackChunkId}");
                        continue;
                    }

                    int trackLength = SwapEndian(reader.ReadInt32());
                    long trackEnd = stream.Position + trackLength;

                    Dictionary<int, MidiNote> activeNotes = new Dictionary<int, MidiNote>();
                    long absoluteTicks = 0;
                    
                    // トラック内のすべてのイベントを読み込む
                    while (stream.Position < trackEnd)
                    {
                        long delta = ReadVariableLengthValue(reader);
                        absoluteTicks += delta;
                        
                        byte eventType = reader.ReadByte();
                        
                        // システムイベントまたはメタイベント
                        if (eventType == 0xFF)
                        {
                            byte metaType = reader.ReadByte();
                            int metaLength = (int)ReadVariableLengthValue(reader);
                            
                            // トラック名
                            if (metaType == 0x03)
                            {
                                track.TrackName = new string(reader.ReadChars(metaLength));
                                Debug.Log($"Track {i} Name: {track.TrackName}");
                            }
                            else
                            {
                                // その他のメタイベントはスキップ
                                reader.ReadBytes(metaLength);
                            }
                        }
                        // ノートオン
                        else if ((eventType & 0xF0) == 0x90)
                        {
                            byte channel = (byte)(eventType & 0x0F);
                            byte noteNumber = reader.ReadByte();
                            byte velocity = reader.ReadByte();
                            
                            // ベロシティが0の場合は実質ノートオフと同じ
                            if (velocity == 0)
                            {
                                if (activeNotes.ContainsKey(noteNumber))
                                {
                                    MidiNote note = activeNotes[noteNumber];
                                    note.Duration = TicksToSeconds(absoluteTicks) - note.StartTime;
                                    activeNotes.Remove(noteNumber);
                                }
                            }
                            else
                            {
                                MidiNote note = new MidiNote
                                {
                                    NoteNumber = noteNumber,
                                    Velocity = velocity,
                                    StartTime = TicksToSeconds(absoluteTicks),
                                    Duration = 0 // 後でノートオフで設定
                                };
                                
                                activeNotes[noteNumber] = note;
                            }
                        }
                        // ノートオフ
                        else if ((eventType & 0xF0) == 0x80)
                        {
                            byte channel = (byte)(eventType & 0x0F);
                            byte noteNumber = reader.ReadByte();
                            byte velocity = reader.ReadByte(); // リリースベロシティ（通常は使わない）
                            
                            if (activeNotes.ContainsKey(noteNumber))
                            {
                                MidiNote note = activeNotes[noteNumber];
                                note.Duration = TicksToSeconds(absoluteTicks) - note.StartTime;
                                track.Notes.Add(note);
                                activeNotes.Remove(noteNumber);
                            }
                        }
                        // その他のMIDIイベント
                        else if ((eventType & 0x80) != 0)
                        {
                            // ランニングステータスの処理（ここでは簡略化）
                            if ((eventType & 0xF0) == 0xC0 || (eventType & 0xF0) == 0xD0)
                            {
                                reader.ReadByte(); // 1バイトだけ読み飛ばす
                            }
                            else
                            {
                                reader.ReadBytes(2); // 2バイト読み飛ばす
                            }
                        }
                    }
                    
                    // アクティブなノートが残っている場合は強制的に終了させる
                    foreach (var note in activeNotes.Values)
                    {
                        note.Duration = TicksToSeconds(absoluteTicks) - note.StartTime;
                        track.Notes.Add(note);
                    }
                    
                    Tracks.Add(track);
                    Debug.Log($"Parsed Track {i}: {track.Notes.Count} notes");
                }
                
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing MIDI file: {e.Message}");
            return false;
        }
    }
    
    // MIDIの可変長数値を読み込む
    private long ReadVariableLengthValue(BinaryReader reader)
    {
        long result = 0;
        byte b;
        
        do
        {
            b = reader.ReadByte();
            result = (result << 7) | (uint)(b & 0x7F);
        } while ((b & 0x80) != 0);
        
        return result;
    }
    
    // ビッグエンディアンからリトルエンディアンに変換（整数）
    private int SwapEndian(int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        return BitConverter.ToInt32(bytes, 0);
    }
    
    // ビッグエンディアンからリトルエンディアンに変換（ショート）
    private short SwapEndian(short value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        return BitConverter.ToInt16(bytes, 0);
    }
    
    // MIDIのティック値を秒に変換
    private float TicksToSeconds(long ticks)
    {
        // 簡易的な変換（120BPMと仮定）
        float secondsPerQuarterNote = 0.5f; // 120BPMの場合
        return (float)ticks / timeDivision * secondsPerQuarterNote;
    }
}