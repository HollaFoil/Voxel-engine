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
        List<int> bufferMaxSizes, bufferedDataSize;
        uint texMap;
        List<uint> vbochunks, vaochunks;
        List<bool> bufferAvailability;

        ProgramShaders program;
        public Renderer(List<Chunk> chunks)
        {
            program = new ProgramShaders();
            bufferMaxSizes = new List<int>();
            bufferedDataSize = new List<int>();
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
                lock (chunks[i])
                {
                    if (chunks[i].bufferID != -1 || !chunks[i].updatedMesh) continue;
                    if (freeBuffers == 0)
                    {
                        CreateBuffer(out uint vao, out uint vbo);
                        chunks[i].bufferID = (int)vao;
                        BufferChunk(chunks[i], (uint)chunks[i].bufferID);
                        continue;
                    }
                    for (int j = index; j < bufferAvailability.Count; j++)
                    {
                        if (!bufferAvailability[j]) continue;
                        index = j + 1;
                        bufferAvailability[j] = false;
                        chunks[i].bufferID = j + 1;
                        freeBuffers--;
                        BufferChunk(chunks[i], (uint)chunks[i].bufferID);
                        break;
                    }
                }
            }
            foreach (var c in chunks)
            {
                lock (c)
                {
                    if (c.bufferedMesh || !c.updatedMesh) continue;
                    BufferChunk(c, (uint)c.bufferID);
                }
            }
        }
        private void UpdateChunkAvailability(List<Chunk> chunks)
        {
            for (int i = 0; i < bufferAvailability.Count; i++) bufferAvailability[i] = true;
            for (int i = 0; i < chunks.Count; i++)
            {
                if (chunks[i].bufferID == -1) continue;
                bufferAvailability[chunks[i].bufferID - 1] = false;
            }
            for (int i = 0; i < bufferAvailability.Count; i++)
            {
                if (bufferAvailability[i]) bufferedDataSize[i] = 0;
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
                if (bufferAvailability[(int)buffer - 1]) continue;
                glBindVertexArray(buffer);
                glDrawArrays(GL_TRIANGLES, 0, bufferedDataSize[(int)buffer - 1] / sizeOfVertex);
            }
        }
        private unsafe void BufferChunk(Chunk chunk, uint buffer)
        {
            
            byte[] data = chunk.data;
            if (data == null || chunk.dataLength == 0 || chunk.bufferID == -1) return;
            glBindBuffer(GL_ARRAY_BUFFER, buffer);
            if (chunk.dataLength < bufferMaxSizes[(int)buffer - 1])
            {
                fixed (void* ptr = &data[0])
                {
                    glBufferSubData(GL_ARRAY_BUFFER, 0, chunk.dataLength, ptr);
                }
                chunk.bufferedMesh = true;
                bufferedDataSize[(int)buffer - 1] = chunk.dataLength;
                return;
            }
            fixed (void* ptr = &data[0])
            {
                glBufferData(GL_ARRAY_BUFFER, chunk.dataLength, ptr, GL_DYNAMIC_DRAW);
            }
            chunk.bufferedMesh = true;
            bufferMaxSizes[(int)buffer - 1] = chunk.dataLength;
            bufferedDataSize[(int)buffer - 1] = chunk.dataLength;
        }
        private unsafe void CreateBuffer(out uint vao, out uint vbo)
        {
            vao = glGenVertexArray();
            vbo = glGenBuffer();
            vaochunks.Add(vao);
            vbochunks.Add(vbo);
            bufferAvailability.Add(false);
            bufferMaxSizes.Add(-1);
            bufferedDataSize.Add(0);

            glBindVertexArray(vao);
            glBindBuffer(GL_ARRAY_BUFFER, vbo);
            glVertexAttribIPointer(0, 1, GL_UNSIGNED_BYTE, sizeOfVertex, NULL);
            glEnableVertexAttribArray(0);
            glVertexAttribIPointer(1, 1, GL_UNSIGNED_BYTE, sizeOfVertex, (void*)(sizeof(byte)));
            glEnableVertexAttribArray(1);
            glVertexAttribIPointer(2, 1, GL_UNSIGNED_BYTE, sizeOfVertex, (void*)(2 * sizeof(byte)));
            glEnableVertexAttribArray(2);
            glVertexAttribIPointer(3, 1, GL_UNSIGNED_BYTE, sizeOfVertex, (void*)(3 * sizeof(byte)));
            glEnableVertexAttribArray(3);
            glVertexAttribIPointer(4, 1, GL_UNSIGNED_BYTE, sizeOfVertex, (void*)(4 * sizeof(byte)));
            glEnableVertexAttribArray(4);
            glVertexAttribIPointer(5, 1, GL_UNSIGNED_BYTE, sizeOfVertex, (void*)(5 * sizeof(byte)));
            glEnableVertexAttribArray(5);
            glVertexAttribIPointer(6, 1, GL_INT, sizeOfVertex, (void*)(6 * sizeof(byte)));
            glEnableVertexAttribArray(6);
            glVertexAttribIPointer(7, 1, GL_INT, sizeOfVertex, (void*)(6 * sizeof(byte) + sizeof(int)));
            glEnableVertexAttribArray(7);
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

        public const int sizeOfVertex = 2 * sizeof(int) + 6 * sizeof(byte);
    }
}