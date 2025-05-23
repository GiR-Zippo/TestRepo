/*
 * Copyright(c) 2025 GiR-Zippo, 2021 MoogleTroupe
 * Licensed under the GPL v3 license. See https://github.com/GiR-Zippo/LightAmp/blob/main/LICENSE for full license information.
 */

namespace BardMusicPlayer.Seer.Events
{
    public sealed class IsLoggedInChanged : SeerEvent
    {
        internal IsLoggedInChanged(EventSource readerBackendType, bool status) : base(readerBackendType)
        {
            EventType = GetType();
            IsLoggedIn = status;
        }

        public bool IsLoggedIn { get; }

        public override bool IsValid()
        {
            return true;
        }
    }
}