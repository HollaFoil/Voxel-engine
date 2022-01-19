#version 330 core
out vec4 result;                                                                                     
in vec2 tex;   
flat in int id;                          
uniform sampler2D uTexture;                                             
void main()                                                
{                           
	vec2 coords = vec2((tex.x*16.0 + (float(id)-1)*16.0)/ (16.0*7.0), tex.y);
	result = texture(uTexture, coords);
} 
//(tex.x*16.0 + (float(id)-1)*16.0)/ (16.0*6.0)