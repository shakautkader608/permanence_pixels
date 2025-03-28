using NeoCortex;
using NeoCortexApi.Entities;
using NeoCortexApi.Utility;
using NeoCortexApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NeoCortexApiSample
{
    internal class ImageBinarizerSpatialPattern
    {
        public string inputPrefix { get; private set; }

        /// <summary>
        /// Implements an experiment that demonstrates how to learn spatial patterns.
        /// SP will learn every presented Image input in multiple iterations.
        /// </summary>
        public void Run()
        {
            Console.WriteLine($"Hello NeocortexApi! Experiment {nameof(ImageBinarizerSpatialPattern)}");

            double minOctOverlapCycles = 1.0;
            double maxBoost = 5.0;
            int numColumns = 84 * 84;
            int imageSize = 52;
            var colDims = new int[] { 84, 84 };

            HtmConfig cfg = new HtmConfig(new int[] { imageSize, imageSize }, new int[] { numColumns })
            {
                CellsPerColumn = 10,
                InputDimensions = new int[] { imageSize, imageSize },
                NumInputs = imageSize * imageSize,
                ColumnDimensions = colDims,
                MaxBoost = maxBoost,
                DutyCyclePeriod = 100,
                MinPctOverlapDutyCycles = minOctOverlapCycles,
                GlobalInhibition = false,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = (int)(0.15 * imageSize * imageSize),
                LocalAreaDensity = -1,
                ActivationThreshold = 10,
                MaxSynapsesPerSegment = (int)(0.01 * numColumns),
                Random = new ThreadSafeRandom(42),
                StimulusThreshold = 10,
            };


            string trainingFolder = "Sample\\TestFiles";  // Folder with images
            string binarizedFolder = "Binarized";         // Folder to save binarized images
            var trainingImages = Directory.GetFiles(trainingFolder, $"{inputPrefix}*.jpg");
            // Run Experiment & get active columns per image
            var (sp, activeColumnsPerImage) = RunExperiment(cfg, inputPrefix);

            // Run Reconstruction using precomputed active columns
            RunRustructuringExperiment(sp, activeColumnsPerImage, trainingImages);

        }

        /// <summary>
        /// Implements the experiment where spatial patterns are learned.
        /// </summary>
        /// <param name="cfg">HTM configuration</param>
        /// <param name="inputPrefix"> The prefix of the image files</param>
        /// <param name="trainingFolder">Folder with image files</param>
        /// <param name="binarizedFolder">Folder to save binarized images</param>
        /// <returns>The trained spatial pooler and the list of active columns for each image</returns>
        private (SpatialPooler, List<int[]>) RunExperiment(HtmConfig cfg, string inputPrefix)
        {
            var mem = new Connections(cfg);
            bool isInStableState = false;
            int numColumns = 84 * 84; // SDR size

            string trainingFolder = "Sample\\TestFiles";  // Folder with images
            string binarizedFolder = "Binarized";         // Folder to save binarized images
            Directory.CreateDirectory(binarizedFolder);

            var trainingImages = Directory.GetFiles(trainingFolder, $"{inputPrefix}*.jpg");
            int imgSize = 52;  // Resized image size
            string testName = "test_image";

            // Initialize Homeostatic Plasticity and SP
            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, trainingImages.Length * 50,
                (isStable, numPatterns, actColAvg, seenInputs) =>
                {
                    isInStableState = isStable;
                    Debug.WriteLine($"STABLE: {isStable}, Patterns: {numPatterns}, Inputs: {seenInputs}");
                }, requiredSimilarityThreshold: 0.975
            );

            SpatialPooler sp = new SpatialPooler(hpa);
            sp.Init(mem, new DistributedMemory() { ColumnDictionary = new InMemoryDistributedDictionary<int, NeoCortexApi.Entities.Column>(1) });

            List<int[]> activeColsList = new List<int[]>(); // Store active columns for all images
            int[] activeArray = new int[numColumns];
            int maxCycles = 10;
            int currentCycle = 0;

            while (!isInStableState && currentCycle < maxCycles)
            {
                Debug.WriteLine($"--- Starting Cycle {currentCycle} ---");

                foreach (var image in trainingImages)
                {
                    // *1. Binarize Image and Save*
                    string binarizedFile = Path.Combine(binarizedFolder, $"{Path.GetFileNameWithoutExtension(image)}.txt");
                    if (!File.Exists(binarizedFile)) // Avoid reprocessing
                    {
                        string generatedFile = NeoCortexUtils.BinarizeImage(image, imgSize, testName);
                        File.Copy(generatedFile, binarizedFile, true);
                    }

                    // *2. Read Binarized Image as Input Vector*
                    int[] inputVector = NeoCortexUtils.ReadCsvIntegers(binarizedFile).ToArray();

                    for (int cycle = 0; cycle < maxCycles; cycle++)
                    {
                        Array.Clear(activeArray, 0, activeArray.Length);
                        sp.compute(inputVector, activeArray, true);
                        var activeCols = ArrayUtils.IndexWhere(activeArray, el => el == 1);

                        activeColsList.Add(activeCols);  // Store active columns

                        // Debug Output
                        Debug.WriteLine($"Cycle {currentCycle} | Image: {Path.GetFileName(image)}");
                        Debug.WriteLine($"INPUT: {Helpers.StringifyVector(inputVector)}");
                        Debug.WriteLine($"SDR  : {Helpers.StringifyVector(activeCols)}");
                        Debug.WriteLine(new string('-', 50)); // Divider for readability
                    }
                }

                currentCycle++;
            }

            // *3. Pass Active Columns to Reconstruction Experiment*
            RunRustructuringExperiment(sp, activeColsList, trainingImages);


            return (sp, activeColsList);
        }

        /// <summary>
        /// Runs a restructuring experiment on a Spatial Pooler (SP) by reconstructing permanence values, 
        /// generating heatmaps, saving binarized images, and computing similarity scores.
        /// </summary>
        /// <param name="sp">The Spatial Pooler instance used for reconstruction.</param>
        /// <param name="activeColsList">A list of active columns representing the SDRs (Sparse Distributed Representations).</param>
        /// <param name="trainingImages">An array of file paths for the training images.</param>
        /// <param name="totalCycles">The number of processing cycles. Defaults to 10.</param>
        /// <param name="imgSize">The size of the image, assumed to be square (default: 52x52).</param>
        /// <param name="thresholdValue">Threshold value for permanence normalization. Defaults to 67.0.</param>
        /// <param name="outputDirectory">The directory where output images and heatmaps will be stored. Defaults to the current directory.</param>
        private void RunRustructuringExperiment(SpatialPooler sp, List<int[]> activeColsList, string[] trainingImages)
        {
            List<int[]> normalizedPermanence = new List<int[]>();
            Dictionary<string, double> highestSimilarityPerImage = new Dictionary<string, double>(); // Stores highest similarity for each image

            int totalCycles = 10; // Number of cycles

            for (int cycleIndex = 0; cycleIndex < totalCycles; cycleIndex++)
            {
                if (cycleIndex == totalCycles - 1) // Process only the last cycle
                {
                    foreach (var actcols in activeColsList)
                    {
                        Debug.WriteLine("Reconstructing permanence for SDR...");

                        // Reconstruct permanence for SDR
                        Dictionary<int, double> reconstructedPermanence = sp.Reconstruct(actcols);
                        Dictionary<int, double> allPermanenceDictionary = new Dictionary<int, double>();

                        foreach (var kvp in reconstructedPermanence)
                        {
                            allPermanenceDictionary[kvp.Key] = kvp.Value;
                        }

                        int imgsize = 52 * 52;

                        // Assign inactive columns permanence = 0
                        for (int inputIndex = 0; inputIndex < imgsize; inputIndex++)
                        {
                            if (!reconstructedPermanence.ContainsKey(inputIndex))
                            {
                                allPermanenceDictionary[inputIndex] = 0.0;
                            }
                        }

                        // Normalize permanence values
                        var ThresholdValue = 67.0;
                        List<double> permanenceValuesList = allPermanenceDictionary.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
                        int[] currentNormalizedPermanence = Helpers.ThresholdingforResetImg(permanenceValuesList, ThresholdValue).ToArray();

                        normalizedPermanence.Add(currentNormalizedPermanence);

                        // Define a unique image index for consistency
                        int imageIndex = activeColsList.IndexOf(actcols);

                        // Generate consistent names for both images
                        string reconstructedImageName = $"ReconstructedImage_{imageIndex}";
                        string heatmapImageName = $"Heatmap_{imageIndex}";

                        // Save the reconstructed binary image
                        NeoCortexUtils.SaveBinarizedImageWithText(currentNormalizedPermanence.ToArray(), reconstructedImageName);

                        // *Save heatmap per cycle*
                        List<List<double>> heatmapData = new List<List<double>> { permanenceValuesList }; // Use sorted permanence
                        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "ImagePermanenceHeatmaps");
                        Directory.CreateDirectory(folderPath);
                        string heatmapFilePath = Path.Combine(folderPath, $"{heatmapImageName}.png");

                        NeoCortexUtils.DrawHeatmapsforImg(heatmapData, heatmapFilePath);

                        // Compare similarity for each training image
                        for (int i = 0; i < trainingImages.Length; i++)
                        {
                            string imageName = Path.GetFileNameWithoutExtension(trainingImages[i]);
                            string binarizedFile = Path.Combine("Binarized", $"{imageName}.txt");

                            if (File.Exists(binarizedFile))
                            {
                                // Read the binarized image (input vector)
                                int[] inputVector = NeoCortexUtils.ReadCsvIntegers(binarizedFile).ToArray();

                                // Compute Jaccard Similarity
                                double jaccardSim = MathHelpers.JaccardSimilarityofBinaryArrays(inputVector, currentNormalizedPermanence);

                                // Track the highest similarity for this image
                                if (!highestSimilarityPerImage.ContainsKey(imageName) || jaccardSim > highestSimilarityPerImage[imageName])
                                {
                                    highestSimilarityPerImage[imageName] = jaccardSim;
                                }

                                Debug.WriteLine($"Cycle {cycleIndex} - Image {imageName}.jpg | Jaccard Similarity: {jaccardSim}");
                            }
                            else
                            {
                                Debug.WriteLine($"Warning: Binarized file {binarizedFile} not found.");
                            }
                        }
                    }
                }
            }

            // Pass only the highest similarity values for the training images
            List<double> finalSimilarityValues = highestSimilarityPerImage.Values.ToList();
            DrawSimilarityPlots(finalSimilarityValues);
        }

        /// <summary>
        /// Generates and saves a similarity plot based on the provided highest similarity scores.
        /// </summary>
        /// <param name="highestSimilarities">A list of double values representing the highest similarity scores.</param>
        /// <param name="outputDirectory">The directory where the plot image will be saved. Defaults to the current directory under "SimilarityPlots".</param>
        /// <param name="fileName">The name of the output image file. Defaults to "highest_similarity_plot.png".</param>
        /// <param name="width">The width of the generated plot image. Defaults to 800.</param>
        /// <param name="height">The height of the generated plot image. Defaults to 1200.</param>
        // Method to draw the similarity plot
        public static void DrawSimilarityPlots(List<double> highestSimilarities)
        {
            if (highestSimilarities == null || highestSimilarities.Count == 0)
            {
                Debug.WriteLine("No similarity data available.");
                return;
            }

            // Define the folder path based on the current directory
            string folderPath = Path.Combine(Environment.CurrentDirectory, "SimilarityPlots");

            // Create the folder if it doesn't exist
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Define the file name
            string fileName = "highest_similarity_plot.png";

            // Define the file path with the folder path and file name
            string filePath = Path.Combine(folderPath, fileName);

            // Draw the similarity plot for the highest similarity scores
            NeoCortexUtils.DrawCombinedSimilarityPlot(highestSimilarities, filePath, 800, 1200);

            Debug.WriteLine($"FilePath: {filePath}");
            Debug.WriteLine("Highest similarity plot generated and saved successfully.");
        }
    }
}
