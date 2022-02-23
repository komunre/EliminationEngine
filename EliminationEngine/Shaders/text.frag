#version 330 core
out vec4 FragColor;
  
in vec2 TexCoord;
in vec4 AddFragColor;

uniform sampler2D ourTexture;

void main()
{
    FragColor = texture(ourTexture, TexCoord) * AddFragColor;
}