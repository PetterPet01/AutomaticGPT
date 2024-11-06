using Cyotek.Collections.Generic;
using DlibDotNet;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using PetterPet.CNNVAD.Frequency;
using PetterPet.CNNVAD.Time;
using PetterPet.CNNVAD.Ultilities;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using UltraFaceDotNet;

namespace AutomaticGPTTest1
{
    internal class FaceShakeDetection : IDisposable
    {
        static readonly int stftSize = 48;
        static double[] filterCoeff = new double[30] {
  -0.00848107235259646,
  0.0019324128215858214,
  -0.0014610875856629118,
  -0.011440672922755609,
  -0.017551103015994326,
  -0.00007846740547810773,
  0.044760456766847506,
  0.08511837557511757,
  0.07034121508879851,
  -0.01856355024788633,
  -0.1322597761539009,
  -0.17795066212912017,
  -0.09765430412550433,
  0.06897757046564425,
  0.20368404468730067,
  0.20368404468730067,
  0.06897757046564425,
  -0.09765430412550433,
  -0.17795066212912017,
  -0.1322597761539009,
  -0.01856355024788633,
  0.07034121508879851,
  0.08511837557511757,
  0.044760456766847506,
  -0.00007846740547810773,
  -0.017551103015994326,
  -0.011440672922755609,
  -0.0014610875856629118,
  0.0019324128215858214,
  -0.00848107235259646
};
        static readonly float[] window = FIRFilterBuilder.ComputeWindowF(stftSize, WindowType.Hann);
        static Point3f[] model_points = new Point3f[]
{
                        new (0.0f, 0.0f, 0.0f),
                        new (0.0f, -330.0f, -65.0f),
                        new (-225.0f, 170.0f, -135.0f),
                        new (225.0f, 170.0f, -135.0f),
                        new (-150.0f, -150.0f, -125.0f),
                        new (150.0f, -150.0f, -125.0f)
};
        static Point3f[] nose_end_point3D = new Point3f[]
            {
                        new (0.0f, 0.0f, 1000.0f)
            };
        static float confThreshold = 0.7f;


        int xy = 1;
        int xz = 1;

        OneEuroFilter zSmoothY = new OneEuroFilter(1, 0, minCutoff: 0.05f, beta: 0.7f);
        OneEuroFilter zSmoothZ = new OneEuroFilter(1, 0, minCutoff: 0.05f, beta: 0.7f);
        MovingAverageBuffer mabY = new MovingAverageBuffer(12);
        MovingAverageBuffer mabZ = new MovingAverageBuffer(12);
        CircularBuffer<float> yy = new CircularBuffer<float>(stftSize);
        MovingAverageBuffer mabY2 = new MovingAverageBuffer();

        SpecializedTransform transform;
        float[] prevyy;

        ShapePredictor predictor;
        UltraFace ultraFace;

        Image? prevImg = null;
        byte[] data = new byte[0];

        Recorder recorder;

        public event EventHandler HeadShakeDetected;

        public Array2D<byte> ToArray2D(Bitmap bitmap)
        {
            int stride;
            //byte[] data;
            int width = bitmap.Width;
            int height = bitmap.Height;

            //Potential issues
            using (Bitmap grayImage = new Bitmap(bitmap))
            {
                BitmapData bits = grayImage.LockBits(
                    new System.Drawing.Rectangle(0, 0, width, height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);
                stride = bits.Stride;
                int length = stride * height;
                if (this.data.Length != length)
                    data = new byte[length];
                Marshal.Copy(bits.Scan0, data, 0, length);
                grayImage.UnlockBits(bits);
            }

            Array2D<byte> array = new Array2D<byte>(height, width);
            int offset = 0;

            for (int y = 0; y < height; y++)
            {
                int curOffset = offset;
                Array2D<byte>.Row<byte> curRow = array[y];

                for (int x = 0; x < width; x++)
                {
                    curRow[x] = (byte)((data[curOffset] + data[curOffset + 1] + data[curOffset + 2]) / 3);
                    //curRow[x] = data[curOffset];
                    curOffset += 4;
                }

                offset += stride;
                curRow.Dispose();
            }

            return array;
        }
        float PredictHeadShake(System.Drawing.Point[] landmark,
    int rows, int cols)
        {
            List<(Point2f, Point2f)> noseEndPoints = new List<(Point2f, Point2f)>();

            Point2f[] image_points = new Point2f[]
            {
                        new (landmark[30].X, landmark[30].Y),
                        new (landmark[8].X, landmark[8].Y),
                        new (landmark[45].X, landmark[45].Y),
                        new (landmark[36].X, landmark[36].Y),
                        new (landmark[54].X, landmark[54].Y),
                        new (landmark[48].X, landmark[48].Y)
            };

            int focal_length = cols;
            PointF center = new PointF(cols / 2, rows / 2);
            double[,] cameraMatrix = new double[3, 3]
            {
                        {focal_length, 0 , center.X},
                        {0, focal_length , center.Y},
                        {0, 0 , 1},
            };
            double[] dist_coeffs = new double[4];
            double[] rotation_vector = new double[3];
            double[] translation_vector = new double[3];
            OpenCvSharp.Cv2.SolvePnP(model_points, image_points, cameraMatrix,
                null, ref rotation_vector, ref translation_vector);

            double[,] rmat, jac, mtxR, mtxQ, qx, qy, qz;
            OpenCvSharp.Cv2.Rodrigues(rotation_vector, out rmat, out jac);
            var vec = OpenCvSharp.Cv2.RQDecomp3x3(rmat, out mtxR, out mtxQ, out qx, out qy, out qz);


            xy++;
            xz++;
            float resY = zSmoothY.Call(xy, (float)vec.Item1);

            float resZ = zSmoothZ.Call(xz, (float)vec.Item2);

            mabY.addDatum(resY);
            mabZ.addDatum(resZ);

            yy.Put(mabY.movingAverage + Math.Abs(mabZ.movingAverage));

            if (yy.Size >= stftSize)
            {
                float[] curyy = yy.Get(stftSize / 2);
                if (prevyy == null)
                {
                    prevyy = curyy;
                    return 0f;
                }

                float[] inputy = new float[stftSize];

                Array.Copy(prevyy, 0, inputy, 0, stftSize / 2);
                Array.Copy(curyy, 0, inputy, stftSize / 2, stftSize / 2);

                Array.Copy(curyy, 0, prevyy, 0, stftSize / 2);

                float[] yyFFT = new float[inputy.Length + 2];

                transform.FFTConvolution(inputy, ref yyFFT, window);

                yyFFT = SpecializedTransform.GetPowerSpectrum(yyFFT);

                int decimatedLen = (int)Math.Round(4d * 34 / stftSize);
                int decimatedStart = 3 * 34 / stftSize;

                float[] resulty = new float[decimatedLen];

                for (int i = 0; i < decimatedLen; i++)
                {
                    mabY2.addDatum(yyFFT[i + decimatedStart]);
                    resulty[i] = mabY2.movingAverage;
                }

                //Debug.WriteLine(resulty.Sum() * 10);
                return resulty.Max();
            }
            return 0f;
        }

        int[] stopIndecies = new int[]
        {
            16, 21, 26, 30, 35, 41, 47, 59, 67
        };
        private bool disposedValue;

        FaceInfo[] ProcessImage(OpenCvSharp.Mat oriFrame)
        {
            Bitmap bitmap = oriFrame.ToBitmap();
            //Array2D<byte> array2DFrame = ToArray2D(bitmap);

            if (prevImg != null) prevImg.Dispose();
            prevImg = bitmap;
            using (var frame = new NcnnDotNet.OpenCV.Mat(oriFrame.Rows, oriFrame.Cols, (int)oriFrame.Type(), oriFrame.Data))
            {
                using (var inMat = NcnnDotNet.Mat.FromPixels(frame.Data, NcnnDotNet.PixelType.Bgr2Rgb, frame.Cols, frame.Rows))
                {
                    return ultraFace.Detect(inMat).ToArray();
                }
            }
        }

        DlibDotNet.Rectangle[] GetFaceRects(FaceInfo[] faces, int offset = 20)
        {
            DlibDotNet.Rectangle[] result = new DlibDotNet.Rectangle[faces.Length];
            for (var j = 0; j < faces.Length; j++)
            {
                var face = faces[j];
                result[j] = new DlibDotNet.Rectangle(
                            (int)face.X1 - offset, (int)face.Y1,
                            (int)face.X2 + offset, (int)face.Y2);
            }
            return result;
        }

        System.Drawing.Point[] DetectLandmark(Array2DBase array2D, DlibDotNet.Rectangle rect)
        {
            System.Drawing.Point[] result = new System.Drawing.Point[68];

            FullObjectDetection landmarks = predictor.Detect(array2D, rect);
            for (uint i = 0; i < 68; i++)
            {
                int x = landmarks.GetPart(i).X;
                int y = landmarks.GetPart(i).Y;
                result[i] = new System.Drawing.Point(x, y);
            }
            return result;
        }

        void OnFrameCaptured(object? sender, OpenCvSharp.Mat frame)
        {
            FaceInfo[] faceInfos = ProcessImage(frame);

            Bitmap bitmap = frame.ToBitmap();

            DlibDotNet.Rectangle[] rects = GetFaceRects(faceInfos, 5);
            using (Array2DBase array2D = ToArray2D(bitmap))
            {
                foreach (DlibDotNet.Rectangle rect in rects)
                {
                    System.Drawing.Point[] landmark = DetectLandmark(array2D, rect);
                    float confidence = PredictHeadShake(landmark, frame.Rows, frame.Cols) * 10;
                    //Debug.WriteLine(confidence);
                    if (confidence >= confThreshold)
                        HeadShakeDetected?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        public FaceShakeDetection(string ultraFaceBinPath, string ultraFaceParamPath, string landmarkPredictorPath)
        {
            UltraFaceParameter parameter = new UltraFaceParameter
            {
                BinFilePath = ultraFaceBinPath,
                ParamFilePath = ultraFaceParamPath,
                InputWidth = 320,
                InputLength = 240,
                NumThread = 1,
                ScoreThreshold = 0.7f
            };
            ultraFace = UltraFace.Create(parameter);

            predictor = ShapePredictor.Deserialize(landmarkPredictorPath);

            recorder = new Recorder(0, 320, 240, 30);
            recorder.FrameCaptured += OnFrameCaptured;
            recorder.StartRecording();

            float[] filterCoeffF = new float[filterCoeff.Length];
            for (int i = 0; i < filterCoeff.Length; i++)
                filterCoeffF[i] = (float)filterCoeff[i];

            transform = new SpecializedTransform(stftSize, filterCoeffF, filterCoeffF.Length, 1);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    recorder.Dispose();
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
    }
}
