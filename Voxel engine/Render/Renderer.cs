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
        List<int> bufferSizes;
        uint texMap;
        List<uint> vbochunks, vaochunks;
        List<bool> bufferAvailability;
        ProgramShaders program;
        public Renderer(List<Chunk> chunks)
        {
            program = new ProgramShaders();
            bufferSizes = new List<int>();
            vbochunks = new List<uint>();
            vaochunks = new List<uint>();
            bufferAvailability = new List<bool>();

            AssignBuffers(chunks);

            CreateTextureMap(out texMap);
            
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

        public void UpdateChunkBuffers(List<Chunk> chunks)
        {
            UpdateChunkAvailability(chunks);
            AssignBuffers(chunks);
        }
        private void AssignBuffers(List<Chunk> chunks) 
        {
            int freeBuffers = 0;
            int index = 0;
            for (int i = 0; i < bufferAvailability.Count; i++) if (bufferAvailability[i]) freeBuffers++;
            for (int i = 0; i < chunks.Count; i++)
            {
                if (chunks[i].bufferID != -1) continue;
                if (freeBuffers == 0)
                {
                    CreateBuffer(out uint vao, out uint vbo);
                    chunks[i].bufferID = (int)vao - 1;
                    BufferChunk(chunks[i], vao);
                }
                for (int j = index; j < bufferAvailability.Count; j++)
                {
                    if (!bufferAvailability[j]) continue;
                    index = j + 1;
                    bufferAvailability[j] = false;
                    chunks[i].bufferID = j;
                    freeBuffers--;
                    BufferChunk(chunks[i], (uint)j + 1);
                }
            }
        }
        private void UpdateChunkAvailability(List<Chunk> chunks)
        {
            for (int i = 0; i < bufferAvailability.Count; i++) bufferAvailability[i] = true;
            for (int i = 0; i < chunks.Count; i++)
            {
                if (chunks[i].bufferID == -1) continue;
                bufferAvailability[chunks[i].bufferID] = false;
            }
        }

        public uint GetProgram()
        {
            return program.program;
        }
        private unsafe void RenderBuffers()
        {
            glBindTexture(GL_TEXTURE_2D, texMap);
            foreach (var buffer in vaochunks)
            {
                glBindVertexArray(buffer);
                glDrawArrays(GL_TRIANGLES, 0, bufferSizes[(int)buffer-1]/sizeOfVertex);
            }
        }
        private unsafe void BufferChunk(Chunk chunk, uint buffer)
        {
            glBindVertexArray(buffer);
            glBindBuffer(GL_ARRAY_BUFFER, buffer);
            byte[] data = ChunkMesh.GenerateMesh(chunk.blockType, chunk.exposedFaces, chunk.x, chunk.y);
            if (bufferSizes[(int)buffer -1] < data.Length)
            {
                fixed (void* ptr = &data[0]) {
                    glBufferData(GL_ARRAY_BUFFER, data.Length, ptr, GL_DYNAMIC_DRAW);
                }
                bufferSizes[(int)buffer - 1] = data.Length;
            }
            else
            {
                glClearBufferfi(GL_ARRAY_BUFFER, 0, bufferSizes[(int)buffer - 1], 0);
                fixed (void* ptr = &data[0])
                {
                    glBufferSubData(GL_ARRAY_BUFFER, 0, data.Length, ptr);
                }
            }
            glBindVertexArray(0);
        }
        private unsafe void CreateBuffer(out uint vao, out uint vbo)
        {
            vao = glGenVertexArray();
            vbo = glGenBuffer();
            vaochunks.Add(vao);
            vbochunks.Add(vbo);
            bufferAvailability.Add(false);
            bufferSizes.Add(-1);

            glBindVertexArray(vao);
            glBindBuffer(GL_ARRAY_BUFFER, vbo);
            glVertexAttribPointer(0, 3, GL_FLOAT, false, sizeOfVertex, NULL);
            glEnableVertexAttribArray(0);
            glVertexAttribPointer(1, 2, GL_FLOAT, false, sizeOfVertex, (void*)(3 * sizeof(float)));
            glEnableVertexAttribArray(1);
            glVertexAttribIPointer(2, 1, GL_UNSIGNED_BYTE, sizeOfVertex, (void*)(5 * sizeof(float)));
            glEnableVertexAttribArray(2);
            //Console.WriteLine(5 * sizeof(float) + sizeof(byte));
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