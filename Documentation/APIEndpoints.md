This file contains documentation on the API endpoints in the backend

# api/FeatureRecognition/uploadFile
### Inputs:
Takes a file (should be .dxf or .dwg) in form data. Takes a content type of application/x-www-url-formencoded or multipart/form-data
### Retuns
Groups of touching entities and all of the Feature groups within the file serialized as a JSON object (FeatureGroup is a model object in the code)
