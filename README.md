# Auto Update Config

## 简介

从网络地址下载YAML格式的配置文件，并新增自定义项或修改某些项的值。

- [x] 获取最新配置文件
- [x] 保留自定义设置

### TODO

- [ ] 程序内定时更新
- [ ] issues

## 运行环境

本程序基于 `.NET8`，运行前需要有 `.NET8` 运行时。`Windows` 直接下载安装包安装即可。`Linux` 需要下载运行时二进制文件，放在容易找到的位置，如 `$HOME/net8/`，后续通过该路径执行 `dotnet` 命令。

> 下载地址 https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0

## 示例场景

假设要下载的配置文件网络链接为 `https://abc.com/test.yaml`。

假设 `test.yaml` 中原始内容如下

```
port: 3000
authentication:
  - "admin:123456"
others: something
```

想要新增 `secret` 参数，并设置值为 `123qwe`，同时将 `port` 的值改为 `7000`，应该如何操作呢？

### Step 1

复制 `app.example.json` 并改名为 `app.json`。

### Step 2

打开 `app.json` 修改程序配置。

修改后内容如下

```
{
  "downloadLink": "http://abc.com/test.yaml",
  "saveName": "config.yml",
  "custom": {
    "port": 7000,
    "secret": "123qwe"
  }
}
```

字段|说明
-:|:-
`downloadLink` | **配置文件下载链接**
`saveName` | **文件保存路径** - 若只写文件名，则保存在本程序运行目录。也可写为 `/tmp/test.yaml`，这样就同时指定路径和文件名。
`custom` | **自定义的选项** - 可为空，为空时不重写任何项；原始配置文件中没有的配置项，则会新增；原始配置文件中存在的配置项，则为修改值

### Step 3

#### Windows

双击 `AutoUpdateConfig.exe`

or

```
# 打开Windows终端 进入程序目录
d:
cd AutoUpdateConfig

# 使用dotnet命令执行
dotnet AutoUpdateConfig.dll
```

#### Linux

```
# 进入程序目录
cd $HOME/AutoUpdateConfig

# 使用dotnet命令 执行
$HOME/net8/dotnet AutoUpdateConfig.dll
```

### Step 4

执行成功后，控制台会输出以下内容

```
2024-06-17T10:14:54 [INFO] Get response from https://abc.com/test.yaml
2024-06-17T10:14:56 [INFO] Get response complete
2024-06-17T10:14:56 [INFO] Results of the merger "config.yml"
2024-06-17T10:14:56 [INFO] Exit in 3 seconds
```

打开 `config.yml` 查看最终内容

```
port: 7000
secret: 123qwe
authentication:
  - "admin:123456"
others: something
```
