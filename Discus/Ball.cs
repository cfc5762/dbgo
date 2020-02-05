using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discus
{
    class Ball:Piece
    {
        public Piece owner;
        public Ball(Team T) : base(T)
        {

        }
    }
}
