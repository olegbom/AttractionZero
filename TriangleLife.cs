using System.Numerics;
using BenchmarkDotNet.Attributes;
using Raylib_cs;

namespace AttractionZero;

public class TriangleLife
{
    private byte[] _backField;
    private byte[] _drawField;

    public const float TriangleWidthInPixels = 41f/5;
    public const float TriangleHeightInPixels = 71f/5;


    public int Width { get; }
    public int Height { get; }


    public TriangleLife() : this(1000, 1000)
    {

    }

    public TriangleLife(int width, int height)
    {
        Width = width;
        Height = height;
        int numberOfBytes = (int)((width * height) / 8);
        if ((width * height) % 8 != 0)
            numberOfBytes += 1;

        _backField = new byte[numberOfBytes];
        _drawField = new byte[numberOfBytes];
        Random.Shared.NextBytes(_backField);
        _backField.CopyTo(_drawField, 0);
    }

    private void Flip() => (_drawField, _backField) = (_backField, _drawField);

    private bool GetPixel(byte[] field, int i, int j)
    {
        if (i < 0 || i >= Width || j < 0 || j >= Height)
            return false;
        int index = i * Height + j;
        int byteIndex = index / 8;
        int bitIndex = index % 8;
        return (field[byteIndex] & (1 << bitIndex)) != 0;
    }

    
    private void SetPixel(byte[] field, int i, int j)
    {
        int index = i * Height + j;
        int byteIndex = index / 8;
        int bitIndex = index % 8;
        field[byteIndex] |= (byte)(1 << bitIndex);
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

    public void Reset()
    {
        Random.Shared.NextBytes(_drawField);
    }
    public void Step()
    {
        Array.Fill(_backField, (byte)0);
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

    public void ParallelStep()
    {
        Array.Fill(_backField, (byte)0);
        Parallel.For(0, Width, (i) =>
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
        });

        Flip();
    }


    [Benchmark]
    public void NaiveBenchmark()
    {
        Step();
    }
    [Benchmark]
    public void ParallelBenchmark()
    {
        ParallelStep();
    }
}