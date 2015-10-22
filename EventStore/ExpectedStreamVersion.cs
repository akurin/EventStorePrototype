using System;

namespace EventStore
{
    public sealed class ExpectedStreamVersion
    {
        private const int EmptyVersionValue = -1;
        private readonly int _version;

        public ExpectedStreamVersion(int version)
        {
            _version = version;
        }

        public static ExpectedStreamVersion Empty => new ExpectedStreamVersion(EmptyVersionValue);

        public bool IsEmpty => _version == EmptyVersionValue;

        public static ExpectedStreamVersion EqualTo(int version)
        {
            if (version < 0)
                throw new ArgumentException("Should be greater or equal to zero", nameof(version));

            return new ExpectedStreamVersion(version);
        }

        public int AsInt()
        {
            return _version;
        }
    }
}