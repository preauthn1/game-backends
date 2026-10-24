# Deploy Server

讓外部可以透過 RestfulAPI 重新部署 Server

## Build Command

#### Use Makefile

``` bash
$ make compile # build for native OS

$ make compile-win # build for windows
```

#### With Golang

``` bash
$ GOOS=${OS} go build -o ./bin/deploy ./cmd/deploy
```

> `${OS}` should be `linux` / `darwin` / `windows`

## Run Server

#### Workspace

在 Workspace 下準備 `configs/deploy.yml` 設定檔，啟動 `deploy.exe` 執行檔

#### Config File `deploy.yml`

|Name            |Type     |Required   |Description                                                   |
|:---------------|:-------:|:---------:|:-------------------------------------------------------------|
|server.port     |int      |Required   |Listen port for http                                          |
|release.root    |string   |Required   |Source folder to copy                                         |
|target.root     |string   |Required   |Target folder to copy                                         |
|target.gsdir    |string   |Required   |Sub folder to copy                                            |
|target.srcdir   |string   |Required   |File folder to copy                                           |
|target.extname  |string   |Optional   |Spectific ext name for executable file                        |
|stop.base       |string   |Required   |The base path for commands that will execute on stop servers  |
|stop.commands   |[]string |Required   |The commands to execute on stop servers                       |
|start.base      |string   |Required   |The base path for commands that will execute on start servers |
|start.commands  |[]string |Required   |The commands to execute on start servers                      |
|notify.webhook  |string   |Required   |Notify API endpoint URL                                       |

#### Flags

|Full Name       |Type     |Short Name |Description                 |
|:---------------|:-------:|:---------:|:---------------------------|
|config          |string   |c          |Config file location        |
|server.port     |int      |p          |Listen port for http        |

Example: 
``` bash
$ deploy -p 8080 -c ./configs/deploy.yml
```