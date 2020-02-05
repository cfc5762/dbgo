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
                            Program.game.ballFlying = false;
                            break;

                        }
                        else if (curhex.upLeftN.piece != null && !(curhex.upLeftN.piece is Ball))
                        {

                            Program.game.actionHex = curhex.upLeftN;
                            Program.game.ballPlaceTeam = Program.game.actionHex.piece.team;
                            Program.game.action = "placeBall";
                            Program.game.abilityHexes = Program.game.actionHex.getMoveArea(1);
                            ballLocationStart.piece = null;
                            Program.game.ballFlying = false;
                            break;
                        }
                        else if (curhex.upLeftN.piece != null)
                        {
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
                            Program.game.ballFlying = false;
                            break;
                        }
                        else if (curhex.upN.piece != null && !(curhex.upN.piece is Ball))
                        {
                            Program.game.actionHex = curhex.upN;
                            Program.game.ballPlaceTeam = Program.game.actionHex.piece.team;
                            Program.game.action = "placeBall";
                            Program.game.abilityHexes = Program.game.actionHex.getMoveArea(1);
                            ballLocationStart.piece = null;
                            Program.game.ballFlying = false;
                            break;
                        }
                        else if (curhex.upN.piece != null)
                        {
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
                            Program.game.ballFlying = false;
                            break;
                        }
                        else if (curhex.upRightN.piece != null && !(curhex.upRightN.piece is Ball))
                        {
                            Program.game.actionHex = curhex.upRightN;
                            Program.game.ballPlaceTeam = Program.game.actionHex.piece.team;
                            Program.game.action = "placeBall";
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
                            Program.game.ballFlying = false;
                            break;
                        }
                        else if (curhex.downRightN.piece != null && !(curhex.downRightN.piece is Ball))
                        {
                            Program.game.actionHex = curhex.downRightN;
                            Program.game.ballPlaceTeam = Program.game.actionHex.piece.team;
                            Program.game.action = "placeBall";
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
                            Program.game.ballFlying = false;
                            break;
                        }
                        else if (curhex.downN.piece != null && !(curhex.downN.piece is Ball))
                        {
                            Program.game.actionHex = curhex.downN;
                            Program.game.ballPlaceTeam = Program.game.actionHex.piece.team;
                            Program.game.action = "placeBall";
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
                            Program.game.ballFlying = false;
                            break;
                        }
                        else if (curhex.downLeftN.piece != null && !(curhex.downLeftN.piece is Ball))
                        {
                            Program.game.actionHex = curhex.downLeftN;
                            Program.game.ballPlaceTeam = Program.game.actionHex.piece.team;
                            Program.game.action = "placeBall";
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
            curhex.piece = ballLocationStart.piece;
            ballLocationStart.piece = null;
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
                Program.game.action = "placeBall";
                Program.game.abilityHexes = Program.game.hoveredSpace.getMoveArea(1);
                Program.game.actionHex = Program.game.hoveredSpace;
            }
        }
    }
}
