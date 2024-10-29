import * as React from 'react';
import { useState } from 'react';
import DragDropZone from './DragDropZone';
import JsonTable from './JsonTable';
import LoadingIndicator from './LoadingIndicator';

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

  const allowedFileExtensions = ['.pdf', '.dwg', '.dxf'];

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
      const res = await fetch("https://localhost:44373/api/FeatureRecognition/uploadFile", {
        method: "POST",
        body: formData
      });

      if (!res.ok) throw new Error(`Server error: ${res.status} ${res.statusText}`);

      const jsonResponse = await res.json(); // Capture JSON responses
      setJsonResponse(jsonResponse); // Store response in state
      setSubmitted(true); // Update the state to indicate successful submission

    } catch (error) {
      alert('An error occurred while submitting the file. Please try again.');
    } finally {
      setIsLoading(false); // End loading
    }
  };

  /*
    Event handler for when the user clicks the back button after submission.
  */
  const backToUpload = () => {
    setSubmitted(false);
    setFile(null); // Clear the file after submission
    setJsonResponse(null); // Clear the JSON response on going back
  };

  return (
    <section className="drag-drop">
      {isLoading ? ( // Display loading screen during file upload
        <LoadingIndicator />
      ) : !submitted ? ( // Display drag-and-drop area if not submitted and not loading
        <>
          <DragDropZone
            file={file}
            allowedFileExtensions={allowedFileExtensions}
            setFile={setFile}
            onFilesSelected={onFilesSelected}
          />
          {file && (
            <button className="animated-button" onClick={handleSubmit}>
              <span>Submit File</span>
              <span></span>
            </button>
          )}
        </>
      ) : ( // Show success message after submission
        <div className="success-message">
          {jsonResponse && ( // Conditionally render the JSON response
            <JsonTable jsonResponse={jsonResponse} />
          )}
          <button className="animated-button" onClick={backToUpload}>
          <span>Go Back</span>
            <span></span>
          </button>
        </div>
      )}
    </section>
  );
};

export default UploadAndShow;
