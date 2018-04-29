using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace PLC_backup
{
    [DataContract]
    class DataBlock
    {
        [DataMember]
        public String Name { get; set; }
        [DataMember]
        public int Number { get; set; }
        [DataMember]
        public int Size { get; set; }

        public byte[] Data { get; set; }

        public DataBlock()
        {
            
        }

        public DataBlock(string name, int number, int size)
        {
            Name = name;
            Number = number;
            Size = size;
            Data = new byte[size];
        }
    }
}