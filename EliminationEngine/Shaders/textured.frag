#version 330

out vec4 outputColor;

in vec2 texCoord;
in vec4 fragAddColor;

uniform sampler2D texture0;

void main()
{
    outputColor = fragAddColor * texture(texture0, texCoord);
}