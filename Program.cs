using System.Collections;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Running;
using Raylib_cs;

namespace AttractionZero
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<TriangleLife>();

            TriangleLife ts = new(24, 12);
            
            Raylib.SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT);

            Raylib.InitWindow(1600, 1000, "Hello World");
            bool isEveryFrame = false;
            while (!Raylib.WindowShouldClose())
            {
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_S)) ts.MatrixStep();
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_P)) isEveryFrame = !isEveryFrame;
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE)) ts.Reset();
                
                if (isEveryFrame) ts.MatrixStep();
                
                Raylib.BeginDrawing();
               
                Raylib.ClearBackground(Color.WHITE);
               

                float wheel = Raylib.GetMouseWheelMove();
                ts.Scale *= (1 + wheel * 0.05f);

                

                ts.Draw();

                var v = ts.DrawCursor(Raylib.GetMousePosition());
                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)
                    && v.HasValue)
                {
                    ts.FlipPixel(v.Value.Item1, v.Value.Item2);
                } 
                
                Raylib.DrawText($"Generation: {ts.Generation}", 1300, 770, 25, Color.BLACK);
                Raylib.DrawText("S - 1 Step", 1300, 800, 25, Color.BLACK);
                Raylib.DrawText("P - Step Every Frame on/off", 1300, 830, 25, Color.BLACK);
                Raylib.DrawText("Space - Reset", 1300, 860, 25, Color.BLACK);
                Raylib.DrawFPS(1500, 960);
                
                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}