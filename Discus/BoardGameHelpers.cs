using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discus
{
    class BoardGameHelpers
    {
        public static void newTurn(Team T)
        {
            for (int i = 0; i < Program.game.boardLocations.Count; i++)
            {
                if (Program.game.boardLocations[i].piece != null)
                {
                    Program.game.boardLocations[i].piece.canMove = true;
                    Program.game.boardLocations[i].piece.hasAbility = true;
                }
            }
            
        }
        public static void resolveCommand(int cmd)
        {
            switch (cmd)
            {
                case 1:
                    
                    if (Program.game.whosTurn == Program.game.playerTeam)
                    {
                        if (Program.game.whosTurn == Team.Red)
                        {
                            Program.game.whosTurn = Team.Blue;
                        }
                        else
                        {
                            Program.game.whosTurn = Team.Red;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        public static ballDir getDir(Hex fromHex, Hex toHex)
        {
            Hex up = fromHex.upNeighbor, upleft = fromHex.upLeftNeighbor, upright = fromHex.upRightNeighbor, down = fromHex.downNeighbor, downleft = fromHex.downLeftNeighbor, downright = fromHex.downRightNeighbor;
            for (int i = 0; i < 10; i++)
            {//search the graph in all 6 given directions for the space
                if (up == toHex)
                {
                    return ballDir.up;
                }
                if (upleft == toHex)
                {
                    return ballDir.upLeft;
                }
                if (upright == toHex)
                {
                    return ballDir.upRight;
                }
                if (down == toHex)
                {
                    return ballDir.down;
                }
                if (downleft == toHex)
                {
                    return ballDir.downLeft;
                }
                if (downright == toHex)
                {
                    return ballDir.downRight;
                }
                up = up.upNeighbor;
            upleft = upleft.upLeftNeighbor;
           upright = upright.upRightNeighbor;
              down = down.downNeighbor;
          downleft = downleft.downLeftNeighbor;
         downright = downright.downRightNeighbor;
            }
            return ballDir.noDir;
        }
        public static void placeBall(Hex h,Ball b,Team whosTurn)
        {
            if (h.piece != null)
            {
               
                    Program.game.ballPlaceTeam = h.piece.team;
                    Program.game.action = "placeball";
                    Program.game.actionHex = h;
                    Program.game.abilityHexes = h.GetNeighbors();
                    Program.game.movementHexes = new List<Hex>(); 
               
            }
            else
            {//place on an empty space
                Program.game.movementHexes = new List<Hex>();
                Program.game.abilityHexes = new List<Hex>();
                h.piece = b;
                ((Ball)h.piece).ownerSpace = Program.game.actionHex;
                h.piece.team = Program.game.actionHex.piece.team;
                Program.game.action = "";
                Program.game.ballFlying = false;
            }
        }
        public static void curveBall(bool curveLeft, ballDir d)
        {
            if (curveLeft)
            {
                if ((int)d == 0)
                {
                    d = (ballDir)6;
                }
                else
                {
                    d = (ballDir)(d - 1);
                }
            }
            else
            {
                if ((int)d == 5)
                {
                    d = (ballDir)0;
                }
                else
                {
                    d = (ballDir)(d + 1);
                }
            }
        }
        public static void moveBall(Hex ballLocationStart, ballDir direction)
        {
            Hex curhex = ballLocationStart;
            switch (direction)
            {
                case ballDir.upLeft:
                    for (int j = 0; j < 3; j++)
                    {
                        if (curhex.upLeftN == null)
                        {
                            Program.game.cyborgThrow = Team.Neutral;
                            Program.game.ballFlying = false;
                            break;

                        }
                        else if (curhex.upLeftN.piece != null && !(curhex.upLeftN.piece is Ball))
                        {

                            Program.game.actionHex = curhex.upLeftN;
                            Program.game.ballPlaceTeam = Program.game.actionHex.piece.team;
                            Program.game.action = "placeball";
                            Program.game.abilityHexes = Program.game.actionHex.getMoveArea(1);
                            ballLocationStart.piece = null;
                            Program.game.ballFlying = false;
                            Program.game.cyborgThrow = Team.Neutral;
                            break;
                        }
                        else if (curhex.upLeftN.piece != null)
                        {
                            Program.game.cyborgThrow = Team.Neutral;
                            Program.game.ballFlying = false;
                            break;
                        }
                        else
                        {
                            curhex = curhex.upLeftN;

                        }

                    }
                    break;
                case ballDir.up:
                    for (int j = 0; j < 3; j++)
                    {
                        if (curhex.upN == null)
                        {
                            Program.game.cyborgThrow = Team.Neutral;
                            Program.game.ballFlying = false;
                            break;
                        }
                        else if (curhex.upN.piece != null && !(curhex.upN.piece is Ball))
                        {
                            Program.game.actionHex = curhex.upN;
                            Program.game.ballPlaceTeam = Program.game.actionHex.piece.team;
                            Program.game.action = "placeball";
                            Program.game.abilityHexes = Program.game.actionHex.getMoveArea(1);
                            ballLocationStart.piece = null;
                            Program.game.ballFlying = false;
                            break;
                        }
                        else if (curhex.upN.piece != null)
                        {
                            Program.game.cyborgThrow = Team.Neutral;
                            Program.game.ballFlying = false;
                            break;
                        }
                        else
                        {
                            curhex = curhex.upN;
                        }

                    }
                    break;
                case ballDir.upRight:
                    for (int j = 0; j < 3; j++)
                    {
                        if (curhex.upRightN == null)
                        {
                            Program.game.cyborgThrow = Team.Neutral;
                            Program.game.ballFlying = false;
                            break;
                        }
                        else if (curhex.upRightN.piece != null && !(curhex.upRightN.piece is Ball))
                        {
                            Program.game.actionHex = curhex.upRightN;
                            Program.game.ballPlaceTeam = Program.game.actionHex.piece.team;
                            Program.game.action = "placeball";
                            Program.game.abilityHexes = Program.game.actionHex.getMoveArea(1);
                            ballLocationStart.piece = null;
                            Program.game.ballFlying = false;
                            break;
                        }
                        else if (curhex.upRightN.piece != null)
                        {

                            Program.game.ballFlying = false;
                            break;
                        }
                        else
                        {
                            curhex = curhex.upRightN; 
                        }
                    }
                    break;
                case ballDir.downRight:
                    for (int j = 0; j < 3; j++)
                    {
                        if (curhex.downRightN == null)
                        {
                            Program.game.cyborgThrow = Team.Neutral;
                            Program.game.ballFlying = false;
                            break;
                        }
                        else if (curhex.downRightN.piece != null && !(curhex.downRightN.piece is Ball))
                        {
                            Program.game.actionHex = curhex.downRightN;
                            Program.game.ballPlaceTeam = Program.game.actionHex.piece.team;
                            Program.game.action = "placeball";
                            Program.game.abilityHexes = Program.game.actionHex.getMoveArea(1);
                            ballLocationStart.piece = null;
                            Program.game.ballFlying = false;
                            break;
                        }
                        else if (curhex.downRightN.piece != null)
                        {
                            Program.game.ballFlying = false;
                            break;
                        }
                        else
                        {
                            curhex = curhex.downRightN;
                        }

                    }
                    break;
                case ballDir.down:
                    for (int j = 0; j < 3; j++)
                    {
                        if (curhex.downN == null)
                        {
                            Program.game.cyborgThrow = Team.Neutral;
                            Program.game.ballFlying = false;
                            break;
                        }
                        else if (curhex.downN.piece != null && !(curhex.downN.piece is Ball))
                        {
                            Program.game.actionHex = curhex.downN;
                            Program.game.ballPlaceTeam = Program.game.actionHex.piece.team;
                            Program.game.action = "placeball";
                            Program.game.abilityHexes = Program.game.actionHex.getMoveArea(1);
                            ballLocationStart.piece = null;
                            Program.game.ballFlying = false;
                            break;
                        }
                        else if (curhex.downN.piece != null)
                        {
                            Program.game.ballFlying = false;
                            break;
                        }
                        else
                        {
                            curhex = curhex.downN;
                        }

                    }
                    break;
                case ballDir.downLeft:
                    for (int j = 0; j < 3; j++)
                    {
                        if (curhex.downLeftN == null)
                        {
                            Program.game.cyborgThrow = Team.Neutral;
                            Program.game.ballFlying = false;
                            break;
                        }
                        else if (curhex.downLeftN.piece != null && !(curhex.downLeftN.piece is Ball))
                        {
                            Program.game.actionHex = curhex.downLeftN;
                            Program.game.ballPlaceTeam = Program.game.actionHex.piece.team;
                            Program.game.action = "placeball";
                            Program.game.abilityHexes = Program.game.actionHex.getMoveArea(1);
                            ballLocationStart.piece = null;
                            Program.game.ballFlying = false;
                            break;
                        }
                        else if (curhex.downLeftN.piece != null)
                        {
                            Program.game.ballFlying = false;
                            break;
                        }
                        else
                        {
                            curhex = curhex.downLeftN;
                        }

                    }
                    break;
                default:
                    break;
                    
            }
            if (Program.game.action != "placeball")
            {
                curhex.piece = ballLocationStart.piece;
                ballLocationStart.piece = null;
            }
        }
        public static void tryMovePiece(Hex toSpace)
        {
            if (Program.game.movementHexes.Contains(toSpace) && toSpace.piece == null)
            {
                Program.game.hoveredSpace.piece = Program.game.actionHex.piece;
                Program.game.actionHex.piece = null;
            }
            else if (Program.game.movementHexes.Contains(toSpace) && toSpace.piece is Ball)
            {
                Program.game.ballPlaceTeam = Program.game.actionHex.piece.team;
                Program.game.hoveredSpace.piece = Program.game.actionHex.piece;
                Program.game.actionHex.piece = null;
                Program.game.action = "placeball";
                Program.game.abilityHexes = Program.game.hoveredSpace.getMoveArea(1);
                Program.game.actionHex = Program.game.hoveredSpace;
            }
        }
    }
}
