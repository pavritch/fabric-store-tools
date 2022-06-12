using System;

namespace ProductScanner.Core.Scanning
{
    public class StaticVersionMismatchException : Exception
    {
        private readonly int _fileVersion;
        private readonly int _dllVersion;

        public StaticVersionMismatchException(int fileVersion, int dllVersion)
        {
            _fileVersion = fileVersion;
            _dllVersion = dllVersion;
        }

        public override string Message
        {
            get
            {
                if (_fileVersion > _dllVersion) 
                    return string.Format("Please update DLL (Version {0}) to match data in static root (Version {1})", _dllVersion, _fileVersion);
                if (_dllVersion > _fileVersion) 
                    return string.Format("Please update data in static root (Version {0}) to match DLL (Version {1})", _fileVersion, _dllVersion);
                return "";
            }
        }
    }
}