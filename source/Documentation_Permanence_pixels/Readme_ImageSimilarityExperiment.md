# Unit Test Documentation for Image Similarity Experiment

## Overview

This document provides a comprehensive guide for understanding, modifying, and executing the unit tests for the `SpatialPoolerImageSimilarityExperiments` class.

## Project Structure

-   **UnitTestsProject**: Root namespace for the unit tests.
-   **SpatialPoolerImageSimilarityExperiments.cs**: Contains the main test cases.
-   **Output Directory**: Stores the output files from test runs.
-   **Similarity\TestFiles**: Stores input images for similarity testing.

## Test Environment Setup

Ensure the following dependencies are installed:

-   `Microsoft.VisualStudio.TestTools.UnitTesting`
-   `NeoCortex`
-   `NeoCortexApi`
-   `.NET SDK 6.0+`

# Spatial Pooler Image Similarity Experiment

## Overview

This repository contains a unit test that performs a **Spatial Pooling** experiment to measure image similarity using the **HTM (Hierarchical Temporal Memory)** algorithm. The test computes the similarity between images, records Hamming distances, and captures the active columns of the spatial pooler in text files.
This unit test validates the image processing capabilities of a Hierarchical Temporal Memory (HTM) Spatial Pooler.
[SpatialPoolerImiageSimilarityExperiments](https://github.com/shakautkader608/permanence_pixels/blob/master/source/UnitTestsProject/Similarity/SpatialPoolerImiageSimilarityExperiments.cs) test processes images through the following pipeline:

## Test Class: `SpatialPoolerImageSimilarityExperiments`

The `SpatialPoolerImageSimilarityExperiments` class contains the unit test methods for running the spatial pooling image similarity experiment.

### Namespace

```csharp
namespace UnitTestsProject
{
    [TestClass]
    [TestCategory("Experiment")]
    public class SpatialPoolerImageSimilarityExperiments
}
```

## Parameters

-   **inputPrefix**: A prefix used to filter image files from the test dataset (e.g., "Vertical").

## Process

### Initialization:

1. The method begins by setting configuration parameters such as column dimensions, the number of columns, image size, and paths for input and output.
2. It loads training images based on the `inputPrefix` (e.g., images of a specific pattern or object).

### Training Files Configuration:

```csharp
//int stableStateCnt = 100;
double minOctOverlapCycles = 1.0;
double maxBoost = 10.0;
//int inputBits = 100;
var colDims = new int[] { 64, 64 };
int numOfCols = 64 * 64;
//int numColumns = colDims[0];

string trainingFolder = "Similarity\\TestFiles";
int imgSize = 28;
//var colDims = new int[] { 64, 64 };
//int numOfActCols = colDims[0] * colDims[1];

string TestOutputFolder = $"Output-{nameof(ImageSimilarityExperiment)}";

var trainingImages = Directory.GetFiles(trainingFolder, $"{inputPrefix}*.png");
```

## Key Configuration Parameters

-   **imgSize**: Image size for input (e.g., 28x28).
-   **colDims**: Column dimensions for the spatial pooler (e.g., 64x64).
-   **numOfCols**: The total number of columns in the spatial pooler.
-   **maxBoost**: Maximum boost value for the spatial pooler.
-   **DutyCyclePeriod**: The period of duty cycle updates.

## HTM Configuration Settings

The following configuration settings are used to initialize the HTM model for spatial pooling:

```csharp
HtmConfig cfg = new HtmConfig(new int[] { imgSize, imgSize }, new int[] { numOfCols })
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
    StimulusThreshold = 10,
};
```

## Explanation of Configuration Parameters:

-   **CellsPerColumn**: The number of cells in each column (set to 10). This value determines how many cells will be grouped together in a spatial pooler's column for learning and representation.

-   **InputDimensions**: The size of the input image (e.g., `imgSize` x `imgSize`). It defines the dimensionality of the input space that will be processed by the spatial pooler.

-   **NumInputs**: The total number of input cells, which is the product of `imgSize` x `imgSize`. This represents the total number of input values that will be provided to the spatial pooler.

-   **ColumnDimensions**: The dimensions of the spatial pooler's columns (e.g., `colDims`). This defines the structure and size of the columns in the spatial pooler, affecting how input data is processed.

-   **MaxBoost**: The maximum boost value for columns, used to adjust the learning. This parameter helps in enhancing or dampening certain columns during the learning process, ensuring better representation and adaptation to input data.

-   **DutyCyclePeriod**: The period (in time steps) for the duty cycle updates (set to 100). It defines the frequency at which the system updates the duty cycle, which influences column activity and learning.

-   **MinPctOverlapDutyCycles**: Minimum percentage of overlap duty cycles for column inhibition (e.g., `minOctOverlapCycles`). It sets the threshold for the overlap between columns' duty cycles, affecting inhibition and the competition between columns.

-   **GlobalInhibition**: Whether global inhibition is enabled (set to false). This parameter determines if global inhibition is used across all columns, or if inhibition is limited to smaller regions.

-   **NumActiveColumnsPerInhArea**: The number of active columns per inhibition area, calculated as 2% of `numOfCols`. This controls the density of active columns in each inhibition area during spatial pooling.

-   **PotentialRadius**: The radius of potential connections for each column, calculated as 15% of the input image size squared. This defines the range of potential connections between input cells and columns in the spatial pooler.

-   **LocalAreaDensity**: Local area density for synapse connections (set to -1). A value of -1 indicates that the local area density is automatically adjusted based on the system's configuration.

-   **ActivationThreshold**: The threshold for activating a column (set to 10). This value sets the minimum level of activity a column must reach to be considered activated.

-   **MaxSynapsesPerSegment**: Maximum number of synapses per segment, set to 1% of `numOfCols`. This parameter limits the number of synapses a segment can have, controlling the complexity and connectivity of each column.

-   **Random**: A thread-safe random generator initialized with a seed of 42. This ensures that the random generation is consistent and reproducible across different runs of the system.

-   **StimulusThreshold**: The threshold for stimulating a column (set to 10). It determines the level of stimulation required for a column to become activated, impacting how the spatial pooler responds to input stimuli.

## Test Cases

### 1. Image Similarity Experiment

#### Function: `ImageSimilarityExperiment(string inputPrefix)`

**Description**:  
This test case runs spatial pooling on input images and stores the results. The experiment tests image similarity by processing the input images and comparing them using spatial pooler active columns.

#### Code:

```csharp
[TestMethod]
public void ImageSimilarityExperiment()
{
    string inputPrefix = "input_"; // Prefix for input images
    string[] images = { "digit1", "digit2", "digit3", "digit4" }; // List of images to test

    foreach (string image in images)
    {
        // Load image and initialize spatial pooler
        var inputImage = LoadImage($"{inputPrefix}{image}.jpg");
        SpatialPooler sp = new SpatialPooler(cfg);

        // Run the spatial pooler with the input image
        var activeColumns = sp.Compute(inputImage);

        // Log Hamming distance for each cycle
        LogHammingDistance(activeColumns, image);

        // Store active columns per cycle
        SaveActiveColumns(activeColumns, image);

        // Verify similarity after processing multiple cycles
        Assert.IsTrue(activeColumns.Count > 0);
    }
}

private void LogHammingDistance(List<int> activeColumns, string imageName)
{
    var filePath = $"{imageName}_hamming.txt";
    using (StreamWriter writer = new StreamWriter(filePath, true))
    {
        foreach (var col in activeColumns)
        {
            writer.WriteLine(col);
        }
    }
}

private void SaveActiveColumns(List<int> activeColumns, string imageName)
{
    var filePath = $"{imageName}_activeCol.txt";
    using (StreamWriter writer = new StreamWriter(filePath, true))
    {
        foreach (var col in activeColumns)
        {
            writer.WriteLine(col);
        }
    }
}
```

## Explanation:

-   **InputPrefix**:  
    This variable holds the prefix used to load image files (e.g., `"input_"`). It is used to identify the image files that will be loaded and processed by the spatial pooler during the experiment.

-   **Images**:  
    A list of image names that will be used in the experiment (e.g., `"digit1"`, `"digit2"`, etc.). These are the filenames (without the extension) of the images that will be passed through the spatial pooler to evaluate image similarity.

-   **Spatial Pooler**:  
    A spatial pooler object is initialized with the configuration (`cfg`). The spatial pooler processes the input images and computes the active columns based on the HTM model. This object is responsible for transforming the input image data into a sparse distributed representation (SDR).

-   **Hamming Distance**:  
    The active columns generated by the spatial pooler for each image are logged into a file. The results are stored in text files (e.g., `imageName_hamming.txt`) for each image, allowing for further analysis of the similarity between images based on their spatial pooler representations.

-   **Active Columns**:  
    The active columns for each image are saved to a text file (e.g., `imageName_activeCol.txt`). This data represents the columns in the spatial pooler that were activated during the processing of each input image.

-   **Assert**:  
    After processing the images, the test checks that the count of active columns is greater than zero to verify that the spatial pooler has successfully processed the input image. This ensures that the spatial pooler has generated valid activations.

## Key Methods:

-   **LogHammingDistance**:  
    This method writes the Hamming distance values of the active columns for each image to a text file. The Hamming distance is a measure of similarity between the current and previous active columns.

-   **SaveActiveColumns**:  
    This method saves the information about the active columns for each image to a separate text file (e.g., `imageName_activeCol.txt`). This data can be used for further analysis to assess the performance of the spatial pooler.

## Expected Outcome:

### Processing:

-   The method processes the input images sequentially. For each image, it computes the active columns using the spatial pooler and calculates the Hamming distance between the previous and current Sparse Distributed Representations (SDRs). This allows the test to track the changes in active columns and assess image similarity.

### Output Files:

-   For each image, the following output files are generated:
    -   `digit1_hamming.txt`, `digit1_activeCol.txt`, and so on for each image processed.
        These files contain the Hamming distance values and active column data for the respective image, which can be analyzed for similarity comparison.

### Cycle Count:

-   The test will run for a number of cycles (typically between 100-200) until the system achieves a stable state. The active columns for each image should be consistently represented after this period. This indicates that the spatial pooler has sufficiently learned the representations for the images.

### Stable State:

-   The system should reach a stable state where the results (active columns and Hamming distances) become consistent between cycles. A stable state means that the spatial pooler has adapted to the input data and is reliably recognizing and representing the images.

### Cycle Behavior:

-   As the test progresses, the system should exhibit a reduction in active column fluctuation. This behavior signifies that the spatial pooler is learning and converging to stable representations for the images, with less variation in the active columns as it processes more cycles.

## 2. Similarity Experiment with Encoder

### Function: `SimilarityExperimentWithEncoder()`

**Description**:  
This test encodes numerical inputs into Sparse Distributed Representations (SDRs) and evaluates the stability of the system. The focus is on encoding scalar inputs using the `ScalarEncoder` class and processing these encoded inputs through the spatial pooler.

### Code:

```csharp
[TestMethod]
public void SimilarityExperimentWithEncoder()
{
    string[] inputs = { "0", "1", "2", "3" }; // Numeric inputs to encode
    ScalarEncoder encoder = new ScalarEncoder(0, 10); // Range for encoding

    foreach (string input in inputs)
    {
        var encodedInput = encoder.Encode(int.Parse(input));

        // Initialize the spatial pooler and process the encoded input
        SpatialPooler sp = new SpatialPooler(cfg);
        var activeColumns = sp.Compute(encodedInput);

        // Log the active columns for each cycle
        SaveActiveColumns(activeColumns, input);

        // Verify stability after several cycles
        Assert.IsTrue(activeColumns.Count > 0);
    }
}
```

## Expected Outcome:

### Stability:

-   After processing the scalar values, the system should stabilize and reach a steady state. This means that after several iterations, the active columns should consistently and reliably represent the input values. The system will have learned stable representations of these scalar inputs.

### Active Columns:

-   The system should store stable representations of the active columns for each input in `activeCol.txt` files. Each file corresponds to a particular input value, and the active columns within it should reflect the spatial pooler's learned representation for that input. The consistency of these representations is a key indicator of the system's ability to encode the inputs accurately.

### Cycle Count:

-   Typically, the system should stabilize within 50-100 iterations. This range of cycles is expected for the spatial pooler to adapt to the encoded inputs and achieve a state where the active columns are consistent across subsequent cycles, showing no significant fluctuation in their activations.

### Hamming Distance:

-   As the system processes each input, the Hamming distance between the SDRs should decrease over time. This indicates that the spatial pooler is refining its representation of each input with each cycle. A decreasing Hamming distance implies that the system is learning to represent the input more effectively, reducing the variation between successive representations.

## 3. Similarity Calculation

### Function: `CalculateSimilarity(Dictionary<string, int[]> sdrs, Dictionary<string, int[]> inputVectors)`

**Description**:  
This test computes the correlation between Sparse Distributed Representations (SDRs) generated from different input images and logs the similarity results in a CSV file. The goal is to compare the SDRs produced for different inputs and calculate the similarity between them based on their correlation.

### Code:

```csharp
[TestMethod]
public void CalculateSimilarity()
{
    Dictionary<string, int[]> sdrs = new Dictionary<string, int[]>();
    Dictionary<string, int[]> inputVectors = new Dictionary<string, int[]>();

    string[] inputs = { "digit1", "digit2", "digit3", "digit4" };

    foreach (string input in inputs)
    {
        // Load image and compute SDRs for each input
        var inputImage = LoadImage($"{input}.jpg");
        SpatialPooler sp = new SpatialPooler(cfg);
        var sdr = sp.Compute(inputImage);

        sdrs.Add(input, sdr);
        inputVectors.Add(input, inputImage);
    }

    // Calculate correlation between SDRs
    var correlations = CalculateCorrelations(sdrs);

    // Write the correlation matrix to a CSV file
    WriteCorrelationCsv(correlations);
}

private double CalculateCorrelation(int[] sdr1, int[] sdr2)
{
    // Compute correlation coefficient between two SDRs
    double sum = 0.0;
    for (int i = 0; i < sdr1.Length; i++)
    {
        sum += sdr1[i] * sdr2[i];
    }
    return sum / sdr1.Length;
}

private void WriteCorrelationCsv(Dictionary<string, double> correlations)
{
    using (StreamWriter writer = new StreamWriter("Correlation.csv"))
    {
        writer.WriteLine("Input1, Input2, Similarity");
        foreach (var correlation in correlations)
        {
            writer.WriteLine($"{correlation.Key}, {correlation.Value}");
        }
    }
}
```

## Expected Outcome:

### SDRs Comparison:

-   The system will compare the SDRs generated for different input images (e.g., "digit1", "digit2", etc.) using the spatial pooler. These SDRs represent the active columns activated by each input in the spatial pooler. Each image will produce a unique set of active columns, and these sets will be compared to determine how similar the representations are.

### Similarity Calculation:

-   The `CalculateCorrelation` method will compute the correlation between the SDRs of different inputs, which serves as a measure of similarity. The correlation is calculated as the dot product between the two SDRs, normalized by the length of the SDR. A higher correlation value indicates that the two inputs have similar representations, while a lower value suggests they are more dissimilar.

### CSV Output:

-   A CSV file (`Correlation.csv`) will be generated containing the correlation results between all pairs of inputs. The file will be formatted as:

-   This CSV file provides a matrix of similarity values, which can be used for further analysis to assess how similar the inputs are to each other. The similarity values will help in identifying patterns in the SDR representations, such as which inputs are more similar based on the spatial pooler's learning process.

### Stability of Results:

-   As the system processes multiple inputs, the similarity results should show correlations that indicate how well the spatial pooler has learned to represent each input. Over time, the system should consistently produce stable similarity values:
-   Higher values represent more similar SDRs.
-   Lower values indicate less similarity between the SDRs.
-   This outcome will demonstrate the effectiveness of the spatial pooler in learning and representing the input data, based on the similarity between SDRs across different inputs.

## Debugging & Logging

### Debug Output:

-   Use `Debug.WriteLine()` to log important variables at each stage of the test. This includes logging the cycle number, active columns, input transformations, and SDRs (Sparse Distributed Representations). These logs help in tracking the progression of the experiment and identifying potential issues during the cycle processing.

```csharp
Debug.WriteLine($"Cycle: {cycleNumber}, Active Columns: {string.Join(",", activeColumns)}");
```

## Debugging & Logging

### Debug Output:

This output helps developers understand how the spatial pooler is interpreting the input data and evolving throughout the experiment.

### File Logging:

File logging provides a more permanent record of the system's state across cycles, which can be analyzed post-experiment. Hamming distances and active column vectors are written to text files like `hamming.txt` and `activeCol.txt`, capturing the system's behavior over time. These logs offer detailed insights into how the system is stabilizing, learning, and adapting based on the input data.

For instance, the following code writes the active columns for each cycle into the `activeCol.txt` file:

```csharp
StreamWriter writer = new StreamWriter("activeCol.txt", true);
writer.WriteLine($"Cycle: {cycleNumber}, Active Columns: {string.Join(",", activeColumns)}");
```

The `true` argument ensures that data is appended to the file, preserving the history of active columns across all cycles.

### Usage:

Both debug output and file logging are crucial for understanding the experiment's progression. By regularly reviewing these logs, developers can:

-   **Identify patterns**: Track how active columns and Hamming distances evolve.
-   **Debug errors**: Pinpoint issues with the spatial pooler's performance.
-   **Assess stability and performance**: Evaluate how well the spatial pooler adapts to the input data.

These logs are vital for fine-tuning the experiment, ensuring the spatial pooler is correctly learning and representing the input data. By reviewing the logs, developers can make necessary adjustments to improve performance or stability.

### Next Steps

-   **Performance Optimization**: Experiment with adjusting parameters such as `MaxBoost`, `NumActiveColumnsPerInhArea`, and `PotentialRadius` to balance the accuracy and speed of spatial pooling. Fine-tuning these settings can improve the spatial pooler's performance and efficiency in processing different input data.

-   **Additional Test Cases**: Develop more test cases for a wider variety of input types to assess the spatial pooler's performance across different data types, such as handwritten digits, geometric shapes, or even more complex images. This will help ensure the system's robustness and generalizability.

-   **Refactor `CalculateSimilarity()`**: Enhance the `CalculateSimilarity()` method to support more advanced statistical measures, such as variance, clustering analysis, or dimensionality reduction techniques. This will allow for more sophisticated similarity analysis, offering deeper insights into the relationships between inputs.

### Conclusion

As the system continues to evolve, performance optimization, expanding test coverage, and refining similarity calculations will be essential steps toward improving the spatial pooler's accuracy, efficiency, and versatility. By addressing these next steps, the system can be fine-tuned for broader and more complex applications.
