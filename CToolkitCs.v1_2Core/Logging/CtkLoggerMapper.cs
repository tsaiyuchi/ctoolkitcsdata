using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace CToolkitCs.v1_2Core.Logging
{
    [Serializable]
    public class CtkLoggerMapper : Dictionary<String, CtkLogger>
    {

        /*[d20230531] 
         * An = Assembly Name
         * Cn = Class Name
         * Id = 追加識別碼
         */


        ~CtkLoggerMapper()
        {
            CtkEventUtil.RemoveEventHandlersOfOwnerByFilter(this, (dlgt) => true);
        }

        public CtkLogger Get(String id = "")
        {
            lock (this)
            {
                if (!this.ContainsKey(id))
                {
                    var logger = new CtkLogger();
                    this.Add(id, logger);

                    this.OnCreated(new CtkLoggerMapperEventArgs() { LoggerId = id, Logger = logger });
                }
            }
            //不能 override/new this[] 會造成無窮迴圈
            return this[id];
        }   





        #region Event
        public event EventHandler<CtkLoggerMapperEventArgs> EhCreated;
        void OnCreated(CtkLoggerMapperEventArgs ea)
        {
            if (this.EhCreated == null)
                return;
            this.EhCreated(this, ea);
        }
        #endregion




    }
}
