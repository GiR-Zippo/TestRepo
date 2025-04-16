﻿/*
 * Copyright(c) 2023 MoogleTroupe
 * Licensed under the GPL v3 license. See https://github.com/BardMusicPlayer/BardMusicPlayer/blob/develop/LICENSE for full license information.
 */

using System.Collections.Concurrent;
using BardMusicPlayer.Quotidian.Structs;
using BardMusicPlayer.Transmogrify.Processor.Utilities;
using BardMusicPlayer.Transmogrify.Song;
using BardMusicPlayer.Transmogrify.Song.Config;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace BardMusicPlayer.Transmogrify.Processor;

internal class ClassicProcessor : BaseProcessor
{
    public ClassicProcessorConfig ProcessorConfig { get; set; }

    internal ClassicProcessor(ClassicProcessorConfig processorConfig, BmpSong song) : base(song)
    {
        ProcessorConfig = processorConfig;
    }

    public override async Task<List<TrackChunk>> Process()
    {
        //Never use the sourcetracks, always use a copy
        var trackChunks = new List<TrackChunk> { (TrackChunk)Song.TrackContainers[ProcessorConfig.Track].SourceTrackChunk.Clone() }
            .Concat(ProcessorConfig.IncludedTracks.Select(track => (TrackChunk)Song.TrackContainers[track].SourceTrackChunk.Clone()))
            .ToList();

        //convert progchanges to lower notes, if it's a guitar
        if (ProcessorConfig.Instrument.InstrumentTone.Index == InstrumentTone.ElectricGuitar.Index)
        {

            foreach (var timedEvent in trackChunks.GetTimedEvents())
            {
                if (timedEvent.Event is not ProgramChangeEvent programChangeEvent)
                    continue;

                //Skip all except guitar
                if (programChangeEvent.ProgramNumber < 27 || programChangeEvent.ProgramNumber > 31)
                    continue;

                var number = (int)Instrument.ParseByProgramChange(programChangeEvent.ProgramNumber).InstrumentToneMenuKey;
                using var manager = trackChunks.Merge().ManageNotes();
                var note = new Note((SevenBitNumber)number);
                var timedEvents = manager.Objects;
                note.Time = timedEvent.Time;
                timedEvents.Add(note);
            }
        }

        var trackChunk = (await 
            trackChunks.GetNoteDictionary(Song.SourceTempoMap, ProcessorConfig.Instrument.InstrumentTone,
                    ProcessorConfig.OctaveRange.LowerNote, 
                    ProcessorConfig.OctaveRange.UpperNote, 
                    (int) ProcessorConfig.Instrument.InstrumentToneMenuKey, 
                    true,
                    -ProcessorConfig.OctaveRange.LowerNote)
                .MoveNoteDictionaryToDefaultOctave(ProcessorConfig.OctaveRange)
                .ConcatNoteDictionaryToList()).ToTrackChunk();

        var playerNotesDictionary = await trackChunk.GetPlayerNoteDictionary(ProcessorConfig.PlayerCount, OctaveRange.C3toC6.LowerNote, OctaveRange.C3toC6.UpperNote);
        var concurrentPlayerTrackDictionary = new ConcurrentDictionary<long, TrackChunk>(ProcessorConfig.PlayerCount, ProcessorConfig.PlayerCount);

        Parallel.ForEach(playerNotesDictionary.Values, async (notesDictionary, _, iteration) =>
            {
                concurrentPlayerTrackDictionary[iteration] = (await notesDictionary.ConcatNoteDictionaryToList().FixChords().OffSet50Ms().FixEndSpacing()).ToTrackChunk();
                concurrentPlayerTrackDictionary[iteration].AddObjects(new List<ITimedObject>{new TimedEvent(new SequenceTrackNameEvent("tone:" + ProcessorConfig.Instrument.InstrumentTone.Name))});
            }
        );
        trackChunks = concurrentPlayerTrackDictionary.Values.ToList();
        return trackChunks;
    }
}