#version 330 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;

uniform mat4 mvpMatrix;
uniform vec4 addColor;

out vec2 TexCoord;
out vec4 AddFragColor;

void main()
{
    gl_Position = vec4(aPosition, 1.0) * mvpMatrix;
    TexCoord = aTexCoord;
    AddFragColor = addColor;
}