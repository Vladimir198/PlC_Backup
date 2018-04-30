using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using S7.Net;

namespace PLC_backup
{
    [DataContract]
    class Prop
    {
        [DataMember]
        public string Ip { get; set; }
        [DataMember]
        public short Rack { get; set; }
        [DataMember]
        public short Slot { get; set; }
        [DataMember]
        public CpuType CpuType { get; set; }
        [DataMember]
        public ObservableCollection<DataBlock> DataBlocks { get; set; }

        public Prop()
        {
            DataBlocks = new ObservableCollection<DataBlock>();
            Rack = 0;
            Slot = 0;
            Ip = "127.0.0.1";
            CpuType = CpuType.S7300;
        }
    }

    
}
