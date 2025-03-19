
 // Copyright (c) Damir Dobric. All rights reserved.
 // Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license info
 using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;






namespace UnitTestsProject
{
    [TestClass]
    [TestCategory("Experiment")]
    public class SpatialPoolerImageSimilarityExperiments
    {
        private const int OutImgSize = 1024;
        private const string TestOutputFolder = "Output-ImageSimilarityExperiment";

        private HtmConfig CreateConfig(int imgSize, int numOfCols, int[] colDims, double minOctOverlapCycles, double maxBoost)
        {
            return new HtmConfig(new int[] { imgSize, imgSize }, new int[] { numOfCols });


        private HtmConfig CreateConfig(int imgSize, int numOfCols, int[] colDims, double minOctOverlapCycles, double maxBoost)
        {
            return new HtmConfig(new int[] { imgSize, imgSize }, new int[] { numOfCols })
            {
                CellsPerColumn = 10,
                InputDimensions = new int[] { imgSize, imgSize },
                NumInputs = imgSize * imgSize,
                ColumnDimensions = colDims,
                MaxBoost = maxBoost,
                DutyCyclePeriod = 100,
                MinPctOverlapDutyCycles = minOctOverlapCycles,
                GlobalInhibition = false,
                NumActiveColumnsPerInhArea = 0.02 * numOfCols,
                PotentialRadius = (int)(0.15 * imgSize * imgSize),
                LocalAreaDensity = -1,
                ActivationThreshold = 10,
                MaxSynapsesPerSegment = (int)(0.01 * numOfCols),
                Random = new ThreadSafeRandom(42),
                StimulusThreshold = 10
            };
        }

        //Image Similarity Experiment

        [TestMethod]
        [TestCategory("LongRunning")]
        [DataRow("Vertical")]
        public void ImageSimilarityExperiment(string inputPrefix)
        {
            int imgSize = 28;
            var colDims = new int[] { 64, 64 };
            int numOfCols = colDims[0] * colDims[1];
            string trainingFolder = "Similarity\\TestFiles";
            string outFolder = $"{TestOutputFolder}\\{inputPrefix}";
            Directory.CreateDirectory(outFolder);
            var trainingImages = Directory.GetFiles(trainingFolder, $"{inputPrefix}*.png");
            HtmConfig cfg = CreateConfig(imgSize, numOfCols, colDims, 1.0, 10.0);
            var mem = new Connections(cfg);
            var sp = new SpatialPoolerMT(new HomeostaticPlasticityController(mem, trainingImages.Length * 50);
            string outputHamDistFile = $"{outFolder}\\hamming.txt";
            string outputActColFile = $"{outFolder}\\activeCol.txt";
            using (StreamWriter swHam = new StreamWriter(outputHamDistFile))
            using (StreamWriter swActCol = new StreamWriter(outputActColFile))
            {
                int counter = 0;
                var sdrs = new Dictionary<string, int[]>();
                var inputVectors = new Dictionary<string, int[]>();
                foreach (var trainingImage in trainingImages)

                {
                    int[] activeArray = new int[numOfCols];
                    int[] oldArray = new int[activeArray.Length];
                    int[] inputVector = NeoCortexUtils.ReadCsvIntegers(inputBinaryImageFile).ToArray();
                    sp.compute(inputVector, activeArray, true);
                    int[] activeCols = ArrayUtils.IndexWhere(activeArray, el => el == 1);
                    int distance = MathHelpers.GetHammingDistance(oldArray, activeArray, true);
                    sdrs[trainingImage] = activeCols;
                    inputVectors[trainingImage] = inputVector;
                    swHam.WriteLine($"{counter++}|{distance} ");
                    swActCol.WriteLine("Active Array: " + Helpers.StringifyVector(activeArray));

                }
                CalculateSimilarity(sdrs, inputVectors);

            }
        }

        //SimilarityExperimentWithEncoder

        [TestMethod]
        [TestCategory("LongRunning")]
        public void SimilarityExperimentWithEncoder()
        {
            int inputBits = 100;
            int imgSize = 28;
            var colDims = new int[] { 64 * 64 };
            int numOfActCols = colDims[0];


        }




    }


    
}


