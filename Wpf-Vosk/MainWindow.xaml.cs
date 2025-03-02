using System.Windows;
using System.Timers;
using NAudio.Wave;
using System.IO;
using Vosk;

namespace Wpf_Vosk;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private System.Timers.Timer? volumeUpdateTimer;
    private WaveInEvent? waveIn;
    private MemoryStream audioStream = new MemoryStream();
    private Model? model;

    public MainWindow()
    {
        InitializeComponent();
        waveIn = new WaveInEvent(); // waveInを初期化
        waveIn.WaveFormat = new WaveFormat(16000, 1); // 録音フォーマットを設定
        //string modelPath = Path.Combine(AppContext.BaseDirectory, "vosk-model-small-ja-0.22");
        string modelPath = Path.Combine(AppContext.BaseDirectory, "vosk-model-ja-0.22");
        model = new Model(modelPath); // Voskモデルを初期化
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        // 音声認識の開始処理
        StartVoiceRecognition();
        StartVolumeUpdate();
        audioStream = new MemoryStream(); // 音声データを保存するストリームを初期化
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        // 音声認識の終了処理
        StopVoiceRecognition();
        StopVolumeUpdate();

        // 音声データを取得してRecognizeSpeechBufferを呼び出し、結果を表示
        byte[] audioData = GetAudioData(); // 音声データを取得するメソッドを呼び出し
        string result = RecognizeSpeech(audioData);
        MessageBox.Show(result, "認識結果");

        // 音声データをWAVファイルとして出力
        SaveAudioDataAsWav(audioData);
    }

    private void StartVoiceRecognition()
    {
        // 音声認識の初期化と開始
        if (waveIn != null)
        {
            waveIn.DataAvailable -= OnDataAvailable; // 既存のイベントハンドラを削除
            waveIn.DataAvailable += OnDataAvailable; // 新しいイベントハンドラを追加
            waveIn.BufferMilliseconds = 50; // バッファサイズを設定
            waveIn.StartRecording();
        }
    }

    private void StopVoiceRecognition()
    {
        // 音声認識の停止とクリーンアップ
        if (waveIn != null)
        {
            waveIn.StopRecording();
            waveIn.DataAvailable -= OnDataAvailable; // イベントハンドラを削除
        }
    }

    private void StartVolumeUpdate()
    {
        volumeUpdateTimer = new System.Timers.Timer(100);
        volumeUpdateTimer.Elapsed += UpdateVolumeBar;
        volumeUpdateTimer.Start();
    }

    private void StopVolumeUpdate()
    {
        if (volumeUpdateTimer != null)
        {
            volumeUpdateTimer.Stop();
            volumeUpdateTimer.Elapsed -= UpdateVolumeBar; // イベントハンドラを削除
            volumeUpdateTimer.Dispose();
        }
    }

    private void UpdateVolumeBar(object? sender, ElapsedEventArgs e)
    {
        // 音量状況を取得してVolumeBarに反映
        Dispatcher.Invoke(() =>
        {
            VolumeBar.Value = GetVolumeLevel();
        });
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        // 音量レベルを計算
        float max = 0;
        for (int index = 0; index < e.BytesRecorded; index += 2)
        {
            short sample = (short)((e.Buffer[index + 1] << 8) | e.Buffer[index + 0]);
            float sample32 = sample / 32768f;
            if (sample32 < 0) sample32 = -sample32;
            if (sample32 > max) max = sample32;
        }
        Dispatcher.Invoke(() =>
        {
            VolumeBar.Value = max * 100;
        });

        // 音声データを保存
        audioStream.Write(e.Buffer, 0, e.BytesRecorded);
    }

    private double GetVolumeLevel()
    {
        // このメソッドは不要になったので削除
        return 0;
    }

    private byte[] GetAudioData()
    {
        return audioStream.ToArray();
    }

    private string RecognizeSpeech(byte[] audioData)
    {
        if (model == null)
        {
            return "Model is not initialized";
        }

        using (var recognizer = new VoskRecognizer(model, 16000.0f))
        {
            using (var ms = new MemoryStream())
            {
                // WAVEヘッダーを書き込む
                WriteWaveHeader(ms, audioData.Length);

                // 音声データを書き込む
                ms.Write(audioData, 0, audioData.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (var reader = new WaveFileReader(ms))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        recognizer.AcceptWaveform(buffer, bytesRead);
                    }
                }
            }

            return recognizer.FinalResult();
        }
    }

    private void WriteWaveHeader(Stream stream, int dataLength)
    {
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
        {
            writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"));
            writer.Write(36 + dataLength);
            writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"));
            writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(16000);
            writer.Write(16000 * 2);
            writer.Write((short)2);
            writer.Write((short)16);
            writer.Write(System.Text.Encoding.UTF8.GetBytes("data"));
            writer.Write(dataLength);
        }
    }

    private void SaveAudioDataAsWav(byte[] audioData)
    {
        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        string fileName = $"audio_{timestamp}.wav";
        string filePath = Path.Combine(AppContext.BaseDirectory, fileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            WriteWaveHeader(fileStream, audioData.Length);
            fileStream.Write(audioData, 0, audioData.Length);
        }

        MessageBox.Show($"音声データが保存されました: {filePath}", "保存完了");
    }

}