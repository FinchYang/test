# 工程云解决方案
## 解决方案结构
注意：创建新项目时，一定要确保创建在文件夹里面，VS默认创建项目在Solution文件夹的同一层级，即使选中文件夹再创建项目也不会在选中的文件夹下创建项目。

* Docs文件夹存放简单的文档，详细文档请移步[trac](https://cadms:9527/projects/CustomerProjects/)
* Core文件夹包含了领域模型，数据库操作，数据服务以及WebAPI项目
* Client文件夹包含了客户端的项目，主要是WPF App以及相关的类库
* Web文件夹下为网站项目，包括门户网站，知识库等
* Apps文件夹存放各种应用，都为WinForm或WPF程序
* Tests文件夹下为各个单元测试项目
## 使用的技术或框架
* Entity Framework (6.1.1) - AecCloud.Data项目中使用；一个面向数据库的ORM框架，使用Code First和Fluent API，详见[官方网站](http://msdn.microsoft.com/en-us/data/ef.aspx)
* ASP.NET Web API (2.2)    - DBWorld.AecCloud.Web项目使用；基于ASP.NET的REST结构的Web Service，详见[官方网站](http://www.asp.net/web-api)
* ASP.NET MVC (5.0)        - DBWorld.AecCloud.Web项目使用， 详见[官方网站](http://www.asp.net/mvc)
* WPF                      - AecCloud.Client以及Apps文件夹下各项目使用，详见[官方网站](http://msdn.microsoft.com/zh-cn/library/ms754130(v=vs.110).aspx)
* Autofac                  - DBWorld.AecCloud.Web项目使用；配置依赖注入(DI)或依赖反转(IoC)，详见[官方网站](http://autofac.org/)
* Microsoft.Owin           - DBWorld.AecCloud.Web项目使用；用于用户的认证(Authentication)，详见[官方网站](http://owin.org/)
* NUnit Framework (2.6.3)  - 所有测试项目使用，一个单元测试框架，可以适配到VS的测试环境，详见[官方网站](http://nunit.org/)
* Moq (4.2)                - 所有测试项目使用，一个生成Fake对象实例的库，方便进行单元测试，详见[官方网站](https://github.com/Moq/moq4)
* log4net (2.0.3)          - AecCloud.Client, DBWorld.AecCloud.Web项目中使用；输出日志，详见[官方网站](http://logging.apache.org/log4net/)
* ASP.NET SignalR          - DBWorld.AecCloud.Web项目中使用；用于消息推送，详见[官方网站](http://signalr.net/)
## 各CS项目功能介绍及之间的关系
* AecCloud.Core为领域模型类库，主要包括了数据库的实体类以及缓存需要的实体类对象
* AecCloud.Data为数据库访问的封装类库，使用Entity Framework Code First，对实体类到数据表的映射使用Fluent API，实体类来自于AecCloud.Core类库
* AecCloud.Service为业务逻辑的封装类库，包含了数据库和缓存访问，以及对M-Files服务端的操作(建库等)等，业务模型来自于AecCloud.Core，数据库访问使用AecCloud.Data
* DBWorld.AecCloud.Web为开放到外网的门户网站以及WebAPI,WebAPI供客户端AecCloud.Client使用(用户登录、获取信息等)
* AecCloud.Client为工程云的PC客户端，其数据来自于DBWorld.AecCloud.Web网站的WebAPI，消息推送以及M-Files客户端；某些链接(如：注册等)会指向门户网站的页面
* Apps文件夹下的项目都为执行程序，最终都会嵌入到AecCloud.Client的界面中作为App
## WebAPI实现服务
* 用户登录（用户名，密码） Token形式
* 用户创建项目（项目模板，名称，描述等信息），后台自动指定服务器根据库模板创建M-Files服务端的库
* 用户邀请其他用户参与项目（项目/M-Files库，邀请的用户）
* 被邀请用户接受参与项目的邀请（项目/M-Files库）
## 网站实现（待实现）
* 用户修改帐户的详细信息（公司，地址，邮编，电话等），否则负责人的手机等信息无法获取(域和MF账户都无此信息)
* 设置用户角色、项目负责人等
* 项目聚合页：项目状态(来源？)、成本(来源？)、负责人信息
* 用户聚合页：工作流任务、通知等
* 添加MF服务器
* 更新模版及相应的数据库信息
