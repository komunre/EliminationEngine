#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec3 aNormal;
uniform mat4 mvpMatrix;
uniform mat4 modelMatrix;
uniform vec3 viewPos;
uniform vec3 worldPos;
out vec2 texCoord;
out vec3 fragPos;
out vec3 normal;
out vec4 fragAddColor;
out vec3 _aNormal;
out vec3 NewPos;
out vec3 FragNormal;
out mat3 normalMat;

void main(void)
{
    vec3 newPos = vec3(vec4(aPosition, 1.0) * modelMatrix);
    NewPos = newPos;
    vec3 result = vec3(0, 0, 0);
    fragAddColor = vec4(result, 1.0);
    normal = aNormal;
    fragPos = vec3(modelMatrix * vec4(aPosition, 1.0));
    texCoord = aTexCoord;
    gl_Position = vec4(aPosition, 1.0) * mvpMatrix;
    _aNormal = aNormal;
    normalMat = mat3(transpose(inverse(modelMatrix)));
}