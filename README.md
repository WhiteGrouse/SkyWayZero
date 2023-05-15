# SkyWayZero
新SkyWayの仕組みをUnityで使う方法が見当たらなかったのでシグナリングクライアントを作りました。  
例外処理、再接続、タイムアウト処理はまだ実装してません。  
とりあえず急いでVideoStream, DataStreamを繋げたくて書きました。  

## 以下のライブラリを使っています

- WebRTC for Unity  
https://github.com/Unity-Technologies/com.unity.webrtc

- Json.NET  
https://github.com/JamesNK/Newtonsoft.Json

- UniTask  
https://github.com/Cysharp/UniTask

- websocket-sharp(プレリリース版)  
https://github.com/sta/websocket-sharp
