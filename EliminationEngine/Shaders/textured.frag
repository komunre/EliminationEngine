#version 420

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

uniform float heightScale;

layout(binding=0) uniform sampler2D texture0;
layout(binding=1) uniform sampler2D normal0;
layout(binding=2) uniform sampler2D displacement0;

struct PointLight {
    vec3 pos;
    float constant;
    float linear;
    float quadratic;
    float ambient;
    float shine;
    vec3 diffuse;
    int directional;
    vec3 direction;
    float cutoff;
};

#define NR_POINT_LIGHTS 20 
uniform PointLight pointLights[NR_POINT_LIGHTS];
uniform int lightsNum = 0;

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec3 lightDir = normalize(light.pos - fragPos);
    float diff = clamp(dot(lightDir, normal), 0.0, 1.0);
    float distance    = length(light.pos - fragPos);
    float attenuation = light.linear / distance;
    vec3 diffuse = light.diffuse * diff;
    diffuse = diffuse * attenuation;
    if (light.directional == 1) {
        float theta = dot(lightDir, -light.direction);
        if (theta < light.cutoff) {
            theta = 0;
        }
        diffuse *= theta;
    }
    return (diffuse);
} 

const float numLayers = 32.0f;

void main()
{
    // displacement vars
    vec3 viewDir = normalize(viewPos - fragPos);
    float layerDepth = 1.0f / numLayers;
    float currentDepth = 0.0f;

    vec2 S = viewDir.xy / viewDir.z * heightScale;
    vec2 deltaUVs = S / numLayers;
    
    vec2 UVs = texCoord;
    float currentDepthMapValue = 1.0f - texture(displacement0, UVs).r;

    // loop until hit
    while (currentDepth < currentDepthMapValue) {
        UVs -= deltaUVs;
        currentDepthMapValue = 1.0f - texture(displacement0, UVs).r;
        currentDepth += layerDepth;
    }

    // average out (interpolate)
    vec2 prevTexCoords = UVs + deltaUVs;
    float afterDepth = currentDepthMapValue - currentDepth;
    float beforeDepth = 1.0f - texture(displacement0, prevTexCoords).r - currentDepth + layerDepth;
    float weight = afterDepth / (afterDepth - beforeDepth);
    UVs = prevTexCoords * weight + UVs * (1.0f - weight);

    // get normal
    vec3 normal = _aNormal * normalize(texture(normal0, UVs).xyz * 2.0f - 1.0f);


    // calc lights affection
    vec3 result = vec3(0, 0, 0);
    for(int i = 0; i < lightsNum; i++)
        result += CalcPointLight(pointLights[i], normal, fragPos, normalize(viewPos - fragPos));
    
    // output
    outputColor = vec4(result, 1.0) * texture(texture0, UVs);
}