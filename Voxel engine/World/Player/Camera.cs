using GlmSharp;
using System.Collections.Generic;
using Voxel_engine.World;

namespace Voxel_engine
{
    internal class Camera
    {
        public float fov = 90.0f;
        public vec3 position = new vec3(0f, 150f, -3f);
        public vec2 yawpitch = new vec2(-90f, 0f);
        public float speed = 0.05f;
        private World.World world;
        public Camera(World.World w)
        {
            world = w;
        }
        public void UpdateYawPitch(vec2 change)
        {
            yawpitch = (yawpitch + change);
            if (yawpitch.y > 89.99f) yawpitch.y = 89.99f;
            if (yawpitch.y < -89.99f) yawpitch.y = -89.99f;
            if (yawpitch.x > 180f) yawpitch.x -= 360;
            if (yawpitch.x < -180f) yawpitch.x += 360;
        }
        public void UpdatePosition(vec3 direction, vec2 change, float modifier, float speed)
        {
            this.speed = speed;
            UpdateYawPitch(change);
            position.y += direction.y*speed * modifier;
            position.z += (glm.Sin(glm.Radians(yawpitch.x+180))*direction.z + 
                           glm.Cos(glm.Radians(yawpitch.x))*direction.x) * speed * modifier;
            position.x += (glm.Sin(glm.Radians(yawpitch.x))*direction.x + 
                           glm.Cos(glm.Radians(yawpitch.x))*direction.z) * speed * modifier;
        }
        public vec3 GetDirection()
        {
            float yaw = -yawpitch.x;
            float pitch = yawpitch.y;
            vec3 direction = new vec3();
            direction.x = glm.Cos(glm.Radians(yaw)) * glm.Cos(glm.Radians(pitch));
            direction.y = glm.Sin(glm.Radians(pitch));
            direction.z = glm.Sin(glm.Radians(yaw)) * glm.Cos(glm.Radians(pitch));
            
            return glm.Normalized(direction);
        }

        private static float NextFaceHit(float x, float dx)
        {
            if (dx == 0.0f) dx = 1.0f;
            dx = dx / Math.Abs(dx);
            
            float frac = (float)(x - Math.Truncate(x));
            if(frac < 0.0f) frac += 1.0f;

            float ret;
            if(dx > 0) ret = 1.0f - frac;
            else ret = -frac;

            if (Math.Abs(ret) <= 1e-6f) return dx;
            else return ret;
        }

        private static float GetMagnitude(float f)
        {
            if (f >= 0.0f) return 1.0f;
            else return -1.0f;
        }

        private static vec3 GetMagnitudes(vec3 direction)
        {
            return new vec3(
                    GetMagnitude(direction.x),
                    GetMagnitude(direction.y),
                    GetMagnitude(direction.z)
                );
        }

        public Tuple<int, int, int> GetFacingBlock(float maxDist) {
            vec3 direction = GetDirection();
            vec3 magnitudes = GetMagnitudes(direction);
            if (direction.x == 0.0f) direction.x = 1e-6f;
            if (direction.y == 0.0f) direction.y = 1e-6f;
            if (direction.z == 0.0f) direction.z = 1e-6f;

            vec3 currPos = new vec3(position);
            Console.WriteLine($"Starting position {position}");
            while (vec3.Distance(currPos, position) <= maxDist)
            {
                float moveX = NextFaceHit(currPos.x, magnitudes.x) / direction.x;
                float moveY = NextFaceHit(currPos.y, magnitudes.y) / direction.y;
                float moveZ = NextFaceHit(currPos.z, magnitudes.z) / direction.z;
                float move = Math.Min(Math.Min(moveX, moveY), moveZ);
                vec3 middlePos = currPos + direction * move * .5f;
                currPos += direction * move;

                int blockx = (int)Math.Floor(middlePos.x);
                int blocky = (int)Math.Floor(middlePos.y);
                int blockz = (int)Math.Floor(middlePos.z);

                Console.WriteLine($"Block {blockx} {blocky} {blockz} currPos {currPos}");


                byte? block = world.GetBlockType(blockx, blocky, blockz);

                if (block != null && block != 0) return new Tuple<int, int, int>(blockx, blocky, blockz);
            }

            return null;
        }
    }
}
