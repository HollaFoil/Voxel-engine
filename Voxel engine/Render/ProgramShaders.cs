using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenGL.GL;

namespace Voxel_engine.Render
{
    internal class ProgramShaders
    {
        public uint program;
        public Shader[] shaders = new Shader[shaderNames.Length];
        public ProgramShaders()
        {
            program = glCreateProgram();
            for (int i = 0; i < shaderNames.Length; i++)
            {
                shaders[i] = new Shader(shaderTypes[i], path+shaderNames[i]);
                glAttachShader(program, shaders[i].GetShader());
            }
            glLinkProgram(program);
            foreach (var shader in shaders) shader.Free();
            glUseProgram(program);
        }


        static string path = "Shaders/";
        static string[] shaderNames = { "vertex.glsl", "fragment.glsl" };
        static int[] shaderTypes = { GL_VERTEX_SHADER, GL_FRAGMENT_SHADER };
    }
}
