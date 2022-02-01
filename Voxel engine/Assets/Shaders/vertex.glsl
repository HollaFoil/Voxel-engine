#version 330 core
layout (location = 0) in int x;
layout (location = 1) in int y;
layout (location = 2) in int z;
layout (location = 3) in int vertexID;
layout (location = 4) in int texID;
layout (location = 5) in int AOin;
layout (location = 6) in int chunkx;
layout (location = 7) in int chunky;

out vec2 tex;
flat out int id;
out float AO;
uniform mat4 view;
uniform mat4 projection;
void main()
{
	float vertices[120] =  float[](
            0, 0, 1.0 ,               0.0 , 1.0 ,
            1.0 , 0 , 1.0 ,           1.0 , 1.0 ,
            0 , 1.0 , 1.0 ,           0.0 , 0.0 ,
            1.0 , 1.0 , 1.0 ,         1.0 , 0.0 ,

            1.0 , 0 , 1.0 ,           0.0 , 1.0 ,
            1.0 , 0 , 0 ,             1.0 , 1.0 ,
            1.0 , 1.0 , 1.0 ,         0.0 , 0.0 ,
            1.0 , 1.0 , 0 ,           1.0 , 0.0 ,

            1.0 , 0 , 0 ,             0.0 , 1.0 ,
            0 , 0 , 0 ,               1.0 , 1.0 ,
            1.0 , 1.0 , 0 ,           0.0 , 0.0 ,
            0 , 1.0 , 0 ,             1.0 , 0.0 ,

            0 , 0 , 0 ,               0.0 , 1.0 ,
            0 , 0 , 1.0 ,             1.0 , 1.0 ,
            0 , 1.0 , 0 ,             0.0 , 0.0 ,
            0 , 1.0 , 1.0 ,           1.0 , 0.0 ,

            0 , 0 , 0 ,               0.0 , 1.0 ,
            1.0 , 0 , 0 ,             1.0 , 1.0 ,
            0 , 0 , 1.0 ,             0.0 , 0.0 ,
            1.0 , 0 , 1.0 ,           1.0 , 0.0 ,

            0 , 1.0 , 1.0 ,           0.0 , 1.0 ,
            1.0 , 1.0 , 1.0 ,         1.0 , 1.0 ,
            0 , 1.0 , 0 ,             0.0 , 0.0 ,
            1.0 , 1.0 , 0 ,           1.0 , 0.0
        );
		//gl_Position = projection * view * vec4 (1.0, 1.0, 1.0, 1.0);
	vec4 position = vec4(float(x) + float(chunkx) * 16.0 + vertices[vertexID*5], float(y) + vertices[vertexID*5+1], float(z) + float(chunky) * 16.0 + vertices[vertexID*5+2], 1.0);
	gl_Position = projection * view * position;
	tex = vec2(vertices[vertexID*5+3], vertices[vertexID*5+4]);
	id = texID;
    AO = float(AOin);
	//tex = vec2(1.0, 1.0);
}
