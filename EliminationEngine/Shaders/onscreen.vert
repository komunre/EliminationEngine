#version 330 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;

uniform vec3 worldPos;
uniform mat4 modelMatrix;

out vec2 TexCoord;
out vec3 AddFragColor;

void main()
{
    gl_Position = vec4(aPosition, 1.0) * modelMatrix;
    TexCoord = aTexCoord;
}