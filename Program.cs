using System.Collections;
using System.Runtime.CompilerServices;
using Raylib_cs;

namespace AttractionZero
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TriangleLife ts = new(25*5, 12*5);
            Raylib.SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT);

            Raylib.InitWindow(1600, 1000, "Hello World");
            bool isEveryFrame = false;
            while (!Raylib.WindowShouldClose())
            {
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_S)) ts.Step();
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_P)) isEveryFrame = !isEveryFrame;
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE)) ts = new(25 * 5, 12 * 5);
                
                if (isEveryFrame) ts.Step();
                
                Raylib.BeginDrawing();
               
                Raylib.ClearBackground(Color.WHITE);
                Raylib.DrawText("S - 1 Step", 1300, 800, 25, Color.BLACK);
                Raylib.DrawText("P - Step Every Frame on/off", 1300, 830, 25, Color.BLACK);
                Raylib.DrawText("Space - Reset", 1300, 860, 25, Color.BLACK);
                Raylib.DrawFPS(1500, 960);
                ts.Draw();

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}