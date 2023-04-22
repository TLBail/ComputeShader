using Silk.NET.OpenGL;

namespace ComputeShader;

public class ComputeShader :  IDisposable
{
    public uint _handle { get; private set; }
    private GL gl;
    private bool disposed = false;

    public ComputeShader(GL gl, string computePath) {
        this.gl = gl;

        uint compute = LoadShader(ShaderType.ComputeShader, computePath);

        _handle = gl.CreateProgram();
        gl.AttachShader(_handle, compute);
        gl.LinkProgram(_handle);
        gl.GetProgram(_handle, GLEnum.LinkStatus, out var status);
        if (status == 0) {
            throw new Exception($"Program failed to link with error: {gl.GetProgramInfoLog(_handle)}");
        }
        
        gl.DetachShader(_handle, compute);
        gl.DeleteShader(compute);
        
        
        

    }
    
    public void Use() {
        gl.UseProgram(_handle);
    }
    
    private uint LoadShader(ShaderType type, string path) {
        string src = File.ReadAllText(path);
        uint handle = gl.CreateShader(type);
        gl.ShaderSource(handle, src);
        gl.CompileShader(handle);
        string infoLog = gl.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog)) {
            throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
        }

        return handle;
    }
    
    public void SetUniform(string name, int value) {
        int location = gl.GetUniformLocation(_handle, name);
        if (location == -1) {
            throw new Exception($"{name} uniform not found on shader.");
        }

        gl.Uniform1(location, value);
    }
    
    public void SetUniform(string name, float value) {
        int location =gl.GetUniformLocation(_handle, name);
        if (location == -1) {
            throw new Exception($"{name} uniform not found on shader.");
        }
        gl.Uniform1(location, value);
    }
    
    ~ComputeShader() {
        Dispose(false);
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    public void Dispose(bool disposing) {
        if (!disposed) {
            if (disposing) {
                gl.DeleteProgram(_handle);
            }

            disposed = true;
        }
    }
}