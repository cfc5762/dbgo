using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discus
{
    [Serializable]
    public class gameplayPacket
    {
        string arbiterId;
        public int id;
        public int activeCommand;
        bool leftClick;
        bool rightClick;
        int[] hexLocation;
    }
}
