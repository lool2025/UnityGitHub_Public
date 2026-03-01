using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MFarm.AStar
{
    public class GridNodes
    {
        private int width;
        private int height;
        private Node[,]gridNode;

        public GridNodes(int width, int height)
        {
            this.width = width;
            this.height = height;

            gridNode = new Node[width, height];
            for(int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    gridNode[x,y]=new Node(new Vector2Int(x,y));
                }
            }
        }

        public Node GetGridNode(int xPos, int yPos)
        {
           
            if(xPos<=width && yPos<=height)
            {
                return gridNode[xPos,yPos];
            }
            Debug.Log("xPos  " +( xPos) + "  yPos  " + (yPos) + "  xPos<width && yPos<height: " + (xPos < width && yPos < height) 
                + " width :" + width+ " height :" + height+ "xPos  " + xPos + "  yPos  " + yPos );
            Debug.Log("³¬³öÍø¸ñ·¶Î§");
            return null;
        }
    }

}
