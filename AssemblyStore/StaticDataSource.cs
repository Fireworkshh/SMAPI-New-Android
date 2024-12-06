using ICSharpCode.SharpZipLib.Zip;

namespace Xamarin.Android.AssemblyStore
{
    internal class StaticDataSource : IStaticDataSource
    {
        private readonly Stream _stream;

        public StaticDataSource(Stream stream)
        {
            _stream = stream;
        }

        public Stream GetSource()
        {
            return _stream;
        }
    }
}