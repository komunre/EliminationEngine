#version 330 core
out vec4 FragColor;
  
in vec2 TexCoord;
in vec3 AddFragColor;

uniform sampler2D ourTexture;

void main()
{
    FragColor = texture(ourTexture, TexCoord) * vec4(AddFragColor, 1.0);
}