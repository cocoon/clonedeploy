﻿using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CloneDeploy_ApiCalls;
using CloneDeploy_Entities.DTOs;
using CloneDeploy_Services.Helpers;
using log4net;

namespace CloneDeploy_Services.Workflows
{
    public class DefaultBootMenu
    {
        private readonly ILog log = LogManager.GetLogger("ApplicationLog");
        private const string NewLineChar = "\n";
        private readonly string _globalComputerArgs = Settings.GlobalComputerArgs;
        private string _userToken { get; set; }
        private readonly string _webPath = Settings.WebPath;
        private readonly BootMenuGenOptionsDTO _defaultBoot;
        readonly Regex _alphaNumericSpace = new Regex("[^a-zA-Z0-9 ]");
        readonly Regex _alphaNumericNoSpace = new Regex("[^a-zA-Z0-9]");
        private readonly BootEntryServices _bootEntryServices;

        public DefaultBootMenu(BootMenuGenOptionsDTO defaultBootMenu)
        {
            _defaultBoot = defaultBootMenu;
            _bootEntryServices = new BootEntryServices();
        }

        public void CreateGlobalDefaultBootMenu()
        {
            if (Settings.DebugRequiresLogin == "No" || Settings.OnDemandRequiresLogin == "No" ||
                Settings.RegisterRequiresLogin == "No")
                _userToken = Settings.UniversalToken;
            else
            {
                _userToken = "";
            }

            var mode = Settings.PxeMode;

            if (_defaultBoot.Type == "standard")
            {
                if (mode.Contains("ipxe"))
                    CreateIpxeMenu();
                else if (mode.Contains("grub"))
                    CreateGrubMenu();
                else
                    CreateSyslinuxMenu();
            }
            else
            {
                CreateIpxeMenu();
                CreateSyslinuxMenu();
                CreateGrubMenu();
            }
        }

        private void CreateGrubMenu()
        {
            var customMenuEntries = _bootEntryServices.SearchBootEntrys().Where(x => x.Type == "grub" && x.Active == 1).OrderBy(x => x.Order).ThenBy(x => x.Name);
            var defaultCustomEntry = customMenuEntries.FirstOrDefault(x => x.Default == 1);

            var grubMenu = new StringBuilder();

            grubMenu.Append("insmod password_pbkdf2" + NewLineChar);
            grubMenu.Append("insmod regexp" + NewLineChar);
            grubMenu.Append("set default=0" + NewLineChar);
            grubMenu.Append("set timeout=10" + NewLineChar);
            grubMenu.Append("set pager=1" + NewLineChar);
            if (!string.IsNullOrEmpty(_defaultBoot.GrubUserName) && !string.IsNullOrEmpty(_defaultBoot.GrubPassword))
            {
                grubMenu.Append("set superusers=\"" + _defaultBoot.GrubUserName + "\"" + NewLineChar);
                string sha = null;
                try
                {
                    sha =
                        new WebClient().DownloadString(
                            "http://docs.clonedeploy.org/grub-pass-gen/encrypt.php?password=" + _defaultBoot.GrubPassword);
                    sha = sha.Replace("\n \n\n\n", "");
                }
                catch
                {
                    log.Debug("Could not generate sha for grub password.  Could not contact http://clonedeploy.org");
                }
                grubMenu.Append("password_pbkdf2 " + _defaultBoot.GrubUserName + " " + sha + "" + NewLineChar);
                grubMenu.Append("export superusers" + NewLineChar);
                grubMenu.Append("" + NewLineChar);
            }
            grubMenu.Append(@"regexp -s 1:b1 '(.{1,3}):(.{1,3}):(.{1,3}):(.{1,3}):(.{1,3}):(.{1,3})' $net_default_mac" +
                            NewLineChar);
            grubMenu.Append(@"regexp -s 2:b2 '(.{1,3}):(.{1,3}):(.{1,3}):(.{1,3}):(.{1,3}):(.{1,3})' $net_default_mac" +
                            NewLineChar);
            grubMenu.Append(@"regexp -s 3:b3 '(.{1,3}):(.{1,3}):(.{1,3}):(.{1,3}):(.{1,3}):(.{1,3})' $net_default_mac" +
                            NewLineChar);
            grubMenu.Append(@"regexp -s 4:b4 '(.{1,3}):(.{1,3}):(.{1,3}):(.{1,3}):(.{1,3}):(.{1,3})' $net_default_mac" +
                            NewLineChar);
            grubMenu.Append(@"regexp -s 5:b5 '(.{1,3}):(.{1,3}):(.{1,3}):(.{1,3}):(.{1,3}):(.{1,3})' $net_default_mac" +
                            NewLineChar);
            grubMenu.Append(@"regexp -s 6:b6 '(.{1,3}):(.{1,3}):(.{1,3}):(.{1,3}):(.{1,3}):(.{1,3})' $net_default_mac" +
                            NewLineChar);
            grubMenu.Append(@"mac=01-$b1-$b2-$b3-$b4-$b5-$b6" + NewLineChar);
            grubMenu.Append("" + NewLineChar);


            if (_defaultBoot.Type == "standard")
            {
                grubMenu.Append("if [ -s /pxelinux.cfg/$mac.cfg ]; then" + NewLineChar);
                grubMenu.Append("configfile /pxelinux.cfg/$mac.cfg" + NewLineChar);
                grubMenu.Append("fi" + NewLineChar);
            }
            else
            {
                grubMenu.Append("if [ -s /proxy/efi64/pxelinux.cfg/$mac.cfg ]; then" + NewLineChar);
                grubMenu.Append("configfile /proxy/efi64/pxelinux.cfg/$mac.cfg" + NewLineChar);
                grubMenu.Append("fi" + NewLineChar);
            }

            if (defaultCustomEntry != null)
            {
                grubMenu.Append("" + NewLineChar);
                grubMenu.Append("menuentry \"" + _alphaNumericSpace.Replace(defaultCustomEntry.Name,"") + "\" --unrestricted {" + NewLineChar);
                grubMenu.Append(defaultCustomEntry.Content + NewLineChar);
                grubMenu.Append("}" + NewLineChar);
            }

            grubMenu.Append("" + NewLineChar);
            grubMenu.Append("menuentry \"Boot To Local Machine\" --unrestricted {" + NewLineChar);
            grubMenu.Append("exit" + NewLineChar);
            grubMenu.Append("}" + NewLineChar);

            grubMenu.Append("" + NewLineChar);

            grubMenu.Append("menuentry \"Client Console\" --user {" + NewLineChar);
            grubMenu.Append("echo Please Wait While The Boot Image Is Transferred.  This May Take A Few Minutes." +
                            NewLineChar);
            grubMenu.Append("linux /kernels/" + _defaultBoot.Kernel + " root=/dev/ram0 rw ramdisk_size=156000 " + " web=" +
                            _webPath +
                            " USER_TOKEN=" + _userToken + " task=debug consoleblank=0 " + _globalComputerArgs + "" + NewLineChar);
            grubMenu.Append("initrd /images/" + _defaultBoot.BootImage + "" + NewLineChar);
            grubMenu.Append("}" + NewLineChar);

            grubMenu.Append("" + NewLineChar);

            grubMenu.Append("menuentry \"On Demand Imaging\" --user {" + NewLineChar);
            grubMenu.Append("echo Please Wait While The Boot Image Is Transferred.  This May Take A Few Minutes." +
                            NewLineChar);
            grubMenu.Append("linux /kernels/" + _defaultBoot.Kernel + " root=/dev/ram0 rw ramdisk_size=156000 " + " web=" +
                            _webPath +
                            " USER_TOKEN=" + _userToken + " task=ond consoleblank=0 " + _globalComputerArgs + "" + NewLineChar);
            grubMenu.Append("initrd /images/" + _defaultBoot.BootImage + "" + NewLineChar);
            grubMenu.Append("}" + NewLineChar);

            grubMenu.Append("" + NewLineChar);

            grubMenu.Append("menuentry \"Add Computer\" --user {" + NewLineChar);
            grubMenu.Append("echo Please Wait While The Boot Image Is Transferred.  This May Take A Few Minutes." +
                            NewLineChar);
            grubMenu.Append("linux /kernels/" + _defaultBoot.Kernel + " root=/dev/ram0 rw ramdisk_size=156000 " + " web=" +
                            _webPath +
                            " USER_TOKEN=" + _userToken + " task=register consoleblank=0 " + _globalComputerArgs + "" +
                            NewLineChar);
            grubMenu.Append("initrd /images/" + _defaultBoot.BootImage + "" + NewLineChar);
            grubMenu.Append("}" + NewLineChar);

            grubMenu.Append("" + NewLineChar);

            grubMenu.Append("menuentry \"Diagnostics\" --user {" + NewLineChar);
            grubMenu.Append("echo Please Wait While The Boot Image Is Transferred.  This May Take A Few Minutes." +
                            NewLineChar);
            grubMenu.Append("linux /kernels/" + _defaultBoot.Kernel + " root=/dev/ram0 rw ramdisk_size=156000 " + " web=" +
                            _webPath +
                            " USER_TOKEN=" + _userToken + " task=diag consoleblank=0 " + _globalComputerArgs + "" + NewLineChar);
            grubMenu.Append("initrd /images/" + _defaultBoot.BootImage + "" + NewLineChar);
            grubMenu.Append("}" + NewLineChar);

            foreach (var customEntry in customMenuEntries)
            {
                if (defaultCustomEntry != null && customEntry.Id == defaultCustomEntry.Id)
                    continue;

                grubMenu.Append("" + NewLineChar);
                grubMenu.Append("menuentry \"" + _alphaNumericSpace.Replace(customEntry.Name, "") +
                                "\" --user {" + NewLineChar);
                grubMenu.Append(customEntry.Content + NewLineChar);
                grubMenu.Append("}" + NewLineChar);

                grubMenu.Append("" + NewLineChar);
            }

            var path = Settings.TftpPath + "grub" + Path.DirectorySeparatorChar + "grub.cfg";

            if (Settings.OperationMode == "Single")
                new FileOps().WritePath(path, grubMenu.ToString());
            else
            {
                if (Settings.TftpServerRole)
                    new FileOps().WritePath(path, grubMenu.ToString());
                var tftpServers = new SecondaryServerServices().SearchSecondaryServers().Where(x => x.TftpRole == 1);
                foreach (var tftpServer in tftpServers)
                {
                    var tftpPath =
                        new APICall(new SecondaryServerServices().GetApiToken(tftpServer.Name))
                            .SettingApi.GetSetting("Tftp Path").Value;

                    var tftpFile = new TftpFileDTO();
                    tftpFile.Contents = grubMenu.ToString();
                    tftpFile.Path = tftpPath + "grub" + Path.DirectorySeparatorChar + "grub.cfg";

                    new APICall(new SecondaryServerServices().GetApiToken(tftpServer.Name))
                        .ServiceAccountApi.WriteTftpFile(tftpFile);
                }
            }
        }

        private void CreateIpxeMenu()
        {
            var customMenuEntries = _bootEntryServices.SearchBootEntrys().Where(x => x.Type == "ipxe" && x.Active == 1).OrderBy(x => x.Order).ThenBy(x => x.Name);
            var defaultCustomEntry = customMenuEntries.FirstOrDefault(x => x.Default == 1);

            var ipxeMenu = new StringBuilder();

            ipxeMenu.Append("#!ipxe" + NewLineChar);
            ipxeMenu.Append("chain 01-${net0/mac:hexhyp}.ipxe || chain 01-${net1/mac:hexhyp}.ipxe || goto Menu" + NewLineChar);
            ipxeMenu.Append("" + NewLineChar);
            ipxeMenu.Append(":Menu" + NewLineChar);
            ipxeMenu.Append("menu Boot Menu" + NewLineChar);
            ipxeMenu.Append("item bootLocal Boot To Local Machine" + NewLineChar);
            ipxeMenu.Append("item console Client Console" + NewLineChar);
            ipxeMenu.Append("item register Add Computer" + NewLineChar);
            ipxeMenu.Append("item ond On Demand" + NewLineChar);
            ipxeMenu.Append("item diag Diagnostics" + NewLineChar);
            foreach (var customEntry in customMenuEntries)
            {
                ipxeMenu.Append("item " + _alphaNumericNoSpace.Replace(customEntry.Name, "") + " " +
                                _alphaNumericSpace.Replace(customEntry.Name, "") + NewLineChar);
            }
            if(defaultCustomEntry == null)
                ipxeMenu.Append("choose --default bootLocal --timeout 5000 target && goto ${target}" + NewLineChar);
            else
            {
                ipxeMenu.Append("choose --default " + _alphaNumericNoSpace.Replace(defaultCustomEntry.Name,"") + " --timeout 5000 target && goto ${target}" + NewLineChar);
            }
            ipxeMenu.Append("" + NewLineChar);

            if (Settings.IpxeRequiresLogin == "True")
            {
                ipxeMenu.Append(":bootLocal" + NewLineChar);
                ipxeMenu.Append("exit" + NewLineChar);
                ipxeMenu.Append("" + NewLineChar);

                ipxeMenu.Append(":console" + NewLineChar);
                ipxeMenu.Append("set task debug" + NewLineChar);
                ipxeMenu.Append("goto login" + NewLineChar);
                ipxeMenu.Append("" + NewLineChar);

                ipxeMenu.Append(":register" + NewLineChar);
                ipxeMenu.Append("set task register" + NewLineChar);
                ipxeMenu.Append("goto login" + NewLineChar);
                ipxeMenu.Append("" + NewLineChar);

                ipxeMenu.Append(":ond" + NewLineChar);
                ipxeMenu.Append("set task ond" + NewLineChar);
                ipxeMenu.Append("goto login" + NewLineChar);

                ipxeMenu.Append("" + NewLineChar);
                ipxeMenu.Append(":diag" + NewLineChar);
                ipxeMenu.Append("set task diag" + NewLineChar);
                ipxeMenu.Append("goto login" + NewLineChar);
                ipxeMenu.Append("" + NewLineChar);

                ipxeMenu.Append(":login" + NewLineChar);
                ipxeMenu.Append("login" + NewLineChar);
                ipxeMenu.Append("params" + NewLineChar);
                ipxeMenu.Append("param uname ${username:uristring}" + NewLineChar);
                ipxeMenu.Append("param pwd ${password:uristring}" + NewLineChar);
                ipxeMenu.Append("param kernel " + _defaultBoot.Kernel + "" + NewLineChar);
                ipxeMenu.Append("param bootImage " + _defaultBoot.BootImage + "" + NewLineChar);
                ipxeMenu.Append("param task " + "${task}" + "" + NewLineChar);
                ipxeMenu.Append("echo Authenticating" + NewLineChar);
                ipxeMenu.Append("chain --timeout 15000 " + Settings.WebPath + "IpxeLogin##params || goto Menu" +
                                NewLineChar);
            }
            else
            {
                ipxeMenu.Append(":bootLocal" + NewLineChar);
                ipxeMenu.Append("exit" + NewLineChar);
                ipxeMenu.Append("" + NewLineChar);

                ipxeMenu.Append(":console" + NewLineChar);
                ipxeMenu.Append("kernel " + Settings.WebPath + "IpxeBoot?filename=" + _defaultBoot.Kernel + "&type=kernel" +
                                " initrd=" + _defaultBoot.BootImage + " root=/dev/ram0 rw ramdisk_size=156000 " + " web=" +
                                Settings.WebPath + " USER_TOKEN=" + _userToken + " task=debug" + " consoleblank=0 " +
                                _globalComputerArgs + NewLineChar);
                ipxeMenu.Append("imgfetch --name " + _defaultBoot.BootImage + " " + Settings.WebPath + "IpxeBoot?filename=" +
                                _defaultBoot.BootImage + "&type=bootimage" + NewLineChar);
                ipxeMenu.Append("boot" + NewLineChar);
                ipxeMenu.Append("" + NewLineChar);

                ipxeMenu.Append(":register" + NewLineChar);
                ipxeMenu.Append("kernel " + Settings.WebPath + "IpxeBoot?filename=" + _defaultBoot.Kernel + "&type=kernel" +
                                " initrd=" + _defaultBoot.BootImage + " root=/dev/ram0 rw ramdisk_size=156000 " + " web=" +
                                Settings.WebPath + " USER_TOKEN=" + _userToken + " task=register" + " consoleblank=0 " +
                                _globalComputerArgs + NewLineChar);
                ipxeMenu.Append("imgfetch --name " + _defaultBoot.BootImage + " " + Settings.WebPath + "IpxeBoot?filename=" +
                                _defaultBoot.BootImage + "&type=bootimage" + NewLineChar);
                ipxeMenu.Append("boot" + NewLineChar);
                ipxeMenu.Append("" + NewLineChar);

                ipxeMenu.Append(":ond" + NewLineChar);
                ipxeMenu.Append("kernel " + Settings.WebPath + "IpxeBoot?filename=" + _defaultBoot.Kernel + "&type=kernel" +
                                " initrd=" + _defaultBoot.BootImage + " root=/dev/ram0 rw ramdisk_size=156000 " + " web=" +
                                Settings.WebPath + " USER_TOKEN=" + _userToken + " task=ond" + " consoleblank=0 " +
                                _globalComputerArgs + NewLineChar);
                ipxeMenu.Append("imgfetch --name " + _defaultBoot.BootImage + " " + Settings.WebPath + "IpxeBoot?filename=" +
                                _defaultBoot.BootImage + "&type=bootimage" + NewLineChar);
                ipxeMenu.Append("boot" + NewLineChar);
                ipxeMenu.Append("" + NewLineChar);

                ipxeMenu.Append(":diag" + NewLineChar);
                ipxeMenu.Append("kernel " + Settings.WebPath + "IpxeBoot?filename=" + _defaultBoot.Kernel + "&type=kernel" +
                                " initrd=" + _defaultBoot.BootImage + " root=/dev/ram0 rw ramdisk_size=156000 " + " web=" +
                                Settings.WebPath + " USER_TOKEN=" + _userToken + " task=diag" + " consoleblank=0 " +
                                _globalComputerArgs + NewLineChar);
                ipxeMenu.Append("imgfetch --name " + _defaultBoot.BootImage + " " + Settings.WebPath + "IpxeBoot?filename=" +
                                _defaultBoot.BootImage + "&type=bootimage" + NewLineChar);
                ipxeMenu.Append("boot" + NewLineChar);
                ipxeMenu.Append("" + NewLineChar);


            }

            //Set Custom Menu Entries
            foreach (var customEntry in customMenuEntries)
            {
                ipxeMenu.Append(":" + _alphaNumericNoSpace.Replace(customEntry.Name, "") + NewLineChar);
                ipxeMenu.Append(customEntry.Content + NewLineChar);
            }

            string path;
            if (_defaultBoot.Type == "standard")
                path = Settings.TftpPath + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default.ipxe";
            else
                path = Settings.TftpPath + "proxy" + Path.DirectorySeparatorChar + _defaultBoot.Type +
                       Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default.ipxe";


            if (Settings.OperationMode == "Single")
                new FileOps().WritePath(path, ipxeMenu.ToString());
            else
            {
                if (Settings.TftpServerRole)
                    new FileOps().WritePath(path, ipxeMenu.ToString());

                var tftpServers = new SecondaryServerServices().SearchSecondaryServers().Where(x => x.TftpRole == 1);
                foreach (var tftpServer in tftpServers)
                {
                    var tftpPath =
                        new APICall(new SecondaryServerServices().GetApiToken(tftpServer.Name))
                            .SettingApi.GetSetting("Tftp Path").Value;

                    var tftpFile = new TftpFileDTO();
                    tftpFile.Contents = ipxeMenu.ToString();
                    if (_defaultBoot.Type == "standard")
                        tftpFile.Path = tftpPath + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default.ipxe";
                    else
                        tftpFile.Path = tftpPath + "proxy" + Path.DirectorySeparatorChar + _defaultBoot.Type +
                               Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default.ipxe";

                    new APICall(new SecondaryServerServices().GetApiToken(tftpServer.Name))
                        .ServiceAccountApi.WriteTftpFile(tftpFile);
                }
            }


            
        }

        private void CreateSyslinuxMenu()
        {
            var customMenuEntries = _bootEntryServices.SearchBootEntrys().Where(x => x.Type == "syslinux/pxelinux" && x.Active == 1).OrderBy(x => x.Order).ThenBy(x => x.Name);
            var defaultCustomEntry = customMenuEntries.FirstOrDefault(x => x.Default == 1);
            var sysLinuxMenu = new StringBuilder();

            sysLinuxMenu.Append("DEFAULT vesamenu.c32" + NewLineChar);
            sysLinuxMenu.Append("MENU TITLE Boot Menu" + NewLineChar);
            sysLinuxMenu.Append("MENU BACKGROUND bg.png" + NewLineChar);
            sysLinuxMenu.Append("menu tabmsgrow 22" + NewLineChar);
            sysLinuxMenu.Append("menu cmdlinerow 22" + NewLineChar);
            sysLinuxMenu.Append("menu endrow 24" + NewLineChar);
            sysLinuxMenu.Append("menu color title                1;34;49    #eea0a0ff #cc333355 std" + NewLineChar);
            sysLinuxMenu.Append("menu color sel                  7;37;40    #ff000000 #bb9999aa all" + NewLineChar);
            sysLinuxMenu.Append("menu color border               30;44      #ffffffff #00000000 std" + NewLineChar);
            sysLinuxMenu.Append("menu color pwdheader            31;47      #eeff1010 #20ffffff std" + NewLineChar);
            sysLinuxMenu.Append("menu color hotkey               35;40      #90ffff00 #00000000 std" + NewLineChar);
            sysLinuxMenu.Append("menu color hotsel               35;40      #90000000 #bb9999aa all" + NewLineChar);
            sysLinuxMenu.Append("menu color timeout_msg          35;40      #90ffffff #00000000 none" + NewLineChar);
            sysLinuxMenu.Append("menu color timeout              31;47      #eeff1010 #00000000 none" + NewLineChar);
            sysLinuxMenu.Append("NOESCAPE 0" + NewLineChar);
            sysLinuxMenu.Append("ALLOWOPTIONS 0" + NewLineChar);
            sysLinuxMenu.Append("" + NewLineChar);
            sysLinuxMenu.Append("LABEL local" + NewLineChar);
            sysLinuxMenu.Append("localboot 0" + NewLineChar);
            if(defaultCustomEntry == null)
                sysLinuxMenu.Append("MENU DEFAULT" + NewLineChar);
            sysLinuxMenu.Append("MENU LABEL Boot To Local Machine" + NewLineChar);
            sysLinuxMenu.Append("" + NewLineChar);

            sysLinuxMenu.Append("LABEL Client Console" + NewLineChar);
            if (!string.IsNullOrEmpty(_defaultBoot.DebugPwd) && _defaultBoot.DebugPwd != "Error: Empty password")
                sysLinuxMenu.Append("MENU PASSWD " + _defaultBoot.DebugPwd + "" + NewLineChar);

            sysLinuxMenu.Append("kernel kernels" + Path.DirectorySeparatorChar + _defaultBoot.Kernel + "" + NewLineChar);
            sysLinuxMenu.Append("append initrd=images" + Path.DirectorySeparatorChar + _defaultBoot.BootImage +
                                " root=/dev/ram0 rw ramdisk_size=156000 " + " web=" + _webPath + " USER_TOKEN=" +
                                _userToken +
                                " task=debug consoleblank=0 " + _globalComputerArgs + "" + NewLineChar);

            sysLinuxMenu.Append("MENU LABEL Client Console" + NewLineChar);
            sysLinuxMenu.Append("" + NewLineChar);

            sysLinuxMenu.Append("LABEL Add Computer" + NewLineChar);
            if (!string.IsNullOrEmpty(_defaultBoot.AddPwd) && _defaultBoot.AddPwd != "Error: Empty password")
                sysLinuxMenu.Append("MENU PASSWD " + _defaultBoot.AddPwd + "" + NewLineChar);

            sysLinuxMenu.Append("kernel kernels" + Path.DirectorySeparatorChar + _defaultBoot.Kernel + "" + NewLineChar);
            sysLinuxMenu.Append("append initrd=images" + Path.DirectorySeparatorChar + _defaultBoot.BootImage +
                                " root=/dev/ram0 rw ramdisk_size=156000 " + " web=" + _webPath + " USER_TOKEN=" +
                                _userToken +
                                " task=register consoleblank=0 " + _globalComputerArgs + "" + NewLineChar);

            sysLinuxMenu.Append("MENU LABEL Add Computer" + NewLineChar);
            sysLinuxMenu.Append("" + NewLineChar);

            sysLinuxMenu.Append("LABEL On Demand" + NewLineChar);
            if (!string.IsNullOrEmpty(_defaultBoot.OndPwd) && _defaultBoot.OndPwd != "Error: Empty password")
                sysLinuxMenu.Append("MENU PASSWD " + _defaultBoot.OndPwd + "" + NewLineChar);

            sysLinuxMenu.Append("kernel kernels" + Path.DirectorySeparatorChar + _defaultBoot.Kernel + "" + NewLineChar);
            sysLinuxMenu.Append("append initrd=images" + Path.DirectorySeparatorChar + _defaultBoot.BootImage +
                                " root=/dev/ram0 rw ramdisk_size=156000 " + " web=" + _webPath + " USER_TOKEN=" +
                                _userToken +
                                " task=ond consoleblank=0 " + _globalComputerArgs + "" + NewLineChar);

            sysLinuxMenu.Append("MENU LABEL On Demand" + NewLineChar);
            sysLinuxMenu.Append("" + NewLineChar);

            sysLinuxMenu.Append("LABEL Diagnostics" + NewLineChar);
            if (!string.IsNullOrEmpty(_defaultBoot.DiagPwd) && _defaultBoot.DiagPwd != "Error: Empty password")
                sysLinuxMenu.Append("MENU PASSWD " + _defaultBoot.DiagPwd + "" + NewLineChar);

            sysLinuxMenu.Append("kernel kernels" + Path.DirectorySeparatorChar + _defaultBoot.Kernel + "" + NewLineChar);
            sysLinuxMenu.Append("append initrd=images" + Path.DirectorySeparatorChar + _defaultBoot.BootImage +
                                " root=/dev/ram0 rw ramdisk_size=156000 " + " web=" + _webPath + " USER_TOKEN=" +
                                _userToken +
                                " task=diag consoleblank=0 " + _globalComputerArgs + "" + NewLineChar);


            sysLinuxMenu.Append("MENU LABEL Diagnostics" + NewLineChar);
            sysLinuxMenu.Append("" + NewLineChar);

            //Insert active custom boot menu entries
            foreach (var customEntry in customMenuEntries)
            {
                sysLinuxMenu.Append("LABEL " + _alphaNumericSpace.Replace(customEntry.Name,"") + NewLineChar);
                sysLinuxMenu.Append(customEntry.Content + NewLineChar);
                if(defaultCustomEntry != null && customEntry.Id == defaultCustomEntry.Id)
                    sysLinuxMenu.Append("MENU DEFAULT" + NewLineChar);
                sysLinuxMenu.Append("MENU LABEL " + _alphaNumericSpace.Replace(customEntry.Name, "") + NewLineChar);
                sysLinuxMenu.Append("" + NewLineChar);
            }

            sysLinuxMenu.Append("PROMPT 0" + NewLineChar);
            sysLinuxMenu.Append("TIMEOUT 50" + NewLineChar);

            string path;
            if (_defaultBoot.Type == "standard")
                path = Settings.TftpPath + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default";
            else
                path = Settings.TftpPath + "proxy" + Path.DirectorySeparatorChar + _defaultBoot.Type +
                       Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default";


            if (Settings.OperationMode == "Single")
                new FileOps().WritePath(path, sysLinuxMenu.ToString());
            else
            {
                if (Settings.TftpServerRole)
                    new FileOps().WritePath(path, sysLinuxMenu.ToString());

                var tftpServers = new SecondaryServerServices().SearchSecondaryServers().Where(x => x.TftpRole == 1);
                foreach (var tftpServer in tftpServers)
                {
                    var tftpPath =
                        new APICall(new SecondaryServerServices().GetApiToken(tftpServer.Name))
                            .SettingApi.GetSetting("Tftp Path").Value;

                    var tftpFile = new TftpFileDTO();
                    tftpFile.Contents = sysLinuxMenu.ToString();
                    if (_defaultBoot.Type == "standard")
                        tftpFile.Path = tftpPath + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default";
                    else
                        tftpFile.Path = tftpPath + "proxy" + Path.DirectorySeparatorChar + _defaultBoot.Type +
                               Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default";

                    new APICall(new SecondaryServerServices().GetApiToken(tftpServer.Name))
                        .ServiceAccountApi.WriteTftpFile(tftpFile);
                }
            }


           
        }
    }
}