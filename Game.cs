using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using StbImageSharp;

namespace OpenTK_Playground;

internal class Game : GameWindow
{
    private int ebo;

    private readonly uint[] indices =
    {
        // top triangle
        0, 1, 2,
        //bottom triangle
        2, 3, 0
    };

    private int shaderProgram;

    //texture coordinates
    private readonly float[] texCoords =
    {
        0f, 1f,
        1f, 1f,
        1f, 0f,
        0f, 0f
    };

    private int textureID;

    private int textureVBO;
    //render pipeline vars

    private int vao;

    //triangle
    private readonly float[] vertrices =
    {
        -0.5f, 0.5f, 0f, //top left
        0.5f, 0.5f, 0f, //top right
        0.5f, -0.5f, 0f, //bottom right
        -0.5f, -0.5f, 0f //bottom left
    };

    //window width and height
    private int windowWidth, windowHeight;

    public Game(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        CenterWindow(new Vector2i(width, height));
        windowWidth = width;
        windowHeight = height;
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
        windowHeight = e.Height;
        windowWidth = e.Width;
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        // Generate and bind the VAO
        vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);

        // --- Bind and set data for vertex positions ---
        var vertexVBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, vertrices.Length * sizeof(float), vertrices,
            BufferUsageHint.StaticDraw);

        // Link vertex data to location 0 in the vertex shader
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // --- Bind and set data for texture coordinates ---
        textureVBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, textureVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, texCoords.Length * sizeof(float), texCoords,
            BufferUsageHint.StaticDraw);

        // Link texture coordinate data to location 1 in the vertex shader
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float),
            0); // Use 2 components for texture coords
        GL.EnableVertexAttribArray(1);

        // --- Bind the EBO (Element Buffer Object) ---
        ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices,
            BufferUsageHint.StaticDraw);

        // Unbind the VAO (EBO is stored within the VAO state)
        GL.BindVertexArray(0);

        // --- Load and compile shaders ---
        shaderProgram = GL.CreateProgram();

        var vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader,
            LoadShaderSource(
                "/Users/mistaluai/RiderProjects/OpenTK-Playground/OpenTK-Playground/Shaders/default.vert"));
        GL.CompileShader(vertexShader);

        var vertexLog = GL.GetShaderInfoLog(vertexShader);
        if (!string.IsNullOrEmpty(vertexLog)) Console.WriteLine($"Vertex Shader Error: {vertexLog}");

        var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader,
            LoadShaderSource(
                "/Users/mistaluai/RiderProjects/OpenTK-Playground/OpenTK-Playground/Shaders/default.frag"));
        GL.CompileShader(fragmentShader);

        var fragmentLog = GL.GetShaderInfoLog(fragmentShader);
        if (!string.IsNullOrEmpty(fragmentLog)) Console.WriteLine($"Fragment Shader Error: {fragmentLog}");

        GL.AttachShader(shaderProgram, vertexShader);
        GL.AttachShader(shaderProgram, fragmentShader);
        GL.LinkProgram(shaderProgram);

        var programLog = GL.GetProgramInfoLog(shaderProgram);
        if (!string.IsNullOrEmpty(programLog)) Console.WriteLine($"Program Linking Error: {programLog}");

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        // --- Load the texture ---
        textureID = GL.GenTexture();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, textureID);

        // Set texture parameters
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);

        // Load image with StbImageSharp
        StbImage.stbi_set_flip_vertically_on_load(1);
        var dirtTexture = ImageResult.FromStream(
            File.OpenRead(
                "/Users/mistaluai/RiderProjects/OpenTK-Playground/OpenTK-Playground/Textures/13738812-dirt_l.png"),
            ColorComponents.RedGreenBlueAlpha);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, dirtTexture.Width, dirtTexture.Height, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, dirtTexture.Data);

        // Unbind the texture
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }


    protected override void OnUnload()
    {
        base.OnUnload();
        GL.DeleteVertexArray(vao);
        GL.DeleteProgram(shaderProgram);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.ClearColor(0.6f, 0.3f, 0.3f, 1f);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        GL.UseProgram(shaderProgram);

        GL.BindTexture(TextureTarget.Texture2D, textureID);
        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
        // GL.DrawArrays(PrimitiveType.Triangles, 0, 4);

        Context.SwapBuffers();

        base.OnRenderFrame(args);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
    }

    public static string LoadShaderSource(string filePath)
    {
        var source = File.ReadAllText(filePath);
        return source;
    }
}