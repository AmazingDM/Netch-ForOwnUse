# Netch-ForOwnUse

[![Platform](https://img.shields.io/badge/platform-windows-orange.svg)](https://github.com/AmazingDM/Netch-ForOwnUse)
[![Version](https://img.shields.io/github/v/release/AmazingDM/Netch-ForOwnUse)](https://github.com/AmazingDM/Netch-ForOwnUse/releases)
[![Downloads](https://img.shields.io/github/downloads/AmazingDM/Netch-ForOwnUse/total.svg)](https://github.com/AmazingDM/Netch-ForOwnUse/releases)
[![Netch CI](https://github.com/AmazingDM/Netch-ForOwnUse/workflows/Netch%20CI/badge.svg)](https://github.com/AmazingDM/Netch-ForOwnUse/actions)
[![License](https://img.shields.io/badge/license-MIT-yellow.svg)](LICENSE)
[![](https://img.shields.io/badge/Telegram-频道-blue)](https://t.me/Netch) [![](https://img.shields.io/badge/Telegram-讨论组-green)](https://t.me/Netch_Discuss_Group)
[![License](https://img.shields.io/badge/license-MIT-yellow.svg)](LICENSE)

游戏加速工具

[网站](https://netch.org/)

[常见问题](https://netch.org/#/docs/zh-CN/faq)

## 相比原版 Netch 新增特性

-   进程模式TCP IP过滤器
-   进程模式已代理IP(TCP&UDP)日志打印
-   TCP UDP 分流（开启分流时无法进行流量统计）
-   允许多开 Netch（进程模式因驱动限制无法多开）
-   [~~进程白名单模式（全局）~~](https://netch.org/#/docs/zh-CN/mode?id=%e8%bf%9b%e7%a8%8b%e4%bb%a3%e7%90%86%e6%a8%a1%e5%bc%8f)
-   ~~进程模式不代理UDP（已合并主仓库）~~
-   ~~流量统计（已合并主仓库）~~
-   ~~子进程捕获（已合并主仓库）~~
-   ~~DNS转发（已合并主仓库）~~
-   ~~ICMP转发（已合并主仓库）~~

## 进程白名单模式

不开白名单选项时，
选bf1游戏模式，只有bf1.exe走代理；其他所有软件直连；

打开白名单选项后,
选bf1游戏模式，变成所有软件都走代理，bf1.exe反而变成直连。

## 进程模式进阶用法

-   进程名模糊匹配和路径匹配（原版有但是没写很多人不知道）

示例：

```
# test, 0
ntt.exe
Desktop
Desktop\NatTypeTester.exe
C:\Users\xxxx\Desktop\NatTypeTester.exe
```

-   IP 过滤器

高阶用法，启用 IP 过滤器需要在模式`备注`加上 `IPFillter=true`，并且规则内需要有 IPV4（匹配模式往下看） ，Netch 会根据模式备注里的 IPFillter 来配置开关，没加过滤器开关时默认为 true，注意`IP过滤器`仅在 TCP 生效

true 除规则内 IP 全走代理

false 仅代理规则内 IP

示例：

```
# test(IPFillter=true), 0
ntt.exe
test.exe
Desktop
Desktop\NatTypeTester.exe
C:\Users\xxxx\Desktop\NatTypeTester.exe
123
123.123
66.66.233
123.123.233.233
45.
```

以示例配置规则为例

IP 过滤器设置：除规则内 IP 全走代理

请求 IP 123.123.233.233 -> 过滤器（匹配规则 `123`） -> 不走代理

请求 IP 66.66.233.56 -> 过滤器（匹配规则 `66.66.233`） -> 不走代理

请求 IP 103.124.99.99 -> 过滤器（没有匹配到过滤 IP） -> 走代理

请求 IP 45.45.45.45 -> 过滤器（匹配规则 `45.`） -> 不走代理

IP 匹配规则是从头开始匹配（为什么不用和 TUN/TAP 规则同样方式？因为懒）

## TOC

-   [Netch](#Netch) - [TOC](#TOC) - [简介](#简介)
    -   [新手入门](doc/Quickstart.zh-CN.md)
    -   [进阶用法](https://github.com/NormanBB/NetchMode/blob/master/docs/README.zh-CN.md) - [依赖（必装，否则会启动失败）](#依赖（必装，否则会启动失败）)
    -   [语言支持](#语言支持)

## 简介

Netch 是一款 Windows 平台的开源游戏加速工具，Netch 可以实现类似 SocksCap64 那样的进程代理，也可以实现 SSTap 那样的全局 TUN/TAP 代理，和 Shadowsocks-Windows 那样的本地 Socks5，HTTP 和系统代理。至于连接至远程服务器的代理协议，目前 Netch 支持以下代理协议：Shadowsocks，VMess，Socks5，ShadowsocksR

与此同时 Netch 避免了 SSTap 的 NAT 问题 ，检查 NAT 类型即可知道是否有 NAT 问题。使用 SSTap 加速部分 P2P 联机，对 NAT 类型有要求的游戏时，可能会因为 NAT 类型严格遇到无法加入联机，或者其他影响游戏体验的情况

## 新手入门

[新手入门教程](Quickstart.zh-CN.md)

## 进阶用法

[进阶教程](https://github.com/NormanBB/NetchMode/blob/master/docs/README.zh-CN.md)

## 依赖（必装，否则会启动失败）

-   [Visual C++ 运行库合集](https://www.google.com/search?q=Visual+C%2B%2B+%E8%BF%90%E8%A1%8C%E5%BA%93%E5%90%88%E9%9B%86)
-   [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net48-offline-installer)
-   [TAP-Windows](https://build.openvpn.net/downloads/releases/tap-windows-9.21.2.exe)

## 语言支持

Netch 支持多种语言，在启动时会根据系统语言选择自身语言。如果需要手动切换语言，可以在启动时加入命令行参数，命令行参数为目前支持的语言代码，可以去 [NetchTranslation/i18n](https://github.com/NetchX/NetchTranslation/tree/master/i18n) 文件夹下查看外部支持的语言代码文件。Netch 目前内置 en-US，zh-CN，外置 zh-TW。欢迎大家为 [NetchTranslation](https://github.com/NetchX/NetchTranslation) 提供其他语言的翻译

## 引用

-   [aiodns](https://github.com/aiocloud/aiodns)
-   [go-tun2socks](https://github.com/eycorsican/go-tun2socks)
-   [shadowsocks-libev](https://github.com/shadowsocks/shadowsocks-libev)
-   [shadowsocksr-libev](https://github.com/shadowsocksrr/shadowsocksr-libev)
-   [v2ray-core](https://github.com/v2ray/v2ray-core)
-   [trojan](https://github.com/trojan-gfw/trojan)
-   [ACL4SSR](https://github.com/ACL4SSR/ACL4SSR)
-   [dnsmasq-china-list](https://github.com/felixonmars/dnsmasq-china-list)
-   [tap-windows6](https://github.com/OpenVPN/tap-windows6)
-   [Privoxy](https://www.privoxy.org/)
-   [NatTypeTester](https://github.com/HMBSbige/NatTypeTester)
-   [NetFilter SDK](https://netfiltersdk.com/)

[![Stargazers over time](https://starchart.cc/AmazingDM/Netch-ForOwnUse.svg)](https://starchart.cc/AmazingDM/Netch-ForOwnUse)     
