This file explains the process that the code takes from the initial entity list from the file to the final 
list of fully seperated entities.

## 1. Grouping Touching Entities

## 2. Set Feature Groups

## 3. Seperating Base Entities

## 4. Feature Detection
Once the program has seperated the base features from the perimeter features, it now runs through the feature detection.
This function calls a lot of individual feature recognition functions that try and find the named feature group in the
corresponding list (BaseEntity, PerimeterFeature, or EntendedEntity). Once all features have been identified, the final
compilation of features is then sent to the front end.
