### 建置工具組
buildgames.sh 編譯遊戲的腳本  
buildmains.sh 編譯非遊戲的腳本  
dockerrelease.sh 將編譯好的 linux 執行檔複制到 approot 供建置docker image 使用  
gobindockerfile 遊戲服務器的 dockerfile, 會把 approot 加進 image 裡  