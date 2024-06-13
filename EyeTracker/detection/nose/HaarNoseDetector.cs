﻿using Emgu.CV;
using EyeTracker.detection.utlis;

namespace EyeTracker.detection.nose
{
    internal class HaarNoseDetector : IFeatureDetector
    {
        private CascadeClassifier noseClassifier = new CascadeClassifier("./classifiers/haarcascade_mcs_nose.xml");
        private PrevDetections prevDetection = new PrevDetections(3);

        double scaleFactor = 1.25;
        int minNeighbours = 10;

        public Rectangle Detect(Mat frame)
        {
            int sectionHeight = (int)(frame.Rows * 2 / 3);
            int sectionWidth = (int)(frame.Cols / 3);

            int startX = (int)(frame.Cols * 1.0 / 3);
            int startY = (int)(frame.Rows * 1.0 / 3);

            Rectangle sectionRect = new Rectangle(startX, startY, sectionWidth, sectionHeight);
            Mat croppedImg = new Mat(frame, sectionRect);

            var noses = noseClassifier.DetectMultiScale(croppedImg, scaleFactor, minNeighbours);
            if (noses.Length == 0) return new Rectangle();

            var averagedNose = RectanglesUtil.SpatialSmoothing(noses);

            prevDetection.AddResult(averagedNose);
            var prevNoses = prevDetection.Rects.ToArray();

            var nose = RectanglesUtil.TemporalSmoothing(prevNoses, averagedNose);

            // Move rectangle to match original image coordinates
            nose.X += startX;
            nose.Y += startY;

            return nose;
        }
    }
}
