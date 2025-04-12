# MIDI Timeline System

Unity Timeline で MIDI ファイルを解析し、タイムラインにクリップとして表示して、コールバックで任意のイベントを実行するためのパッケージです。

## インストール方法

### Unity Package Manager経由（推奨）
1. Package Managerウィンドウを開く（Window > Package Manager）
2. 「+」ボタン > Add package from git URL
3. 以下のURLを入力：
   ```
   https://github.com/yourusername/MidiTimelineSystem.git
   ```

### .unitypackageファイル経由
1. [Releases](https://github.com/yourusername/MidiTimelineSystem/releases)から最新の.unitypackageをダウンロード
2. Unity Editorにドラッグ&ドロップ
3. 必要なアセットを選択してインポート

## 基本機能

### 1. MIDI ファイルの解析
- MIDIファイルを読み込み、トラックとノートイベントに分解
- タイムライン上で扱いやすい形式に変換

### 2. Timeline 統合
- MIDIトラックをTimelineのトラックとして表示
- ノートイベントをTimelineのクリップとして表示
- エディタ上での視覚的な編集をサポート

### 3. イベントシステム
- MIDIイベントに対応するコールバックの登録
- タイムライン再生時のイベント発火
- カスタムイベントハンドラーの実装が可能

## 使用方法

1. MIDIファイルのインポート
   ```csharp
   // MIDIファイルは自動的にインポートされ、アセットとして認識されます
   var midiAsset = AssetDatabase.LoadAssetAtPath<MidiAsset>("Assets/Music/song.mid");
   ```

2. タイムラインへの追加
   ```csharp
   // タイムラインにMIDIトラックを追加
   var timeline = GetComponent<PlayableDirector>();
   var midiTrack = timeline.CreateTrack<MidiTimelineTrack>();
   ```

3. イベントハンドラーの実装
   ```csharp
   public class MyEventHandler : MonoBehaviour
   {
       public void OnNoteOn(int note, int velocity)
       {
           // ノートオンイベントの処理
           Debug.Log($"Note On: {note}, Velocity: {velocity}");
       }

       public void OnNoteOff(int note)
       {
           // ノートオフイベントの処理
           Debug.Log($"Note Off: {note}");
       }
   }
   ```

## サンプル

`Samples~/BasicMidiEvents`フォルダに基本的な使用例が含まれています。
1. サンプルをプロジェクトにインポート
2. `SampleScene`を開く
3. 再生して動作を確認

## 依存パッケージ

- Unity Timeline (com.unity.timeline) 1.6.0以上

## ライセンス

MIT License
