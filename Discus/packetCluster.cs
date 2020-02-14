using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discus
{
    [Serializable]
    class packetCluster
    {
        public string arbiterId;
        public bool acknowledged;
        public List<gameplayPacket> cluster;
    }
}
