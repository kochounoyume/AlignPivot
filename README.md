# AlignPivot
AlignPivotは、簡単な操作で、Unityのゲームオブジェクトのピボットをゲームオブジェクトの中央や足元に変更するパッケージです。

AlignPivotを導入すると、以下の９つの位置にゲームオブジェクトのピボットを変更することが可能になります。

- Center(中央)
	選択されたゲームオブジェクト本体と親子関係にあるゲームオブジェクト全体から算出される中心点。
	※早い話が、UnityのツールのCenterボタンを押したときに表示される位置です。
![スクリーンショット (231)](https://user-images.githubusercontent.com/78918084/185788704-fbfa5ca9-d342-4a33-8b94-1727dd147086.png)

- Up(上)：
- Down(下)
- Left(左)
- Right(右)
- Front(手前)
- Back(奥)
- Min(最小)
- Max(最大)


## 使い方
AlignPivotはUnityエディタに導入後、該当のゲームオブジェクトをヒエラルキービューで右クリックして、表示されるメニューから機能を使用することができます。

Center(中央)は `Align Pivot to Center` にて、それ以外のピボットの位置への変更は `Align Pivot SomeOne` から選択することができます。

![pivot](https://user-images.githubusercontent.com/78918084/185788736-7a35c506-eb2a-483b-b7a4-f45c6ba13be7.gif)
