#version 420

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec3 aNormal;
uniform mat4 mvpMatrix;
uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform vec3 viewPos;
uniform vec3 worldPos;

out DATA
{
	vec2 texCoord;
    vec3 fragPos;
    vec4 fragAddColor;
    vec3 _aNormal;
    vec3 NewPos;
    vec3 FragNormal;
    mat3 normalMat;
} geom_data;

void main(void)
{
    vec3 newPos = vec3(vec4(aPosition, 1.0) * modelMatrix);
    geom_data.NewPos = newPos;
    vec3 result = vec3(0, 0, 0);
    geom_data.fragAddColor = vec4(result, 1.0);
    geom_data.fragPos = vec3(modelMatrix * vec4(aPosition, 1.0));
    geom_data.texCoord = aTexCoord;
    gl_Position = vec4(aPosition, 1.0);  //* mvpMatrix;
    geom_data.normalMat = mat3(transpose(inverse(modelMatrix)));
    geom_data._aNormal = aNormal;
}