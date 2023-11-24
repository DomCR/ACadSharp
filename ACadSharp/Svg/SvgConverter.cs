using ACadSharp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.Svg
{
	public class SvgConverter
	{
		public string Convert(Entity entity)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"<svg width=\"100\" height=\"100\">");

			switch (entity)
			{
				case Line line:
					sb.AppendLine(convertLine(line));
					break;
				default:
					throw new NotImplementedException($"Svg convertion not implemented for {entity.SubclassMarker}");
			}

			sb.AppendLine($"</svg>");

			return sb.ToString();
		}

		private string convertLine(Line line)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append($"\t");
			sb.Append($"<line ");
			sb.Append($"x1=\"{line.StartPoint.X}\" ");
			sb.Append($"y1=\"{line.StartPoint.Y}\" ");
			sb.Append($"x2=\"{line.EndPoint.X}\" ");
			sb.Append($"y2=\"{line.EndPoint.Y}\" ");
			sb.Append(this.entityStyle(line));
			sb.Append($"/>");

			return sb.ToString();
		}

		private string entityStyle(Entity entity)
		{
			StringBuilder style = new StringBuilder();

			style.Append($"style=");
			
			style.Append($"\"stroke:rgb({entity.Color.R},{entity.Color.G},{entity.Color.B})\"");

			return style.ToString();
		}
	}
}
