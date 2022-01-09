using GLFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Voxel_engine.World;
using Voxel_engine.World.Generation;
using static OpenGL.GL;

namespace Voxel_engine.Render
{
 
    public class Renderer
    {
        Random rand = new();
        int[] bufferSizes;
        int renderDistance;
        uint texMap;
        uint[] vbo, vao;
        Chunk[,] chunks;
        ProgramShaders program;
        public Renderer(int distance)
        {
            chunks = new Chunk[distance,distance];
            renderDistance = distance;
            program = new ProgramShaders();
            CreateTextureMap(out texMap);

            CreateBuffers();
            PopulateChunks();
            
            glEnable(GL_DEPTH_TEST);
            glDepthFunc(GL_LESS);
            glClearColor(0.5f, 0.95f, 1.0f, 1.0f);

            glEnable(GL_CULL_FACE);
            glCullFace(GL_BACK);
            Console.WriteLine(bufferSizes[0]);
        }
        public void Flush()
        {
            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
            RenderBuffers();
          
        }
        public uint GetProgram()
        {
            return program.program;
        }
        private unsafe void RenderBuffers()
        {
            glBindTexture(GL_TEXTURE_2D, texMap);
            foreach (var buffer in vao)
            {
                glBindVertexArray(buffer);
                glDrawArrays(GL_TRIANGLES, 0, bufferSizes[buffer-1]/sizeOfVertex);
            }
        }
        private void PopulateChunks()
        {
            for (int i = 0; i < renderDistance; i++)
            {
                for (int j = 0; j < renderDistance; j++)
                {
                    chunks[i, j] = ChunkGenerator.GenerateChunk(i- renderDistance / 2, j - renderDistance / 2);
                }
            }
            for (int i = 0; i < renderDistance; i++)
            {
                for (int j = 0; j < renderDistance; j++)
                {
                    Chunk up = null, down = null, left = null, right = null;

                    if (i < renderDistance - 1) left = chunks[i + 1, j];
                    if (i > 0) right = chunks[i - 1, j];
                    if (j > 0) up = chunks[i, j - 1];
                    if (j < renderDistance - 1) down = chunks[i, j + 1];
                    chunks[i, j].UpdateExposedFaces(right, left, down, up);
                    BufferChunk(chunks[i, j], (uint)(i * renderDistance + j) + 1);
                }
            }
        }
        private unsafe void BufferChunk(Chunk chunk, uint buffer)
        {
            glBindVertexArray(buffer);
            glBindBuffer(GL_ARRAY_BUFFER, buffer);
            byte[] data = ChunkMesh.GenerateMesh(chunk.blockType, chunk.exposedFaces, chunk.x, chunk.y);
            if (bufferSizes[buffer-1] < data.Length)
            {
                fixed (void* ptr = &data[0]) {
                    glBufferData(GL_ARRAY_BUFFER, data.Length, ptr, GL_DYNAMIC_DRAW);
                }
                bufferSizes[buffer - 1] = data.Length;
            }
            else
            {
                glClearBufferfi(GL_ARRAY_BUFFER, 0, bufferSizes[buffer - 1], 0);
                fixed (void* ptr = &data[0])
                {
                    glBufferSubData(GL_ARRAY_BUFFER, 0, data.Length, ptr);
                }
            }
            glBindVertexArray(0);
        }
        private unsafe void CreateBuffers()
        {
            vao = glGenVertexArrays(renderDistance * renderDistance);
            vbo = glGenBuffers(renderDistance * renderDistance);
            bufferSizes = new int[renderDistance * renderDistance];
            for (int i = 0; i < renderDistance * renderDistance; i++)
            {
                glBindVertexArray(vao[i]);
                glBindBuffer(GL_ARRAY_BUFFER, vbo[i]);
                glVertexAttribPointer(0, 3, GL_FLOAT, false, sizeOfVertex, NULL);
                glEnableVertexAttribArray(0);
                glVertexAttribPointer(1, 2, GL_FLOAT, false, sizeOfVertex, (void*)(3 * sizeof(float)));
                glEnableVertexAttribArray(1);
                glVertexAttribIPointer(2, 1, GL_UNSIGNED_BYTE, sizeOfVertex, (void*)(5 * sizeof(float)));
                glEnableVertexAttribArray(2);
            }

            Console.WriteLine(5 * sizeof(float) + sizeof(byte));
            /*glVertexAttribPointer(0, 3, GL_FLOAT, false, 5 * sizeof(float) + sizeof(byte), NULL);
            glEnableVertexAttribArray(0);
            glVertexAttribPointer(1, 2, GL_FLOAT, false, 5 * sizeof(float) + sizeof(byte), (void*)(3 * sizeof(float)));
            glEnableVertexAttribArray(1);
            glVertexAttribPointer(2, 1, GL_BYTE, false, 5 * sizeof(float) + sizeof(byte), (void*)(5 * sizeof(float)));
            glEnableVertexAttribArray(2);*/
        }
        private unsafe void CreateTextureMap(out uint id)
        {
            id = glGenTexture();
            byte[] tex = TextureUtils.LoadTexture(0, out int width, out int height);
            glBindTexture(GL_TEXTURE_2D, id);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
            fixed (byte* ptr = &tex[0])
            {
                glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA8, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, ptr);
            }
            glGenerateMipmap(GL_TEXTURE_2D);
        }

        public const int sizeOfVertex = 5 * sizeof(float) + sizeof(byte);
    }
}