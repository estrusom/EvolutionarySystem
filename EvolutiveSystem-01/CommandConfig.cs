using SocketManagerInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutiveSystem_01
{
    internal class CommandConfig
    {
        public event EventHandler<string> ExecuteCmdSync;
        public event EventHandler<string> ExecuteOpenDB;
        public event EventHandler<string> ExecuteSaveDB;
        public event EventHandler<string> ExecuteDBStruct;
        public void CmdSync(string SomeData)
        {
            ExecuteCmdSync?.Invoke(this, SomeData);
        }
        public void CmdOpenDB(string SomeData)
        {
            ExecuteOpenDB?.Invoke(this, SomeData);
        }
        public void CmdSaveDB(string SomeData)
        {
            ExecuteSaveDB?.Invoke(this, SomeData);
        }
        public void CmdStructDb(string SomeData)
        {
            ExecuteDBStruct?.Invoke(this, SomeData);
        }
    }
}
