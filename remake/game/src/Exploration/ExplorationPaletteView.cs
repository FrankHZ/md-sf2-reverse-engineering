using Godot;
using Sf2.Remake.Application.Runtime.Exploration;

namespace Sf2.Remake.GodotAdapter.Exploration;

// One reusable lookup texture for the world's current color mapping. No texture
// is allocated per simulation tick, and UI/portrait children keep their materials.
internal sealed class ExplorationPaletteView(Control owner) : IDisposable
{
    private Shader? _shader;
    private ShaderMaterial? _material;
    private ImageTexture? _texture;
    private Image? _image;
    private PaletteColorTransition? _last;
    private long _tick = -1;
    internal object? Projection { get; private set; }
    internal void Project(PaletteColorTransition? colors, long tick)
    {
        if (colors is null) { owner.Material = null; _last = null; Projection = null; return; }
        if (_last == colors && _tick == tick) return;
        if (_image is null)
        {
            _image = Image.CreateEmpty(512, 1, false, Image.Format.Rgba8);
            _texture = ImageTexture.CreateFromImage(_image);
            _shader = new Shader { Code = """
                shader_type canvas_item;
                uniform sampler2D color_lookup : filter_nearest;
                varying vec4 vertex_color;
                void vertex() { vertex_color = COLOR; }
                void fragment() {
                    vec4 source = texture(TEXTURE, UV);
                    ivec3 bits = ivec3(round(source.rgb * 7.0));
                    int index = bits.r + bits.g * 8 + bits.b * 64;
                    vec4 mapped = texelFetch(color_lookup, ivec2(index, 0), 0);
                    COLOR = vec4(mapped.rgb, source.a) * vertex_color;
                }
                """ };
            _material = new ShaderMaterial { Shader = _shader };
            _material.SetShaderParameter("color_lookup", _texture);
        }
        var mapping = colors.ColorsAt(tick);
        for (int i = 0; i < mapping.Count; i++)
        {
            int word = mapping[i];
            _image.SetPixel(i, 0, new((word & 14) / 14f, ((word >> 4) & 14) / 14f, ((word >> 8) & 14) / 14f));
        }
        _texture!.Update(_image);
        owner.Material = _material;
        _last = colors; _tick = tick;
        Projection = new { tick, startedAt = colors.StartedAt, black = mapping[0], white = mapping[511],
            red = mapping[7], green = mapping[56], blue = mapping[448],
            texture = _texture.GetInstanceId().ToString(), material = owner.Material?.GetInstanceId().ToString() };
    }

    public void Dispose()
    {
        owner.Material = null;
        _material?.Dispose(); _shader?.Dispose(); _texture?.Dispose(); _image?.Dispose();
    }
}
