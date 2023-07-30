#version 420

out vec4 outputColor;

in vec3 fragPos;
in vec2 texCoord;
in vec4 fragAddColor;
in vec3 _aNormal;
in vec3 NewPos;
in mat3 normalMat;
in mat3 TBN;
in vec3 T;
in vec3 B;
in vec3 N;
in vec3 TBNFragPos;
uniform mat4 mvpMatrix;
uniform mat4 modelMatrix;
uniform vec3 viewPos;
uniform vec3 worldPos;
uniform vec3 cameraForward;

uniform float heightScale;
uniform float zeroHeight;

layout(binding=0) uniform sampler2D texture0;
layout(binding=1) uniform sampler2D normal0;
layout(binding=2) uniform sampler2D displacement0;

struct PointLight {
    vec3 pos;
    float constant;
    float constantDiffuse;
    float linear;
    float quadratic;
    float ambient;
    float shine;
    vec3 diffuse;
    int directional;
    float distanceFactor;
    float maxBrightness;
    vec3 direction;
    float cutoff;
};

#define NR_POINT_LIGHTS 20
uniform PointLight pointLights[NR_POINT_LIGHTS];
uniform int lightsNum = 0;

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec3 lightpos = light.pos;
    vec3 lightDir = normalize(lightpos - fragPos);
    float diff = clamp(dot(lightDir, normal), 0.0, 1.0);
    float distance    = length(lightpos - fragPos);
    float attenuation = light.linear / (distance * light.distanceFactor);
    vec3 diffuse = light.diffuse * diff;
    diffuse = diffuse * attenuation;
    if (light.directional == 1) {
        float theta = dot(lightDir, -light.direction);
        if (theta < light.cutoff) {
            theta = 0;
        }
        diffuse *= theta;
    }
    diffuse.xyz = clamp(diffuse.xyz, 0.0, 1.0);
    diffuse = diffuse * light.maxBrightness;
    return diffuse + (vec3(1, 1, 1) * light.constant);
} 

const float numLayers = 32.0f;

void main()
{
    // displacement vars
    vec3 fragToCameraPos = viewPos - fragPos;
    vec3 viewDir = normalize(fragToCameraPos);
    float layerDepth = 1.0f / numLayers;
    float currentDepth = 0.0f;

    vec3 displaceViewDir = viewDir;

    vec3 unreadyNormal = -N * _aNormal;

    vec2 S = vec2(0, 0);
    if (unreadyNormal.y > unreadyNormal.x && unreadyNormal.y > unreadyNormal.z) {
        S = displaceViewDir.xz / displaceViewDir.y * heightScale;
    }
    else {
        S = displaceViewDir.xy / displaceViewDir.z * heightScale;
    }
    vec2 deltaUVs = S / numLayers;
    
    vec2 UVs = texCoord;
    float currentDepthMapValue =zeroHeight - texture(displacement0, UVs).r;

    // loop until hit
    while (currentDepth < currentDepthMapValue) {
        UVs -= deltaUVs;
        currentDepthMapValue = zeroHeight - texture(displacement0, UVs).r;
        currentDepth += layerDepth;
    }

    // average out (interpolate)
    vec2 prevTexCoords = UVs + deltaUVs;
    float afterDepth = currentDepthMapValue - currentDepth;
    float beforeDepth = zeroHeight - texture(displacement0, prevTexCoords).r - currentDepth + layerDepth;
    float weight = afterDepth / (afterDepth - beforeDepth);
    UVs = prevTexCoords * weight + UVs * (1.0f - weight);

    //if(UVs.x > 1.0 || UVs.y > 1.0 || UVs.x < 0.0 || UVs.y < 0.0)
    //    discard;

    // get normal
    vec3 normal = -N * normalize(texture(normal0, UVs).xyz);


    // calc lights affection
    vec3 result = vec3(0, 0, 0);
    for(int i = 0; i < lightsNum; i++)
        result += CalcPointLight(pointLights[i], normal, fragPos, viewDir);
    
    // output
    outputColor = vec4(result, 1.0) * texture(texture0, UVs);
}