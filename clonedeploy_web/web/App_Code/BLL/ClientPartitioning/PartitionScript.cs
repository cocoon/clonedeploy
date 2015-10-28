﻿using System;
using System.Collections.Generic;
using System.Linq;
using Models.ClientPartition;

namespace BLL.ClientPartitioning
{
    public class PartitionScript
    {
        private Models.ImageSchema.ImageSchema ImageSchema { get; set; }
        public string ClientHd { get; set; }
        //private ExtendedPartitionHelper Ep { get; set; }
        private int HdNumberToGet { get; set; }
        private List<ClientPartition> LogicalPartitions { get; set; }
        private List<ClientLv> LogicalVolumes { get; set; }
        public string PartitionLayoutText { get; set; }
        private int PartitionSectorStart { get; set; }
        //private List<ClientPartition> PrimaryAndExtendedPartitions { get; set; }
        public string TaskType { get; set; }

        public PartitionScript()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public void CreateOutput()
        {
            if (TaskType == "debug")
            {
                try
                {
                    Ep.AgreedSizeBlk = Ep.AgreedSizeBlk * 512 / 1024 / 1024;
                }
                catch
                {
                    // ignored
                }
                foreach (var p in PrimaryAndExtendedPartitions)
                    p.Size = p.Size * 512 / 1024 / 1024;
                foreach (var p in LogicalPartitions)
                    p.Size = p.Size * 512 / 1024 / 1024;
                foreach (var p in LogicalVolumes)
                    p.Size = p.Size * 512 / 1024 / 1024;
            }

            //Create Menu
            if (ImageSchema.HardDrives[HdNumberToGet].Table.ToLower() == "mbr")
            {
                var counter = 0;
                var partCount = PrimaryAndExtendedPartitions.Count;

                string partitionCommands;
                if (Convert.ToInt32(PrimaryAndExtendedPartitions[0].Start) < 2048)
                    partitionCommands = "fdisk -c=dos " + ClientHd + " &>>/tmp/clientlog.log <<FDISK\r\n";
                else
                    partitionCommands = "fdisk " + ClientHd + " &>>/tmp/clientlog.log <<FDISK\r\n";

                foreach (var part in PrimaryAndExtendedPartitions)
                {
                    counter++;
                    partitionCommands += "n\r\n";
                    switch (part.Type)
                    {
                        case "primary":
                            partitionCommands += "p\r\n";
                            break;
                        case "extended":
                            partitionCommands += "e\r\n";
                            break;
                    }
                    //if (PrimaryAndExtendedPartitions.Count == 1)
                    //{
                    //partitionCommands += "1" + "\r\n";
                    //}
                    //else
                    //{
                    partitionCommands += part.Number + "\r\n";
                    //}

                    if (counter == 1)
                        partitionCommands += PartitionSectorStart + "\r\n";
                    else
                        partitionCommands += "\r\n";
                    if (part.Type == "extended")
                        partitionCommands += "+" + (Convert.ToInt64(Ep.AgreedSizeBlk) - 1) + "\r\n";
                    else //FDISK seems to include the starting sector in size so we need to subtract 1
                        partitionCommands += "+" + (Convert.ToInt64(part.Size) - 1) + "\r\n";

                    partitionCommands += "t\r\n";
                    if (counter == 1)
                        partitionCommands += part.FsId + "\r\n";
                    else
                    {
                        partitionCommands += part.Number + "\r\n";
                        partitionCommands += part.FsId + "\r\n";
                    }
                    if ((counter == 1 && part.IsBoot) || PrimaryAndExtendedPartitions.Count == 1)
                        partitionCommands += "a\r\n";
                    if (counter != 1 && part.IsBoot)
                    {
                        partitionCommands += "a\r\n";
                        partitionCommands += part.Number + "\r\n";
                    }
                    if ((counter != partCount || LogicalPartitions.Count != 0)) continue;
                    partitionCommands += "w\r\n";
                    partitionCommands += "FDISK\r\n";
                }


                var logicalCounter = 0;
                foreach (var logicalPart in LogicalPartitions)
                {
                    logicalCounter++;
                    partitionCommands += "n\r\n";

                    if (PrimaryAndExtendedPartitions.Count < 4)
                        partitionCommands += "l\r\n";


                    partitionCommands += "\r\n";

                    if (TaskType == "debug")
                        partitionCommands += "+" + (Convert.ToInt64(logicalPart.Size) - (logicalCounter * 1)) + "\r\n";
                    else
                        partitionCommands += "+" + (Convert.ToInt64(logicalPart.Size) - (logicalCounter * 2049)) + "\r\n";


                    partitionCommands += "t\r\n";

                    partitionCommands += logicalPart.Number + "\r\n";
                    partitionCommands += logicalPart.FsId + "\r\n";

                    if (logicalPart.IsBoot)
                    {
                        partitionCommands += "a\r\n";
                        partitionCommands += logicalPart.Number + "\r\n";
                    }
                    if (logicalCounter != LogicalPartitions.Count) continue;
                    partitionCommands += "w\r\n";
                    partitionCommands += "FDISK\r\n";
                }
                PartitionLayoutText += partitionCommands;
            }
            else
            {
                var counter = 0;
                var partCount = PrimaryAndExtendedPartitions.Count;

                var partitionCommands = "gdisk " + ClientHd + " &>>/tmp/clientlog.log <<GDISK\r\n";

                bool isApple = false;
                foreach (var part in PrimaryAndExtendedPartitions)
                {
                    if (part.FsType.Contains("hfs"))
                    {
                        isApple = true;
                        break;
                    }
                }
                if (PartitionSectorStart < 2048 && isApple) //osx cylinder boundary is 8
                {
                    partitionCommands += "x\r\nl\r\n8\r\nm\r\n";
                }
                foreach (var part in PrimaryAndExtendedPartitions)
                {
                    counter++;

                    partitionCommands += "n\r\n";

                    partitionCommands += part.Number + "\r\n";
                    if (counter == 1)
                        partitionCommands += PartitionSectorStart + "\r\n";
                    else
                        partitionCommands += "\r\n";
                    //GDISK seems to NOT include the starting sector in size so don't subtract 1 like in FDISK
                    partitionCommands += "+" + Convert.ToInt64(part.Size) + "\r\n";


                    partitionCommands += part.FsId + "\r\n";


                    if ((counter != partCount)) continue;
                    partitionCommands += "w\r\n";
                    partitionCommands += "y\r\n";
                    partitionCommands += "GDISK\r\n";
                }
                PartitionLayoutText += partitionCommands;
            }


            foreach (var part in from part in ImageSchema.HardDrives[HdNumberToGet].Partitions
                                 where part.Active
                                 where part.VolumeGroup != null
                                 where part.VolumeGroup.LogicalVolumes != null
                                 select part)
            {
                PartitionLayoutText += "echo \"pvcreate -u " + part.Uuid + " --norestorefile -yf " +
                                       ClientHd + part.VolumeGroup.PhysicalVolume[part.VolumeGroup.PhysicalVolume.Length - 1] +
                                       "\" >>/tmp/lvmcommands \r\n";
                PartitionLayoutText += "echo \"vgcreate " + part.VolumeGroup.Name + " " + ClientHd +
                                       part.VolumeGroup.PhysicalVolume[part.VolumeGroup.PhysicalVolume.Length - 1] + " -yf" +
                                       "\" >>/tmp/lvmcommands \r\n";
                PartitionLayoutText += "echo \"" + part.VolumeGroup.Uuid + "\" >>/tmp/vg-" + part.VolumeGroup.Name +
                                       " \r\n";
                foreach (var lv in part.VolumeGroup.LogicalVolumes)
                {
                    foreach (var rlv in LogicalVolumes)
                    {
                        if (lv.Name != rlv.Name || lv.VolumeGroup != rlv.Vg) continue;
                        if (TaskType == "debug")
                        {
                            PartitionLayoutText += "echo \"lvcreate -L " +
                                                   rlv.Size + "mb -n " +
                                                   rlv.Name + " " + rlv.Vg +
                                                   "\" >>/tmp/lvmcommands \r\n";
                        }
                        else
                        {
                            PartitionLayoutText += "echo \"lvcreate -L " +
                                                   ((Convert.ToInt64(rlv.Size) - 8192)) + "s -n " +
                                                   rlv.Name + " " + rlv.Vg +
                                                   "\" >>/tmp/lvmcommands \r\n";
                        }
                        PartitionLayoutText += "echo \"" + rlv.Uuid + "\" >>/tmp/" + rlv.Vg +
                                               "-" + rlv.Name + "\r\n";
                    }
                }
                PartitionLayoutText += "echo \"vgcfgbackup -f /tmp/lvm-" + part.VolumeGroup.Name +
                                       "\" >>/tmp/lvmcommands\r\n";
            }

            //If mbr / gpt is hybrid, set the boot flag
            /*if (!string.IsNullOrEmpty(BootPart))
            {
                PartitionLayoutText += "fdisk " + ClientHd + " &>>/tmp/clientlog.log <<FDISK\r\n";
                PartitionLayoutText += "x\r\nM\r\nr\r\na\r\n";
                PartitionLayoutText += BootPart + "\r\n";
                PartitionLayoutText += "w\r\nq\r\n";
                PartitionLayoutText += "FDISK\r\n";
            }*/
        }
    }
}