#version 420

layout (triangles) in;
layout (triangle_strip, max_vertices = 3) out;

uniform mat4 mvpMatrix;
uniform mat4 modelMatrix;
uniform vec3 viewPos;

out vec2 texCoord;
out vec3 fragPos;
out vec4 fragAddColor;
out vec3 _aNormal;
out vec3 NewPos;
out vec3 FragNormal;
out mat3 normalMat;
out mat3 TBN;
out vec3 T;
out vec3 B;
out vec3 N;
out vec3 TBNFragPos;
out vec3 TBNViewPos;

in DATA
{
	vec2 texCoord;
    vec3 fragPos;
    vec4 fragAddColor;
    vec3 _aNormal;
    vec3 NewPos;
    vec3 FragNormal;
    mat3 normalMat;
} geom_data[];

struct TBNData {	
	vec3 T;
	vec3 B;
	vec3 N;
	mat3 TBN;
};

TBNData get_TBN(int v0, int v1, int v2) {
	vec3 edge0 = gl_in[v1].gl_Position.xyz - gl_in[v0].gl_Position.xyz;
	vec3 edge1 = gl_in[v2].gl_Position.xyz - gl_in[v0].gl_Position.xyz;
	vec2 deltaUV0 = geom_data[v1].texCoord - geom_data[v0].texCoord;
	vec2 deltaUV1 = geom_data[v2].texCoord - geom_data[v0].texCoord;

	float invDet = 1.0f / (deltaUV0.x * deltaUV1.y - deltaUV1.x * deltaUV0.y);

	vec3 tangent = vec3(invDet * (deltaUV1.y * edge0 - deltaUV0.y * edge1));
	vec3 bitangent = vec3(invDet * (-deltaUV1.x * edge0 + deltaUV0.x * edge1));

	T = normalize(vec3(vec4(tangent, 0.0f)));
	B = normalize(vec3(vec4(bitangent, 0.0f)));
	N = normalize(vec3(vec4(cross(edge1, edge0), 0.0f)));

	TBN = transpose(mat3(T, B, N));

	return TBNData(T, B, N, TBN);
}

void main() {
	TBNData data = get_TBN(0, 1, 2);

	T = data.T;
	B = data.B;
	N = data.N;
	TBN = data.TBN;
	
	TBNViewPos = TBN * viewPos;

	gl_Position = gl_in[0].gl_Position * mvpMatrix;
	texCoord = geom_data[0].texCoord;
	fragPos = geom_data[0].fragPos;
	TBNFragPos = TBN * geom_data[0].fragPos;
	fragAddColor = geom_data[0].fragAddColor;
	_aNormal = normalize(geom_data[0]._aNormal);
	NewPos = geom_data[0].NewPos;
	FragNormal = geom_data[0].FragNormal;
	normalMat = geom_data[0].normalMat;
	EmitVertex();

	gl_Position = gl_in[1].gl_Position * mvpMatrix;
	texCoord = geom_data[1].texCoord;
	fragPos = geom_data[1].fragPos;
	TBNFragPos = TBN * geom_data[1].fragPos;
	fragAddColor = geom_data[1].fragAddColor;
	_aNormal = geom_data[1]._aNormal;
	NewPos = geom_data[1].NewPos;
	FragNormal = geom_data[1].FragNormal;
	normalMat = geom_data[1].normalMat;
	EmitVertex();

	gl_Position = gl_in[2].gl_Position * mvpMatrix;
	texCoord = geom_data[2].texCoord;
	fragPos = geom_data[2].fragPos;
	TBNFragPos = TBN * geom_data[2].fragPos;
	fragAddColor = geom_data[2].fragAddColor;
	_aNormal = normalize(geom_data[2]._aNormal);
	NewPos = geom_data[2].NewPos;
	FragNormal = geom_data[2].FragNormal;
	normalMat = geom_data[2].normalMat;
	EmitVertex();

	EndPrimitive();
}