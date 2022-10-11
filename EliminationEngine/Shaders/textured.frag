#version 330

out vec4 outputColor;

in vec3 fragPos;
in vec2 texCoord;
in vec4 fragAddColor;
in vec3 _aNormal;
in vec3 NewPos;
in mat3 normalMat;
uniform mat4 mvpMatrix;
uniform mat4 modelMatrix;
uniform vec3 viewPos;
uniform vec3 worldPos;

uniform sampler2D texture0;

struct PointLight {
    vec3 pos;
    float constant;
    float linear;
    float quadratic;
    float ambient;
    float shine;
    vec3 diffuse;
};

#define NR_POINT_LIGHTS 20 
uniform PointLight pointLights[NR_POINT_LIGHTS];
uniform int lightsNum = 0;

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec3 lightDir = normalize(light.pos - fragPos);
    float distance    = length(light.pos - fragPos);
    float attenuation = light.linear / distance;
    vec3 diffuse  = light.diffuse;
    diffuse  *= attenuation;
    return (diffuse);
} 

void main()
{
    vec3 result = vec3(0, 0, 0);
    for(int i = 0; i < lightsNum; i++)
        result += CalcPointLight(pointLights[i], normalMat * _aNormal, fragPos, normalize(viewPos - fragPos));
    outputColor = vec4(result, 1.0) * texture(texture0, texCoord);
}