using System.Collections.Generic;
using System.Text.RegularExpressions;
using TwitchTools.Api.Rewards;

namespace TwitchTools.Rewards;

public class PsxReward : Reward
{
    public override string Id => "dev.flazhik.twitch-tools.rewards.psx";
    public override string Name => "PSX";
    public override string Description => "Modifies PSX settings";

    private readonly PrefsManager _prefs = PrefsManager.Instance;
        
    private int _originalPixelization;
    private float _originalDithering;
    private float _originalTextureWarping;
    private int _originalVertexWarping;

    private static readonly Dictionary<int, int> Resolutions = new()
    {
        {720, 1},
        {480, 2},
        {360, 3},
        {240, 4},
        {144, 5},
        {0, 0},
    };

    protected override void Execute(string prompt)
    {
        _originalPixelization = _prefs.GetInt("pixelization");
        _originalDithering = _prefs.GetFloat("dithering");
        _originalTextureWarping = _prefs.GetFloat("textureWarping");
        _originalVertexWarping = _prefs.GetInt("vertexWarping");

        var downscaling = TryGetPromptParameter("downscaling", prompt);
        var dithering = TryGetPromptParameter("dithering", prompt);
        var twarping = TryGetPromptParameter("twarping", prompt);
        var vwarping = TryGetPromptParameter("vwarping", prompt);

        if (downscaling.HasValue && Resolutions.TryGetValue(downscaling.Value, out var resolution))
            _prefs.SetInt("pixelization", resolution);
            
        if (dithering is >= 0 and <= 500)
            _prefs.SetFloat("dithering", dithering.Value / 500f);
            
        if (twarping is >= 0 and <= 100)
            _prefs.SetFloat("textureWarping", twarping.Value / 100f);
            
        if (vwarping is >= 0 and <= 5)
            _prefs.SetInt("vertexWarping", vwarping.Value);
    }
        
    protected override void CancelOut()
    {
        _prefs.SetInt("pixelization", _originalPixelization);
        _prefs.SetFloat("dithering", _originalDithering);
        _prefs.SetFloat("textureWarping", _originalTextureWarping);
        _prefs.SetInt("vertexWarping", _originalVertexWarping);
    }

    private static int? TryGetPromptParameter(string parameter, string prompt)
    {
        var matches = Regex.Matches(prompt, @$"{parameter}\s*\:\s*([0-9]+)", RegexOptions.IgnoreCase);
        if (matches.Count == 0)
            return null;

        var value = matches[0].Groups[1].Value;
        return int.TryParse(value, out var i) ? i : null;
    }

    public override void RegisterSettings()
    {
        // No extra settings
    }
}