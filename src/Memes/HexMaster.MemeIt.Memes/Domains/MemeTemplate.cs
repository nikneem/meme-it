using HexMaster.MemeIt.Memes.Domains.ValueObjects;

namespace HexMaster.MemeIt.Memes.Domains;

/// <summary>
/// Aggregate root representing a meme template with image and text area definitions.
/// </summary>
public class MemeTemplate
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = default!;
    public string ImageUrl { get; private set; } = default!;
    public int Width { get; private set; }
    public int Height { get; private set; }

    private readonly List<TextAreaDefinition> _textAreas = new();
    public IReadOnlyList<TextAreaDefinition> TextAreas => _textAreas.AsReadOnly();

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Private constructor for EF Core
    private MemeTemplate() { }

    /// <summary>
    /// Factory method to create a new meme template.
    /// </summary>
    public static MemeTemplate Create(string title, string imageUrl, int width, int height, IEnumerable<TextAreaDefinition> textAreas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
        ArgumentException.ThrowIfNullOrWhiteSpace(imageUrl, nameof(imageUrl));
        ArgumentNullException.ThrowIfNull(textAreas, nameof(textAreas));

        if (width <= 0)
            throw new DomainException("Width must be greater than 0");
        if (height <= 0)
            throw new DomainException("Height must be greater than 0");

        var textAreasList = textAreas.ToList();
        if (textAreasList.Count == 0)
            throw new DomainException("At least one text area must be defined");

        if (!imageUrl.StartsWith('/'))
            throw new DomainException("Image URL must be a relative path starting with /");

        var template = new MemeTemplate
        {
            Id = Guid.NewGuid(),
            Title = title,
            ImageUrl = imageUrl,
            Width = width,
            Height = height,
            CreatedAt = DateTimeOffset.UtcNow
        };

        foreach (var textArea in textAreasList)
        {
            template._textAreas.Add(textArea);
        }

        return template;
    }

    /// <summary>
    /// Updates the meme template with new values.
    /// </summary>
    public void Update(string title, string imageUrl, int width, int height, IEnumerable<TextAreaDefinition> textAreas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
        ArgumentException.ThrowIfNullOrWhiteSpace(imageUrl, nameof(imageUrl));
        ArgumentNullException.ThrowIfNull(textAreas, nameof(textAreas));

        if (width <= 0)
            throw new DomainException("Width must be greater than 0");
        if (height <= 0)
            throw new DomainException("Height must be greater than 0");

        var textAreasList = textAreas.ToList();
        if (textAreasList.Count == 0)
            throw new DomainException("At least one text area must be defined");

        if (!imageUrl.StartsWith('/'))
            throw new DomainException("Image URL must be a relative path starting with /");

        Title = title;
        ImageUrl = imageUrl;
        Width = width;
        Height = height;
        UpdatedAt = DateTimeOffset.UtcNow;

        _textAreas.Clear();
        foreach (var textArea in textAreasList)
        {
            _textAreas.Add(textArea);
        }
    }

    /// <summary>
    /// Adds a text area to the template.
    /// </summary>
    public void AddTextArea(TextAreaDefinition textArea)
    {
        ArgumentNullException.ThrowIfNull(textArea, nameof(textArea));
        _textAreas.Add(textArea);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Removes a text area from the template.
    /// </summary>
    public void RemoveTextArea(int index)
    {
        if (index < 0 || index >= _textAreas.Count)
            throw new DomainException("Invalid text area index");

        if (_textAreas.Count == 1)
            throw new DomainException("Cannot remove the last text area - at least one is required");

        _textAreas.RemoveAt(index);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Domain exception for business rule violations.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
