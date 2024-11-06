//
//  Copyright © 2023 Ho Minh Quan. All rights reserved.
//

using Cyotek.Collections.Generic;
using NAudio.Wave;
using PetterPet.FFTSSharp;
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;
using System.Timers;
using TensorFlow;

namespace PetterPet.CNNVAD.Time
{
    using static audioProcessing;

    public class FrameProcessedEventArgs
    {
        public bool IsSpeaking { get; set; }
        public byte[] Data { get; set; }
        public byte[] Append { get; set; }
    }

    public class AudioRecorder : IDisposable
    {
        #region Tensorflow
        TFSession model;

        TFGraph graph = new TFGraph();

        string inputOpName = "inputs/x-input";
        string outputOpName = "model/Softmax";
        #endregion

        #region Audio
        WaveIn waveIn;
        public Variables memoryPointer;

        public static readonly float capSecs = 0.0125f; //600 and 48000 is the default frameSize and sampleRate, which i'm calculating the seconds audio captures rate 
        static readonly float maxCapSecs = 0.0135f; //13.5ms maximum, boundary for fft size
        int bufferAppendLength;
        int FRAMESIZE = 600;
        int SAMPLINGFREQUENCY;
        CircularBuffer<byte> buffer = new CircularBuffer<byte>(2048 * 16 * 2);
        Channel<byte[]> bufferQueue = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(80)
        {
            AllowSynchronousContinuations = false,
            FullMode = BoundedChannelFullMode.DropOldest,
        });
        CircularBuffer<byte> bufferAppendQueue;
        public MovingAverageBuffer predictBuffer;
        #endregion

        #region Processing
        bool isSessionOccupied;

        System.Timers.Timer inferenceTimer = new System.Timers.Timer();
        int CNNTime = 62;
        #endregion

        public ActionBlock<FrameProcessedEventArgs> FrameProcessed;

        protected virtual void OnFrameProcessed(FrameProcessedEventArgs e)
        {
            //if (e.IsSpeaking)
            //    Debug.WriteLine("processing frame");
            if (!FrameProcessed.Post(e))
                Debug.WriteLine("Failed to write buffer to Channel FrameProcessed");
        }

        private bool disposedValue;

        void loadGraphFromPath(string modelPath)
        {
            graph.Import(File.ReadAllBytes(modelPath));
        }
        void createSession()
        {
            model = new TFSession(graph);
        }
        // This is called periodically by the media server.
        public void processAudio(object? sender, WaveInEventArgs e /*float[] audio*/)
        {
            //int bufferLength = e.BytesRecorded;
            float[] inputBufferFloat;

            //float[] audio = new float[bufferLength / 2];
            //for (int i = 0; i < bufferLength; i += 2)
            //{
            //    audio[i / 2] = (short)((e.Buffer[i + 1] << 8) | e.Buffer[i + 0]) * SHORT2FLOAT;  //Instead of diving by 32767
            //}
            buffer.Put(e.Buffer);
            if (buffer.Size >= FRAMESIZE * 2)
            {
                inputBufferFloat = new float[FRAMESIZE];
                byte[] bufferB = buffer.Get(FRAMESIZE * 2);
                byte[] copy = new byte[bufferB.Length];
                Buffer.BlockCopy(bufferB, 0, copy, 0, bufferB.Length);
                if (bufferQueue.Reader.Count > 8) Debug.WriteLine(bufferQueue.Reader.Count);
                if (!bufferQueue.Writer.TryWrite(copy))
                    Debug.WriteLine("Failed to write buffer to Channel");
                for (int i = 0; i < FRAMESIZE * 2; i += 2)
                {
                    inputBufferFloat[i / 2] = (short)((bufferB[i + 1] << 8) | bufferB[i + 0]) * SHORT2FLOAT;  //Instead of diving by 32767
                }
            }
            else
                return;
            //if (buffer.Size >= FRAMESIZE)
            //    if (prevBufferFloat != null)
            //        inputBufferFloat = buffer.Get(FRAMESIZE);
            //    else
            //    {
            //        prevBufferFloat = buffer.Get(FRAMESIZE);
            //        return;
            //    }
            //else
            //    return;
            //inputBufferFloat = prevBufferFloat.Concat(inputBufferFloat);
            //prevBufferFloat = inputBufferFloat.SubArray(FRAMESIZE, FRAMESIZE);
            compute(ref memoryPointer, inputBufferFloat);
        }
        void predict(object? sender, ElapsedEventArgs e)
        {

            //float[,] input = new float[1, 1600];
            //for (int i = 0; i < 40; i++)
            //    for (int j = 0; j < 40; j++)
            //        input[0, 40 * i + j] = memoryPointer.melSpectrogram.melSpectrogramImage[i, j];

            //System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            //watch.Start();
            if (isSessionOccupied) return;
            isSessionOccupied = true;
            var tensor = new TFTensor(memoryPointer.melSpectrogram.melSpectrogramImage);
            TFSession.Runner runner = model.GetRunner();

            runner.AddInput(inputOpName, tensor);
            runner.Fetch(outputOpName);
            var output = runner.Run();
            //watch.Stop();

            TFTensor result = output[0];
            var realRes = (float[,])result.GetValue();
            predictBuffer.addDatum(realRes[0, 1]);

            //Console.WriteLine(predictBuffer.movingAverage);
            isSessionOccupied = false;
            //watch.Stop();

            //Debug.WriteLine(watch.ElapsedMilliseconds);
        }

        async ValueTask Consume(ChannelReader<byte[]> reader)
        {
            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out byte[]? currentBuffer))
                {
                    byte[] tmp = new byte[currentBuffer.Length < bufferAppendLength ? currentBuffer.Length : bufferAppendLength];
                    Buffer.BlockCopy(currentBuffer, currentBuffer.Length - tmp.Length, tmp, 0, tmp.Length);

                    if (currentBuffer.Length > 0 && bufferAppendQueue.Size > 0)
                        OnFrameProcessed(new FrameProcessedEventArgs
                        {
                            IsSpeaking = predictBuffer.movingAverage > 0.5,
                            Data = currentBuffer,
                            Append = bufferAppendQueue.Peek(bufferAppendQueue.Size)
                        });
                    bufferAppendQueue.Put(tmp);
                }
            }
        }

        public void start(string modelPath, int sampleFreq, Action<FrameProcessedEventArgs> action, float bufferAppendSecs = 0.0125f)
        {
            FFTSManager.LoadAppropriateDll();

            //Tensorflow
            loadGraphFromPath(modelPath);
            createSession();

            //Variable Init
            SAMPLINGFREQUENCY = sampleFreq;

            FRAMESIZE = (int)(sampleFreq * capSecs);
            memoryPointer = initialize(SAMPLINGFREQUENCY, FRAMESIZE, (int)(maxCapSecs * SAMPLINGFREQUENCY));

            predictBuffer = new MovingAverageBuffer(5);

            inferenceTimer = new System.Timers.Timer(CNNTime);
            inferenceTimer.Elapsed += predict;
            inferenceTimer.Start();

            bufferAppendLength = (int)(SAMPLINGFREQUENCY * bufferAppendSecs * 2);
            bufferAppendQueue = new CircularBuffer<byte>(bufferAppendLength);
            //reader = FrameProcessed.Reader;
            FrameProcessed = new ActionBlock<FrameProcessedEventArgs>(action, new ExecutionDataflowBlockOptions
            {
                EnsureOrdered = true,
                SingleProducerConstrained = true,
                MaxDegreeOfParallelism = 1
            });

            //Recording Settings
            waveIn = new WaveIn();
            waveIn.DeviceNumber = 0;
            waveIn.DataAvailable += processAudio;
            waveIn.WaveFormat = new NAudio.Wave.WaveFormat(sampleFreq, 1);
            waveIn.BufferMilliseconds = (int)Math.Floor(capSecs * 1000);
            waveIn.StartRecording();

            Consume(bufferQueue.Reader);
        }

        /// <summary>
        /// Stop all operations and release all variables without actually disposing
        /// </summary>
        public void stop()
        {
            model.CloseSession();
            waveIn.Dispose();
            inferenceTimer.Stop();
        }
        /// <summary>
        /// Stop receiving new audio data
        /// </summary>
        public void pause()
        {
            waveIn.StopRecording();
        }
        /// <summary>
        /// Continue receiving new audio data
        /// </summary>
        public void resume()
        {
            waveIn.StartRecording();
            inferenceTimer.Start();
        }


        #region Dispose Implementation
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (inferenceTimer != null)
                        inferenceTimer.Dispose();
                    if (model != null)
                        model.Dispose();
                    if (graph != null)
                        graph.Dispose();
                    if (waveIn != null)
                        waveIn.Dispose();
                    //if (memoryPointer != null)
                    //    memoryPointer.Dispose();
                    if (memoryPointer != null)
                        memoryPointer.Dispose();
                    bufferQueue.Writer.Complete();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
