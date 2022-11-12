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
           // var summary = BenchmarkRunner.Run<TriangleLife>();

            TriangleLife ts = new(71*3, 41*3)
            {
                Scale = 0.55f/3
            };
            Raylib.SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT);

            Raylib.InitWindow(1600, 1000, "Hello World");
            Raylib.SetTargetFPS(60);
            
            bool isEveryFrame = false;
            bool isAnimatedEveryFrame = true;
            const int animationFramesMax = 60;
            int animationCounter = 0;

            while (!Raylib.WindowShouldClose())
            {
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_S))
                {
                    ts.MatrixStep();
                    ts.AnimationPreparation();
                    animationCounter = animationFramesMax;
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_P)) isEveryFrame = !isEveryFrame;
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_A)) isAnimatedEveryFrame = !isAnimatedEveryFrame;
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_R))
                {
                    ts.SetRandomRectangleAlternative(0.3, 0.3, 0.4, 0.4);
                }
                if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE)) ts.Reset();

                if (isEveryFrame)
                {
                    animationCounter = 0;
                    ts.MatrixStep();
                }


                Raylib.BeginDrawing();
               
                Raylib.ClearBackground(Color.WHITE);
               

                float wheel = Raylib.GetMouseWheelMove();
                ts.Scale *= (1 + wheel * 0.05f);
                
                
                if (animationCounter != 0)
                {
                    animationCounter--;
                    float t = 1.0f - (float)animationCounter / animationFramesMax;
                    float t2 = t * t;
                    ts.DrawAnimation(t*t2*(6*t2 - 15*t + 10));
                    //ts.DrawAnimation(t);
                }
                else
                {
                    ts.Draw();
                    if (!isEveryFrame && isAnimatedEveryFrame)
                    {
                        ts.MatrixStep();
                        ts.AnimationPreparation();
                        animationCounter = animationFramesMax;
                    }
                }


                var v = ts.DrawCursor(Raylib.GetMousePosition());
                if (v.HasValue )
                {
                    if(Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
                    {
                        ts.SetPixel(v.Value.Item1, v.Value.Item2);
                    }
                    else if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
                    {
                        ts.ResetPixel(v.Value.Item1, v.Value.Item2);
                    }
                } 
                
                Raylib.DrawText($"Generation: {ts.Generation}", 1300, 770, 25, Color.BLACK);
                Raylib.DrawText("S - 1 Step", 1300, 800, 25, Color.BLACK);
                Raylib.DrawText("P - Step Every Frame", 1300, 830, 25, Color.BLACK);
                Raylib.DrawText("A - Animation Every Frame", 1300, 860, 25, Color.BLACK);
                Raylib.DrawText("Space - Reset", 1300, 890, 25, Color.BLACK);
                Raylib.DrawText("LMB - Set Cell", 1300, 920, 25, Color.BLACK);
                Raylib.DrawText("RMB - Reset Cell", 1300, 950, 25, Color.BLACK);
                Raylib.DrawFPS(1500, 980);
                
                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}