using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttractionZero;

public struct TriangleRotateAnimation
{

    public int Column;
    public int Row;

    /// <summary>
    /// Point of Rotation
    ///    0
    ///   /\
    ///  /  \
    /// /____\
    /// 2     1
    ///    
    /// 2______0   
    ///  \    / 
    ///   \  /  
    ///    \/
    ///    1
    /// </summary>
    public int RotationPointNumber;
    
    /// <summary>
    /// Rotate Anticlockwise, 1 bit - 60°
    /// </summary>
    public int NumberOfTurns;

    public TriangleRotateAnimation(int column, int row, int rotationPointNumber, int numberOfTurns)
    {
        Column = column;
        Row = row;
        RotationPointNumber = rotationPointNumber;
        NumberOfTurns = numberOfTurns;
    }



}

