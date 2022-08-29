using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Managers
{
    public interface IDisposableManager : IManager
    {
        void Dispose();
    }
}
