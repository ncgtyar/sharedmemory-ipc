using System;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading;

namespace sharedmemoryipc
{
    public class SharedMemoryString
    {
        private readonly string _map;
        private readonly int _capacity;
        private MemoryMappedFile _mmf;

        public SharedMemoryString(string map, int capacity = 1024)
        {
            _map = map;
            _capacity = capacity;
            _mmf = MemoryMappedFile.CreateOrOpen(_map, _capacity);
        }

        public void Write(string message)
        {
            if (_mmf == null)
                throw new ObjectDisposedException(nameof(SharedMemoryString));

            var accessor = _mmf.CreateViewAccessor(0, _capacity);
            byte[] bytes = Encoding.UTF8.GetBytes(message);

            if (bytes.Length + 1 > _capacity)
                throw new ArgumentException("Message too large for shared memory capacity");

            accessor.Write(0, (byte)bytes.Length);           //Store length at byte 0
            accessor.WriteArray(1, bytes, 0, bytes.Length);  //Store string bytes starting at byte 1
        }

        public string Read()
        {
            if (_mmf == null)
                throw new ObjectDisposedException(nameof(SharedMemoryString));

            var accessor = _mmf.CreateViewAccessor(0, _capacity);
            byte length = accessor.ReadByte(0);
            if (length == 0 || length >= _capacity)
                return null;  //Nothing written or invalid length

            byte[] bytes = new byte[length];
            accessor.ReadArray(1, bytes, 0, length);

            return Encoding.UTF8.GetString(bytes);
        }
    }
}
