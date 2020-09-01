このリポジトリーは、最も単純なスクリーンキャプチャープログラムのソースコードを収録しています。

## ビルド方法
Microsoft Windows 7 Professional SP1 32-bitでの例を以下に示します。

```
> c:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe /t:winexe /win32icon:.\icon\capture.ico capture.cs
```

Visual Studioは必要ありません。


## キャプチャー方法
Ctrl+PrintScreenでアクティブなウィンドウをJPEGファイルで保存します。

Shift+PrintScreenでデスクトップをJPEGファイルで保存します。


## ライセンス
このソースコードは、MIT/Xライセンスでリリースされています。
LICENSEファイルを参照してください。


## 作者
Ryo ONODERA <ryo@tetera.org>
