using System.IO;

namespace EasyTotp
{
	internal class ByteBuffer
	{
		private readonly byte[] buffer;
		private readonly BinaryWriter bw;

		private ByteBuffer(int v)
		{
			buffer = new byte[v];
			var ms = new MemoryStream(buffer);
			bw = new BinaryWriter(ms);
		}

		public ByteBuffer putLong(long v)
		{
			bw.Write(v);
			return this;
		}

		public byte[] array()
		{
			return buffer;
		}

		public static ByteBuffer allocate(int byteSize)
		{
			return new ByteBuffer(byteSize);
		}
	}
}