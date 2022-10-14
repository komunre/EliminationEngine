using EliminationEngine.GameObjects;
using EliminationEngine.Tools;
using ManagedBass;
using OpenTK.Audio.OpenAL;
using OpenTK.Mathematics;

namespace EliminationEngine.Systems
{
    public enum SoundType
    {
        Wave,
        Noise,
        Triangle,
    }
    public class SoundSystem : EntitySystem
    {
        protected ALContext? Context;

        protected List<int> Handles = new();
        protected Dictionary<int, float> Sources = new();
        protected Dictionary<int, float> Buffers = new();

        public SoundSystem(Elimination e) : base(e)
        {
        }

        public override void OnLoad()
        {
            base.OnLoad();

            if (!Bass.Init())
            {
                Logger.Error("Unable to initialize bass: " + Bass.LastError);
            }
        }

        public int CacheSound(Stream stream, bool loop = false)
        {
            var handle = Bass.CreateStream(StreamHelper.ReadFully(stream), 0, stream.Length, loop ? BassFlags.Loop : BassFlags.Default);
            if (handle == 0)
            {
                Logger.Warn("Sound handle creation error: " + Bass.LastError);
                return 0;
            }
            Handles.Add(handle);
            return handle;
        }

        public int PlaySound(Stream stream, bool loop = false)
        {
            var handle = CacheSound(stream, loop);
            if (!Bass.ChannelPlay(handle))
            {
                Logger.Warn("No sound was played: " + Bass.LastError);
            }
            return handle;
        }

        public void StopSound(int handle)
        {
            Bass.ChannelStop(handle);
            Handles.Remove(handle);
        }

        public void PauseSound(int handle)
        {
            Bass.ChannelPause(handle);
        }

        public void ResumeSound(int handle)
        {
            Bass.ChannelPlay(handle);
        }

        public void PlayCached(int handle)
        {
            Bass.ChannelPlay(handle, true);
        }

        public void GenSound(int freq, SoundType type, float ampl, float length)
        {
            if (Context == null)
            {
                var device = ALC.OpenDevice(null);
                Context = ALC.CreateContext(device, new ALContextAttributes());
            }
            ALC.MakeContextCurrent(Context.Value);
            int buffer, source;
            buffer = AL.GenBuffer();
            source = AL.GenSource();
            var data = new short[(int)(freq * length) + 1];
            switch (type)
            {
                case SoundType.Wave:
                    for (var i = 0; i < data.Length; i++)
                    {
                        data[i] = (short)(ampl * MathHelper.Sin(i * freq));
                    }
                    break;
                case SoundType.Noise:
                    for (var i = 0; i < data.Length; i++)
                    {
                        var random = new Random();
                        data[i] = (short)random.Next(0, 255);
                    }
                    break;
            }
            AL.BufferData(buffer, ALFormat.Mono16, data, freq);
            AL.Source(source, ALSourcei.Buffer, buffer);

            AL.SourcePlay(source);

            var delTime = (float)(Engine.Elapsed.TotalMilliseconds + length * 1000);
            if (Sources.ContainsKey(source))
            {
                Sources.Remove(source);
            }
            Sources.Add(source, delTime);
            if (Buffers.ContainsKey(buffer))
            {
                Buffers.Remove(buffer);
            }
            Buffers.Add(buffer, delTime);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            List<int> sourcesToRemove = new();
            foreach (var source in Sources)
            {
                if (source.Value <= Engine.Elapsed.TotalMilliseconds)
                {
                    AL.SourceStop(source.Key);
                    AL.DeleteSource(source.Key);
                    sourcesToRemove.Add(source.Key);
                }
            }
            foreach (var source in sourcesToRemove)
            {
                Sources.Remove(source);
            }

            List<int> buffersToRemove = new();
            foreach (var buffer in Sources)
            {
                if (buffer.Value <= Engine.Elapsed.TotalMilliseconds)
                {
                    AL.DeleteSource(buffer.Key);
                    sourcesToRemove.Add(buffer.Key);
                }
            }
            foreach (var buffer in buffersToRemove)
            {
                Sources.Remove(buffer);
            }
        }
    }
}
