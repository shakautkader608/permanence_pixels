# Spatial Pooler Image Similarity Unit Test

## Overview
This unit test validates the image processing capabilities of a Hierarchical Temporal Memory (HTM) Spatial Pooler.The
[SpatialPoolerImiageSimilarityExperiments](https://github.com/shakautkader608/permanence_pixels/blob/master/source/UnitTestsProject/Similarity/SpatialPoolerImiageSimilarityExperiments.cs) test processes images through the following pipeline:

1. **Image Conversion**: Transforms PNG images into binary input vectors
2. **Spatial Pooling**: Learns patterns using a 64x64 column grid
3. **Stability Monitoring**: Ensures consistent learning through homeostatic plasticity
4. **Similarity Analysis**: Compares input/output patterns using Hamming distance
5. **Visualization**: Generates diagnostic images and data files

## Core Implementation

### 1. Test Initialization
The test begins by configuring the Spatial Pooler with specific parameters that control how it processes images:

```csharp
HtmConfig cfg = new HtmConfig(new int[] { imgSize, imgSize }, new int[] { numOfCols })
{
    ColumnDimensions = [64, 64],          // Organizes columns in 64x64 grid
    PotentialRadius = (int)(0.15 * imgSize * imgSize), // Columns connect to ~15% of inputs
    GlobalInhibition = false,             // Uses local inhibition
    MaxBoost = 10.0,                      // Maximum boosting strength
    ActivationThreshold = 10,             // Minimum overlap to activate
    Random = new ThreadSafeRandom(42)     // Fixed seed for reproducibility
};
```


### 2. Image Processing Pipeline
- **For each test image (28x28 PNG files matching "Vertical.png"):** 
```csharp
// Convert image to binary array
string inputBinaryImageFile = NeoCortexUtils.BinarizeImage(trainingImage, imgSize, testName);
int[] inputVector = NeoCortexUtils.ReadCsvIntegers(inputBinaryImageFile).ToArray();

// Process through Spatial Pooler
sp.compute(inputVector, activeArray, true);

// Track active columns
var activeCols = ArrayUtils.IndexWhere(activeArray, (el) => el == 1);
```
### 3. Learning Stability Mechanism
- **The test includes a sophisticated stability checker that:**
```csharp
HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(
    mem, 
    trainingImages.Length * 50, 
    (isStable, numPatterns, actColAvg, seenInputs) => 
    {
        Assert.IsTrue(isStable);          // Verify system stability
        Assert.IsTrue(numPatterns == trainingImages.Length); // All patterns learned
        isInStableState = true;           // Enable result processing
    }, 
    requiredSimilarityThreshold: 0.975);  // 97.5% similarity threshold
```
### 4. Diagnostic Outputs
- **The test generates three types of outputs:**
- **Data Files:** 1)hamming.txt: Tracks how SDRs change between cycles 2) activeCol.txt: Records which columns activate for each pattern 3)Correlation.csv: Matrix comparing all input/output pairs

**Visualizations:**
```csharp
// Input vs SDR comparison
NeoCortexUtils.DrawBitmaps(arrays, outputImage, Color.Yellow, Color.Gray, 1024, 1024);

// Learning process heatmaps
NeoCortexUtils.DrawHeatmaps(overlapArrays, $"{outputImage}_overlap.png", 1024, 1024, 150, 50, 5);
NeoCortexUtils.DrawHeatmaps(bostArrays, $"{outputImage}_boost.png", 1024, 1024, 150, 50, 5);
```
**Key Technical Specification**
| Component           | Specification                 | Purpose                                                                 |
|---------------------|-------------------------------|-------------------------------------------------------------------------|
| **Input Images**    | 28x28 PNG, "Vertical*.png"    | Standardized input size and naming convention                          |
| **Column Grid**     | 64x64 columns                 | Determines spatial resolution for pattern recognition                  |
| **Learning Threshold** | 97.5% similarity            | Strict stability requirement ensuring reliable results                 |
| **Visualization**   | 1024x1024 PNG outputs         | High-resolution diagnostic images for analysis                         |
| **Reproducibility** | Fixed random seed (42)        | Ensures consistent, verifiable results across test executions          |

**Validation Methodology**

**The test incorporates multiple validation mechanisms:**

**a.Automatic Assertions:**
```csharp
Assert.IsTrue(isStable);
Assert.IsTrue(numPatterns == trainingImages.Length);
```
**b.Quantitative Metrics:**
* **Hamming distance between SDRs**
* **Input/output similarity scores**

**c.Visual Inspection:**
* **Bitmap comparisons**
* **Overlap/boost heatmaps**

**Complete Workflow**

**1.Initializes Spatial Pooler with specific parameters**

**2.Processes each image through binarization and vector conversion**

**3.Runs multiple learning cycles until stability achieved**

**4.Calculates similarity metrics for all pattern combinations**

**5.Generates diagnostic files and visualizations**

**6.Validates results through automated checks**

This implementation provides researchers with a comprehensive tool for evaluating Spatial Pooler performance on image pattern recognition tasks, complete with diagnostic outputs and validation mechanisms.