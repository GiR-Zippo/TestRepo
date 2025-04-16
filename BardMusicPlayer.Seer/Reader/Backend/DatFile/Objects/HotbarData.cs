﻿/*
 * Copyright(c) 2023 MoogleTroupe, 2018-2020 parulina
 * Licensed under the GPL v3 license. See https://github.com/BardMusicPlayer/BardMusicPlayer/blob/develop/LICENSE for full license information.
 */

using System;
using System.Collections.Generic;

namespace BardMusicPlayer.Seer.Reader.Backend.DatFile.Objects
{
    internal sealed class HotbarData : IDisposable
    {
        public Dictionary<int, HotbarRow> Rows = new();

        public HotbarRow this[int i]
        {
            get
            {
                if (!Rows.ContainsKey(i)) Rows[i] = new HotbarRow();

                return Rows[i];
            }
            set => Rows[i] = value;
        }

        public void Dispose()
        {
            if (Rows == null) return;

            foreach (var slot in Rows.Values) slot?.Dispose();

            Rows.Clear();
        }

        ~HotbarData()
        {
            Dispose();
        }
    }
}