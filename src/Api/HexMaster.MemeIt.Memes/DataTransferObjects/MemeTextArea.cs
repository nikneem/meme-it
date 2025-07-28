using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexMaster.MemeIt.Memes.DataTransferObjects;

public record MemeTextArea(int X, int Y, int Width, int Height, string FontFamily, int FontSize, string FontColor, int MaxLength);
