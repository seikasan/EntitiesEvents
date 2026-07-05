# Entities Events

Entities Events は Unity ECS 向けの軽量な System 間メッセージングライブラリです。このフォークは Unity 6 と Entities 1.4 を対象にし、Roslyn Source Generator と unmanaged `ISystem` ベースの cleanup system だけを使います。

[![license](https://img.shields.io/badge/LICENSE-MIT-green.svg)](LICENSE)

[English README is here](README.md)

## 要件

- Unity 6.0 / 6000.0 以上
- Entities 1.4.7 以上

## インストール

Package Manager で `Add package from git URL` を選び、以下を入力します。

```text
https://github.com/seikasan/EntitiesEvents.git?path=Assets/EntitiesEvents
```

または `Packages/manifest.json` の `dependencies` に追加します。

```json
{
  "dependencies": {
    "com.seikasan.entities-events": "https://github.com/seikasan/EntitiesEvents.git?path=Assets/EntitiesEvents"
  }
}
```

## 基本的な使い方

イベントは unmanaged な struct 型をキーにします。Source Generator が cleanup system と generic component registration を生成できるように、使うイベント型を assembly scope で登録します。

```cs
using EntitiesEvents;

public struct MyEvent
{
    public int Value;
}

[assembly: RegisterEvent(typeof(MyEvent))]
```

Writer と Reader は `OnCreate` で取得してキャッシュします。`EventReader<T>` は reader ごとの読み取り位置を持つため、毎フレーム作り直すと同じイベントを重複して読む可能性があります。

```cs
using EntitiesEvents;
using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public partial struct WriteEventSystem : ISystem
{
    EventWriter<MyEvent> _writer;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _writer = state.GetEventWriter<MyEvent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _writer.Write(new MyEvent { Value = 1 });
    }
}

public partial struct ReadEventSystem : ISystem
{
    EventReader<MyEvent> _reader;

    public void OnCreate(ref SystemState state)
    {
        _reader = state.GetEventReader<MyEvent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var eventData in _reader.Read())
        {
            // eventData を処理する。
        }
    }
}
```

## 並列書き込み

並列 writer は内部バッファをリサイズしません。Job を schedule する前に十分な容量を確保し、ワーカースレッド側では `WriteNoResize` を呼びます。

```cs
using EntitiesEvents;
using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public partial struct ParallelWriteEventSystem : ISystem
{
    EventParallelWriter<MyEvent> _writer;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.EnsureEventCapacity<MyEvent>(1024);
        _writer = state.GetEventParallelWriter<MyEvent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Job に _writer を渡し、Execute 内で WriteNoResize を呼ぶ。
    }
}
```

`EnsureEventCapacity<T>(capacity)` は現在フレーム用と次フレーム用の両方のバッファに絶対容量を確保します。`EnsureAdditionalEventCapacity<T>(additionalCapacity)` は現在フレームの書き込み済み数を基準に追加容量を確保します。直接 `Events<T>` を使う場合も、`Capacity`、`CurrentFrameCount`、`RemainingCurrentFrameCapacity`、`EnsureCapacity`、`EnsureAdditionalCapacity` を利用できます。

## イベントの寿命

書き込まれたイベントは、同じフレームと次のフレームで読めます。`Update` が 2 回呼ばれると未読イベントは破棄されます。この設計により、受信 System が送信 System より先に実行されても 1 フレーム遅延で処理できます。ただし、イベントを失いたくない reader は毎フレーム実行してください。同一フレームでの処理が必須なら、`UpdateBefore` や `UpdateAfter` で System の実行順を明示します。

## Events<T>

`Events<T>` は内部で使っている native container です。生成された ECS cleanup system ではなく、自分でイベント寿命を管理したい場合に使います。

```cs
using EntitiesEvents;
using Unity.Collections;

var events = new Events<MyEvent>(32, Allocator.Temp);
var writer = events.GetWriter();
var reader = events.GetReader();

writer.Write(new MyEvent { Value = 1 });

foreach (var eventData in reader.Read())
{
    // eventData を処理する。
}

events.Update();
events.Dispose();
```

## Source Generator の保守

`Assets/EntitiesEvents/Generator/EntitiesEventsGenerator.dll` のソースは `SourceGenerators/EntitiesEvents.Generator` にあります。このフォークは Unity 6 以降のみを対象にするため、`Microsoft.CodeAnalysis.CSharp` は 4.3.0 に固定しています。Unity 2022.3 / Roslyn 3.8 互換経路は残していません。

Generator のソースを変更した後は、DLL をビルドしてパッケージ側へコピーします。

```bash
SourceGenerators/EntitiesEvents.Generator/install-generator.sh
```

Windows ではこちらを使います。

```bat
SourceGenerators\EntitiesEvents.Generator\install-generator.cmd
```

`Assets/EntitiesEvents/Generator/EntitiesEventsGenerator.dll.meta` の `RoslynAnalyzer` ラベルと、プラグイン platform をすべて無効にする設定は維持してください。

## テストとサンプル

Runtime test は `Assets/EntitiesEvents/Tests/Runtime` にあります。Package Manager から取り込める sample は `Assets/EntitiesEvents/Samples~/BasicUsage` にあり、package manifest に登録済みです。

## ライセンス

[MIT License](LICENSE)
