
////////////////////////////////////////////////////////////////
// DNW for Linux
// shiguanghu
////////////////////////////////////////////////////////////////

0.解压 dnw_for_linux.zip， 有俩文件夹secbulk和dnw。

1.编译secbulk.c驱动文件：

	 	secbulk.c文件更改： 当然要添加你的USB设备的 VID，PID。

		//在uboot上输入命令dnw，然后在PC终端上敲lsusb，就能看到有新usb设备显示出来，如：
		//  Bus 002 Device 017: ID 04e8:1234 Samsung Electronics Co., Ltd 

		static struct usb_device_id secbulk_table[]= {
			{ USB_DEVICE(0x04e8, 0x1234) }, 			/* my Tiny4412 */
			{ }
		};

		make编译成功生成driver文件后：
		chmod 777 secbulk.ko
		sudo insmod secbulk.ko 加载进系统。

2.Linux PC还要安装一个库：
	sudo apt-get install libusb-dev

3.编译dnw.c App程序：
	make成功后，把dnw程序 cp 到 /usr/bin 等处，为了在shell下面能直接敲dnw命令。



dnw程序也编译好并可以使用了，secbulk usb驱动也insmode成功了，那就可以开始usb传输了。
4.用法：
		现在可以启动minicom等工具，来尝试用 DNW 从 USB 口下载文件了：

		(1)先在板子串口(uboot)上执行 dnw 0x40008000	//或者不指定下载到SDRAM的地址也可，有默认地址.

		TINY4412 # dnw
		OTG cable Connected!
		Now, Waiting for DNW to transmit data


		(2)然后在另一个终端上执行 dnw arch/arm/boot/zImage ，PC端就开始传了，板子端也在接收了...

		Download Done!! Download Address: 0xc0000000, Download Filesize:0x4808f8
		Checksum is being calculated.....
		Checksum O.K.




