This file contains documentation on the API endpoints in the backend

# Feature Recognition Endpoints

## POST: api/FeatureRecognition/uploadFile

**Inputs:**
Takes a file (should be .dxf or .dwg) in form data. Takes a content type of application/x-www-url-formencoded or multipart/form-data <br/><br/>
**Returns:**
Groups of touching entities and all of the Feature groups within the file serialized as a JSON object (FeatureGroup is a model object in the code)

# Pricing Endpoints

## POST: api/Pricing/EstimatePrice
**Inputs:**
Takes a QuoteSubmissionDto as a Json object in the Post Body <br/><br/>
**Returns:**
The estimated price

## POST: api/Pricing/UpdatePunchPrice/{PossibleFeatureType}
**Inputs:**
Takes a PossibleFeatureType enum in the request URL and a List of Punch Prices in the Request body <br/><br/>
**Returns:** just the status code for the operation

## POST: api/Pricing/UpdateFeaturePrice
**Inputs:**
Takes a List of Feature prices in the request body <br/><br/>
**Returns:** just the status code for the operation

## POST: api/Pricing/AddPunchPrice/{PossibleFeatureType}
**Inputs:**
Takes a PossibleFeatureTypes enum in the request URL and a Punch Price object in the Request body. <br/><br/>
**Returns:** Just the status code for the operation

## POST: api/Pricing/UpdateRates
**Inputs:** Takes a RatesPrices object in the request body<br/> 
**Returns:** just the status code for the operation  

## GET: api/Pricing/GetFeaturPrices
**Inputs:** None <br/><br/>
**Returns:**
A JSON List of feature price objects

## GET: api/Pricing/GetPunchPrices
**Inputs:** None <br/> <br/>
**Returns:**
A JSON array containg Lists of Punch Prices each list is labeled with the punch type.

## GET: api/Pricing/GetBaseRates
**Inputs:** None <br/><br/>
**Returns:**
An object contaiing base information variables

