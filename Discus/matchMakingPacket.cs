using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discus
{
    [Serializable]
    public class matchMakingPacket
    {
        public int id;
        public bool acknowledged;
        public byte[] ip; //size 4
        public int port;
        public int color;
        public string clientID;
        public string enemyID;
    }
}
