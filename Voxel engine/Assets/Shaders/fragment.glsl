#version 330 core
out vec4 result;                                                                                     
in vec2 tex;   
flat in int id;           
in float AO;
uniform sampler2D uTexture;                                             
void main()                                                
{               
	float a = (AO+2) / 5;
	vec2 coords = vec2((tex.x*16.0 + (float(id)-1)*16.0)/ (16.0*10.0), tex.y);
	vec4 texColor = texture(uTexture, coords) * vec4(a,a,a,1.0);
    if(texColor.a < 0.1)
        discard;
    result = texColor;
	//result = vec4(float(abc)/3,float(abc)/3,float(abc)/3,1);
} 
//(tex.x*16.0 + (float(id)-1)*16.0)/ (16.0*6.0)