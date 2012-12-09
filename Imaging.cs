using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace JustCSharp
{
	public class ImageInfo
	{
		public ImageInfo() { }

		public Image ImageFromDisk(string Location)
		{
			return Image.FromFile(Location);
		}

		public void ImageToDisk(string Location, byte[] Buffer)
		{
			using (FileStream Stream = new FileStream(Location, FileMode.CreateNew, FileAccess.Write))
			{
				Stream.Write(Buffer, 0, Buffer.Length);
				Stream.Flush();
				Stream.Close();
			}
		}

		public byte[] ImageToBytes(Image Img)
		{
			byte[] Bytes = null;
			using (MemoryStream Stream = new MemoryStream())
			{
				if (Stream != null)
				{
					Img.Save(Stream, Img.RawFormat);
					Bytes = Stream.ToArray();
					Stream.Close();
					return Bytes;
				}
			}
			return null;
		}

		public Image BytesToImage(byte[] Bytes)
		{
			using (MemoryStream Stream = new MemoryStream(Bytes))
			{
				if (Stream != null)
				{
					Image Img = Image.FromStream(Stream);
					Stream.Close();
					return Img;
				}
			}
			return null;
		}

		public bool ValidImage(byte[] ImageBytes)
		{
			byte[] GIFBytesOne = { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 };
			byte[] GIFBytesTwo = { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };
			byte[] PNGBytes = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
			byte[] BMPBytes = { 0x42, 0x4D };
			byte[] JPGBytes = { 0xFF, 0xD8, 0xFF };
			byte[] JPEGBytes = { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20 };
			byte[] TIFFMonoChrome = { 0x0C, 0xED };
			byte[] TIFFOne = { 0x49, 0x20, 0x49 };
			byte[] TIFFTwo = { 0x49, 0x49, 0x2A, 0x00 };
			byte[] TIFFThree = { 0x4D, 0x4D, 0x00, 0x2A };
			byte[] TIFFFour = { 0x4D, 0x4D, 0x00, 0x2B };

			byte[][] All = {GIFBytesOne, GIFBytesTwo, PNGBytes, BMPBytes, JPGBytes, JPGBytes, TIFFMonoChrome,
						   TIFFOne, TIFFTwo, TIFFThree, TIFFFour};

			Func<byte[], byte[], bool> BytesMatch = (delegate(byte[] One, byte[] Two)
			{
				for (int I = 0; I < One.Length; ++I)
				{
					if (One[I] != Two[I])
						return false;
				}
				return true;
			});

			foreach (byte[] Bytes in All)
			{
				if (BytesMatch(Bytes, ImageBytes))
					return true;
			}
			return false;
		}
	}

	[Serializable()]
	public class Captcha
	{
		private string FontName;
		private int FontSize;
		private bool InsaneNoise;
		private int AmountOfLines;
		private int Width, Height;
		private string RandomText;
		private FontStyle fontStyle;
		private Random random = new Random();
		private HatchStyle TextStyle, BackStyle;
		private Color BackForeColour, BackgroundColour;
		private Color TextForeColour, TextBackColour, NoiseColour;
		private static string CharSet = "1234567890ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

		public Captcha(int Width, int Height, HatchStyle TextStyle = HatchStyle.NarrowHorizontal, HatchStyle BackStyle = HatchStyle.DottedGrid)
		{
			this.Width = Width;
			this.Height = Height;
			this.TextStyle = TextStyle;
			this.BackStyle = BackStyle;
		}

		public void SetFontStyle(string FontName, int FontSize, FontStyle fontStyle)
		{
			this.FontName = FontName;
			this.FontSize = FontSize;
			this.fontStyle = fontStyle;
		}

		public void SetInfo(int AmountOfLines, Color TextForeColour, Color TextBackColour, Color NoiseColour, Color BackForeColour, Color BackgroundColour, bool InsaneNoise)
		{
			this.AmountOfLines = AmountOfLines;
			this.TextForeColour = TextForeColour;
			this.TextBackColour = TextBackColour;
			this.NoiseColour = NoiseColour;
			this.BackForeColour = BackForeColour;
			this.BackgroundColour = BackgroundColour;
			this.InsaneNoise = InsaneNoise;
		}

		private void AddNoise(ref Bitmap Bmp, int Width, int Height)
		{
			Random R = new Random();
			for (int I = 0; I < Width; ++I)
			{
				for (int J = 0; J < Height; ++J)
				{
					int K = R.Next(0, 256);
					Bmp.SetPixel(I, J, Color.FromArgb(255, K, K, K));
				}
			}
		}

		private void WarpText(ref Rectangle Rect, ref GraphicsPath Path)
		{
			PointF[] points =
			{
				new PointF(random.Next(Rect.Width) / 4F, random.Next(Rect.Height) / 4F),
				new PointF(Rect.Width - random.Next(Rect.Width) / 4F, random.Next(Rect.Height) / 4F),
				new PointF(random.Next(Rect.Width) / 4F, Rect.Height - random.Next(Rect.Height) / 4F),
				new PointF(Rect.Width - random.Next(Rect.Width) / 4F, Rect.Height - random.Next(Rect.Height) / 4F)
			};
			Matrix matrix = new Matrix();
			matrix.Translate(0F, 0F);
			Path.Warp(points, Rect, matrix, WarpMode.Perspective, 0F);
		}

		private void RandomizeText(int StringSize)
		{
			char[] Buffer = new char[StringSize];

			for (int I = 0; I < StringSize; ++I)
			{
				Buffer[I] = CharSet[random.Next(CharSet.Length)];
			}
			RandomText = new string(Buffer);
		}

		public string GetImageText()
		{
			return RandomText;
		}

		public Bitmap Image
		{
			get
			{
				Bitmap bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
				Graphics g = null;
				Font font = null;
				HatchBrush TextBrush = null;
				HatchBrush BackBrush = null;
				HatchBrush NoiseBrush = null;

				try
				{
					RandomizeText(8);
					g = Graphics.FromImage(bitmap);
					g.SmoothingMode = SmoothingMode.AntiAlias;
					Rectangle Rect = new Rectangle(0, 0, Width, Height);
					TextBrush = new HatchBrush(TextStyle, TextForeColour, TextBackColour);
					BackBrush = new HatchBrush(BackStyle, BackForeColour, BackgroundColour);
					NoiseBrush = new HatchBrush(TextStyle, NoiseColour, NoiseColour);
					g.FillRectangle(BackBrush, Rect);
					if (InsaneNoise) AddNoise(ref bitmap, Width, Height);

					StringFormat stringFormat = new StringFormat();
					stringFormat.Alignment = StringAlignment.Center;
					stringFormat.LineAlignment = StringAlignment.Center;
					font = new Font(FontName, FontSize, fontStyle);
					GraphicsPath Path = new GraphicsPath();
					Path.AddString(RandomText, font.FontFamily, (int)font.Style, font.Size, Rect, stringFormat);
					WarpText(ref Rect, ref Path);
					g.FillPath(TextBrush, Path);

					int Lines = ((int)AmountOfLines / 30) + 1;
					using (Pen P = new Pen(NoiseBrush, 1))
					{
						for (int I = 0; I < Lines; ++I)
						{
							PointF[] Points = new PointF[Lines > 3 ? Lines - 1 : 3];
							for (int J = 0; J < Points.Length; ++J)
							{
								Points[J] = new PointF(random.Next(Rect.Width), random.Next(Rect.Height));
							}
							g.DrawCurve(P, Points, 1.75F);
						}
					}
				}
				finally
				{
					if (g != null) g.Dispose();
					if (font != null) font.Dispose();
					if (TextBrush != null) TextBrush.Dispose();
					if (BackBrush != null) BackBrush.Dispose();
				}
				return bitmap;
			}
		}
	}
}