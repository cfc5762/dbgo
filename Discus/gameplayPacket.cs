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
        public bool acknowledged;
        public string arbiterId;
        public string id;
        public int activeCommand;
        public bool leftClick;
        public bool rightClick;
        public int col;
        public int row;
    }
}
