using CodeExpress.Ctk;
using CodeExpress.Ctk.Net;
using CodeExpress.Ctk.Net.SocketTx;
using CodeExpress.v1_1Core;
using CodeExpress.v1_1Core.Modbus;
using CodeExpress.v1_1Core.Secs;
using System;
using System.Collections;
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

                new NetTubeCleanAlarmRow(000, "AIR PRESSURE DOWN"         , ""                    , ENetTubeCleanAlarmGroup.MainBody),
                new NetTubeCleanAlarmRow(001, "EXHUAST DOWN"              , ""                    , ENetTubeCleanAlarmGroup.MainBody),
                new NetTubeCleanAlarmRow(002, "LEAK"                      , ""                    , ENetTubeCleanAlarmGroup.MainBody),
                new NetTubeCleanAlarmRow(003, "CLEAN UNIT FAN ALARM"      , ""                    , ENetTubeCleanAlarmGroup.MainBody),


                new NetTubeCleanAlarmRow(004, "FULL LEVEL ALARM"          , ""                    , ENetTubeCleanAlarmGroup.ChemicalBath1),
                new NetTubeCleanAlarmRow(037, "HF SUPPLY OVER TIME"       , ""                    , ENetTubeCleanAlarmGroup.ChemicalBath1),
                new NetTubeCleanAlarmRow(027, "M1 SUPPLY OVER TIME"       , ""                    , ENetTubeCleanAlarmGroup.ChemicalBath1),
                new NetTubeCleanAlarmRow(028, "DIW SUPPLY OVER TIME"      , ""                    , ENetTubeCleanAlarmGroup.ChemicalBath1),
                new NetTubeCleanAlarmRow(006, "LID OPEN/CLOSE"            , ""                    , ENetTubeCleanAlarmGroup.ChemicalBath1),
                new NetTubeCleanAlarmRow(019, "PUMP:STROKE ALARM"         , "PUMP1 STROKE ALARM"  , ENetTubeCleanAlarmGroup.ChemicalBath1),

                new NetTubeCleanAlarmRow(005, "FULL LEVEL ALARM"          , ""                    , ENetTubeCleanAlarmGroup.ChemicalBath2),
                new NetTubeCleanAlarmRow(029, "HF SUPPLY OVER TIME"       , ""                    , ENetTubeCleanAlarmGroup.ChemicalBath2),
                new NetTubeCleanAlarmRow(030, "DIW SUPPLY OVER TIME"      , ""                    , ENetTubeCleanAlarmGroup.ChemicalBath2),
                new NetTubeCleanAlarmRow(007, "LID OPEN/CLOSE OVER TIME"  , ""                    , ENetTubeCleanAlarmGroup.ChemicalBath2),
                new NetTubeCleanAlarmRow(021, "PUMP:STROKE ALARM"         , "PUMP2 STROKE ALARM"  , ENetTubeCleanAlarmGroup.ChemicalBath1),

                new NetTubeCleanAlarmRow(008, "FORWARD OVER RUN"          , ""                    , ENetTubeCleanAlarmGroup.Transfer_ForwarReadMotor),
                new NetTubeCleanAlarmRow(009, "REAR OVER RUN"             , ""                    , ENetTubeCleanAlarmGroup.Transfer_ForwarReadMotor),
                new NetTubeCleanAlarmRow(010, "CONTROLLER ALARM"          , ""                    , ENetTubeCleanAlarmGroup.Transfer_ForwarReadMotor),
                new NetTubeCleanAlarmRow(026, "OVER TIME"                 , ""                    , ENetTubeCleanAlarmGroup.Transfer_ForwarReadMotor),

                new NetTubeCleanAlarmRow(011, "UP OVER RUN"               , ""                    , ENetTubeCleanAlarmGroup.Transfer_UpDownMotor),
                new NetTubeCleanAlarmRow(012, "DOWN OVER RUN"             , ""                    , ENetTubeCleanAlarmGroup.Transfer_UpDownMotor),
                new NetTubeCleanAlarmRow(013, "CONTROLLER ALARM"          , ""                    , ENetTubeCleanAlarmGroup.Transfer_UpDownMotor),
                new NetTubeCleanAlarmRow(024, "OVER TIME"                 , ""                    , ENetTubeCleanAlarmGroup.Transfer_UpDownMotor),

                new NetTubeCleanAlarmRow(014, "UP OVER RUN"               , ""                    , ENetTubeCleanAlarmGroup.Transfer_SubArmUpDownMotor),
                new NetTubeCleanAlarmRow(015, "DOWN OVER RUN"             , ""                    , ENetTubeCleanAlarmGroup.Transfer_SubArmUpDownMotor),
                new NetTubeCleanAlarmRow(025, "OVER TIME"                 , ""                    , ENetTubeCleanAlarmGroup.Transfer_SubArmUpDownMotor),

                new NetTubeCleanAlarmRow(016, "OPEN OVER RUN"             , ""                    , ENetTubeCleanAlarmGroup.Transfer_SubArmUpDownMotor),
                new NetTubeCleanAlarmRow(017, "CLOSE OVER RUN"            , ""                    , ENetTubeCleanAlarmGroup.Transfer_SubArmUpDownMotor),
                new NetTubeCleanAlarmRow(018, "CONTROLLER ALARM"          , ""                    , ENetTubeCleanAlarmGroup.Transfer_SubArmUpDownMotor),
                new NetTubeCleanAlarmRow(023, "MOVING NG"                 , ""                    , ENetTubeCleanAlarmGroup.Transfer_SubArmUpDownMotor),
                new NetTubeCleanAlarmRow(031, "OVER TIME"                 , ""                    , ENetTubeCleanAlarmGroup.Transfer_SubArmUpDownMotor),

                new NetTubeCleanAlarmRow(048, "EMERGENCY"                 , ""                    , ENetTubeCleanAlarmGroup.CcssSignal_HfSignal),
                new NetTubeCleanAlarmRow(049, "NO READY"                  , ""                    , ENetTubeCleanAlarmGroup.CcssSignal_HfSignal),
                new NetTubeCleanAlarmRow(050, "TROUBLE"                   , ""                    , ENetTubeCleanAlarmGroup.CcssSignal_HfSignal),

                new NetTubeCleanAlarmRow(051, "EMERGENCY"                 , ""                    , ENetTubeCleanAlarmGroup.CcssSignal_M1Signal),
                new NetTubeCleanAlarmRow(052, "NO READY"                  , ""                    , ENetTubeCleanAlarmGroup.CcssSignal_M1Signal),
                new NetTubeCleanAlarmRow(053, "TROUBLE"                   , ""                    , ENetTubeCleanAlarmGroup.CcssSignal_M1Signal),



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

        const ushort AddrOf_Coil_PageOfAutoStart = 0x152;
        const ushort AddrOf_Coil_PageOfMainMenu = 0x150;
        const ushort AddrOf_Coil_PageOfProcess = 0x151;
        const ushort AddrOf_Coil_TriggerEarlyStop = 0x160;
        const ushort AddrOf_Data_Ad01Ch01 = 1201;
        const ushort AddrOf_Data_Ad01Ch02 = 1203;
        const ushort AddrOf_Data_Ad01Ch03 = 1205;
        const ushort AddrOf_Data_Ad01Ch04 = 1207;
        const ushort AddrOf_Data_GreenMode = 1501;
        const ushort AddrOf_Data_GreenModeLine01 = 1515;
        const ushort AddrOf_Data_GreenModeLine02 = 1517;
        const ushort AddrOf_Data_ProcessNo = 1500;

        ENetTubeCleanCommand command = ENetTubeCleanCommand.None;
        /// <summary> Holding Register </summary> ushort DeviceStartIndexD = 0;
        /// <summary> Coil </summary>
        ushort DeviceStartIndexM = 8192;
        /// <summary> Coil </summary>        ushort DeviceStartIndexY = 0;

        ManualResetEvent handshake = new ManualResetEvent(true);
        CxModbusMessage mbMessage;
        CxModbusMessageReceiver modbusReceiver = new CxModbusMessageReceiver();
        BitArray respOf_Coil;
        List<Int16> respOf_HoldRegister;
        CtkSocketTcp socket;

        public NetTubeCleanOp() { CxHsmsMgr.CxSetup(); }


        public void Connect(string ip, int port)
        {
            if (this.socket != null)
            {
                if (this.socket.IsRemoteConnected) return;
                if (this.socket.IsOpenConnecting) return;

                CtkNetUtil.DisposeSocketTry(this.socket);
            }


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
                        var bitary = this.respOf_Coil = data.DataToBitArray();
                        var intary = this.respOf_HoldRegister = data.DataToListOfInt16();

                        switch (data.funcCode)
                        {
                            case CxModbusMessage.fcReadCoil:
                                break;
                            case CxModbusMessage.fcReadHoldingRegister:
                                break;
                            case CxModbusMessage.SlaveDeviceFailure:
                                throw new Exception("Slave Device Failure");

                        }

                    }
                    catch (Exception ex) { this.ExceptionAdd(ex); }
                }
                this.handshake.Set();


            };
            socket.ConnectTryStart();
        }

        public void Disconnect()
        {
            using (var obj = this.socket) { if (obj != null) obj.Disconnect(); }
        }

        void ExceptionAdd(Exception ex)
        {
            this.Exceptions.Add(ex);
            while (this.Exceptions.Count > 100) this.Exceptions.RemoveAt(0);
        }





        #region Handshake Related

        async Task CmdPulseCoil(ushort addr)
        {
            try
            {
                this.HandshakeBegin(ENetTubeCleanCommand.WriteCoil);
                mbMessage = CxModbusMessage.CreateWriteSingleCoil(addr, false);
                this.socket.WriteMsg(mbMessage.ToRequestBytes());
                await this.HandshakeWaitOne();

                this.HandshakeBegin(ENetTubeCleanCommand.WriteCoil);
                mbMessage = CxModbusMessage.CreateWriteSingleCoil(addr, true);
                this.socket.WriteMsg(mbMessage.ToRequestBytes());
                await this.HandshakeWaitOne();

                this.HandshakeBegin(ENetTubeCleanCommand.WriteCoil);
                mbMessage = CxModbusMessage.CreateWriteSingleCoil(addr, false);
                this.socket.WriteMsg(mbMessage.ToRequestBytes());
                await this.HandshakeWaitOne();
            }
            catch (Exception ex)
            {
                this.HandshakeEnd();
                throw ex;
            }
        }
        async Task<BitArray> CmdReadCoils(ushort addr, ushort length = 8)
        {
            try
            {
                this.HandshakeBegin(ENetTubeCleanCommand.ReadCoil);
                mbMessage = CxModbusMessage.CreateReadCoil(addr, length);
                this.socket.WriteMsg(mbMessage.ToRequestBytes());
                await this.HandshakeWaitOne();
                return this.respOf_Coil;
            }
            catch (Exception ex)
            {
                this.HandshakeEnd();
                throw ex;
            }
        }
        async Task<short> CmdReadHoldingRegister(ushort addr)
        {
            try
            {
                this.HandshakeBegin(ENetTubeCleanCommand.ReadData);
                mbMessage = CxModbusMessage.CreateReadHoldingRegister(addr, 1);
                this.socket.WriteMsg(mbMessage.ToRequestBytes());
                await this.HandshakeWaitOne();
                return this.respOf_HoldRegister.FirstOrDefault();
            }
            catch (Exception ex)
            {
                this.HandshakeEnd();
                throw ex;
            }
        }
        async Task CmdWriteHoldingRegister(ushort addr, ushort data)
        {
            try
            {
                this.HandshakeBegin(ENetTubeCleanCommand.WriteData);
                mbMessage = CxModbusMessage.CreateWriteSingleRegister(addr, data);
                this.socket.WriteMsg(mbMessage.ToRequestBytes());
                await this.HandshakeWaitOne();
            }
            catch (Exception ex)
            {
                this.HandshakeEnd();
                throw ex;
            }
        }
        
        
        
        void HandshakeBegin(ENetTubeCleanCommand cmd)
        {//Once one command
            this.handshake.WaitOne();
            this.handshake.Reset();
            this.command = cmd;
        }
        void HandshakeEnd() { this.handshake.Set(); }
        async Task<bool> HandshakeWaitOne(int timeout = 10 * 1000)
        {
            return await Task.Run(() => this.handshake.WaitOne(timeout));
        }

        #endregion





        #region Commands

        public async Task<NetTubeCleanAlarmRow[]> CmdDoReadAlarms()
        {
            foreach (var alarmRow in this.AlarmRows)
            {
                alarmRow.IsUpdated = false;
                alarmRow.IsOn = false;
            }

            foreach (var alarmRow in this.AlarmRows)
            {
                if (alarmRow.IsUpdated) continue;
                var mIndex = alarmRow.Index;
                var mAddress = this.DeviceStartIndexM + mIndex;

                var bits = await this.CmdReadCoils((ushort)mAddress, 8);

                for (var i = 0; i < bits.Count; i++)
                {
                    var row = this.AlarmRows.Where(x => x.Index == mIndex + i).FirstOrDefault();
                    if (row == null) continue;
                    row.IsOn = bits[i];
                    row.IsUpdated = true;
                }
            }

            return this.AlarmRows;
        }

        public async Task CmdDoRecipeSelection(ushort procno)
        {
            await this.CmdSwitchPageToMainMenu();
            await this.CmdWriteDataProcessNo(procno);
            await this.CmdSwitchPageToProcess();
            await this.CmdSwitchPageToAutoStart();
        }


        public async Task<short> CmdReadDataAd01Ch1() { return await CmdReadHoldingRegister(AddrOf_Data_Ad01Ch01); }
        public async Task<short> CmdReadDataAd01Ch2() { return await CmdReadHoldingRegister(AddrOf_Data_Ad01Ch02); }
        public async Task<short> CmdReadDataAd01Ch3() { return await CmdReadHoldingRegister(AddrOf_Data_Ad01Ch03); }
        public async Task<short> CmdReadDataAd01Ch4() { return await CmdReadHoldingRegister(AddrOf_Data_Ad01Ch04); }


        public async Task<short> CmdReadDataGreenMode() { return await CmdReadHoldingRegister(AddrOf_Data_GreenMode); }
        public async Task<short> CmdReadDataGreenModeLine01() { return await CmdReadHoldingRegister(AddrOf_Data_GreenModeLine01); }
        public async Task<short> CmdReadDataGreenModeLine02() { return await CmdReadHoldingRegister(AddrOf_Data_GreenModeLine02); }
        public async Task<short> CmdReadDataProcessNo() { return await CmdReadHoldingRegister(AddrOf_Data_ProcessNo); }


        public async Task CmdSwitchPageToAutoStart() { await CmdPulseCoil(AddrOf_Coil_PageOfAutoStart); }
        public async Task CmdSwitchPageToMainMenu() { await CmdPulseCoil(AddrOf_Coil_PageOfMainMenu); }
        public async Task CmdSwitchPageToProcess() { await CmdPulseCoil(AddrOf_Coil_PageOfProcess); }
        public async Task CmdTriggerEarlyStop() { await CmdPulseCoil(AddrOf_Coil_TriggerEarlyStop); }
        public async Task CmdWriteDataProcessNo(ushort data) { await CmdWriteHoldingRegister(AddrOf_Data_ProcessNo, data); }


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
