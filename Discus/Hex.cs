using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discus
{
    public class Hex
    {
        public List<Hex> getMoveAreaInterceptor(int dist,ballDir direction)
        {
            List<Hex> moveArea = new List<Hex>();
            for (int i = 0; i < 6; i++)
            {
                Hex curhex = this;
                if (i == 0)//upleft
                {
                    int tempDist = dist;
                    if ((int)direction == i)
                    {
                        tempDist++;
                    }
                    for (int j = 0; j < tempDist; j++)
                    {
                        if (curhex.upLeftN == null)
                        {
                            break;
                        }
                        else if (curhex.upLeftN.piece is Ball)
                        {
                            curhex = curhex.upLeftN;
                            moveArea.Add(curhex);
                            break;
                        }
                        else if (curhex.upLeftN.piece != null)
                        {
                            break;
                        }
                        else
                        {
                            curhex = curhex.upLeftN;
                            moveArea.Add(curhex);
                        }
                        
                    }
                }
                if (i == 1)//up
                {
                    int tempDist = dist;
                    if ((int)direction == i)
                    {
                        tempDist++;
                    }
                    for (int j = 0; j < tempDist; j++)
                    {
                        if (curhex.upN == null)
                        {
                            break;
                        }
                        else if (curhex.upN.piece is Ball)
                        {
                            curhex = curhex.upN;
                            moveArea.Add(curhex);
                            break;
                        }
                        else if (curhex.upN.piece != null)
                        {
                            break;
                        }
                        else
                        {
                            curhex = curhex.upN;
                            moveArea.Add(curhex);
                        }

                    }
                }
                if (i == 2)//upright
                {
                    int tempDist = dist;
                    if ((int)direction == i)
                    {
                        tempDist++;
                    }
                    for (int j = 0; j < tempDist; j++)
                    {
                        if (curhex.upRightN == null)
                        {
                            break;
                        }
                        else if (curhex.upRightN.piece is Ball)
                        {
                            curhex = curhex.upRightN;
                            moveArea.Add(curhex);
                            break;
                        }
                        else if (curhex.upRightN.piece != null)
                        {
                            break;
                        }
                        else
                        {
                            curhex = curhex.upRightN;
                            moveArea.Add(curhex);
                        }

                    }
                }
                if (i == 3)//downright
                {
                    int tempDist = dist;
                    if ((int)direction == i)
                    {
                        tempDist++;
                    }
                    for (int j = 0; j < tempDist; j++)
                    {
                        if (curhex.downRightN == null)
                        {
                            break;
                        }
                        else if (curhex.downRightN.piece is Ball)
                        {
                            curhex = curhex.downRightN;
                            moveArea.Add(curhex);
                            break;
                        }
                        else if (curhex.downRightN.piece != null)
                        {
                            break;
                        }
                        else
                        {
                            curhex = curhex.downRightN;
                            moveArea.Add(curhex);
                        }

                    }
                }
                if (i == 4)//down
                {
                    int tempDist = dist;
                    if ((int)direction == i)
                    {
                        tempDist++;
                    }
                    for (int j = 0; j < tempDist; j++)
                    {
                        if (curhex.downN == null)
                        {
                            break;
                        }
                        else if (curhex.downN.piece is Ball)
                        {
                            curhex = curhex.downN;
                            moveArea.Add(curhex);
                            break;
                        }
                        else if (curhex.downN.piece != null)
                        {
                            break;
                        }
                        else
                        {
                            curhex = curhex.downN;
                            moveArea.Add(curhex);
                        }

                    }
                }
                if (i == 5)//downleft
                {
                    int tempDist = dist;
                    if ((int)direction == i)
                    {
                        tempDist++;
                    }
                    for (int j = 0; j < tempDist; j++)
                    {
                        if (curhex.downLeftN == null)
                        {
                            break;
                        }
                        else if (curhex.downLeftN.piece is Ball)
                        {
                            curhex = curhex.downLeftN;
                            moveArea.Add(curhex);
                            break;
                        }
                        else if (curhex.downLeftN.piece != null)
                        {
                            break;
                        }
                        else
                        {
                            curhex = curhex.downLeftN;
                            moveArea.Add(curhex);
                        }

                    }
                }
            }
            return moveArea;
        }
        public List<Hex> GetNeighbors()
        {
            List<Hex> moveArea = new List<Hex>();
             for (int i = 0; i < 6; i++)
            {
                Hex curhex = this;
                if (i == 0)//upleft
                {
                    
                        if (curhex.upLeftN == null)
                        {
                            break;
                        }
                        
                       
                        else
                        {
                            curhex = curhex.upLeftN;
                            moveArea.Add(curhex);
                        }

                    
                }
                if (i == 1)//up
                {
                    
                        if (curhex.upN == null)
                        {
                            break;
                        }
                       
                        else
                        {
                            curhex = curhex.upN;
                            moveArea.Add(curhex);
                        }

                    
                }
                if (i == 2)//upright
                { 
                        if (curhex.upRightN == null)
                        {
                            break;
                        }
                        else
                        {
                            curhex = curhex.upRightN;
                            moveArea.Add(curhex);
                        }
                }
                if (i == 3)//downright
                {
                        if (curhex.downRightN == null)
                        {
                            break;
                        }
                        else
                        {
                            curhex = curhex.downRightN;
                            moveArea.Add(curhex);
                        }
                }
                if (i == 4)//down
                {
                    
                        if (curhex.downN == null)
                        {
                            break;
                        }
                        else
                        {
                            curhex = curhex.downN;
                            moveArea.Add(curhex);
                        }
                }
                if (i == 5)//downleft
                {
                   
                        if (curhex.downLeftN == null)
                        {
                            break;
                        }
                        else
                        {
                            curhex = curhex.downLeftN;
                            moveArea.Add(curhex);
                        }

                    
                }
            }
            return moveArea;
        }
        public List<Hex> getMoveArea(int dist)
        {
            List<Hex> moveArea = new List<Hex>();
            for (int i = 0; i < 6; i++)
            {
                Hex curhex = this;
                if (i == 0)//upleft
                {
                    for (int j = 0; j < dist; j++)
                    {
                        if (curhex.upLeftN == null)
                        {
                            break;
                        }
                        else if (curhex.upLeftN.piece is Ball)
                        {
                            curhex = curhex.upLeftN;
                            moveArea.Add(curhex);
                            break;
                        }
                        else if (curhex.upLeftN.piece != null)
                        {
                            break;
                        }
                        else
                        {
                            curhex = curhex.upLeftN;
                            moveArea.Add(curhex);
                        }
                       
                    }
                }
                if (i == 1)//up
                {
                    for (int j = 0; j < dist; j++)
                    {
                        if (curhex.upN == null)
                        {
                            break;
                        }
                        else if (curhex.upN.piece is Ball)
                        {
                            curhex = curhex.upN;
                            moveArea.Add(curhex);
                            break;
                        }
                        else if (curhex.upN.piece != null)
                        {
                            break;
                        }
                        else
                        {
                            curhex = curhex.upN;
                            moveArea.Add(curhex);
                        }

                    }
                }
                if (i == 2)//upright
                {
                    for (int j = 0; j < dist; j++)
                    {
                        if (curhex.upRightN == null)
                        {
                            break;
                        }
                        else if (curhex.upRightN.piece is Ball)
                        {
                            curhex = curhex.upRightN;
                            moveArea.Add(curhex);
                            break;
                        }
                        else if (curhex.upRightN.piece != null)
                        {
                            break;
                        }
                        else
                        {
                            curhex = curhex.upRightN;
                            moveArea.Add(curhex);
                        }

                    }
                }
                if (i == 3)//downright
                {
                    for (int j = 0; j < dist; j++)
                    {
                        if (curhex.downRightN == null)
                        {
                            break;
                        }
                        else if (curhex.downRightN.piece is Ball)
                        {
                            curhex = curhex.downRightN;
                            moveArea.Add(curhex);
                            break;
                        }
                        else if (curhex.downRightN.piece != null)
                        {
                            break;
                        }
                        else
                        {
                            curhex = curhex.downRightN;
                            moveArea.Add(curhex);
                        }

                    }
                }
                if (i == 4)//down
                {
                    for (int j = 0; j < dist; j++)
                    {
                        if (curhex.downN == null)
                        {
                            break;
                        }
                        else if (curhex.downN.piece is Ball)
                        {
                            curhex = curhex.downN;
                            moveArea.Add(curhex);
                            break;
                        }
                        else if (curhex.downN.piece != null)
                        {
                            break;
                        }
                        else
                        {
                            curhex = curhex.downN;
                            moveArea.Add(curhex);
                        }

                    }
                }
                if (i == 5)//downleft
                {
                    for (int j = 0; j < dist; j++)
                    {
                        if (curhex.downLeftN == null)
                        {
                            break;
                        }
                        else if (curhex.downLeftN.piece is Ball)
                        {
                            curhex = curhex.downLeftN;
                            moveArea.Add(curhex);
                            break;
                        }
                        else if (curhex.downLeftN.piece != null)
                        {
                            break;
                        }
                        else
                        {
                            curhex = curhex.downLeftN;
                            moveArea.Add(curhex);
                        }

                    }
                }
            }
            return moveArea;
        }
        public Hex(Vector2 l,int row, int col)
        {
            location = l;
            this.row = row;
            this.col = col;
        }
        public int[] gridLocation
        {
            get
            {
                int[] coords = new int[2];
                coords[0] = row;
                coords[1] = col;
                return coords;
            }
        }
        
        public Hex upN;
        public Hex downN;
        public Hex upRightN;
        public Hex downRightN;
        public Hex upLeftN;
        public Hex downLeftN;
        public Hex upNeighbor
        {
            get
            {
                if (upN != null)
                {
                    return upN;
                }
                return this;
            }
        }
        public Hex downNeighbor
        {
            get
            {
                if (downN != null)
                {
                    return downN;
                }
                return this;
            }
        }
        public Hex upRightNeighbor
        {
            get
            {
                if (upRightN != null)
                {
                    return upRightN;
                }
                return this;
            }
        }
        public Hex downRightNeighbor
        {
            get
            {
                if (downRightN != null)
                {
                    return downRightN;
                }
                return this;
            }
        }
        public Hex upLeftNeighbor
        {
            get
            {
                if (upLeftN != null)
                {
                    return upLeftN;
                }
                return this;
            }
        }
        public Hex downLeftNeighbor
        {
            get
            {
                if (downLeftN != null)
                {
                    return downLeftN;
                }
                return this;
            }
        }
        public void PopulateNeighbors(List<Hex> allSpaces)
        {
            for (int i = 0; i<allSpaces.Count;i++)
            {
                Hex space = allSpaces[i];
                if (this.gridLocation[0] - 1 == space.gridLocation[0] && this.gridLocation[1] == space.gridLocation[1])
                {
                    upN = space;
                }
                else if (this.gridLocation[0] + 1 == space.gridLocation[0] && this.gridLocation[1] == space.gridLocation[1])
                {
                    downN = space;
                }
                else if (this.gridLocation[1] % 2 == 1)
                {
                    if (this.gridLocation[0] == space.gridLocation[0] && this.gridLocation[1] + 1 == space.gridLocation[1])
                    {
                        upRightN = space;
                    }
                    else if (this.gridLocation[0] + 1 == space.gridLocation[0] && this.gridLocation[1] - 1 == space.gridLocation[1])
                    {
                        downLeftN = space;
                    }
                    else if (this.gridLocation[0] + 1 == space.gridLocation[0] && this.gridLocation[1] + 1 == space.gridLocation[1])
                    {
                        downRightN = space;
                    }
                    else if (this.gridLocation[0] == space.gridLocation[0] && this.gridLocation[1] - 1 == space.gridLocation[1])
                    {
                        upLeftN = space;
                    }
                }
                else
                {
                    if (this.gridLocation[0] == space.gridLocation[0] && this.gridLocation[1] - 1 == space.gridLocation[1])
                    {
                        downLeftN = space;
                    }
                    else if (this.gridLocation[0]-1 == space.gridLocation[0] && this.gridLocation[1] + 1 == space.gridLocation[1])
                    {
                        upRightN = space;
                    }
                    else if (this.gridLocation[0] == space.gridLocation[0] && this.gridLocation[1] + 1 == space.gridLocation[1])
                    {
                        downRightN = space;
                    }
                    else if (this.gridLocation[0]-1 == space.gridLocation[0] && this.gridLocation[1] - 1 == space.gridLocation[1])
                    {
                        upLeftN = space;
                    }
                }
                
            }
        }
        int row;
        int col;
        public readonly Vector2 location;
        private Piece _piece;
        public Piece piece
        {
            get { return _piece; }
            set { _piece = value;
                if(value!=null)
                _piece.hexPos = new Vector2(this.gridLocation[0], this.gridLocation[1]);//im da king rat who makes all da rules
                if (value is Ball)
                {
                    Program.game.ballHex = this;
                }
            }
        }
        public static Vector2 HexToPoints(float height, float row, float col)
        {
            // Start with the leftmost corner of the upper left hexagon.
            float width = HexWidth(height);
            float y = height / 2;
            float x = 0;

            // Move down the required number of rows.
            y += row * height;

            // If the column is odd, move down half a hex more.
            if (col % 2 == 1) y += height / 2;

            // Move over for the column number.
            x += col * (width * 0.75f);
            Vector2 location = Vector2.Zero;
            location += new Vector2(x, y) +
            new Vector2(x + width * 0.25f, y - height / 2) +
            new Vector2(x + width * 0.75f, y - height / 2) +
            new Vector2(x + width, y) +
            new Vector2(x + width * 0.75f, y + height / 2) +
            new Vector2(x + width * 0.25f, y + height / 2);
            location /= 6;
            return location;
            // Generate the points.
            /*return new Vector2[]
                {
            new Vector2(x, y),
            new Vector2(x + width * 0.25f, y - height / 2),
            new Vector2(x + width * 0.75f, y - height / 2),
            new Vector2(x + width, y),
            new Vector2(x + width * 0.75f, y + height / 2),
            new Vector2(x + width * 0.25f, y + height / 2),
               };*/
        }
        public static void PointToHex(float x, float y, float height,
    out int row, out int col)
        {
            // Find the test rectangle containing the point.
            float width = HexWidth(height);
            col = (int)(x / (width * 0.75f));

            if (col % 2 == 0)
                row = (int)(y / height);
            else
                row = (int)((y - height / 2) / height);

            // Find the test area.
            float testx = col * width * 0.75f;
            float testy = row * height;
            if (col % 2 == 1) testy += height / 2;

            // See if the point is above or
            // below the test hexagon on the left.
            bool is_above = false, is_below = false;
            float dx = x - testx;
            if (dx < width / 4)
            {
                float dy = y - (testy + height / 2);
                if (dx < 0.001)
                {
                    // The point is on the left edge of the test rectangle.
                    if (dy < 0) is_above = true;
                    if (dy > 0) is_below = true;
                }
                else if (dy < 0)
                {
                    // See if the point is above the test hexagon.
                    if (-dy / dx > Math.Sqrt(3)) is_above = true;
                }
                else
                {
                    // See if the point is below the test hexagon.
                    if (dy / dx > Math.Sqrt(3)) is_below = true;
                }
            }

            // Adjust the row and column if necessary.
            if (is_above)
            {
                if (col % 2 == 0) row--;
                col--;
            }
            else if (is_below)
            {
                if (col % 2 == 1) row++;
                col--;
            }
        }
        private static float HexWidth(float height)
        {
            return (float)(4 * (height / 2 / Math.Sqrt(3)));
        }
    }
}
