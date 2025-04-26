namespace FeatureRecognitionAPI.Models.Enums;

public enum OperationStatus
{
    Unknown,
    Ok,
    ExternalApiFailure,
    UnsupportedFileType,
    CorruptFile,
    BadRequest
}