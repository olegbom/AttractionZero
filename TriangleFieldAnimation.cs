using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttractionZero
{

    public enum TriangleAnimationType
    {

    }
    
    public struct TriangleRotateAnimation
    {

        public int Column;
        public int Row;
        public int RotationPointNumber;
        public int NumberOfTurns;

        public TriangleRotateAnimation(int column, int row, int rotationPointNumber, int numberOfTurns)
        {
            Column = column;
            Row = row;
            RotationPointNumber = rotationPointNumber;
            NumberOfTurns = numberOfTurns;
        }



    }
}
