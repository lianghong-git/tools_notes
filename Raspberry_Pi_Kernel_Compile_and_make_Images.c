////////////////////////////////////////////
Cross-compiling
////////////////////////////////////////////

First, you will need a suitable Linux cross-compilation host. We tend to use Ubuntu; since Raspbian is also a Debian distribution, it means many aspects are similar, such as the command lines.

You can either do this using VirtualBox (or VMWare) on Windows, or install it directly onto your computer. For reference, you can follow instructions online at Wikihow.
Install toolchain

Use the following command to install the toolchain:

git clone https://github.com/raspberrypi/tools

You can then copy the /tools/arm-bcm2708/gcc-linaro-arm-linux-gnueabihf-raspbian directory to a common location, and add /tools/arm-bcm2708/gcc-linaro-arm-linux-gnueabihf-raspbian/bin to your $PATH in the .bashrc in your home directory. For 64-bit host systems, use /tools/arm-bcm2708/gcc-linaro-arm-linux-gnueabihf-raspbian-x64/bin. While this step isn't strictly necessary, it does make it easier for later command lines!
Get sources

To get the sources, refer to the original GitHub repository for the various branches.

$ git clone --depth=1 https://github.com/raspberrypi/linux

Build sources

To build the sources for cross-compilation, there may be extra dependencies beyond those you've installed by default with Ubuntu. If you find you need other things, please submit a pull request to change the documentation.

Enter the following commands to build the sources and Device Tree files:

\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
For Pi 1 or Compute Module:
///////////////////////////////////////
cd linux
KERNEL=kernel
make ARCH=arm CROSS_COMPILE=arm-linux-gnueabihf- bcmrpi_defconfig

\\\\\\\\\\\\\\\\\\\\\\\\
For Pi 2/3:
////////////////////////
cd linux
KERNEL=kernel7
make ARCH=arm CROSS_COMPILE=arm-linux-gnueabihf- bcm2709_defconfig

Then, for both:

make ARCH=arm CROSS_COMPILE=arm-linux-gnueabihf- zImage modules dtbs

Note: To speed up compilation on multiprocessor systems, and get some improvement on single processor ones, use -j n, where n is the number of processors * 1.5. Alternatively, feel free to experiment and see what works!
Install directly onto the SD card

Having built the kernel, you need to copy it onto your Raspberry Pi and install the modules; this is best done directly using an SD card reader.

First, use lsblk before and after plugging in your SD card to identify it. You should end up with something like this:

sdb
   sdb1
   sdb2

with sdb1 being the FAT (boot) partition, and sdb2 being the ext4 filesystem (root) partition.

If it's a NOOBS card, you should see something like this:

sdb
  sdb1
  sdb2
  sdb5
  sdb6
  sdb7

with sdb6 being the FAT (boot) partition, and sdb7 being the ext4 filesystem (root) partition.

Mount these first, adjusting the partition numbers for NOOBS cards:

mkdir mnt/fat32
mkdir mnt/ext4
sudo mount /dev/sdb1 mnt/fat32
sudo mount /dev/sdb2 mnt/ext4

Next, install the modules:

sudo make ARCH=arm CROSS_COMPILE=arm-linux-gnueabihf- INSTALL_MOD_PATH=mnt/ext4 modules_install

Finally, copy the kernel and Device Tree blobs onto the SD card, making sure to back up your old kernel:

sudo cp mnt/fat32/$KERNEL.img mnt/fat32/$KERNEL-backup.img
sudo scripts/mkknlimg arch/arm/boot/zImage mnt/fat32/$KERNEL.img
sudo cp arch/arm/boot/dts/*.dtb mnt/fat32/
sudo cp arch/arm/boot/dts/overlays/*.dtb* mnt/fat32/overlays/
sudo cp arch/arm/boot/dts/overlays/README mnt/fat32/overlays/
sudo umount mnt/fat32
sudo umount mnt/ext4

Another option is to copy the kernel into the same place, but with a different filename - for instance, kernel-myconfig.img - rather than overwriting the kernel.img file. You can then edit the config.txt file to select the kernel that the Pi will boot into:

kernel=kernel-myconfig.img

This has the advantage of keeping your kernel separate from the kernel image managed by the system and any automatic update tools, and allowing you to easily revert to a stock kernel in the event that your kernel cannot boot.

Finally, plug the card into the Pi and boot it!









/////////////////////////////////////////
//RaspberryPi Kernel Compile On fedora
// shiguanghu
////////////////////////////////////////
//
yum -y install gcc-arm-linux-gnu
arm-linux-gnu-gcc --version

//
make ARCH=arm CROSS_COMPILE=/usr/bin/arm-linux-gnu- bcmrpi_cutdown_defconfig
or
make ARCH=arm CROSS_COMPILE=/usr/bin/arm-linux-gnu- bcm2835_defconfig
make ARCH=arm CROSS_COMPILE=/usr/bin/arm-linux-gnu- bcmrpi_defconfig //OK!

//
make ARCH=arm CROSS_COMPILE=/usr/bin/arm-linux-gnu-


///////////////////////////////////////
// RaspberryPi Kernel Compile On Ubuntu
// shiguanghu
///////////////////////////////////////

//start!
First, install the package dependencies, git and the cross-compilation toolchain:

    sudo apt-get install git-core gcc-4.6-arm-linux-gnueabi

Create a symlink for the cross compiler:

    sudo ln -s /usr/bin/arm-linux-gnueabi-gcc-4.6 /usr/bin/arm-linux-gnueabi-gcc

Make a directory for the sources and tools, then clone them with git:

    mkdir raspberrypi
    cd raspberrypi
    git clone https://github.com/raspberrypi/tools.git
    git clone https://github.com/raspberrypi/linux.git
    cd linux

Generate the .config file from the pre-packaged raspberry pi one:

    make ARCH=arm CROSS_COMPILE=/usr/bin/arm-linux-gnueabi- bcmrpi_cutdown_defconfig
    or
    make ARCH=arm CROSS_COMPILE=/usr/bin/arm-linux-gnueabi- bcm2835_defconfig
    make ARCH=arm CROSS_COMPILE=/usr/bin/arm-linux-gnueabi- bcmrpi_defconfig	//OK!

If you want to make changes to the configuration, run make menuconfig (optional):

    make ARCH=arm CROSS_COMPILE=/usr/bin/arm-linux-gnueabi- menuconfig

Once you have made the desired changes, save and exit the menuconfig screen. Now we are ready to start the build. You can speed up the compilation process by enabling parallel make with the -j flag. The recommended use is ‘processor cores + 1′, e.g. 5 if you have a quad core processor:

    make ARCH=arm CROSS_COMPILE=/usr/bin/arm-linux-gnueabi- -k -j5
    or
    make ARCH=arm CROSS_COMPILE=/usr/bin/arm-linux-gnueabi-

Assuming the compilation was sucessful, create a directory for the modules:

    mkdir ../modules

Then compile and ‘install’ the loadable modules to the temp directory:

    //
    make modules_install ARCH=arm CROSS_COMPILE=/usr/bin/arm-linux-gnueabi- INSTALL_MOD_PATH=../modules/

Now we need to use imagetool-uncompressed.py from the tools repo to get the kernel ready for the Pi.

    //kernel.img file in /tools/mkimage/
    cd ../tools/mkimage/
    ./imagetool-uncompressed.py ../../linux/arch/arm/boot/Image

This creates a kernel.img in the current directory.

Plug in the SD card of the existing Debian image that you wish to install the new kernel on. Delete the existing kernel.img and replace it with the new one, substituting “boot-partition-uuid” with the identifier of the partion as it is mounted in Ubuntu.

    sudo rm /media/boot-partition-uuid/kernel.img
    sudo mv kernel.img /media/boot-partition-uuid/

Next, remove the existing /lib/modules and lib/firmware directories, substituting “rootfs-partition-uuid” with the identifier of the root filesystem partion mounted in Ubuntu.

    sudo rm -rf /media/rootfs-partition-uuid/lib/modules/
    sudo rm -rf /media/rootfs-partition-uuid/lib/firmware/

Go to the destination directory of the previous make modules_install, and copy the new modules and firmware in their place:

    cd ../../modules/
    sudo cp -a lib/modules/ /media/rootfs-partition-uuid/lib/
    sudo cp -a lib/firmware/ /media/rootfs-partition-uuid/lib/
    sync

That’s it! Exject the SD card, and boot the new kernel on the Raspberry Pi!
