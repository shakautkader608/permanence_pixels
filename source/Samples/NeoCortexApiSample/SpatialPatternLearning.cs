using NeoCortex;
using NeoCortexApi;
using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApi.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NeoCortexApiSample
{
    /// <summary>
    /// Implements an experiment that demonstrates how to learn spatial patterns.
    /// The Spatial Pooler (SP) learns every presented input over multiple iterations.
    /// </summary>
    public class SpatialPatternLearning
    {
        public void Run()
        {
            Console.WriteLine($"Hello NeocortexApi! Experiment {nameof(SpatialPatternLearning)}");

            // Parameters for Homeostatic Plasticity and boosting
            double minOctOverlapCycles = 1.0;
            double maxBoost = 5.0;

            // Define the number of input bits and columns
            int inputBits = 200;
            int numColumns = 1024;

            // Setup configuration for the experiment
            HtmConfig cfg = new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
            {
                CellsPerColumn = 10,
                MaxBoost = maxBoost,
                DutyCyclePeriod = 100,
                MinPctOverlapDutyCycles = minOctOverlapCycles,
                GlobalInhibition = false,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = (int)(0.15 * inputBits),
                LocalAreaDensity = -1,
                ActivationThreshold = 10,
                MaxSynapsesPerSegment = (int)(0.01 * numColumns),
                Random = new ThreadSafeRandom(42),
                StimulusThreshold = 10,
            };

            double max = 100;

            // Typical encoder settings
            Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 15},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
                { "MaxVal", max}
            };

            EncoderBase encoder = new ScalarEncoder(settings);

            // Generate 100 random input values
            List<double> inputValues = new List<double>();
            for (int i = 0; i < (int)max; i++)
            {
                inputValues.Add((double)i);
            }

            // Run the experiment
            var sp = RunExperiment(cfg, encoder, inputValues);

            // Run restructuring experiment for analysis
            RunRustructuringExperiment(sp, encoder, inputValues);
        }

        /// <summary>
        /// Implements the spatial pattern learning experiment.
        /// </summary>
        private static SpatialPooler RunExperiment(HtmConfig cfg, EncoderBase encoder, List<double> inputValues)
        {
            // Initialize HTM memory
            var mem = new Connections(cfg);
            bool isInStableState = false;

            // Homeostatic Plasticity Controller (HPC) for stability check
            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, inputValues.Count * 40,
                (isStable, numPatterns, actColAvg, seenInputs) =>
                {
                    isInStableState = isStable;
                    Debug.WriteLine(isStable ? "STABLE STATE" : "INSTABLE STATE");
                });

            // Initialize the Spatial Pooler (SP)
            SpatialPooler sp = new SpatialPooler(hpa);
            sp.Init(mem, new DistributedMemory() { ColumnDictionary = new InMemoryDistributedDictionary<int, NeoCortexApi.Entities.Column>(1) });

            // Define Cortex Layer
            CortexLayer<object, object> cortexLayer = new CortexLayer<object, object>("L1");
            cortexLayer.HtmModules.Add("encoder", encoder);  // Encoder module
            cortexLayer.HtmModules.Add("sp", sp);  // Spatial Pooler module

            double[] inputs = inputValues.ToArray();
            Dictionary<double, int[]> prevActiveCols = new Dictionary<double, int[]>();
            Dictionary<double, double> prevSimilarity = new Dictionary<double, double>();

            // Initialize previous values for similarity and active columns
            foreach (var input in inputs)
            {
                prevSimilarity.Add(input, 0.0);
                prevActiveCols.Add(input, new int[0]);
            }

            int maxSPLearningCycles = 1000;
            int numStableCycles = 0;

            // Learning process for 1000 cycles
            for (int cycle = 0; cycle < maxSPLearningCycles; cycle++)
            {
                Debug.WriteLine($"Cycle {cycle} Stability: {isInStableState}");

                // Train the model with the input pattern
                foreach (var input in inputs)
                {
                    double similarity;

                    // Compute the output from the cortex layer (SDR output)
                    var lyrOut = cortexLayer.Compute((object)input, true) as int[];
                    var activeColumns = cortexLayer.GetResult("sp") as int[];
                    var actCols = activeColumns.OrderBy(c => c).ToArray();

                    similarity = MathHelpers.CalcArraySimilarity(activeColumns, prevActiveCols[input]);
                    Debug.WriteLine($"[cycle={cycle:D4}, i={input}, cols={actCols.Length} s={similarity}] SDR: {Helpers.StringifyVector(actCols)}");

                    prevActiveCols[input] = activeColumns;
                    prevSimilarity[input] = similarity;
                }

                if (isInStableState)
                {
                    numStableCycles++;
                }

                if (numStableCycles > 5)
                    break;
            }

            return sp;
        }

        /// <summary>
        /// Runs the restructuring experiment to analyze Spatial Pooler behavior and visualize results.
        /// </summary>
        private void RunRustructuringExperiment(SpatialPooler sp, EncoderBase encoder, List<double> inputValues)
        {
            // Store heatmap data, normalized permanence, and similarities
            List<List<double>> heatmapData = new List<List<double>>();
            List<int[]> normalizedPermanence = new List<int[]>();
            List<int[]> encodedInputs = new List<int[]>();
            List<double[]> similarityList = new List<double[]>();

            foreach (var input in inputValues)
            {
                var inpSdr = encoder.Encode(input);  // Encode input
                var actCols = sp.Compute(inpSdr, false);  // Get active columns without learning

                // Reconstruct permanence values and store them
                Dictionary<int, double> reconstructedPermanence = sp.Reconstruct(actCols);
                int maxInput = inpSdr.Length;

                // Build dictionary of all permanence values
                Dictionary<int, double> allPermanenceDictionary = new Dictionary<int, double>();
                foreach (var kvp in reconstructedPermanence)
                {
                    allPermanenceDictionary[kvp.Key] = kvp.Value;
                }

                // Ensure all input indices up to maxInput are represented
                for (int inputIndex = 0; inputIndex < maxInput; inputIndex++)
                {
                    if (!reconstructedPermanence.ContainsKey(inputIndex))
                    {
                        allPermanenceDictionary[inputIndex] = 0.0;
                    }
                }

                // Sort the permanence dictionary and convert to list
                List<double> permanenceValuesList = allPermanenceDictionary.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
                heatmapData.Add(permanenceValuesList);

                Debug.WriteLine($"Input: {input} SDR: {Helpers.StringifyVector(actCols)}");

                // Normalize permanence values and add to lists
                var thresholdValue = 8.3;
                var normalizePermanenceList = Helpers.ThresholdingProbabilities(permanenceValuesList, thresholdValue);
                normalizedPermanence.Add(normalizePermanenceList.ToArray());
                encodedInputs.Add(inpSdr);

                // Measure similarity and add to similarity list
                var similarity = MathHelpers.JaccardSimilarityofBinaryArrays(inpSdr, normalizePermanenceList.ToArray());
                similarityList.Add(new double[] { similarity });

                // Draw and save heatmaps for permanence values
                string folderPath = Path.Combine(Environment.CurrentDirectory, "PermanenceHeatmaps");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, "permanence_heatmap.png");
                NeoCortexUtils.DrawCombinedHeatmapsScalerInputs(heatmapData, filePath);
                Debug.WriteLine($"Permanence heatmap saved to {filePath}");

                // Save heatmap for encoded vs permanence
                string heatmapFolder = Path.Combine(Environment.CurrentDirectory, "EncodedVsPermanenceHeatmaps");
                if (!Directory.Exists(heatmapFolder)) Directory.CreateDirectory(heatmapFolder);

                string heatmapFilePath = Path.Combine(heatmapFolder, $"heatmap_{input}.png");
                NeoCortexUtils.DrawEncodedVsReconstructedHeatmap(inpSdr, normalizePermanenceList.ToArray(), heatmapFilePath);
               Debug.WriteLine($"Encoded vs Permanence heatmap saved to {heatmapFilePath}");
            }

            // Generate similarity plot
            DrawSimilarityPlots(similarityList);
        }

        /// <summary>
        /// Draws a combined similarity plot based on the list of similarity arrays.
        /// </summary>
        public static void DrawSimilarityPlots(List<double[]> similaritiesList)
        {
            List<double> combinedSimilarities = similaritiesList.SelectMany(sim => sim).ToList();

            string folderPath = Path.Combine(Environment.CurrentDirectory, "SimilarityPlots");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, "combined_similarity_plot.png");
            NeoCortexUtils.DrawCombinedSimilarityPlot(combinedSimilarities, filePath, 4500, 1100);
            Debug.WriteLine($"Combined similarity plot generated and saved successfully to {filePath}");
        }
    }
}
