using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace ComputeShader;

public class Game
{

    private Camera camera;

    private OpenGl openGl;
    

    private ComputeShader computeShader;
    
    private Shader shaderTexture;
    private uint texture;

    private VertexArrayObject<ImageVertex, uint> vao;
    private BufferObject<ImageVertex> vbo;
    private BufferObject<uint> ebo;

    private int tick = 0;

    private Stopwatch stopwatch;
    public Game() {
        this.openGl = OpenGl.Instance();
        openGl.DrawEvent += Draw;
        openGl.UpdateEvent += Update;
        openGl.LoadEvent += Load; 
        openGl.OnCloseEvent += Stop;

        stopwatch = new Stopwatch();
        openGl.Run();
    }

   

    private unsafe void Load(GL gl) {
        camera = new Camera(openGl.window);
        camera.Position = new Vector3(0, 0, 8);
        
        //shader
        shaderTexture = new Shader(gl, "Shaders/texture.vertex.glsl", "Shaders/texture.fragment.glsl");

        computeShader = new ComputeShader(gl, "Shaders/compute.glsl");
        
        
        // vbo vao
        ImageVertex[] vertices = new[]
        {
            new ImageVertex(
                new Vector3(0.5f, 0.5f, 0.0f), 
                new Vector2(1.0f, 0.0f)
                ),
            new ImageVertex(
                new Vector3(0.5f, -0.5f, 0.0f), 
                new Vector2(1.0f, 1.0f)
            ),
            new ImageVertex(
                new Vector3(-0.5f, -0.5f, 0.0f), 
                new Vector2(0.0f, 1.0f)
            ),
            new ImageVertex(
                new Vector3(-0.5f, 0.5f, 0.5f), 
                new Vector2(0.0f, 0.0f)
            ),

        };
        
        uint[] indices = new uint[]
        {
            0, 1, 3,
            1, 2, 3
        };

        ebo = new BufferObject<uint>(gl, indices, BufferTargetARB.ElementArrayBuffer);
        vbo = new BufferObject<ImageVertex>(gl, vertices, BufferTargetARB.ArrayBuffer);
        vao = new VertexArrayObject<ImageVertex, uint>(gl, vbo, ebo);
        vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, "position");
        vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, "texCoords");
        
        //texture 
         const uint textureWidth = 512;
         const uint textureHeight = 512;
         texture = gl.GenTexture();
         gl.ActiveTexture(TextureUnit.Texture0);
         gl.BindTexture(TextureTarget.Texture2D, texture);
        
         gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
         gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
         gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
         gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        
         gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba32f,
                 textureWidth, textureHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);    
        
        gl.BindImageTexture(0, texture, 0, false, 0, GLEnum.ReadWrite, InternalFormat.Rgba32f);
        
        
    }

    private  void Update(double deltatime) {
        tick++;
    }

    private unsafe void Draw(GL gl) {
        stopwatch.Restart();
        SetUniforms(gl);

        computeShader.Use();
        computeShader.SetUniform("t",tick);

        gl.DispatchCompute((uint)512, (uint)512, 1);
        gl.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);
        
        vao.Bind();
        shaderTexture.Use();
        
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, texture);

        shaderTexture.SetUniform("tex", 0);
        
        
        gl.DrawElements(PrimitiveType.Triangles, (uint)6, DrawElementsType.UnsignedInt, null);        
        stopwatch.Stop();
        ImGui.Begin("elapsed time");
        ImGui.Text(stopwatch.ElapsedTicks.ToString());
        ImGui.End();

        ImGui.ShowDemoWindow();
    }

    private unsafe void SetUniforms(GL gl) {
        
        gl.BindBuffer(BufferTargetARB.UniformBuffer, openGl.uboWorld);
        Matrix4x4 projectionMatrix = camera.GetProjectionMatrix();
        gl.BufferSubData(BufferTargetARB.UniformBuffer, 0, (uint)sizeof(Matrix4x4), projectionMatrix);
        gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);
        
        
        gl.BindBuffer(BufferTargetARB.UniformBuffer, openGl.uboWorld);
        Matrix4x4 viewMatrix = camera.GetViewMatrix();
        gl.BufferSubData(BufferTargetARB.UniformBuffer, sizeof(Matrix4x4), (uint)sizeof(Matrix4x4), viewMatrix);
        gl.BindBuffer(BufferTargetARB.UniformBuffer, 0);

    }

    private void Stop() {
        vbo.Dispose();
        vao.Dispose();
        ebo.Dispose();
        shaderTexture.Dispose();
        computeShader.Dispose();
    }
    
}