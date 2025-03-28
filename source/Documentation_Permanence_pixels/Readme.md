# ML 24/25-04 Implement the visualization of permanence value

In this documentation, we will outline our contributions to this project

#### Instruction for Running the Project

-   Clone the Repository
-   You will get the project here
    [permanence_pixels](https://github.com/shakautkader608/permanence_pixels)

#### Two Experiments

-   Link for the Project Folder here
    [NeoCortexApiSample](https://github.com/shakautkader608/permanence_pixels/tree/master/source/Samples/NeoCortexApiSample)
-   **`SpatialPatternLearning.cs`**: Numerical Inputs
    [SpatialPatternLearning.cs](https://github.com/shakautkader608/permanence_pixels/blob/master/source/Samples/NeoCortexApiSample/SpatialPatternLearning.cs)
-   **`ImageBinarizerSpatialPattern.cs`**: Image Inputs
    [ImageBinarizerSpatialPattern.cs](https://github.com/shakautkader608/permanence_pixels/blob/master/source/Samples/NeoCortexApiSample/ImageBinarizerSpatialPattern.cs)

###### The uploaded image input sets are already available here to use

-   [Test Data Link](https://github.com/shakautkader608/permanence_pixels/tree/master/source/Documentation_Permanence_pixels) Download from here and follow below steps.
-   Download the dataset sample folder from the above link then Copy the "Sample" dataset folder to the root folder of the project
-   Example C:\Users\Project\permanence_pixels\source\Samples\NeoCortexApiSample\bin\Debug\net9.0

###### Just modify the active commands here

-   **`Program.cs`**: Goto Program.cs file of NeoCortexApiSample
-   Update the codes here. Click the link below, and it will redirect you.
    [Program.cs](https://github.com/shakautkader608/permanence_pixels/blob/master/source/Samples/NeoCortexApiSample/Program.cs)

###### All the output will be saved here

-   Project\permanence_pixels\source\Samples\NeoCortexApiSample\bin\Debug\net9.0

## Introduction

Hierarchical Temporal Memory (HTM), developed by Numenta, advances our understanding of how the brain processes information, particularly in temporal pattern recognition. Inspired by the human neocortex, HTM models spatial and temporal data interactions, enabling more complex pattern recognition. A key component of HTM is the Spatial Pooler (SP), which converts raw input into Sparse Distributed Representations (SDRs), mimicking the brain’s encoding methods. This study focuses on the Reconstruct() function, a new addition to the Neocortex API, and how it enables the Spatial Pooler to regenerate input sequences. We examine the reconstruction of input patterns through two representations of Permanence values: heatmaps for image-based data and integer arrays for numerical data. This analysis enhances our understanding of the Spatial Pooler's reconstructive capabilities in HTM.

# Methodology

Our approach consists of two separate methods: one using encoded numerical values and the other utilizing images as input.

For the numerical values, the process starts by providing data ranging from 0 to 99. This numerical input is encoded into int[] arrays, each containing 200 bits after encoding, representing a series of 0s and 1s. These encoded arrays are solely used as input for the experiment, allowing us to assess the Reconstruct() function in HTM's "NeoCortexAPI" for the reconstruction of input sequences.

For the image input method, visual data is extracted from images and preprocessed for compatibility with HTM. The images are converted into numerical format, typically as pixel value arrays, and these pixel arrays are then encoded to fit the HTM framework. The encoded image representations are used as input in the experiment, enabling us to explore how the Reconstruct() function within HTM's Spatial Pooler can be applied to reconstruct image sequences.

**Fig: Methodology Flowchart**
![Methodology Flowchart](https://raw.githubusercontent.com/shakautkader608/permanence_pixels/refs/heads/master/source/Documentation_Permanence_pixels/Diagrams/Workflow.png)

## Hierarchical Temporal Memory (HTM) Spatial Pooler

The encoded int[] arrays are processed through the HTM Spatial Pooler, which transforms them into Sparse Distributed Representations (SDRs). This process involves creating a sparse and distributed encoding of the input data, where only a small fraction of the available neurons are activated, mimicking the brain’s method of processing information. These SDRs serve as a compact and efficient representation of the input patterns, forming the crucial foundation for subsequent steps in the experiment. This transformation is key for enabling the HTM framework to effectively recognize and reconstruct patterns in the data.

## Reconstruct() Method:

Using the Reconstruct() method from the NeocortexAPI, we carefully reverse the transformation of the encoded int[] arrays. The reconstructed representations are formed based on the permanence values derived from the Reconstruct() process, which guides the reformation of the original input patterns. These permanence values play a crucial role in reconstructing the precise sequence of data by adjusting the connectivity and strength of synaptic connections within the HTM framework.

```csharp
 public Dictionary<int, double> Reconstruct(int[] activeMiniColumns)
 {
     if (activeMiniColumns == null)
     {
         throw new ArgumentNullException(nameof(activeMiniColumns));
     }

     var cols = connections.GetColumnList(activeMiniColumns);

     Dictionary<int, double> permancences = new Dictionary<int, double>();


     foreach (var col in cols)
     {
         col.ProximalDendrite.Synapses.ForEach(s =>
         {
             double currPerm = 0.0;


             if (permancences.TryGetValue(s.InputIndex, out currPerm))
             {

                 permancences[s.InputIndex] = s.Permanence + currPerm;
             }
             else
             {

                 permancences[s.InputIndex] = s.Permanence;
             }
         });
     }

     return permancences;
 }
```

[Reconstruction in SP](https://github.com/shakautkader608/permanence_pixels/blob/master/source/NeoCortexApi/SpatialPooler.cs)

#### Reconstruction Method Overview:

-   **Input Validation:** The process begins by checking the integrity of the input data. If the input is invalid (null), an error is triggered. If valid, the process continues.
-   **Column Retrieval:** We identify the active mini-columns (groups of neurons) that are active in response to input patterns. This step involves creating a dictionary of permanence values for the synapses (connections between neurons) linked to these active mini-columns.
-   **Reconstruction Process:** We loop through each column and retrieve its synapses, which are connected to the input data or other columns. The permanence values of these synapses help determine the strength of their connections. The input data is reconstructed by mapping each index to its corresponding permanence value, creating a dictionary of the reconstructed input

# Running Reconstruct Method for Numerical Inputs

```
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

```

[Running Reconstruct Method for Numeric Data](https://github.com/shakautkader608/permanence_pixels/blob/master/source/Samples/NeoCortexApiSample/SpatialPatternLearning.cs#L179-L247) Lines (179 to 247)

# Running Reconstruct Method for Image Inputs

```
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

```

[Running Reconstruct Method for Image Data](https://github.com/shakautkader608/permanence_pixels/blob/master/source/Samples/NeoCortexApiSample/ImageBinarizerSpatialPattern.cs#L155-L249) Lines (155 to 249)

# Implementation specifics for both input Type():

###### Retrieve Permanence Values

-   Use the Spatial Pooler (sp) to reconstruct permanence values from the active columns.

```
Dictionary<int, double> reconstructedPermanence = sp.Reconstruct(actCols);

```

###### Determine Maximum Input Index

Set maxInput based on the size of encoded input vectors.

-   For numerical inputs, maxInput is 200 bits.

-   For image inputs, maxInput depends on height × width of the encoded image.

```
int maxInput = lengthOfInputVectors;

```

###### Initialize Dictionary to Store Permanence Values

-   This dictionary will store all input indices and their associated permanence probabilities.

```
Dictionary<int, double> allPermanenceDictionary = new Dictionary<int, double>();

```

###### Store Permanence for Active Columns

-   Add the reconstructed permanence values to the dictionary.

```
foreach (var kvp in reconstructedPermanence)
{
    int inputIndex = kvp.Key;
    double probability = kvp.Value;
    allPermanenceDictionary[inputIndex] = probability;
}

```

###### Handle Inactive Columns

-   Assign a default permanence value of 0.0 for indices not present in reconstructedPermanence.

```
for (int inputIndex = 0; inputIndex < maxInput; inputIndex++)
{
    if (!reconstructedPermanence.ContainsKey(inputIndex))
    {
        allPermanenceDictionary[inputIndex] = 0.0;
    }
}


```

###### Final Note

-   `reconstructedPermanence` is only a subset of all possible permanence values.

-   `allPermanenceDictionary` ensures that all input indices are represented, even if they were not part of the active columns.

This structure ensures completeness by including both active and inactive columns in the final permanence dictionary.

## Collecting Data for Result Visualization.

```csharp
 List<List<double>> heatmapData = new List<List<double>>();
 List<int[]> normalizedPermanence = new List<int[]>();
 List<int[]> encodedInputs = new List<int[]>();
 List<double[]> similarityList = new List<double[]>();
```

## Normalizing the Permanence Values for Numeric Input Data

```// Normalize permanence values and add to lists
var thresholdValue = 8.3;
var normalizePermanenceList = Helpers.ThresholdingProbabilities(permanenceValuesList, thresholdValue);
normalizedPermanence.Add(normalizePermanenceList.ToArray());
encodedInputs.Add(inpSdr);
```

-   Set a threshold (`thresholdValue`): Defines the threshold value (8.3) for normalizing permanence values.

###### Apply thresholding (`Helpers.ThresholdingProbabilities()`):

-   This function takes `permanenceValuesList` and the threshold.

-   It processes the list to apply a probability-based thresholding mechanism.

###### Store the normalized permanence values:

-   `normalizePermanenceList` is converted to an array and added to normalizedPermanence.

###### Store the encoded input (`inpSdr`):

-   `inpSdr` is added to `encodedInputs`.

## Normalizing the Permanence Values for Image Input Data

###### Normalization of Permanence Values

-   The code first defines a threshold value (`ThresholdValue`) that will be used to filter permanence values.

```
var ThresholdValue = 67.0;
```

###### Extracting and Sorting Permanence Values

-   It retrieves the values from the `allPermanenceDictionary`, orders them by the key, and converts them into a list:

```
List<double> permanenceValuesList = allPermanenceDictionary.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();

```

###### Thresholding the Permanence Values

-   The `ThresholdingforResetImg` method is called to apply the threshold and normalize the permanence values:

```
int[] currentNormalizedPermanence = Helpers.ThresholdingforResetImg(permanenceValuesList, ThresholdValue).ToArray();
```

-   This method filters and processes the permanence values based on the threshold.

###### Storing the Normalized Permanence

```
normalizedPermanence.Add(currentNormalizedPermanence);
```

###### Dependencies

-   `Helpers.ThresholdingforResetImg`: Ensure that the helper function used to threshold and normalize the permanence values is implemented

## Thresholding Probabilities - Normalization Function

This function takes a list of numeric values and applies a threshold to each value, converting them to binary form. Any value greater than or equal to the specified threshold is converted to 1, and values less than the threshold are converted to 0. The function returns a list of integers representing the thresholded values.

###### Function Signature

```
public List<int> ThresholdValues(List<int> values, int threshold)
```

###### Parameters

-   `values` (List<int>): A list of integers that you want to apply the thresholding to. Can contain both positive and negative integers.

-   `threshold` (int): The threshold `value`. Values in the values list that are greater than or equal to this threshold will be converted to `1`; otherwise, they will be converted to `0`

###### Return Value

The function returns a list of integers (`List<int>`), where each element represents the thresholded result:

-   `1` if the corresponding value in the input list is greater than or equal to the threshold.

-   `0` if the corresponding value is less than the threshold.

```public static List<int> ThresholdingProbabilities(IEnumerable<double> values, double threshold)
 {
     if (values == null)
     {
         return null;
     }

     List<int> resultList = new List<int>();

     foreach (var numericValue in values)
     {
         int thresholdedValue = (numericValue >= threshold) ? 1 : 0;

         resultList.Add(thresholdedValue);
     }

     return resultList;
 }
```

Here is the Function
[Helpers.cs
](https://github.com/shakautkader608/permanence_pixels/blob/master/source/NeoCortexApi/Helpers.cs#L645-L670) - Lines (645 to 670)

## Generate DrawCombinedHeatmapsScalerInputs Function for Numeric Input types

```csharp
public static void DrawCombinedHeatmapsScalerInputs(List<List<double>> heatmapData, string outputFolder, int bmpWidth = 2048, int enlargementFactor = 2)
{
    // Ensure output folder exists
    Directory.CreateDirectory(outputFolder);

    // Apply enlargement factor
    bmpWidth *= enlargementFactor;

    int titlePadding = 40; // Space for the title

    for (int heatmapIndex = 0; heatmapIndex < heatmapData.Count; heatmapIndex++)
    {
        var permanenceValues = heatmapData[heatmapIndex];

        // Determine the best square shape for grid size
        int gridSize = (int)Math.Ceiling(Math.Sqrt(permanenceValues.Count));

        // Adjust grid dimensions
        int gridWidth = bmpWidth / gridSize;
        int gridHeight = gridWidth;
        int totalHeatmapHeight = gridSize * gridHeight;

        // Create bitmaps
        Bitmap coloredBitmap = new Bitmap(bmpWidth, totalHeatmapHeight + titlePadding);
        Graphics graphics = Graphics.FromImage(coloredBitmap);

        // Set fonts
        Font titleFont = new Font("Arial", 20, FontStyle.Bold);
        Font font = new Font("Arial", 30, FontStyle.Bold);
        Brush textBrush = Brushes.White;
        Pen outlinePen = Pens.Black;

        // Draw title
        string title = $"Permanence Heatmap {heatmapIndex + 1}";
        graphics.DrawString(title, titleFont, Brushes.Black, new PointF(bmpWidth / 3, 10));

        double maxPermanence = permanenceValues.Max();

        for (int i = 0; i < permanenceValues.Count; i++)
        {
            double permanence = permanenceValues[i];

            // Convert 1D index to 2D coordinates
            int x = i % gridSize;
            int y = i / gridSize;

            // Calculate color (Red for high permanence, Blue for low)
            int red = (int)(255 * (permanence / maxPermanence));
            int blue = (int)(255 * (1 - permanence / maxPermanence));
            Color pixelColor = Color.FromArgb(red, 0, blue);

            // Draw heatmap block
            using (SolidBrush brush = new SolidBrush(pixelColor))
            {
                graphics.FillRectangle(brush, x * gridWidth, y * gridHeight + titlePadding, gridWidth, gridHeight);
            }
            graphics.DrawRectangle(outlinePen, x * gridWidth, y * gridHeight + titlePadding, gridWidth, gridHeight);

            // Draw permanence value
            string valueText = $"{permanence:F1}";
            float textX = x * gridWidth + (gridWidth / 4);
            float textY = y * gridHeight + titlePadding + (gridHeight / 4);
            graphics.DrawString(valueText, font, textBrush, textX, textY);
        }

        // Save each heatmap separately
        string filePath = Path.Combine(outputFolder, $"heatmap_{heatmapIndex + 1}.png");
        coloredBitmap.Save(filePath, ImageFormat.Png);
        Console.WriteLine($"Heatmap {heatmapIndex + 1} saved to {filePath}");
    }
}
```

[GenarateHeatmap Function For Numeric Data](https://github.com/shakautkader608/permanence_pixels/blob/master/source/NeoCortexUtils/NeoCortexUtils.cs#L220-L290) - Lines (220 to 290)
The DrawCombinedHeatmapsScalerInputs method generates and saves heatmaps representing permanence values. Each heatmap visualizes a list of values on a grid, where colors range from blue (low values) to red (high values). The generated images are saved as .png files in the specified output folder.

###### Parameters

-   `heatmapData` (List<List<double>>)
    A list containing multiple lists of permanence values. Each inner list corresponds to a separate heatmap.
-   `outputFolder` (string)
    The directory where the heatmaps will be saved. The method ensures the directory exists.

-   `bmpWidth` (int, optional, default = 2048)
    The base width of the heatmap image in pixels. This value is multiplied by enlargementFactor before being used.
-   `enlargementFactor` (int, optional, default = 2)
    A scaling factor that enlarges the width of the heatmap for better visualization.

###### Functionality

Ensures Output Directory Exists

-   The method first checks if the specified `outputFolder` exists and creates it if necessary.

###### Adjusts Image Dimensions

-   The image width is adjusted using `enlargementFactor`.

-   The method determines an optimal square grid size based on the number of permanence values.

Creates Bitmap Graphics for Heatmap

-   Initializes a `Bitmap` object and a `Graphics` instance for drawing.

-   Sets font styles and colors for titles and labels.

###### Generates Heatmap

-   Loops through all permanence values:

-   Determines the grid position (x, y).

-   Calculates a color gradient from blue (low permanence) to red (high permanence).

-   Draws a rectangle for each value with the computed color.

-   Outlines each cell with a black border.

-   Adds a textual label of the permanence value within each cell.

###### Saves the Heatmap as a PNG File

-   The completed heatmap is saved to the specified output folder with the naming format:
    `heatmap_{index}.png`

###### Notes

-   The method dynamically determines the best grid size for given data.

-   Text labels may overlap if the grid is too small; adjusting bmpWidth or enlargementFactor can improve readability.

-   The permanence values are normalized relative to the maximum value in each heatmap.

### Calling HeatMap Genarte Function for Numeric Permanence Visualization

```csharp
  NeoCortexUtils.DrawEncodedVsReconstructedHeatmap(inpSdr, normalizePermanenceList.ToArray(), heatmapFilePath);
```

## Visualization for Numeric Inputs

We Applied this Function to DrawCombinedHeatmapsScalerInputs
Click Below for More Details
[DrawCombinedHeatmapsScalerInputs](https://github.com/shakautkader608/permanence_pixels/blob/master/source/NeoCortexUtils/NeoCortexUtils.cs#L220-L290) - Lines (222 to 351)

**Outcomes:**

-   HeatMap Image for all inputs as Image Visualization.
-   allPermanenceValues as List of List

**Results Example:**
**Fig: Final Outcome**
![Final Outcome](https://raw.githubusercontent.com/shakautkader608/permanence_pixels/refs/heads/master/source/Documentation_Permanence_pixels/Sample%20Output/heatmap_1.png)

## Generate DrawHeatmapsforImg Function for Image Input types

```csharp
 public static void DrawHeatmapsforImg(List<List<double>> heatmapData, string imageName, int gridSize = 52, int rescalingFactor = 40)
 {
     // Define folder path to save heatmaps
     string folderPath = Path.Combine(Environment.CurrentDirectory, "PermanenceHeatmaps");
     Directory.CreateDirectory(folderPath); // Ensure folder exists

     // Set the width and height of the image
     int imgWidth = gridSize * rescalingFactor;
     int imgHeight = gridSize * rescalingFactor;

     // Set the font and brush for text rendering
     Font font = new Font("Arial", rescalingFactor / 3, FontStyle.Bold); // Adjusted text size if needed
     Brush textBrush = Brushes.White; // Changed to white
     Pen outlinePen = Pens.Black;

     // Loop through each cycle's heatmap data
     for (int idx = 0; idx < heatmapData.Count; idx++)
     {
         var permanenceValues = heatmapData[idx];
         double maxPermanence = permanenceValues.Max();

         // Define the file path for saving the heatmap image
         string filePath = Path.Combine(folderPath, $"{imageName}cycle{idx}.png");

         using (Bitmap bmp = new Bitmap(imgWidth, imgHeight))
         using (Graphics g = Graphics.FromImage(bmp))
         {
             g.Clear(Color.White); // Set background to white

             // Loop through the grid and plot each cell's value
             for (int y = 0; y < gridSize; y++)
             {
                 for (int x = 0; x < gridSize; x++)
                 {
                     int pixelIndex = y * gridSize + x;
                     if (pixelIndex >= permanenceValues.Count) continue;

                     double permanence = permanenceValues[pixelIndex];

                     // *Color Scaling* - Set color based on permanence value
                     int red = Math.Min(255, (int)(255 * (permanence / maxPermanence)));
                     int blue = Math.Min(255, (int)(255 * (1 - permanence / maxPermanence)));
                     Color pixelColor = Color.FromArgb(red, 0, blue);

                     float xPos = x * rescalingFactor;
                     float yPos = y * rescalingFactor;

                     // *Draw heatmap cell* - Draw the colored rectangle for the cell
                     using (SolidBrush brush = new SolidBrush(pixelColor))
                     {
                         g.FillRectangle(brush, xPos, yPos, rescalingFactor, rescalingFactor);
                     }
                     g.DrawRectangle(outlinePen, xPos, yPos, rescalingFactor, rescalingFactor);

                     // *Draw permanence value inside cell* - Render the permanence value in the center of each cell
                     string valueText = $"{permanence:F1}"; // Only one decimal place
                     SizeF textSize = g.MeasureString(valueText, font);
                     float textX = xPos + (rescalingFactor - textSize.Width) / 2;
                     float textY = yPos + (rescalingFactor - textSize.Height) / 2;
                     g.DrawString(valueText, font, textBrush, textX, textY);
                 }
             }

             // Save the image to the specified path
             bmp.Save(filePath, ImageFormat.Png);
         }

         // Log the saved heatmap for debugging purposes
         Debug.WriteLine($"Saved heatmap for cycle {idx}: {filePath}");
     }
 }
```

[DrawHeatmapsforImg](https://github.com/shakautkader608/permanence_pixels/blob/master/source/NeoCortexUtils/NeoCortexUtils.cs#L375-L445) - Lines (375 to 445)
The DrawCombinedHeatmapsScalerInputs method generates and saves heatmaps representing permanence values. Each heatmap visualizes a list of values on a grid, where colors range from blue (low values) to red (high values). The generated images are saved as .png files in the specified output folder.

## **Implementation Details: `DrawHeatmapsforImg` Function**

## **Function Overview**

The `DrawHeatmapsforImg` function generates and saves heatmap images based on input permanence values. Each image represents a heatmap for a specific cycle, stored in the `PermanenceHeatmaps` directory.

## **Parameters**

| Parameter         | Type                  | Description                                                  |
| ----------------- | --------------------- | ------------------------------------------------------------ |
| `heatmapData`     | `List<List<double>>`  | A list containing lists of permanence values for each cycle. |
| `imageName`       | `string`              | Name prefix for the generated heatmap images.                |
| `gridSize`        | `int` (default: `52`) | Defines the number of grid cells in one row or column.       |
| `rescalingFactor` | `int` (default: `40`) | Factor used to scale each grid cell in pixels.               |

## **Implementation Steps**

1. **Create the Output Directory:**

    - The function first ensures that the `PermanenceHeatmaps` directory exists.
    - It is located in the current working directory.

2. **Initialize Image Properties:**

    - Image dimensions are determined by `gridSize × rescalingFactor`.
    - A font (`Arial`) and brush (`White`) are set up for rendering permanence values inside the grid cells.

3. **Loop Through Heatmap Data:**

    - Iterates through `heatmapData` where each cycle contains a list of permanence values.
    - Determines the maximum permanence value for normalization.

4. **Heatmap Color Mapping:**

    - Each cell's color is determined based on the permanence value:
        - **High permanence** → Red intensity increases.
        - **Low permanence** → Blue intensity increases.
    - The color is calculated using:
        ```csharp
        int red = Math.Min(255, (int)(255 * (permanence / maxPermanence)));
        int blue = Math.Min(255, (int)(255 * (1 - permanence / maxPermanence)));
        ```

5. **Drawing the Heatmap:**

    - Loops over the `gridSize × gridSize` cells:
        - Fills each cell with a color representing permanence.
        - Draws a black outline around each cell for clarity.
        - Places the permanence value in the center of the cell.

6. **Save the Heatmap Image:**
    - The generated heatmap is saved as a `.png` file in the `PermanenceHeatmaps` folder.
    - The filename format is:
        ```
        {imageName}cycle{idx}.png
        ```
    - Logs the file path using `Debug.WriteLine` for tracking.

## **Example Usage**

````csharp
List<List<double>> sampleData = new List<List<double>>
{
    new List<double> { 0.1, 0.5, 0.8, 0.2, 0.9, 0.4, 0.3, 0.7, 0.6 }, // Example permanence values for cycle 1
    new List<double> { 0.2, 0.6, 0.9, 0.1, 0.8, 0.5, 0.4, 0.3, 0.7 }  // Example permanence values for cycle 2
};
DrawHeatmapsforImg(sampleData, "TestHeatmap");


### Calling HeatMap Genarte Function for Image Permanence Visualization
```csharp
  NeoCortexUtils.DrawHeatmapsforImg(heatmapData, heatmapFilePath);
````

## Visualization for Image Inputs

We Applied this Function to DrawHeatmapsforImg
Click Below for More Details
[DrawHeatmapsforImg](https://github.com/shakautkader608/permanence_pixels/blob/master/source/NeoCortexUtils/NeoCortexUtils.cs#L375-L445) - Lines (375 to 445)
**Outcomes:**

-   HeatMap Image for all inputs as Image Visualization.
-   allPermanenceValues as List of List

**Results Example:**
**Fig: Final Outcome**
![Final Outcome](https://raw.githubusercontent.com/shakautkader608/permanence_pixels/refs/heads/master/source/Documentation_Permanence_pixels/Sample%20Output/Heatmap_0.pngcycle0.png)

## Calculating similarity with the Jaccard similarity Coefficient

This function calculates the Jaccard Similarity between two binary arrays. The Jaccard Similarity is a metric used to measure the similarity and diversity of two sets. For binary arrays, the similarity is determined by the ratio of the intersection of the two arrays to the union of the two arrays.

```csharp
   public static double JaccardSimilarityofBinaryArrays(int[] arr1, int[] arr2)
{
    if (arr1.Length != arr2.Length)
    {
        throw new ArgumentException("Arrays must have the same length.");
    }

    int intersectionCount = 0;
    int unionCount = 0;

    for (int i = 0; i < arr1.Length; i++)
    {
        if (arr1[i] == 1 && arr2[i] == 1)
        {
            intersectionCount++;
        }
        if (arr1[i] == 1 || arr2[i] == 1)
        {
            unionCount++;
        }
    }

    return (double)intersectionCount / unionCount;
}
```

Here is the Function
[MathHelpers.cs](https://github.com/shakautkader608/permanence_pixels/blob/master/source/NeoCortexApi/Utility/MathHelpers.cs#L182-L206) - Lines (182 to 206)

# DrawEncodedVsReconstructedHeatmap Function

## Description

The `DrawEncodedVsReconstructedHeatmap` function generates a heatmap comparing encoded input values to reconstructed values. It visually highlights mismatches in red and matches in green, then saves the heatmap as a PNG file.

## Parameters

-   **`encodedInput (int[])`**: The array representing the encoded input values.
-   **`reconstructed (int[])`**: The array representing the reconstructed values.
-   **`filePath (string)`**: The output file path where the heatmap image will be saved.

## Functionality

1. Determines the grid dimensions (width and height) based on the length of `encodedInput`.
2. Initializes a `Bitmap` object with a scale factor for better readability.
3. Uses `Graphics` to render the heatmap:
    - Each cell represents an entry in `encodedInput`.
    - Cells are filled with **green** if the value matches the corresponding `reconstructed` value, otherwise **red**.
    - Values are displayed inside each cell in the format **E,R** (Encoded, Reconstructed).
4. Saves the final heatmap image to the specified `filePath`.

## Visual Representation

-   **Green cells**: Indicate correct matches.
-   **Red cells**: Indicate mismatches.
-   **Black borders**: Improve clarity between cells.
-   **Text labels**: Show the encoded and reconstructed values.
    **Outcomes:**
-   A Difference of Heatmap (E,R)--->(Encoded,Reconstructed)--->(1,1)

**Only for numeric Numeric Data to compare with original Encoded vs Reconstructed:**
**Fig: Encoded vs Reconstructed**
![Final Outcome](https://raw.githubusercontent.com/shakautkader608/permanence_pixels/refs/heads/master/source/Documentation_Permanence_pixels/Sample%20Output/heatmap_0.png)

## Create a graph of similarities

This method generates a similarity plot visualizing a list of similarity values as bars. The bars are drawn on a bitmap and saved as a PNG file.

###### Parameters

-   similarities: A list of similarity values to plot.

-   filePath: The path where the resulting image will be saved.

-   imageWidth: Width of the output image.

-   imageHeight: Height of the output image.

###### Features

-   Scales the bars based on similarity values.

-   Customizable plot size and appearance.

-   Adds titles, axis labels, and a color scale for better visualization.

We used this function to generate the Combined Similarity Plot. Click below for more details.
[DrawSimilarityPlot](https://github.com/shakautkader608/permanence_pixels/blob/master/source/NeoCortexUtils/NeoCortexUtils.cs#L876-L984) - Lines (876 to 984)

**Outcomes:**

-   Bar charts representing the similarity for each input.

**Results Example For Numeric Data:**
**Fig: Final Outcome for Numeric input**
![Final Outcome](https://raw.githubusercontent.com/shakautkader608/permanence_pixels/refs/heads/master/source/Documentation_Permanence_pixels/Sample%20Output/combined_similarity_plot.png)

**Results Example for Image Data:**
**Fig: Final Outcome for Image input**
![Final Outcome](https://raw.githubusercontent.com/shakautkader608/permanence_pixels/refs/heads/master/source/Documentation_Permanence_pixels/Sample%20Output/highest_similarity_plot.png)
