using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TFarm.AStar
{
    public class GridNodes
    {
        private int width;
        private int height;
        private Node[,] gridNode;

        /// <summary>
        /// Constructor Function to Initialize all the nodes in the map
        /// </summary>
        /// <param name="width">The width of the map</param>
        /// <param name="height">The height of the map</param>
        public GridNodes(int width, int height)
        {
            this.width = width;
            this.height = height;

            gridNode = new Node[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    gridNode[x, y] = new Node(new Vector2Int(x, y));
                }
            }
        }


        public Node GetGridNode(int xPos, int yPos)
        {
            if (xPos < width && yPos < height)
            {
                return gridNode[xPos, yPos];
            }
            Debug.Log("Outside the normal Grid Map");
            return null;
        }
    }
}