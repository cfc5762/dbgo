using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discus
{
    public enum Team
    {
        Red,
        Blue,
        Neutral
    }
    public class Piece
    {
        int grounded;
        public bool canMove;
        public bool hasAbility;
        public Vector2 hexPos;
        public Team team;
        public Piece(Team t)
        {
            grounded = 0;
            hasAbility = true;
            team = t;
            hexPos = Vector2.Zero;
        }
    }
}
