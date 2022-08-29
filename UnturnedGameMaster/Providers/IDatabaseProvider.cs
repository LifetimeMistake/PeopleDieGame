using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnturnedGameMaster.Providers
{
    public interface IDatabaseProvider<T>
    {
        T GetData();
        bool CommitData();
    }
}
