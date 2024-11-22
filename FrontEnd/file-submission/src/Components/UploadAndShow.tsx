import * as React from "react";
import { useState } from "react";
import DragDropZone from "./DragDropZone";
import QuoteSubmission from "./QuoteSubmission";
import ParentModal from "./SupportedFeaturesModal";
import "bootstrap/dist/css/bootstrap.min.css";

/*
  Defines the shape of the props that the UploadAndShow component accepts.
*/
interface UploadAndShowProps {
  onFilesSelected?: (files: File[]) => void;
}

/*
  Main component that handles drag-and-drop and file submission.
*/
const UploadAndShow: React.FC<UploadAndShowProps> = ({ onFilesSelected }) => {
  // State hooks
  const [file, setFile] = useState<File | null>(null); // Only allow one file
  const [submitted, setSubmitted] = useState(false); // Tracks submission
  const [jsonResponse, setJsonResponse] = useState<any>(null); // Stores JSON response
  const [isLoading, setIsLoading] = useState(false); // State for loading
  const [showModal, setShowModal] = useState(false); // State for modal

  const allowedFileExtensions = [".pdf", ".dwg", ".dxf"];

  /*
    Event handler for when the user clicks the submit file button.
    Submits the file to the server and captures the JSON response.
  */
  const handleSubmit = async () => {
    if (!file) return;

    setIsLoading(true); // Start loading

    const formData = new FormData();
    formData.append("file", file);

    try {
      const res = await fetch(
        "https://localhost:44373/api/FeatureRecognition/uploadFile",
        {
          method: "POST",
          body: formData,
        }
      );

      if (!res.ok)
        throw new Error(`Server error: ${res.status} ${res.statusText}`);

      const jsonResponse = await res.json(); // Capture JSON responses
      console.log(jsonResponse);
      setJsonResponse(jsonResponse); // Store response in state
      setSubmitted(true); // Update the state to indicate successful submission
    } catch (error) {
      alert("An error occurred while submitting the file. Please try again.");
    } finally {
      setIsLoading(false); // End loading
    }
  };

  const handleCloseModal = () => setShowModal(false);

  /*
    Event handler for when the user clicks the back button after submission.
  */
  const backToUpload = () => {
    setSubmitted(false);
    setFile(null); // Clear the file after submission
    setJsonResponse(null); // Clear the JSON response on going back
  };

  return (
    <div className="upload-and-show">
      {isLoading ? ( // Display loading screen during file upload
        <div className="loader"></div>
      ) : !submitted ? ( // Display drag-and-drop area if not submitted and not loading
        <>
          <div className="upload-container">
            <button
              className="animated-button modal-button"
              onClick={() => setShowModal(true)}
            >
              <span>Supported Features</span>
              <span></span>
            </button>
            <ParentModal showModal={showModal} handleCloseModal={handleCloseModal} />
            
            <div className="drag-drop">
              <DragDropZone
                file={file}
                allowedFileExtensions={allowedFileExtensions}
                setFile={setFile}
                onFilesSelected={onFilesSelected}
              />
            </div>
            {file && (
              <div className="submit-button-container">
                <button className="animated-button" onClick={handleSubmit}>
                  <span>Submit File</span>
                  <span></span>
                </button>
              </div>
            )}
          </div>
        </>
      ) : (
        // Show retrieved data
        <div className="response-container">
          {jsonResponse && ( // Conditionally render the JSON response
            <QuoteSubmission
              jsonResponse={jsonResponse}
              backToUpload={backToUpload}
            />
          )}
        </div>
      )}
    </div>
  );
};

export default UploadAndShow;
