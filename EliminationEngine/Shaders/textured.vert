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
    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    // specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), light.shine);
    // attenuation
    float distance    = length(light.pos - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + 
  			     light.quadratic * (distance * distance));    
    // combine results
    //vec3 ambient  = light.ambient  * light.diffuse;
    vec3 diffuse  = light.diffuse  * diff;
    //vec3 specular = light.specular * spec * light.diffuse;
    //ambient  *= attenuation;
    diffuse  *= attenuation;
    //specular *= attenuation;
    return (diffuse);
} 

void main(void)
{
    vec3 newPos = vec3(vec4(aPosition, 1.0) * modelMatrix);
    vec3 result = vec3(0, 0, 0);
    for(int i = 0; i < lightsNum; i++)
        result += CalcPointLight(pointLights[i], mat3(transpose(inverse(modelMatrix))) * aNormal, newPos, normalize(viewPos - newPos));
    fragAddColor = vec4(result, 1.0);
    normal = aNormal;
    fragPos = aPosition;
    texCoord = aTexCoord;
    gl_Position = vec4(aPosition, 1.0) * mvpMatrix;
}