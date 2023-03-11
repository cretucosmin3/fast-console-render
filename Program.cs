using System.Diagnostics;
namespace Console_Render;

static class Program
{
    // Controls
    private static double v;
    private static double speed = 5f;
    private static bool pause;

    // Rendering
    private static readonly List<(int x, int y)> DrawPoints = new();
    private static readonly Stopwatch timer = new();
    private static readonly Stopwatch frameTimer = new();
    private static bool renderControlChange = true;
    private static int frameWait = 17;
    private static int fpsTarget = 60;
    const double speedIncrement = 0.25d;
    static int lastFps = 0;

    private static readonly List<(double x, double y)> PathPoints = new()
    {
        new(5, 5),
        new(50, 5),
        new(50, 20),
        new(5, 20),
        new(5, 5),
    };

    static void Main()
    {
        DisplayControls();
        Init();
        RunLoop();
    }

    private static void Init()
    {
        Console.CursorVisible = false;

        Enumerable.Range(0, 100).ToList().ForEach(
                _ => PathPoints.Add(
                    (
                        new Random().Next(10, 70),
                        new Random().Next(5, 20)
                    )
                )
            );

        new Thread(RunControlsLoop).Start();
    }

    private static void RunControlsLoop()
    {
        while (true)
        {
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.Spacebar:
                    pause = !pause;
                    break;

                case ConsoleKey.C:
                    Console.Clear();
                    renderControlChange = true;
                    break;

                case ConsoleKey.LeftArrow:
                    speed = Math.Clamp(speed - speedIncrement, 0.1, 60);
                    renderControlChange = true;
                    break;

                case ConsoleKey.RightArrow:
                    speed = Math.Clamp(speed + speedIncrement, 0.1, 60);
                    renderControlChange = true;
                    break;

                case ConsoleKey.UpArrow:
                    fpsTarget++;
                    renderControlChange = true;
                    break;

                case ConsoleKey.DownArrow:
                    fpsTarget--;
                    renderControlChange = true;
                    break;
            }

            fpsTarget = Math.Max(fpsTarget, 15);
            frameWait = (int)Math.Ceiling(1000f / fpsTarget);
        }
    }

    private static void DisplayControls()
    {
        MoveCursor(4, 2);
        Draw("{C} - refresh console", false);

        MoveCursor(4, 3);
        Draw("{Spacebar} - pause animation", false);

        MoveCursor(4, 4);
        Draw("{Left, Right} - adjust speed", false);

        MoveCursor(4, 5);
        Draw("{Up, Down} - adjust FPS", false);

        MoveCursor(4, 7);
        Draw("Press any key to continue...", false);

        Console.ReadKey();
        Console.Clear();
    }

    private static void RunLoop()
    {
        int cPath = 0;
        int nPath = 1;
        double i = 0d;
        int frames = 0;

        int prevX = 0;
        int prevY = 0;

        timer.Restart();
        while (true)
        {
            frameTimer.Restart();
            frames++;

            if (!pause)
                i += speed;

            v = Math.Sin(Math.PI / 2d * i / 180d);

            if (renderControlChange)
            {
                MoveCursor(13, 0);
                Draw($"speed = {speed:0.00}", false);
                renderControlChange = false;
            }

            int x = Norm(v, PathPoints[cPath].x, PathPoints[nPath].x);
            int y = Norm(v, PathPoints[cPath].y, PathPoints[nPath].y);

            bool posChanged = !(prevX == x && prevY == y);

            if (posChanged)
            {
                ClearPrevious();
                DrawBox(x, y);
                prevX = x; prevY = y;
            }


            if (pause)
            {
                MoveCursor(x - 1, y - 2);

                Console.ForegroundColor = ConsoleColor.Red;

                Draw($"Paused!");

                Console.ForegroundColor = ConsoleColor.White;

                Thread.Sleep(100);
                continue;
            }

            while (frameTimer.ElapsedMilliseconds < frameWait) { }

            if (i > 180d)
            {
                i = 0d;
                cPath++;
                nPath++;
            }

            if (nPath == PathPoints.Count)
            {
                cPath = 0; nPath = 1;
            }

            if (frames >= 15)
            {
                timer.Stop();
                ShowFps();
                frames = 0;

                timer.Restart();
            }
        }
    }

    static void ShowFps()
    {
        int fps = (int)(1000f / (timer.ElapsedMilliseconds / 15f));

        if (fps == lastFps) return;

        MoveCursor(1, 0);

        Console.BackgroundColor = ConsoleColor.Green;
        Console.ForegroundColor = ConsoleColor.Black;

        Draw($" FPS {fps} ", false);

        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.Black;

        lastFps = fps;
    }

    static void MoveCursor(int x, int y)
    {
        Console.CursorLeft = x;
        Console.CursorTop = y;
    }

    static void ClearPrevious()
    {
        for (int i = 0; i < DrawPoints.Count; i++)
        {
            MoveCursor(DrawPoints[i].x, DrawPoints[i].y);
            Console.Write(' ');
        }

        DrawPoints.Clear();
    }

    static void Draw(string text, bool storeDrawPoints = true)
    {
        if (storeDrawPoints)
        {
            int x = Console.CursorLeft;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != ' ')
                    DrawPoints.Add(new(x, Console.CursorTop));

                x++;
            }
        }

        Console.Write(text);
    }

    static int Norm(double val, double min, double max)
    {
        return (int)Math.Round(min + val * (max - min) / 1d);
    }

    static void DrawBox(int x, int y)
    {
        MoveCursor(x, y);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Draw("┌───┐");

        MoveCursor(Console.CursorLeft - 5, Console.CursorTop + 1);

        Draw("│   │");

        MoveCursor(Console.CursorLeft - 5, Console.CursorTop + 1);

        Draw("└───┘");
        Console.ForegroundColor = ConsoleColor.White;
    }
}