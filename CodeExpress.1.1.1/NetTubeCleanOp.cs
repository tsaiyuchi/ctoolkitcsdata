using CodeExpress.Ctk.Net.SocketTx;
using CodeExpress.v1_1Core.Modbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetTubeClean.Sample01.Operator
{
    public class NetTubeCleanOp : IDisposable
    {
        public NetTubeCleanAlarmRow[] AlarmRows = new NetTubeCleanAlarmRow[] {

                new NetTubeCleanAlarmRow(000, "AIR PRESSURE DOWN"         , ""                    , EMyAlarmGroup.MainBody),
                new NetTubeCleanAlarmRow(001, "EXHUAST DOWN"              , ""                    , EMyAlarmGroup.MainBody),
                new NetTubeCleanAlarmRow(002, "LEAK"                      , ""                    , EMyAlarmGroup.MainBody),
                new NetTubeCleanAlarmRow(003, "CLEAN UNIT FAN ALARM"      , ""                    , EMyAlarmGroup.MainBody),


                new NetTubeCleanAlarmRow(004, "FULL LEVEL ALARM"          , ""                    , EMyAlarmGroup.ChemicalBath1),
                new NetTubeCleanAlarmRow(037, "HF SUPPLY OVER TIME"       , ""                    , EMyAlarmGroup.ChemicalBath1),
                new NetTubeCleanAlarmRow(027, "M1 SUPPLY OVER TIME"       , ""                    , EMyAlarmGroup.ChemicalBath1),
                new NetTubeCleanAlarmRow(028, "DIW SUPPLY OVER TIME"      , ""                    , EMyAlarmGroup.ChemicalBath1),
                new NetTubeCleanAlarmRow(006, "LID OPEN/CLOSE"            , ""                    , EMyAlarmGroup.ChemicalBath1),
                new NetTubeCleanAlarmRow(019, "PUMP:STROKE ALARM"         , "PUMP1 STROKE ALARM"  , EMyAlarmGroup.ChemicalBath1),

                new NetTubeCleanAlarmRow(005, "FULL LEVEL ALARM"          , ""                    , EMyAlarmGroup.ChemicalBath2),
                new NetTubeCleanAlarmRow(029, "HF SUPPLY OVER TIME"       , ""                    , EMyAlarmGroup.ChemicalBath2),
                new NetTubeCleanAlarmRow(030, "DIW SUPPLY OVER TIME"      , ""                    , EMyAlarmGroup.ChemicalBath2),
                new NetTubeCleanAlarmRow(007, "LID OPEN/CLOSE OVER TIME"  , ""                    , EMyAlarmGroup.ChemicalBath2),
                new NetTubeCleanAlarmRow(021, "PUMP:STROKE ALARM"         , "PUMP2 STROKE ALARM"  , EMyAlarmGroup.ChemicalBath1),

                new NetTubeCleanAlarmRow(008, "FORWARD OVER RUN"          , ""                    , EMyAlarmGroup.Transfer_ForwarReadMotor),
                new NetTubeCleanAlarmRow(009, "REAR OVER RUN"             , ""                    , EMyAlarmGroup.Transfer_ForwarReadMotor),
                new NetTubeCleanAlarmRow(010, "CONTROLLER ALARM"          , ""                    , EMyAlarmGroup.Transfer_ForwarReadMotor),
                new NetTubeCleanAlarmRow(026, "OVER TIME"                 , ""                    , EMyAlarmGroup.Transfer_ForwarReadMotor),

                new NetTubeCleanAlarmRow(011, "UP OVER RUN"               , ""                    , EMyAlarmGroup.Transfer_UpDownMotor),
                new NetTubeCleanAlarmRow(012, "DOWN OVER RUN"             , ""                    , EMyAlarmGroup.Transfer_UpDownMotor),
                new NetTubeCleanAlarmRow(013, "CONTROLLER ALARM"          , ""                    , EMyAlarmGroup.Transfer_UpDownMotor),
                new NetTubeCleanAlarmRow(024, "OVER TIME"                 , ""                    , EMyAlarmGroup.Transfer_UpDownMotor),

                new NetTubeCleanAlarmRow(014, "UP OVER RUN"               , ""                    , EMyAlarmGroup.Transfer_SubArmUpDownMotor),
                new NetTubeCleanAlarmRow(015, "DOWN OVER RUN"             , ""                    , EMyAlarmGroup.Transfer_SubArmUpDownMotor),
                new NetTubeCleanAlarmRow(025, "OVER TIME"                 , ""                    , EMyAlarmGroup.Transfer_SubArmUpDownMotor),

                new NetTubeCleanAlarmRow(016, "OPEN OVER RUN"             , ""                    , EMyAlarmGroup.Transfer_SubArmUpDownMotor),
                new NetTubeCleanAlarmRow(017, "CLOSE OVER RUN"            , ""                    , EMyAlarmGroup.Transfer_SubArmUpDownMotor),
                new NetTubeCleanAlarmRow(018, "CONTROLLER ALARM"          , ""                    , EMyAlarmGroup.Transfer_SubArmUpDownMotor),
                new NetTubeCleanAlarmRow(023, "MOVING NG"                 , ""                    , EMyAlarmGroup.Transfer_SubArmUpDownMotor),
                new NetTubeCleanAlarmRow(031, "OVER TIME"                 , ""                    , EMyAlarmGroup.Transfer_SubArmUpDownMotor),

                new NetTubeCleanAlarmRow(048, "EMERGENCY"                 , ""                    , EMyAlarmGroup.CcssSignal_HfSignal),
                new NetTubeCleanAlarmRow(049, "NO READY"                  , ""                    , EMyAlarmGroup.CcssSignal_HfSignal),
                new NetTubeCleanAlarmRow(050, "TROUBLE"                   , ""                    , EMyAlarmGroup.CcssSignal_HfSignal),

                new NetTubeCleanAlarmRow(051, "EMERGENCY"                 , ""                    , EMyAlarmGroup.CcssSignal_M1Signal),
                new NetTubeCleanAlarmRow(052, "NO READY"                  , ""                    , EMyAlarmGroup.CcssSignal_M1Signal),
                new NetTubeCleanAlarmRow(053, "TROUBLE"                   , ""                    , EMyAlarmGroup.CcssSignal_M1Signal),



                new NetTubeCleanAlarmRow(370,""                           , "ALARM!"                                 ),
                new NetTubeCleanAlarmRow(373,""                           , "ALARM!!!"                               ),
                new NetTubeCleanAlarmRow(630,""                           , "facility Alarm"                         ),
                new NetTubeCleanAlarmRow(631,""                           , "Clean Unit Alarm"                       ),
                new NetTubeCleanAlarmRow(632,""                           , "Chemical Bath1 Alarm"                   ),
                new NetTubeCleanAlarmRow(633,""                           , "Chemical Bath2 Alarm"                   ),
                new NetTubeCleanAlarmRow(634,""                           , "Transfer Up Alarm"                      ),
                new NetTubeCleanAlarmRow(635,""                           , "Tarnsfer Down Alarm"                    ),
                new NetTubeCleanAlarmRow(636,""                           , "Transfer Forward Alarm"                 ),
                new NetTubeCleanAlarmRow(637,""                           , "Transfer Rear Alarm"                    ),
                new NetTubeCleanAlarmRow(638,""                           , "Inside Shutter Close Alarm"             ),
                new NetTubeCleanAlarmRow(639,""                           , "Inside Shutter Open Alarm"              ),
                new NetTubeCleanAlarmRow(640,""                           , "Lid1 Alarm"                             ),
                new NetTubeCleanAlarmRow(641,""                           , "Lid2 Alarm"                             ),
                new NetTubeCleanAlarmRow(642,""                           , "Shutter Alarm"                          ),

                new NetTubeCleanAlarmRow(860,""                           , "Alarm UP GO"                            ),
                new NetTubeCleanAlarmRow(861,""                           , "Alarm UP END"                           ),
                new NetTubeCleanAlarmRow(862,""                           , "Alarm-Transfer Move"                    ),

                new NetTubeCleanAlarmRow(970,""                           , "Alarm-Inside Shutter Open"              ),
                new NetTubeCleanAlarmRow(971,""                           , "Alarm-Inside Shutter Open End"          ),
                new NetTubeCleanAlarmRow(972,""                           , "Alarm-Forward Transfer"                 ),
                new NetTubeCleanAlarmRow(973,""                           , "Alarm-Transfer Bath3 Stop"              ),
                new NetTubeCleanAlarmRow(974,""                           , "Alarm-Inside Shutter Close"             ),
                new NetTubeCleanAlarmRow(975,""                           , "Alarm-Inside Shutter Close End"         ),
                new NetTubeCleanAlarmRow(976,""                           , "Alarm-Transfer Down"                    ),
                new NetTubeCleanAlarmRow(977,""                           , "Alarm-Transfer Down End"                ),
                new NetTubeCleanAlarmRow(978,""                           , "Alarm-Lid Close"                        ),
                new NetTubeCleanAlarmRow(979,""                           , "Alarm-Lid Close End"                    ),
                new NetTubeCleanAlarmRow(980,""                           , "Alarm Move Reset"                       ),
            };

        public List<Exception> Exceptions = new List<Exception>();
        const ushort AddrOf_Coil_TriggerEarlyStop = 0x160;
        const ushort AddrOf_Coil_PageOfAutoStart = 0x152;
        const ushort AddrOf_Coil_PageOfMainMenu = 0x150;
        const ushort AddrOf_Coil_PageOfProcess = 0x151;
        const ushort AddrOf_Data_Ad01Ch01 = 1201;
        const ushort AddrOf_Data_Ad01Ch02 = 1203;
        const ushort AddrOf_Data_Ad01Ch03 = 1205;
        const ushort AddrOf_Data_Ad01Ch04 = 1207;
        const ushort AddrOf_Data_GreenModeLine01 = 1515;
        const ushort AddrOf_Data_GreenModeLine02 = 1517;
        const ushort AddrOf_Data_ProcessNo = 1500;
        ushort AlarmStartIndex = 8192;
        ENetTubeCleanCommand command = ENetTubeCleanCommand.None;
        ManualResetEvent handshake = new ManualResetEvent(true);
        CxModbusMessage mbMessage;
        CxModbusMessageReceiver modbusReceiver = new CxModbusMessageReceiver();
        ushort readIndex = 0;
        short respOf_ReadData;
        CtkSocketTcp socket;
        public void Connect(string ip, int port)
        {
            if (this.socket != null && this.socket.IsRemoteConnected) return;

            var socket = this.socket = new CtkSocketTcp();
            socket.RemoteUri = new Uri($"net.tcp://{ip}:{port}");
            socket.IsActively = true;
            socket.EhDataReceive += (mys, mye) =>
            {

                var buffer = mye.TrxMessage.ToBuffer();
                this.modbusReceiver.Receive(buffer.Buffer, buffer.Offset, buffer.Length);

                while (this.modbusReceiver.Count > 0)
                {

                    try
                    {
                        var data = this.modbusReceiver.Dequeue();
                        var myIndex = this.readIndex;
                        var bitary = data.DataToBitArray();
                        var intary = data.DataToListOfInt16();

                        switch (data.funcCode)
                        {
                            case CxModbusMessage.fcReadCoil:

                                for (var i = 0; i < bitary.Count; i++)
                                {
                                    var row = this.AlarmRows.Where(x => x.Index == myIndex + i).FirstOrDefault();
                                    if (row == null) continue; row.IsOn = bitary[i];
                                    row.IsUpdated = true;
                                }

                                break;
                            case CxModbusMessage.fcReadHoldingRegister:
                                if (intary.Count == 1)
                                    this.respOf_ReadData = intary[0];
                                break;

                                
                        }

                    }
                    catch (Exception ex) { this.Exceptions.Add(ex); }
                }
                this.handshake.Set();


            };
            socket.ConnectTryStart();
        }
        public void Disconnect()
        {
            using (var obj = this.socket) { if (obj != null) obj.Disconnect(); }
        }

        public async Task<NetTubeCleanAlarmRow[]> ReadAlarms()
        {
            foreach (var alarmRow in this.AlarmRows)
            {
                alarmRow.IsUpdated = false;
                alarmRow.IsOn = false;
            }
            await Task.Run(() =>
            {
                foreach (var alarmRow in this.AlarmRows)
                {
                    if (alarmRow.IsUpdated) continue;
                    this.readIndex = alarmRow.Index;
                    var mAddress = this.AlarmStartIndex + this.readIndex;
                    mbMessage = CxModbusMessage.CreateReadCoil((ushort)mAddress, 8);
                    this.socket.WriteMsg(mbMessage.ToRequestBytes());

                    this.handshake.WaitOne();
                }
            });
            return this.AlarmRows;
        }



        void HandshakeStart(ENetTubeCleanCommand cmd)
        {//Once one command
            this.handshake.WaitOne();
            this.handshake.Reset();
            this.command = cmd;
            this.Exceptions.Clear();
        }
        void HandshakeWaitOne()
        {
            this.handshake.WaitOne();
        }


        #region Command of Common
        short CmdReadData(ushort addr)
        {
            this.HandshakeStart(ENetTubeCleanCommand.ReadData);
            mbMessage = CxModbusMessage.CreateReadHoldingRegister(addr, 1);
            this.socket.WriteMsg(mbMessage.ToRequestBytes());
            this.HandshakeWaitOne();
            return this.respOf_ReadData;
        }
        void CmdWriteData(ushort addr, ushort data)
        {
            this.HandshakeStart(ENetTubeCleanCommand.WriteData);
            mbMessage = CxModbusMessage.CreateWriteSingleRegister(addr, data);
            this.socket.WriteMsg(mbMessage.ToRequestBytes());
            this.HandshakeWaitOne();
        }
        void CoilPluse(ushort addr)
        {
            this.HandshakeStart(ENetTubeCleanCommand.WriteCoil);
            mbMessage = CxModbusMessage.CreateWriteSingleCoil(addr, false);
            this.socket.WriteMsg(mbMessage.ToRequestBytes());
            this.HandshakeWaitOne();

            this.HandshakeStart(ENetTubeCleanCommand.WriteCoil);
            mbMessage = CxModbusMessage.CreateWriteSingleCoil(addr, true);
            this.socket.WriteMsg(mbMessage.ToRequestBytes());
            this.HandshakeWaitOne();

            this.HandshakeStart(ENetTubeCleanCommand.WriteCoil);
            mbMessage = CxModbusMessage.CreateWriteSingleCoil(addr, false);
            this.socket.WriteMsg(mbMessage.ToRequestBytes());
            this.HandshakeWaitOne();
        }


        #endregion


        #region Commands

        public short CmdReadDataAd01Ch1() { return CmdReadData(AddrOf_Data_Ad01Ch01); }
        public short CmdReadDataAd01Ch2() { return CmdReadData(AddrOf_Data_Ad01Ch02); }

        public short CmdReadDataAd01Ch3() { return CmdReadData(AddrOf_Data_Ad01Ch03); }

        public short CmdReadDataAd01Ch4() { return CmdReadData(AddrOf_Data_Ad01Ch04); }

        public short CmdReadDataGreenModeLine01() { return CmdReadData(AddrOf_Data_GreenModeLine01); }
        public short CmdReadDataGreenModeLine02() { return CmdReadData(AddrOf_Data_GreenModeLine02); }
        public short CmdReadDataProcessNo() { return CmdReadData(AddrOf_Data_ProcessNo); }


        public void CmdSwitchPageToAutoStart() { CoilPluse(AddrOf_Coil_PageOfAutoStart); }

        public void CmdSwitchPageToMainMenu() { CoilPluse(AddrOf_Coil_PageOfMainMenu); }


        public void CmdSwitchPageToProcess() { CoilPluse(AddrOf_Coil_PageOfProcess); }


        public void CmdTriggerEarlyStop() { CoilPluse(AddrOf_Coil_TriggerEarlyStop); }


        public void CmdWriteProcessNo(ushort data) { CmdWriteData(AddrOf_Data_ProcessNo, data); }


        #endregion



        #region Dispose

        bool disposed = false;
        public void Dispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                // Free any managed objects here.
            }
            // Free any unmanaged objects here.
            //
            this.Disconnect();
            this.handshake.Set();
            disposed = true;
        }

        #endregion


    }
}
