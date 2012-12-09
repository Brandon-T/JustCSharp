using System;
using System.IO;
using System.IO.Compression;

namespace JustCSharp
{
	public class Archive
	{
		private ZipType TypeOfZip;
		public enum ZipType { WinZipArchive, GZipArchive };

		public Archive(ZipType TypeOfArchive)
		{
			this.TypeOfZip = TypeOfArchive;
		}

		public void ZipDirectory(string SourcePath, string DestinationPath)
		{
			ZipFile.CreateFromDirectory(SourcePath, DestinationPath + ".zip");
		}

		public void UnZipFile(string SourceFile, string DestinationPath)
		{
			ZipFile.ExtractToDirectory(SourceFile, DestinationPath);
		}

		public void ExtractFiles(string[] FilesToExtract, string ZipPath, string DestinationPath)
		{
			using (ZipArchive Archive = ZipFile.OpenRead(ZipPath))
			{
				int I = 0;
				foreach (ZipArchiveEntry ZippedFile in Archive.Entries)
				{
					//FilesToExtract.
				}
			}
		}
	}
}