using Xunit.Abstractions;

namespace ACadSharp.Tests.TestModels
{
	public class FileModel : IXunitSerializable
	{
		public string Extension { get { return System.IO.Path.GetExtension(this.Path); } }

		public string FileName { get; private set; }

		public string NoExtensionName { get { return System.IO.Path.GetFileNameWithoutExtension(this.Path); } }

		public string Path { get; private set; }

		public string Folder { get { return System.IO.Path.GetDirectoryName(this.Path); } }

		public bool IsDxf { get { return System.IO.Path.GetExtension(this.Path) == ".dxf"; } }

		public FileModel()
		{
			this.FileName = string.Empty;
		}

		public FileModel(string path)
		{
			this.Path = path;
			this.FileName = System.IO.Path.GetFileName(this.Path);
		}

		public void Deserialize(IXunitSerializationInfo info)
		{
			this.Path = info.GetValue<string>(nameof(this.Path));
			this.FileName = info.GetValue<string>(nameof(this.FileName));
		}

		public void Serialize(IXunitSerializationInfo info)
		{
			info.AddValue(nameof(this.Path), this.Path);
			info.AddValue(nameof(this.FileName), this.FileName);
		}

		public override string ToString()
		{
			return this.FileName;
		}
	}
}
