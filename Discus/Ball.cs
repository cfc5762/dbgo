using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discus
{
    class Ball:Piece
    {
        public Hex ownerSpace;
        public Ball(Team T) : base(T)
        {

        }
    }
}
