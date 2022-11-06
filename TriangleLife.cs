using System.Numerics;
using BenchmarkDotNet.Attributes;
using Raylib_cs;

namespace AttractionZero;

public class TriangleLife
{
    private static byte[] _solveMatrix;

    static TriangleLife()
    {
        const int numberOfSolutions = (1 << 16);
        _solveMatrix = new byte[(1 << (16 - 3))];
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
                    _solveMatrix[i / 8] |= (byte)(1 << (i % 8));
            }
            else
            {
                if (s > 21 && s < 32)
                    _solveMatrix[i / 8] |= (byte)(1 << (i % 8));
            }
        }
    }



    private uint[] _backField;
    private uint[] _drawField;

    public readonly float TriangleWidthInPixels = 41f/5;
    public readonly float TriangleHeightInPixels = 71f/5;


    public int Width { get; }
    public int Height { get; }


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
    }

    private void Flip() => (_drawField, _backField) = (_backField, _drawField);

    public void Reset()
    {
        unsafe
        {
            fixed (uint* ptr = _drawField)
            {
                Random.Shared.NextBytes(new Span<byte>(ptr, _drawField.Length * 4));
            }
        }
    }

    private bool GetPixel(uint[] field, int i, int j)
    {
        if (i < 0 || i >= Width || j < 0 || j >= Height)
            return false;
        int index = i * Height + j;
        int wordIndex = index / 32;
        int bitIndex = index % 32;
        return (field[wordIndex] & (1 << bitIndex)) != 0;
    }

    
    private void SetPixel(uint[] field, int i, int j)
    {
        int index = i * Height + j;
        int wordIndex = index / 32;
        int bitIndex = index % 32;
        Interlocked.Or(ref field[wordIndex], (1u << bitIndex));
    }

    private int GetThreePixels(uint[] field, int si, int sj)
    {
        if (si < 0 || si >= Width) return 0;

        int startIndex = sj + si * Height;
        uint startByte = (sj < 0) ? 0 : field[startIndex / 32];
        int startBitIndex = startIndex % 32;
        if (startBitIndex < 30)
        {
            return (int)((startByte >> startBitIndex) & 0b111);
        }
        else
        {
            int stopIndex = sj + 2 + si * Height;
            uint stopByte = (sj + 2 >= Height) ? 0 : field[stopIndex / 32];
            return (int)(((stopByte << (32 - startBitIndex)) & 0b110) | (startByte >> startBitIndex));
        }
    }

    public void Draw()
    {

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
                }
                else
                {
                    a = center + new Vector2(0, TriangleHeightInPixels / 2);
                    b = center + new Vector2(TriangleWidthInPixels, -TriangleHeightInPixels / 2);
                    c = center + new Vector2(-TriangleWidthInPixels, -TriangleHeightInPixels / 2);
                }
                if (GetPixel(_drawField, i,j))
                {
                    Raylib.DrawTriangle(a, b, c, Color.BLACK);
                }
                else
                {
                    
                    // Raylib.DrawTriangleLines(a, b, c, Color.BLACK);
                }
            }
        }
            
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
    }

    
    public void MatrixStep()
    {
        Array.Fill(_backField, (byte)0);
        
         
        for (int j = 0; j < Height; j++)
        {

            int index = (GetThreePixels(_drawField, 0, j-1) << 3) |
                        (GetThreePixels(_drawField, 1, j-1) );
            for (int i = 0; i < Width; i++)
            {
                index <<= 3;
                index |= (GetThreePixels(_drawField, i+2, j-1));
                index &= 0x7FFF;
                index |= ((i + j) & 1) << 15;
                
                int byteIndex = index / 8;
                int bitIndex = index % 8;
                if ((_solveMatrix[byteIndex] & (1 << bitIndex)) != 0)
                {
                    SetPixel(_backField, i, j);
                }
            }
        }

        Flip();
    }



    public void ParallelStep()
    {
        Array.Fill(_backField, (byte)0);
        Parallel.For(0, Height, (j) =>
        {
            int index = (GetThreePixels(_drawField, 0, j - 1) << 3) |
                        (GetThreePixels(_drawField, 1, j - 1));
            for (int i = 0; i < Width; i++)
            {
                index <<= 3;
                index |= (GetThreePixels(_drawField, i + 2, j - 1));
                index &= 0x7FFF;
                index |= ((i + j) & 1) << 15;

                int byteIndex = index / 8;
                int bitIndex = index % 8;
                if ((_solveMatrix[byteIndex] & (1 << bitIndex)) != 0)
                {
                    SetPixel(_backField, i, j);
                }
            }
        });

        Flip();
    }


   
    public void NaiveBenchmark()
    {
        Step();
    }

    [Benchmark]
    public void MatrixBenchmark()
    {
        MatrixStep();
    }

    [Benchmark]
    public void ParallelBenchmark()
    {
        ParallelStep();
    }
}