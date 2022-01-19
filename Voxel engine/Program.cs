using System;
using System.Diagnostics;
using GLFW;
using GlmSharp;
using Voxel_engine;
using Voxel_engine.Render;
using Voxel_engine.World;
using static OpenGL.GL;

class Program
{
    static Stopwatch sw = new Stopwatch();
    /// <summary>
    /// Obligatory name for your first OpenGL example program.
    /// </summary>
    private const string TITLE = "Voxel engine";
    static uint fps = 60;
    static int screenWidth = 1280, screenHeight = 720;
    static uint vao, vbo, ebo;
    static uint[] texIds;
    static Renderer renderer;
    static void outputnoise(int x, int y, float[] map)
    {
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                Console.Write("{0, -4:f} ", map[i * y + j]);
            }
            Console.WriteLine();
        }
    }
    static unsafe void Main(string[] args)
    {
        SizeCallback resizeCallback = WindowSizeCallback;
        PrepareContext();
        var window = CreateWindow(screenWidth, screenHeight);
        

        Noise2d.Reseed();
        World world = new World(40);
        renderer = new Renderer(world.loadedChunks);
        rand = new Random();

        InputHandler inputHandler = new InputHandler(window);
        Camera camera = new Camera();
        int viewLoc = glGetUniformLocation(renderer.GetProgram(), "view");

        

        mat4 projection;
        projection = mat4.Perspective(glm.Radians(90.0f), screenWidth / (float)screenHeight, 0.1f, 600.0f);
        float[] p = projection.ToArray();
        int projectionLoc = glGetUniformLocation(renderer.GetProgram(), "projection");
        glUniformMatrix4fv(projectionLoc, 1, false, p);




        

        sw.Start();



        Glfw.SetWindowSizeCallback(window, resizeCallback);
        sw.Start();
        while (!Glfw.WindowShouldClose(window))
        {
            camera.UpdateYawPitch(inputHandler.UpdateMouse());
            Glfw.PollEvents();
            if (sw.ElapsedMilliseconds < 1000 / fps) continue;
            camera.UpdatePosition(inputHandler.GetDirection(), inputHandler.UpdateMouse(), (1000 / fps) / (float)sw.ElapsedMilliseconds, inputHandler.GetSpeed());
            world.UpdatePosition(camera.position);
            glUniformMatrix4fv(viewLoc, 1, false, (mat4.LookAt(camera.position, camera.position + camera.getDirection(), new vec3(0, 1, 0))).ToArray());
            sw.Restart();

            //world.LoadAndUnloadChunks((int)Math.Floor(camera.position.x / 16.0f), (int)Math.Floor(camera.position.z / 16.0f));
            lock (world.loadedChunks)
            {
                renderer.UpdateChunkBuffers(world.loadedChunks);
            }

            renderer.Flush();

            Glfw.SwapBuffers(window);

            PollErrors();
            
        }

        Glfw.Terminate();
    }
    public static void PollErrors()
    {
        var err = Glfw.GetError(out string description);
        if (!err.Equals(ErrorCode.None)) Console.WriteLine(description);
    }
    private static void PrepareContext()
    {
        Glfw.WindowHint(Hint.ClientApi, ClientApi.OpenGL);
        Glfw.WindowHint(Hint.ContextVersionMajor, 3);
        Glfw.WindowHint(Hint.ContextVersionMinor, 3);
        Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
        Glfw.WindowHint(Hint.Doublebuffer, true);
        Glfw.WindowHint(Hint.Decorated, true);
        Glfw.WindowHint(Hint.OpenglDebugContext, true);
    }
    private static void WindowSizeCallback(GLFW.Window window, int width, int height)
    {
        if (width == 0 || height == 0) return;
        glViewport(0, 0, width, height);
        mat4 projection;
        projection = mat4.Perspective(glm.Radians(90.0f), width / (float)height, 0.1f, 600.0f);
        float[] p = projection.ToArray();
        int projectionLoc = glGetUniformLocation(renderer.GetProgram(), "projection");
        glUniformMatrix4fv(projectionLoc, 1, false, p);
    }
    private static Window CreateWindow(int width, int height)
    {
        var window = Glfw.CreateWindow(width, height, TITLE, GLFW.Monitor.None, Window.None);
        var screen = Glfw.PrimaryMonitor.WorkArea;
        var x = (screen.Width - width) / 2;
        var y = (screen.Height - height) / 2;
        Glfw.SetWindowPosition(window, x, y);

        Glfw.MakeContextCurrent(window);
        Import(Glfw.GetProcAddress);
        glViewport(0, 0, width, height);

        Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Hidden);
        return window;
    }

    private static Random rand;
    static float[] vertices = {
        //Vertices according to faces + texture coordinates
            0f, 0f, 1.0f,             0.0f, 1.0f,
            1.0f, 0f, 1.0f,           1.0f, 1.0f,
            0f, 1.0f, 1.0f,           0.0f, 0.0f,
            1.0f, 1.0f, 1.0f,         1.0f, 0.0f,

            1.0f, 0f, 1.0f,           0.0f, 1.0f,
            1.0f, 0f, 0f,             1.0f, 1.0f,
            1.0f, 1.0f, 1.0f,         0.0f, 0.0f,
            1.0f, 1.0f, 0f,           1.0f, 0.0f,

            1.0f, 0f, 0f,             0.0f, 1.0f,
            0f, 0f, 0f,               1.0f, 1.0f,
            1.0f, 1.0f, 0f,           0.0f, 0.0f,
            0f, 1.0f, 0f,             1.0f, 0.0f,

            0f, 0f, 0f,               0.0f, 1.0f,
            0f, 0f, 1.0f,             1.0f, 1.0f,
            0f, 1.0f, 0f,             0.0f, 0.0f,
            0f, 1.0f, 1.0f,           1.0f, 0.0f,

            0f, 0f, 0f,               0.0f, 1.0f,
            1.0f, 0f, 0f,             1.0f, 1.0f,
            0f, 0f, 1.0f,             0.0f, 0.0f,
            1.0f, 0f, 1.0f,           1.0f, 0.0f,

            0f, 1.0f, 1.0f,           0.0f, 1.0f,
            1.0f, 1.0f, 1.0f,         1.0f, 1.0f,
            0f, 1.0f, 0f,             0.0f, 0.0f,
            1.0f, 1.0f, 0f,           1.0f, 0.0f,
        };
}