using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace NetTubeClean.Sample01.Operator
{
    public class NetTubeCleanAlarmRow
    {

        public ENetTubeCleanAlarmGroup Group;
        public String HmiName;
        public ushort Index;
        public bool IsOn;
        public bool IsUpdated;
        public String PlcName;
        public Object RelatedObj;
        public NetTubeCleanAlarmRow(ushort index, String hmiName, String plcName, ENetTubeCleanAlarmGroup group = ENetTubeCleanAlarmGroup.None)
        {
            this.Index = index;
            this.HmiName = hmiName;
            this.PlcName = plcName;
            this.Group = group;
        }

        public String GetFullName()
        {
            var name = this.HmiName;
            if (String.IsNullOrEmpty(name))
                name = this.PlcName;

            if (this.Group != ENetTubeCleanAlarmGroup.None)
                return this.Group + "/" + name;
            return name;

        }

        public String GetName()
        {
            if (!String.IsNullOrEmpty(this.HmiName))
                return this.HmiName;
            return this.PlcName;
        }
    }
}
