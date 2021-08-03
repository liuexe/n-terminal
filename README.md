# nTerminal
A multi-account ordering terminal written in C# to connect to the MT4 web version via WebSocket.
It be tested on Vultr's New York VPS, but is not guaranteed to be available now and in the future.
[Vultr VPS free $100 Credit to Get Started](https://www.vultr.com/?ref=8382011-6G)

#### 介绍
C#写的通过WebSocket连接MT4网页版的多账户下单终端。

把要登录的账号分master/slave一行一个账号(例：account,password,ICMarketsSC-Live06,master/slave)写在accounts.txt文件里，通过终端登录网页版MT4，终端即可操作slave账号跟着master账号下单/平仓。

终端与MT4官方服务器通过WebSocket直接通信。通信协议是根据网页版MT4的js文件和抓取WebSocket数据分析(猜)出来的。resources文件夹里是分析过程中的用的一些主要参考。

本人水平&精力有限很多东西并没有猜出来，but貌似不影响下单和平仓。

曾经实盘几十个账户在Vultr的New York虚拟机上跑了大概半年，但是时间久远不能保证现在及以后还能用。

[附：Vultr免费$100注册码](https://www.vultr.com/?ref=8382011-6G)

有兴趣的小伙伴可以一起研究研究 qq群：660937351
