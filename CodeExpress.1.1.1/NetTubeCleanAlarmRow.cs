using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace NetTubeClean.Sample01.Operator
{
    public class NetTubeCleanAlarmRow
    {


        public ushort Index;
        public String HmiName;
        public String PlcName;
        public Label Label;
        public EMyAlarmGroup Group;
        public bool IsUpdated;
        public bool IsOn;

        public NetTubeCleanAlarmRow(ushort index, String hmiName, String plcName, EMyAlarmGroup group = EMyAlarmGroup.None)
        {
            this.Index = index;
            this.HmiName = hmiName;
            this.PlcName = plcName;
            this.Group = group;
        }

        public String GetName()
        {
            if (!String.IsNullOrEmpty(this.HmiName))
                return this.HmiName;
            return this.PlcName;
        }
        public String GetFullName()
        {
            var name = this.HmiName;
            if (String.IsNullOrEmpty(name))
                name = this.PlcName;

            if (this.Group != EMyAlarmGroup.None)
                return this.Group + "/" + name;
            return name;

        }


    }
}
