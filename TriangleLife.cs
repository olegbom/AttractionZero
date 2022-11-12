using System.Collections;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Raylib_cs;

namespace AttractionZero;

[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.Net70)]
public class TriangleLife
{
    private static readonly byte[] SolveMatrix;

    static TriangleLife()
    {
        const int numberOfSolutions = (1 << 16);
        SolveMatrix = new byte[(1 << (16 - 3))];
        for (int i = 0; i < numberOfSolutions; i++)
        {

            bool GetBit(int di, int dj)
            {
                return (i & (1 << (14 + (dj - 1) - (di + 2) * 3 ))) != 0;
            }
            int s = 0;
            if (GetBit( -1, 0)) s += 12;
            if (GetBit( +1, 0)) s += 12;
            if (GetBit( -1, -1)) s += 4;
            if (GetBit( +1, -1)) s += 4;
            if (GetBit( -2, 0)) s += 4;
            if (GetBit( +2, 0)) s += 4;
            if (GetBit( -1, +1)) s += 4;
            if (GetBit( +1, +1)) s += 4;

            bool isTriangleUp = (i & (1 << 15)) == 0;
            
            if (isTriangleUp)
            {
                if (GetBit(0, +1)) s += 12;

                if (GetBit(0, -1)) s += 3;
                if (GetBit(-2, +1)) s += 3;
                if (GetBit(+2, +1)) s += 3;
            }
            else
            {
                if (GetBit(0, -1)) s += 12;

                if (GetBit(0, +1)) s += 3;
                if (GetBit(-2, -1)) s += 3;
                if (GetBit(+2, -1)) s += 3;
            }

            if (GetBit(0, 0))
            {
                if (s > 13 && s < 32)
                    SolveMatrix[i / 8] |= (byte)(1 << (i % 8));
            }
            else
            {
                if (s > 21 && s < 32)
                    SolveMatrix[i / 8] |= (byte)(1 << (i % 8));
            }
        }
    }



    private uint[] _backField;
    private uint[] _drawField;



    public readonly float TriangleWidthInPixels = 41f/MathF.Sqrt(3);
    public readonly float TriangleHeightInPixels = 41f;


    public int Width { get; }
    public int Height { get; }
    public int Generation { get; private set; }
    public float Scale { get; set; } = 1;


    public TriangleLife() : this(1000, 1000)
    {

    }

    public TriangleLife(int width, int height)
    {
        Width = width;
        Height = height;
        int numberOfWords = (int)((width * height) / 32);
        if ((width * height) % 32 != 0)
            numberOfWords += 1;

        _backField = new uint[numberOfWords];
        _drawField = new uint[numberOfWords];
        unsafe
        {
            fixed (uint* ptr = _backField)
            {
                Random.Shared.NextBytes(new Span<byte>(ptr, numberOfWords * 4));
            }
        }
        
        _backField.CopyTo(_drawField, 0);
        Generation = 0;
    }
    public void Reset()
    {
        unsafe
        {
            fixed (uint* ptr = _drawField)
            {
                Random.Shared.NextBytes(new Span<byte>(ptr, _drawField.Length * 4));
            }
        }

        Generation = 0;
    }

    private void Flip() => (_drawField, _backField) = (_backField, _drawField);

    

    private bool GetPixel(uint[] field, int i, int j)
    {
        if (i < 0 || i >= Width || j < 0 || j >= Height)
            return false;
        int index = i * Height + j;
        int wordIndex = index >> 5;
        int bitIndex = index & 0x1F;
        return (field[wordIndex] & (1 << bitIndex)) != 0;
    }

    


    
    private void SetPixel(uint[] field, int i, int j)
    {
        int index = i * Height + j;
        int wordIndex = index / 32;
        int bitIndex = index % 32;
        Interlocked.Or(ref field[wordIndex], (1u << bitIndex));
    }
    private void ResetPixel(uint[] field, int i, int j)
    {
        int index = i * Height + j;
        int wordIndex = index / 32;
        int bitIndex = index % 32;
        Interlocked.And(ref field[wordIndex], ~(1u << bitIndex));
    }


    public void SetPixel(int i, int j) => SetPixel(_drawField, i, j);
    public void ResetPixel(int i, int j) => ResetPixel(_drawField, i, j);

    private int GetTwoPixels(uint[] field, int si, int sj)
    {
        if (si < 0 || si >= Width) return 0;
        int startIndex = sj + si * Height;
        uint startByte = field[startIndex >> 5];
        int startBitIndex = startIndex & 0x1F;
        if (startBitIndex < 31)
        {
            return (int)((startByte >> startBitIndex) & 0b11);
        }
        else
        {
            int stopIndex = startIndex + 1;
            uint stopByte = field[stopIndex >> 5];
            return (int)(((stopByte << 1) & 0b10) | (startByte >> startBitIndex));
        }
    }

    private int GetThreePixels(uint[] field, int si, int sj)
    {
        if (si < 0 || si >= Width) return 0;

        int startIndex = sj + si * Height;
        uint startByte = field[startIndex >> 5];
        int startBitIndex = startIndex & 0x1F;
        if (startBitIndex < 30)
        {
            return (int)((startByte >> startBitIndex) & 0b111);
        }
        else
        {
            int stopIndex = startIndex + 2;
            uint stopByte = field[stopIndex >> 5];
            return (int)(((stopByte << (32 - startBitIndex)) & 0b110) | (startByte >> startBitIndex));
        }
    }

    public void SetRandomRectangleAlternative(double x, double y, double width, double height, double pho = 0.5) =>
        SetRandomRectangle((int)(Width * x), (int)(Height * y), (int)(Width * width), (int)(Height * height), pho);

    public void SetRandomRectangle(int x, int y, int width, int height, double rho = 0.5f)
    {
        if (x < 0)
        {
            width += x;
            x = 0;
        }
        else if (x >= Width)
            return;

        if (y < 0)
        {
            height += y;
            y = 0;
        }
        else if (y >= Height) 
            return;

        if (x + width > Width) width = Width - x;
        if (y + height > Height) height = Height - y;
        Array.Fill(_drawField, 0u);
        for (int i = x; i < x + width; i++)
        {
            for (int j = y; j < y + height; j++)
            {
                if (Random.Shared.NextDouble() < rho)
                {
                    SetPixel(_drawField, i, j);
                }
            }
        }

    }

    public void Draw()
    {
        Raylib.DrawRectangle(0, 0, (int)(TriangleWidthInPixels * Scale * (Width + 1)),
            (int)(TriangleHeightInPixels * Scale * (Height)), Color.SKYBLUE);
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                Vector2 center = new Vector2(TriangleWidthInPixels + i * TriangleWidthInPixels,
                    TriangleHeightInPixels/2 + j * TriangleHeightInPixels);

                Vector2 a, b, c;
                if ((i + j) % 2 == 0)
                {
                    a = center + new Vector2(0, -TriangleHeightInPixels / 2);
                    b = center + new Vector2(-TriangleWidthInPixels, TriangleHeightInPixels / 2);
                    c = center + new Vector2(TriangleWidthInPixels, TriangleHeightInPixels / 2);
                    //Raylib.DrawTriangleLines(a * Scale, b * Scale, c * Scale, Color.BLACK);
                }
                else
                {
                    a = center + new Vector2(0, TriangleHeightInPixels / 2);
                    b = center + new Vector2(TriangleWidthInPixels, -TriangleHeightInPixels / 2);
                    c = center + new Vector2(-TriangleWidthInPixels, -TriangleHeightInPixels / 2);
                }
                if (GetPixel(_drawField, i,j))
                {
                    Raylib.DrawTriangle(a * Scale, b * Scale, c * Scale, Color.BLACK);
                }
                else
                {
                    // Raylib.DrawTriangleLines(a, b, c, Color.BLACK);
                }
                
            }
        }
    }

    public (int, int)? DrawCursor(Vector2 pos)
    {
        float yf = pos.Y / TriangleHeightInPixels/Scale;
        
        int y = (int)MathF.Floor(yf);
        float dy = yf - y;
        int x = (int)MathF.Floor(pos.X / TriangleWidthInPixels / Scale );

        if (((x + y) & 1) == 0)
        {
            x = (int)MathF.Floor(pos.X / TriangleWidthInPixels / Scale - (1-dy));
        }
        else
        {
            x = (int)MathF.Floor(pos.X / TriangleWidthInPixels / Scale - dy);
        }

        if (x < 0 || x >= Width || y < 0 || y >= Height) return null;

        Vector2 center = new Vector2(TriangleWidthInPixels + x * TriangleWidthInPixels,
            TriangleHeightInPixels / 2 + y * TriangleHeightInPixels);
        Vector2 a, b, c;
        if ((x + y) % 2 == 0)
        {
            a = center + new Vector2(0, -TriangleHeightInPixels / 2);
            b = center + new Vector2(-TriangleWidthInPixels, TriangleHeightInPixels / 2);
            c = center + new Vector2(TriangleWidthInPixels, TriangleHeightInPixels / 2);
        }
        else
        {
            a = center + new Vector2(0, TriangleHeightInPixels / 2);
            b = center + new Vector2(TriangleWidthInPixels, -TriangleHeightInPixels / 2);
            c = center + new Vector2(-TriangleWidthInPixels, -TriangleHeightInPixels / 2);
        }

        Raylib.DrawTriangle(a * Scale, b * Scale, c * Scale, new Color(0,255, 0, 120));
        return (x, y);
    }

    private List<TriangleRotateAnimation> _rotationAnimations = new List<TriangleRotateAnimation>();
    private List<(int,int)> _nascentAnimations = new List<(int, int)>();
    private List<(int, int)> _dyingAnimations = new List<(int, int)>();
    private List<(int, int)> _unchangedAnimations = new List<(int, int)>();

    public void AnimationPreparation()
    {
        uint[] deadCells = new uint[_backField.Length];
        for (int i = 0; i < _backField.Length; i++)
        {
            deadCells[i] = _backField[i] & (~_drawField[i]);
        }

        _rotationAnimations.Clear();
        _nascentAnimations.Clear();
        _dyingAnimations.Clear();
        void CreateAnimation(int i, int j, int rotPoint, int nOfTurns)
        {
            _rotationAnimations.Add(new TriangleRotateAnimation(i,j, rotPoint, nOfTurns));
            ResetPixel(deadCells, i, j);
        }

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (!GetPixel(_backField, i, j) && 
                     GetPixel(_drawField, i, j)) //Nascent cell
                {
                    if ((i + j) % 2 == 0)
                    {
                        if      (GetPixel(deadCells, i    , j + 1)) CreateAnimation(i    , j + 1, 0, -1);
                        else if (GetPixel(deadCells, i + 1, j    )) CreateAnimation(i + 1, j    , 2, -1);
                        else if (GetPixel(deadCells, i - 1, j    )) CreateAnimation(i - 1, j    , 1, -1);
                        else if (GetPixel(deadCells, i - 1, j - 1)) CreateAnimation(i - 1, j - 1, 1,  2);
                        else if (GetPixel(deadCells, i + 1, j - 1)) CreateAnimation(i + 1, j - 1, 2, -2);
                        else if (GetPixel(deadCells, i - 2, j    )) CreateAnimation(i - 2, j    , 1, -2);
                        else if (GetPixel(deadCells, i + 2, j    )) CreateAnimation(i + 2, j    , 2,  2);
                        else if (GetPixel(deadCells, i - 1, j + 1)) CreateAnimation(i - 1, j + 1, 0,  2);
                        else if (GetPixel(deadCells, i + 1, j + 1)) CreateAnimation(i + 1, j + 1, 0, -2);
                        else if (GetPixel(deadCells, i    , j - 1)) CreateAnimation(i    , j - 1, 1, -3);
                        else if (GetPixel(deadCells, i - 2, j + 1)) CreateAnimation(i - 2, j + 1, 0, -3);
                        else if (GetPixel(deadCells, i + 2, j + 1)) CreateAnimation(i + 2, j + 1, 2, -3);
                        else _nascentAnimations.Add((i,j));
                    }
                    else
                    {
                        if      (GetPixel(deadCells, i - 1, j    )) CreateAnimation(i - 1, j    , 1, -1);
                        else if (GetPixel(deadCells, i + 1, j    )) CreateAnimation(i + 1, j    , 0, -1);
                        else if (GetPixel(deadCells, i    , j - 1)) CreateAnimation(i    , j - 1, 2, -1);
                        else if (GetPixel(deadCells, i - 1, j - 1)) CreateAnimation(i - 1, j - 1, 1, -2);
                        else if (GetPixel(deadCells, i + 1, j - 1)) CreateAnimation(i + 1, j - 1, 1,  2);
                        else if (GetPixel(deadCells, i - 2, j    )) CreateAnimation(i - 2, j    , 0,  2);
                        else if (GetPixel(deadCells, i + 2, j    )) CreateAnimation(i + 2, j    , 2, -2);
                        else if (GetPixel(deadCells, i - 1, j + 1)) CreateAnimation(i - 1, j + 1, 0, -2);
                        else if (GetPixel(deadCells, i + 1, j + 1)) CreateAnimation(i + 1, j + 1, 2,  2);
                        else if (GetPixel(deadCells, i    , j + 1)) CreateAnimation(i    , j + 1, 0, -3);
                        else if (GetPixel(deadCells, i - 2, j - 1)) CreateAnimation(i - 2, j - 1, 1, -3);
                        else if (GetPixel(deadCells, i + 2, j - 1)) CreateAnimation(i + 2, j - 1, 2, -3);
                        else _nascentAnimations.Add((i, j));
                    }

                }
            }
        }

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (GetPixel(deadCells, i, j))
                    _dyingAnimations.Add((i, j));
            }
        }
    }

    public void DrawAnimation(float t)
    {
        Raylib.DrawRectangle(0, 0, (int)(TriangleWidthInPixels * Scale * (Width + 1)),
            (int)(TriangleHeightInPixels * Scale * (Height)), Color.SKYBLUE);
        Vector2 a, b, c;

        void CalculateTriangle(int i, int j)
        {
            Vector2 center = new Vector2(TriangleWidthInPixels + i * TriangleWidthInPixels,
                TriangleHeightInPixels / 2 + j * TriangleHeightInPixels);
            if ((i + j) % 2 == 0)
            {
                a = center + new Vector2(0, -TriangleHeightInPixels / 2);
                b = center + new Vector2(-TriangleWidthInPixels, TriangleHeightInPixels / 2);
                c = center + new Vector2(TriangleWidthInPixels, TriangleHeightInPixels / 2);
            }
            else
            {
                a = center + new Vector2(0, TriangleHeightInPixels / 2);
                b = center + new Vector2(TriangleWidthInPixels, -TriangleHeightInPixels / 2);
                c = center + new Vector2(-TriangleWidthInPixels, -TriangleHeightInPixels / 2);
            }
        }

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                //if ((i + j) % 2 == 0)
                //{
                //    CalculateTriangle(i, j);
                //    Raylib.DrawTriangleLines(a * Scale, b * Scale, c * Scale, Color.BLACK);
                //}

                if (GetPixel(_backField, i, j) &&
                    GetPixel(_drawField, i, j))
                {
                    CalculateTriangle(i, j);
                    Raylib.DrawTriangle(a * Scale, b * Scale, c * Scale, Color.BLACK);
                }

            }
        }

        foreach (var (i, j) in _dyingAnimations)
        {
            CalculateTriangle(i, j);
            Raylib.DrawTriangle(a * Scale, b * Scale, c * Scale, new Color(0, 0, 0, (int)((1-t) * 255)));
        }

        foreach (var (i,j) in _nascentAnimations)
        {
            CalculateTriangle(i, j);
            Raylib.DrawTriangle(a * Scale, b * Scale, c * Scale, new Color(0, 0, 0, (int)(t * 255)));
        }

        Span<Vector2> v = stackalloc Vector2[3]; 
        foreach (var ra in _rotationAnimations)
        {
            CalculateTriangle(ra.Column, ra.Row);
            
            if ((ra.Column + ra.Row) % 2 == 0)
            {
                v[0] = a;
                v[1] = c;
                v[2] = b;
            }
            else
            {
                v[0] = b;
                v[1] = a;
                v[2] = c;
            }
            var m = Matrix3x2.CreateRotation(-MathF.PI / 3 * ra.NumberOfTurns * t, v[ra.RotationPointNumber]);
            for (int i = 0; i < 3; i++)
            {
                v[i] = Vector2.Transform(v[i], m);
            }

            Raylib.DrawTriangle(v[0] * Scale, v[2] * Scale, v[1] * Scale, Color.BLACK);
        }
    }

    public void FlipPixel(int i, int j)
    {
        int index = i * Height + j;
        int wordIndex = index / 32;
        int bitIndex = index % 32;
        _drawField[wordIndex] ^= (1u << bitIndex);
    }


    public void Step()
    {
        Array.Fill(_backField, 0u);
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                int s = 0;

                if (GetPixel(_drawField, i - 1, j)) s += 12;
                if (GetPixel(_drawField, i + 1, j)) s += 12;
                if (GetPixel(_drawField, i - 1, j - 1)) s += 4;
                if (GetPixel(_drawField, i + 1, j - 1)) s += 4;
                if (GetPixel(_drawField, i - 2, j)) s += 4;
                if (GetPixel(_drawField, i + 2, j)) s += 4;
                if (GetPixel(_drawField, i - 1, j + 1)) s += 4;
                if (GetPixel(_drawField, i + 1, j + 1)) s += 4;

                if ((i + j) % 2 == 0)
                {
                    if (GetPixel(_drawField, i, j + 1)) s += 12;

                    if (GetPixel(_drawField, i, j - 1)) s += 3;
                    if (GetPixel(_drawField, i - 2, j + 1)) s += 3;
                    if (GetPixel(_drawField, i + 2, j + 1)) s += 3;
                }
                else
                {
                    if (GetPixel(_drawField, i, j - 1)) s += 12;

                    if (GetPixel(_drawField, i, j + 1)) s += 3;
                    if (GetPixel(_drawField, i - 2, j - 1)) s += 3;
                    if (GetPixel(_drawField, i + 2, j - 1)) s += 3;
                }

                if (GetPixel(_drawField, i, j))
                {
                    if (s > 13 && s < 32)
                        SetPixel(_backField, i, j);
                }
                else
                {
                    if (s > 21 && s < 32)
                        SetPixel(_backField, i, j);
                }
            }
        }

        Flip();
        Generation++;
    }

    
    public void MatrixStep()
    {
        Array.Fill(_backField, 0u);

        int index = (GetTwoPixels(_drawField, 0, 0) << 4) |
                    (GetTwoPixels(_drawField, 1, 0) << 1);
        for (int i = 0; i < Width; i++)
        {
            index <<= 3;
            index |= (GetTwoPixels(_drawField, i + 2, 0) << 1);
            index &= 0x7FFF;
            index |= (i & 1) << 15;

            if ((SolveMatrix[index >> 3] & (1 << (index & 0x7))) != 0)
            {
                SetPixel(_backField, i, 0);
            }
        }

        for (int j = 1; j < Height - 1; j++)
        {

            index = (GetThreePixels(_drawField, 0, j-1) << 3) |
                        (GetThreePixels(_drawField, 1, j-1) );
            for (int i = 0; i < Width; i++)
            {
                index <<= 3;
                index |= (GetThreePixels(_drawField, i+2, j-1));
                index &= 0x7FFF;
                index |= ((i + j) & 1) << 15;
                
                if ((SolveMatrix[index >> 3] & (1 << (index & 0x7))) != 0)
                {
                    SetPixel(_backField, i, j);
                }
            }
        }
        index = (GetTwoPixels(_drawField, 0, Height - 2) << 3) |
                (GetTwoPixels(_drawField, 1, Height - 2));
        for (int i = 0; i < Width; i++)
        {
            index <<= 3;
            index |= (GetTwoPixels(_drawField, i + 2, Height - 2));
            index &= 0x7FFF;
            index |= ((i + Height - 1) & 1) << 15;

            if ((SolveMatrix[index >> 3] & (1 << (index & 0x7))) != 0)
            {
                SetPixel(_backField, i, Height - 1);
            }
        }
        Flip();
        Generation++;
    }




    public void ParallelStep()
    {
        Array.Fill(_backField, (byte)0);

        int index = (GetTwoPixels(_drawField, 0, 0) << 4) |
                    (GetTwoPixels(_drawField, 1, 0) << 1);
        for (int i = 0; i < Width; i++)
        {
            index <<= 3;
            index |= (GetTwoPixels(_drawField, i + 2, 0) << 1);
            index &= 0x7FFF;
            index |= (i & 1) << 15;

            if ((SolveMatrix[index >> 3] & (1 << (index & 0x7))) != 0)
            {
                SetPixel(_backField, i, 0);
            }
        }

        Parallel.For(1, Height-1, (j) =>
        {
            int indexParallel = (GetThreePixels(_drawField, 0, j - 1) << 3) |
                                (GetThreePixels(_drawField, 1, j - 1));
            for (int i = 0; i < Width; i++)
            {
                indexParallel <<= 3;
                indexParallel |= GetThreePixels(_drawField, i + 2, j - 1);
                indexParallel &= 0x7FFF;
                indexParallel |= ((i + j) & 1) << 15;

                if ((SolveMatrix[indexParallel >> 3] & (1 << (indexParallel & 0x7))) != 0)
                {
                    SetPixel(_backField, i, j);
                }
            }
        });
        index = (GetTwoPixels(_drawField, 0, Height - 2) << 3) |
                (GetTwoPixels(_drawField, 1, Height - 2));
        for (int i = 0; i < Width; i++)
        {
            index <<= 3;
            index |= (GetTwoPixels(_drawField, i + 2, Height - 2));
            index &= 0x7FFF;
            index |= ((i + Height - 1) & 1) << 15;

            if ((SolveMatrix[index >> 3] & (1 << (index & 0x7))) != 0)
            {
                SetPixel(_backField, i, Height - 1);
            }
        }
        Flip();
        Generation++;
    }

    public void NaiveBenchmark()
    {
        Step();
        Flip();
    }

    [Benchmark]
    public void MatrixBenchmark()
    {
        MatrixStep();
        Flip();
    }

    [Benchmark]
    public void ParallelBenchmark()
    {
        ParallelStep();
        Flip(); 
    }
}