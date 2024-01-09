using System;
using System.Collections.Generic;
using System.Text;

namespace NetTubeClean.Sample01.Operator
{
    public enum ENetTubeCleanAlarmGroup
    {
        None,


        MainBody,

        ChemicalBath1,

        ChemicalBath2,

        Transfer_ForwarReadMotor,
        Transfer_UpDownMotor,
        Transfer_SubArmUpDownMotor,

        InsideShutter,

        CcssSignal_HfSignal,
        CcssSignal_M1Signal,
    }
}
