using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GLFW;
using GlmSharp;
using Voxel_engine;
using Voxel_engine.Render;
using Voxel_engine.World;
using Voxel_engine.World.Generation;
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
    public static World world;
    static unsafe void Main(string[] args)
    {
        SizeCallback resizeCallback = WindowSizeCallback;
        PrepareContext();
        var window = CreateWindow(screenWidth, screenHeight);
        

        Noise2d.Reseed();
        world = new World(7, 5);
        renderer = new Renderer(world.loadedChunks);
        rand = new Random();
        Camera camera = new Camera(world);
        InputHandler inputHandler = new InputHandler(window, camera, world);

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
            glUniformMatrix4fv(viewLoc, 1, false, (mat4.LookAt(camera.position, camera.position + camera.GetDirection(), new vec3(0, 1, 0))).ToArray());
            sw.Restart();
            lock (world.loadedChunks)
            {
                renderer.UpdateChunkBuffers(world.loadedChunks);
            }
            renderer.Flush();
            Glfw.SwapBuffers(window);
            //Console.WriteLine(camera.position);
            PollErrors();
        }
        world.TerminateThread();
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