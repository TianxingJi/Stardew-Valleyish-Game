using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TFarm.AStar
{
    public class Node : IComparable<Node>
    {
        public Vector2Int gridPosition; //the position of grid
        public int gCost = 0;   //The distance to the Start Grid
        public int hCost = 0;   //The distance to the Target Grid
        public int FCost => gCost + hCost;  //The current grid's distance addition 
        public bool isObstacle = false; //If the current grid is obstacle
        public Node parentNode;

        public Node(Vector2Int pos)
        {
            gridPosition = pos;
            parentNode = null;
        }

        public int CompareTo(Node other)
        {
            //Compare to find the lowest F value and return -1, 0, 1
            int result = FCost.CompareTo(other.FCost);
            if (result == 0)
            {
                result = hCost.CompareTo(other.hCost);
            }
            return result;
        }
    }
}