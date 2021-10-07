
# AR Engine Remote

pc通过usb连接Android手机， 然后通过adb forward建立端口转发。

测试使用的转发端口是30000， 35000， 测试前请避免被其他进程占用。



# 效果1


### 流程

* USB连接到pc， 确保开启调试模式
* Unity导出apk, 在Android手机上安装， 运行起来点击Listening, 开始监听
* Editor运行起来， 点击Adbforward, 建立端口转发
* Editor 点击Connect，建立长链接
* 之后点击Send就可以互发消息了。

切换到 master 分支， 可以联调：

![](.image/adb3.jpg)


# 效果2

预览流推送，切换到 arengine 分支， 可以看到效果


### 流程

* USB连接到pc， 确保开启调试模式
* Unity导出apk, 在Android手机上安装， 运行起来点击Listening, 开始监听
* Editor运行起来， ConnectTest点击齿轮按钮， 分别 connect 建立连接之后就可以看到预览流了
![](.image/2.JPG)

![](.image/1.JPG)


### 特性

目前之前的AR Engine里的特性如下：

1. 预览流推送

![](.image/world.jpg)

2. 点云的绘制

3. 平面检测

4. 场景Mesh生成


5. 手势识别



![](.image/hand.jpg)