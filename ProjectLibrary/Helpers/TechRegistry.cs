using ProjectLibrary.Models;

namespace ProjectLibrary.Helpers;

/// <summary>
/// Catalogue of all tech stacks we recognize, with display names and brand colors.
/// Colors per the Project Library spec section 5.
/// </summary>
public static class TechRegistry
{
    public static readonly TechInfo Node      = new("node",      "Node.js",      "#5FA04E");
    public static readonly TechInfo React     = new("react",     "React",        "#F7DF1E");
    public static readonly TechInfo Vue       = new("vue",       "Vue",          "#42B883");
    public static readonly TechInfo Angular   = new("angular",   "Angular",      "#DD0031");
    public static readonly TechInfo Next      = new("next",      "Next.js",      "#000000");
    public static readonly TechInfo Typescript= new("typescript","TypeScript",   "#3178C6");
    public static readonly TechInfo Python    = new("python",    "Python",       "#3776AB");
    public static readonly TechInfo Rust      = new("rust",      "Rust",         "#CE422B");
    public static readonly TechInfo Go        = new("go",        "Go",           "#00ADD8");
    public static readonly TechInfo Java      = new("java",      "Java",         "#ED8B00");
    public static readonly TechInfo Csharp    = new("csharp",    "C#",           "#68217A");
    public static readonly TechInfo Html      = new("html",      "HTML/CSS",     "#E34F26");
    public static readonly TechInfo Markdown  = new("markdown",  "Markdown",     "#000000");
    public static readonly TechInfo Git       = new("git",       "Git",          "#7A7A7A");

    /// <summary>Lookup a TechInfo by its stable key. Returns null if unknown.</summary>
    public static TechInfo? Find(string key) => key.ToLowerInvariant() switch
    {
        "node"      => Node,
        "react"     => React,
        "vue"       => Vue,
        "angular"   => Angular,
        "next"      => Next,
        "typescript"=> Typescript,
        "python"    => Python,
        "rust"      => Rust,
        "go"        => Go,
        "java"      => Java,
        "csharp"    => Csharp,
        "html"      => Html,
        "markdown"  => Markdown,
        "git"       => Git,
        _           => null
    };
}
