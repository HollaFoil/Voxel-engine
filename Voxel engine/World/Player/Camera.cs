using GlmSharp;

namespace Voxel_engine
{
    internal class Camera
    {
        public float fov = 90.0f;
        public vec3 position = new vec3(0f, 150f, -3f);
        public vec2 yawpitch = new vec2(-90f, 0f);
        public float speed = 0.05f;
        public Camera()
        {
            
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
        public vec3 getDirection()
        {
            float yaw = -yawpitch.x;
            float pitch = yawpitch.y;
            vec3 direction = new vec3();
            direction.x = glm.Cos(glm.Radians(yaw)) * glm.Cos(glm.Radians(pitch));
            direction.y = glm.Sin(glm.Radians(pitch));
            direction.z = glm.Sin(glm.Radians(yaw)) * glm.Cos(glm.Radians(pitch));
            
            return glm.Normalized(direction);
        }
    }
}
