using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Model.Abstract;
using Model.Operators;
using Model.OperatorsHelper;
using Color = System.Drawing.Color;

namespace Model

{
	public enum OperatorsEnum {
		BrightnessOperator = 0, InvertionOperator, IdentityOperator,
		GaussOperator, KannyOperator, SobelOperator, LaplasOperator, PruittOperator, RobertsOperator
	}

	public class Initialization
	{

		private BitmapSource _destination;

		#region Properties
		public string Path { get; set; }
		public OperatorsEnum Operator { get; set; }
		public string Extantion { get; set; }
		public Bitmap Source { get; set; }
        public bool RGBOperator { get; set; }
        public int ReapplyCount { get; set; }

        public BitmapSource Destination
		{
			get { return _destination; }
			set
			{
				_destination = value;
				_destinationChanged?.Invoke(Destination);
			}
		}

		private event Action<BitmapSource> _destinationChanged;
		public event Action<BitmapSource> DestinationChanged
		{
			add { _destinationChanged += value; }
			remove { _destinationChanged -= value; }
		}

		#endregion

		#region Methods


        public void Start()
		{
            //try {
            Source = new Bitmap(Path);
            //} catch (Exception e) {
            //	Path = @"E:\GitRepos\BordersFilters\BordersFilters\bin\Debug\..\..\Resours\Shrek.png";
            //}

            var srcMatrix = GetBitMapColorMatrix(Source);
			//byte[] pixels = BitmapSourceToArray(Source);
			IOperator oper = null;
			switch (Operator)
			{
				case OperatorsEnum.BrightnessOperator:
                    oper = new BrightnessOperator();
                    break;
				case OperatorsEnum.InvertionOperator:
					oper = new InvertionOperator();
					break;
				case OperatorsEnum.IdentityOperator:
					oper = new IdentityOperator();
					break;
				case OperatorsEnum.KannyOperator:
				    oper = new KannyOperator();
                    break;
				case OperatorsEnum.SobelOperator:
				    oper = new SobelOperator();
                    break;
				case OperatorsEnum.LaplasOperator:
					oper = new LaplasOperator();
					break;
				case OperatorsEnum.PruittOperator:
					oper = new PrevittOperator();
					break;
				case OperatorsEnum.RobertsOperator:
				    oper = new RobertsOperator();
                    break;
				case OperatorsEnum.GaussOperator:
					oper = new GaussOperator();
					break;
				default:
					break;
			}
		    if (oper != null)
		    {
		        var result = !RGBOperator? 
		            oper.Transform(srcMatrix.GetGrayArray(), ReapplyCount)?.GetColorArray() :
		            srcMatrix.GetColorArray(oper.Transform(srcMatrix.GetRedArray(), ReapplyCount),
                                            oper.Transform(srcMatrix.GetGreenArray(), ReapplyCount),
                                            oper.Transform(srcMatrix.GetBlueArray(), ReapplyCount));

		        Bitmap bm = SetBitMapColorMatrix(result ?? srcMatrix);
		        Destination = GetBitmapSource(bm);
		    }
		}
		#region Bitmap

		public Color[,] GetBitMapColorMatrix(Bitmap b1)
		{
			Color[,] colorMatrix = new Color[b1.Width, b1.Height];
		    colorMatrix.ForEach((i, j) => colorMatrix[i, j] = b1.GetPixel(i, j));

			return colorMatrix;
		}

		public Bitmap SetBitMapColorMatrix(Color[,] colorMatrix)
		{
			Bitmap bm = new Bitmap(colorMatrix.GetLength(0), colorMatrix.GetLength(1));
		    colorMatrix.ForEach((i, j) => bm.SetPixel(i, j, colorMatrix[i, j]));

			return bm;
		}

		private Bitmap BitmapFromSource(BitmapSource bitmapsource)
		{
			Bitmap bitmap;
			using (MemoryStream outStream = new MemoryStream())
			{
				BitmapEncoder enc = new BmpBitmapEncoder();
				enc.Frames.Add(BitmapFrame.Create(bitmapsource));
				enc.Save(outStream);
				bitmap = new Bitmap(outStream);
			}
			return bitmap;
		}

		public BitmapSource GetBitmapSource(Bitmap bitmap)
		{
			BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap
			(
				bitmap.GetHbitmap(),
				IntPtr.Zero,
				Int32Rect.Empty,
				BitmapSizeOptions.FromEmptyOptions()
			);

			return bitmapSource;
		}

		#endregion
		#endregion
	}
}
