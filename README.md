# Wpf-Vosk

Voskを用いて音声認識を行うWPFアプリケーションです。

## 必要条件

- .NET Framework 4.7.2以上
- Visual Studio 2019以上
- Voskライブラリ

## セットアップ

1. リポジトリをクローンします。

    ```sh
    git clone https://github.com/yourusername/Wpf-Vosk.git
    ```

2. Visual Studioでプロジェクトを開きます。

3. 必要なNuGetパッケージをインストールします。

    ```sh
    Install-Package Vosk
    Install-Package NAudio
    ```

4. [Voskモデル](https://alphacephei.com/vosk/models)をダウンロードし、解凍します。

5. 解凍したモデルを実行ファイル（exe）と同じディレクトリに配置します。

## 使用方法

1. アプリケーションをビルドします。

2. アプリケーションを実行します。

3. マイクを使用して音声を入力し、認識結果を確認します。

## モデルパスの変更

使用するモデルによっては、以下の部分を書き換える必要があります。`MainWindow.xaml.cs`ファイル内のモデルパスを適切なものに変更してください。

```csharp
string modelPath = Path.Combine(AppContext.BaseDirectory, "vosk-model-ja-0.22");
```

例えば、`vosk-model-small-ja-0.22`を使用する場合は、以下のように変更します。

```csharp
string modelPath = Path.Combine(AppContext.BaseDirectory, "vosk-model-small-ja-0.22");
```

## 貢献

1. フォークします。
2. フィーチャーブランチを作成します。

    ```sh
    git checkout -b feature/your-feature
    ```

3. 変更をコミットします。

    ```sh
    git commit -m 'Add some feature'
    ```

4. ブランチにプッシュします。

    ```sh
    git push origin feature/your-feature
    ```

5. プルリクエストを作成します。

## ライセンス

このプロジェクトはMITライセンスの下でライセンスされています。
