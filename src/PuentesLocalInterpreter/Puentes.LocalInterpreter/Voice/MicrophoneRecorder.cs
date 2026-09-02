using System;
using System.IO;
using NAudio.Wave;

namespace Puentes.LocalInterpreter.Voice;

/// <summary>Captures microphone audio as 16kHz mono PCM WAV, the format Whisper.net expects.</summary>
public static class MicrophoneRecorder
{
    private const int SampleRateHz = 16000;

    public static MemoryStream RecordUntilKeyPress(string stopKeyLabel = "ENTER")
    {
        var waveFormat = new WaveFormat(SampleRateHz, 16, 1);
        var memoryStream = new MemoryStream();

        using (var waveIn = new WaveInEvent { WaveFormat = waveFormat })
        using (var writer = new WaveFileWriter(
                   new IgnoreDisposeStream(memoryStream),
                   waveFormat))
        {
            waveIn.DataAvailable += (_, e) =>
                writer.Write(e.Buffer, 0, e.BytesRecorded);

            Console.WriteLine($"Grabando... presioná {stopKeyLabel} para detener.");

            waveIn.StartRecording();
            Console.ReadLine();
            waveIn.StopRecording();
        }
        // writer.Dispose() above finalizes the RIFF header sizes; only safe to rewind after that

        memoryStream.Position = 0;

        return memoryStream;
    }

    /// <summary>Prevents WaveFileWriter's Dispose from closing the underlying MemoryStream.</summary>
    private sealed class IgnoreDisposeStream : Stream
    {
        private readonly Stream _inner;

        public IgnoreDisposeStream(Stream inner) => _inner = inner;

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => _inner.CanWrite;
        public override long Length => _inner.Length;

        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        public override void Flush() => _inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void SetLength(long value) => _inner.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);

        protected override void Dispose(bool disposing)
        {
            // no-op: caller keeps ownership of the MemoryStream
        }
    }
}
