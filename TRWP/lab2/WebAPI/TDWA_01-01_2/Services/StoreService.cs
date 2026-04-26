using TDWA_01_01_2.Models;

namespace TDWA_01_01_2.Services
{
    public class StoreService
    {
        private static Response? _store;
        private static readonly object _lock = new();

        public static Response? Get()
        {
            lock (_lock)
            {
                return _store;
            }
        }

        public static void Set(Response response)
        {
            lock (_lock)
            {
                _store = response;
            }
        }

        public static bool Exists()
        {
            lock (_lock)
            {
                return _store != null;
            }
        }

        public static bool Delete()
        {
            lock (_lock)
            {
                if (_store == null) return false;
                _store = null;
                return true;
            }
        }
    }
}
