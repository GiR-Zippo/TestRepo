﻿/*
 * Copyright(c) 2023 MoogleTroupe, isaki, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/BardMusicPlayer/BardMusicPlayer/blob/develop/LICENSE for full license information.
 */

using BardMusicPlayer.Quotidian;

namespace BardMusicPlayer.Coffer;

public class BmpCofferException : BmpException
{
    public BmpCofferException(string message) : base(message) { }

    public BmpCofferException(string message, Exception inner) : base(message, inner) { }
}