using System;

namespace SimpleSyncPlugin.Threading
{
    public class SessionInfo
    {
        public string SessionId { get; set; }
    }

    public class SessionChangedEventArgs : EventArgs
    {
        public SessionChangedEventArgs(SessionInfo oldSession, SessionInfo newSession)
        {
            OldSession = oldSession;
            NewSession = newSession;
        }

        public SessionInfo OldSession { get; }
        public SessionInfo NewSession { get; }
    }

    public static class SessionManager
    {
        private static SessionInfo _currentSession;

        public static event EventHandler CurrentSessionChanged;

        public static SessionInfo CurrentSession
        {
            get => _currentSession;
            set
            {
                if (ReferenceEquals(_currentSession, value))
                    return;

                var oldSession = _currentSession;
                _currentSession = value;
                OnCurrentSessionChanged(oldSession, value);
            }
        }

        private static void OnCurrentSessionChanged(SessionInfo oldSession, SessionInfo newSession)
        {
            CurrentSessionChanged?.Invoke(
                null,
                new SessionChangedEventArgs(oldSession, newSession));
        }
    }
}