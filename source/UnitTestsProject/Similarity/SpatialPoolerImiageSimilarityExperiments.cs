
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
    }
    private HtmConfig CreateConfig(int imgSize, int numOfCols, int[] colDims, double minOctOverlapCycles, double maxBoost)
        {
            return new HtmConfig(new int[] { imgSize, imgSize }, new int[] { numOfCols });
        }
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
                MinPctOverlapDutyCycles = minOctOverlapCycles
            };
        }
    }