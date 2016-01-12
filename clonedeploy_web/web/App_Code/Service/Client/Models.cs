﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Services.Client
{
    public class ImageList
    {
        public List<string> Images  { get; set; }
    }

    public class ImageProfileList
    {
        public string Count { get; set; }
        public string FirstProfileId { get; set; }
        public List<string> ImageProfiles { get; set; } 
        
    }

    public class CheckIn
    {
        public string Result { get; set; }
        public string Message { get; set; }
        public string TaskArguments { get; set; }
    }

    public class SMB
    {
        public string SharePath { get; set; }
        public string Domain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class QueueStatus
    {
        public string Result { get; set; }
        public string Position { get; set; }
    }

    public class HardDriveSchema
    {
        public string IsValid { get; set; }
        public string Message { get; set; }
        public int SchemaHdNumber { get; set; }
        public int PhysicalPartitionCount { get; set; }
        public string PartitionType { get; set; }
        public string BootPartition { get; set; }
        public string Guid { get; set; }
        public string UsesLvm { get; set; }
        public List<PhysicalPartition> PhysicalPartitions { get; set; }
    }


    public class PhysicalPartition
    {
        public string Number { get; set; }
        public string PartcloneFileSystem { get; set; }
        public string Compression { get; set; }
        public string FileSystem { get; set; }
        public string Uuid { get; set; }
        public string Guid { get; set; }
        public string Type { get; set; }
        public string Prefix { get; set; }
        public string EfiBootLoader { get; set; }
        public VolumeGroup VolumeGroup { get; set; }
    }

    public class VolumeGroup
    {
        public string Name { get; set; }
        public int LogicalVolumeCount { get; set; }
        public List<LogicalVolume> LogicalVolumes { get; set; } 
    }

    public class LogicalVolume
    {
        public string Name { get; set; }
        public string PartcloneFileSystem { get; set; }
        public string Compression { get; set; }
        public string FileSystem { get; set; }
        public string Uuid { get; set; }
    }
}
