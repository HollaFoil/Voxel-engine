using GLFW;
using GlmSharp;
using System.Collections.Generic;
using Voxel_engine.World;

namespace Voxel_engine
{
    internal class InputHandler
    {
        Window window;
        int keyCount = 7;
        float sensitivity = 0.05f;
        vec2 facingChange = new vec2(0,0);
        Dictionary<Keys, int> Keybinds = new Dictionary<Keys, int>();
        Dictionary<int, bool> ActionHeldState = new Dictionary<int, bool>();
        KeyCallback keyCallback;
        MouseCallback mouseCallback;
        MouseCallback scrollCallback;
        Camera camera;
        World.World world;
        float speed = 0.05f;
        int width, height, windowx, windowy;
        public InputHandler(Window window, Camera camera, World.World world)
        {
            keyCallback = KeyCallback;
            mouseCallback = MouseCallback;
            scrollCallback = ScrollCallback;
            this.world = world;
            this.camera = camera;
            InitKeybinds();
            foreach (var Key in possibleKeys) ActionHeldState.Add(Keybinds[Key], false);
            Glfw.SetScrollCallback(window, scrollCallback);
            Glfw.SetKeyCallback(window, keyCallback);
            Glfw.SetCursorPositionCallback(window, mouseCallback);

            
            Glfw.GetWindowSize(window, out width, out height);
            Glfw.GetWindowPosition(window, out windowx, out windowy);
        }
        private void KeyCallback(Window window, Keys key, int scancode, InputState state, ModifierKeys modifiers) {

            if (key == Keys.Q && state == InputState.Press) BreakBlock();

            if (window == null) return;
            if (state == InputState.Repeat || !Keybinds.ContainsKey(key)) return;
            else if (state == InputState.Press) ActionHeldState[Keybinds[key]] = true;
            else ActionHeldState[Keybinds[key]] = false;
        }
        private void BreakBlock()
        {
            //Console.WriteLine("pressed Q");
            Tuple<int, int, int> block = camera.GetFacingBlock(5.0f);
            if (block == null) return;

            (int blockx, int blocky, int blockz) = block;
            world.SetBlock(blockx, blocky, blockz, 0);
            Chunk c = world.GetChunkFromBlockCoords(blockx, blocky, blockz);
            c.SetNotUpdated();
            c.SetNeighboursNotUpdated();

            //Console.WriteLine($"Removed {block}");
        }
        private void ScrollCallback(Window window, double xoffset, double yoffset)
        {
            speed += (float)yoffset / 100.0f;
        }
        private void MouseCallback(Window window, double x, double y)
        {
            if (window == null) return;
            var screen = Glfw.PrimaryMonitor.WorkArea;
            float centerx = ((windowx + width / 2) / (float)screen.Width);
            float centery = ((windowy + height / 2) / (float)screen.Height);
            facingChange += new vec2(centerx*width-(float)x, centery*height-(float)y)*sensitivity;
            Glfw.SetCursorPosition(window, centerx*width, centery*height);
        }
        private void InitKeybinds()
        {
            for (int i = 0; i < keyCount; i++)
            {
                Keybinds.Add(possibleKeys[i], i+1);
            }
        }
        public float GetSpeed()
        {
            return speed;
        }
        public vec3 GetDirection()
        {
            vec3 direction = new vec3(0, 0, 0);
            if (ActionHeldState[(int)Action.Forward]) direction.z += 1f;
            if (ActionHeldState[(int)Action.Backward]) direction.z += -1f;
            if (ActionHeldState[(int)Action.Right]) direction.x += 1f;
            if (ActionHeldState[(int)Action.Left]) direction.x += -1f;
            if (ActionHeldState[(int)Action.Up]) direction.y += 1f;
            if (ActionHeldState[(int)Action.Down]) direction.y += -1f;
            return direction;
        }
        public vec2 UpdateMouse()
        {
            vec2 copy = new vec2(facingChange);
            facingChange *= 0;
            return copy;
        }

        private static Keys[] possibleKeys = { Keys.W, Keys.S, Keys.A, Keys.D
                , Keys.LeftShift, Keys.Space, Keys.Q};
    }
    public enum Action:int
    {
        Forward = 1,
        Backward = 2,
        Left = 3,
        Right = 4,
        Down = 5,
        Up = 6,
        Break = 7,
    }

    
}
