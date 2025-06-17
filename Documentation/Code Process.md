This file explains the process that the code takes from the initial entity list from the file to the final 
list of fully seperated entities.

## 1. Grouping Touching Entities

The first file that handles a lot of backend functions for the program is `SupportedFile.cs`. This file helps parse the entities from the submitted CAD files into the entity list. Once all the entities are retrieved, the program then condenses all entities that are supposed to be grouped together. This is due to some file types dividing some entities such as circles and lines into seperate entities. The program then groups entities that are connected or intersecting each other with `GroupFeatureEntities()`.

## 2. Extending Entities

## 3. Seperating Base Entities

Using the extended lines, the program tries to find closed shapes that would be considered base features using  `SeperateBaseEntities()`. This is done by examining where there appears to be two features touching each other, which is a base feature and a perimeter feature. After this process is done, all perimeter features will essentially be cut off from their base features they were attched to.

## 4. Feature Detection
Once the program has seperated the base features from the perimeter features, it now runs through the feature detection using `DetectFeatures`. This function calls a lot of individual feature recognition functions that try and find the named feature group in the corresponding list (BaseEntity, PerimeterFeature, or EntendedEntity). Each of these feature detection groups are seperated in `Feature.cs` using regions. Once all features have been identified, the final compilation of features is then sent to the front end.
