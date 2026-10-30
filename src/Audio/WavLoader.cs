using System;
using System.IO;
using System.Text;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace MichaelJacksonRole.Audio;

/// <summary>
/// Minimal RIFF/WAVE decoder that turns a .wav file on disk into a Unity <see cref="AudioClip"/>.
///
/// We decode manually because UnityWebRequestMultimedia is not reliably available in the
/// IL2CPP build of Among Us.
/// Supports 8/16/24/32-bit integer PCM and 32-bit IEEE float, any channel count.
/// </summary>
public static class WavLoader
{
    private const ushort FormatPcm = 1;
    private const ushort FormatFloat = 3;
    private const ushort FormatExtensible = 0xFFFE;

    public static AudioClip Load(string path)
    {
        var bytes = File.ReadAllBytes(path);
        using var reader = new BinaryReader(new MemoryStream(bytes));

        if (ReadTag(reader) != "RIFF") throw new InvalidDataException("Missing RIFF header.");
        reader.ReadInt32(); // file size
        if (ReadTag(reader) != "WAVE") throw new InvalidDataException("Missing WAVE header.");

        ushort format = 0, channels = 0, bitsPerSample = 0;
        var sampleRate = 0;
        byte[] data = null;

        while (reader.BaseStream.Position + 8 <= reader.BaseStream.Length)
        {
            var chunkId = ReadTag(reader);
            var chunkSize = reader.ReadInt32();
            var chunkStart = reader.BaseStream.Position;

            switch (chunkId)
            {
                case "fmt ":
                    format = reader.ReadUInt16();
                    channels = reader.ReadUInt16();
                    sampleRate = reader.ReadInt32();
                    reader.ReadInt32();  // byte rate
                    reader.ReadUInt16(); // block align
                    bitsPerSample = reader.ReadUInt16();
                    if (format == FormatExtensible && chunkSize >= 26)
                    {
                        reader.ReadUInt16(); // cbSize
                        reader.ReadUInt16(); // valid bits per sample
                        reader.ReadInt32();  // channel mask
                        format = reader.ReadUInt16(); // first two bytes of the sub-format GUID
                    }
                    break;

                case "data":
                    data = reader.ReadBytes(chunkSize);
                    break;
            }

            // Chunks are word-aligned.
            reader.BaseStream.Position = chunkStart + chunkSize + (chunkSize & 1);
        }

        if (data == null || channels == 0 || sampleRate == 0)
            throw new InvalidDataException("WAV file has no fmt or data chunk.");

        var samples = Decode(data, format, bitsPerSample);
        var clip = AudioClip.Create(Path.GetFileNameWithoutExtension(path), samples.Length / channels, channels, sampleRate, false);
        clip.SetData(new Il2CppStructArray<float>(samples), 0);
        // Keep Unity from unloading the clip between scenes.
        clip.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        return clip;
    }

    private static float[] Decode(byte[] data, ushort format, ushort bits)
    {
        var bytesPerSample = bits / 8;
        if (bytesPerSample == 0) throw new InvalidDataException($"Unsupported bit depth: {bits}.");

        var count = data.Length / bytesPerSample;
        var samples = new float[count];

        for (var i = 0; i < count; i++)
        {
            var offset = i * bytesPerSample;
            samples[i] = (format, bits) switch
            {
                (FormatPcm, 8) => (data[offset] - 128) / 128f,
                (FormatPcm, 16) => BitConverter.ToInt16(data, offset) / 32768f,
                (FormatPcm, 24) => ((data[offset] | data[offset + 1] << 8 | (sbyte)data[offset + 2] << 16)) / 8388608f,
                (FormatPcm, 32) => BitConverter.ToInt32(data, offset) / 2147483648f,
                (FormatFloat, 32) => BitConverter.ToSingle(data, offset),
                _ => throw new InvalidDataException($"Unsupported WAV encoding (format {format}, {bits}-bit)."),
            };
        }

        return samples;
    }

    private static string ReadTag(BinaryReader reader) => Encoding.ASCII.GetString(reader.ReadBytes(4));
}
