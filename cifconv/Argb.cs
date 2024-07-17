using System.Runtime.InteropServices;

namespace cifconv
{
	[StructLayout(LayoutKind.Explicit)]
	public struct Argb
	{
		[FieldOffset(0)] public byte B;
		[FieldOffset(1)] public byte G;
		[FieldOffset(2)] public byte R;
		[FieldOffset(3)] public byte A;

		public Argb(byte a, byte r, byte g, byte b)
		{
			A = a;
			R = r;
			G = g;
			B = b;
		}
	}
}
