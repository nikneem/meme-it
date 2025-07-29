

namespace HexMaster.MemeIt.Memes.DataTransferObjects;


// This model is used for uploading empty meme templates. There is a source medium, which can be
// an image or a video, and the dimensions of the source medium. The meme also contains one or more
// text boxes, which are defined by their position and size relative to the source medium. These text
// boxes are used to overlay text on the source medium when the meme is created.
public record CreateMemeRequest(
    string Name,
    string? Description,
    string SourceImage, 
    int SourceWidth, 
    int SourceHeight, 
    MemeTextArea[] Textareas);

