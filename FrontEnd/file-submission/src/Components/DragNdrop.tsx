import * as React from 'react';
import { AiOutlineCloudUpload } from "react-icons/ai";
import { MdClear } from "react-icons/md";
import "./drag-drop.css";
import { useState } from 'react';
import testJson from './testData.json';

/*
  Defines the shape of the props that the DragNdrop component accepts.
*/
interface DragNdropProps {
  onFilesSelected?: (files: File[]) => void;
}

/*
  Main component that handles the drag and drop functionality.
*/
const DragNdrop: React.FC<DragNdropProps> = ({
  onFilesSelected,
}) => {
  // State hooks
  const [file, setFile] = useState<File | null>(null); // Only allow one file
  const [submitted, setSubmitted] = useState(false); // Tracks submission
  const [jsonResponse, setJsonResponse] = useState<any>(null); // Stores JSON response
  const [isLoading, setIsLoading] = useState(false); // New state for loading
  const allowedFileExtensions = ['.pdf', '.dwg', '.dxf'];

  /*
    Handles the dropped file and checks if it's of an allowed file type.
  */
  const handleFileDrop = (newFile: File) => {
    const fileExtension = newFile.name.split('.').pop()?.toLowerCase();
    if (fileExtension && allowedFileExtensions.includes(`.${fileExtension}`)) {
      setFile(newFile);
      if (onFilesSelected) {
        onFilesSelected([newFile]); // Pass the file to the parent component
      }
    } else {
      alert('Invalid file type. Please upload a PDF, DWG, or DXF file.'); // Alert for invalid file type
    }
  };

  /*
    Event handler for when a file is dropped onto the drop zone.
  */
  const handleDrop = (event: React.DragEvent<HTMLDivElement>) => {
    event.preventDefault(); // Prevent the default behavior of opening the file in the browser
    const droppedFiles = event.dataTransfer.files; // Get the files that were dropped
    if (droppedFiles.length > 0) {
      const newFile = droppedFiles[0]; // Only pick the first file
      handleFileDrop(newFile);
    }
  };

  /*
    Event handler for when the user clicks the remove file button.
  */
  const handleRemoveFile = () => {
    setFile(null); // Clear the selected file
  };

  /*
    Event handler for when the user selects a file using the file input.
  */
  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFiles = event.target.files; // Get the files that were selected
    if (selectedFiles && selectedFiles.length > 0) {
      const newFile = selectedFiles[0]; // Only pick the first file
      handleFileDrop(newFile);
    }
  };

  /*
    Event handler for when the user clicks the submit file button.
    Submits the file to the server and captures the JSON response.
  */
  const handleSubmit = async () => {
    if (!file) {
      console.error("No file selected");
      return;
    }

    setIsLoading(true); // Start loading

    const formData = new FormData();
    formData.append("file", file);
    try {
      const res = await fetch("https://localhost:44373/api/FeatureRecognition/uploadFile", {
        method: "POST",
        body: formData
      });

      if (!res.ok) {
        throw new Error(`Server error: ${res.status} ${res.statusText}`);
      }

      const jsonResponse = await res.json(); // Capture JSON responses
      console.log(jsonResponse);
      //setJsonResponse(jsonResponse); // Store response in state
      setJsonResponse(testJson);
      setSubmitted(true); // Update the state to indicate successful submission

    } catch (error) {
      console.error("Error uploading file:", error);
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

  const DisplayData = testJson.map(
    (info)=>{
      return(
      <tr>
        <td>{info.group}</td>
        <td> {info.perOver20 ? (
          <span className="checkmark">&#10003;</span>
        ) : (
          <span className="crossmark">&#10005;</span>
        )} </td>
        <td> {info.multipleRadius ? (
          <span className="checkmark">&#10003;</span>
        ) : (
          <span className="crossmark">&#10005;</span>
        )} </td>
        <td> {info.kissCut ? (
          <span className="checkmark">&#10003;</span>
        ) : (
          <span className="crossmark">&#10005;</span>
        )} </td>
        <td> {info.border ? (
          <span className="checkmark">&#10003;</span>
        ) : (
          <span className="crossmark">&#10005;</span>
        )} </td>
      </tr>
      )
    }
  )

  return (
    <section className="drag-drop">
      {isLoading ? ( // Display loading screen during file upload
        <><span className="loader"></span><div className="loading-text">Uploading...</div></>
      ) : !submitted ? ( // Display drag-and-drop area if not submitted and not loading
        <>
          <div
            className={`document-uploader ${file ? "upload-box active" : "upload-box"}`}
            onDrop={handleDrop}
            onDragOver={(event) => event.preventDefault()}
          >
            <div className="upload-info">
              <AiOutlineCloudUpload />
              <div>
                <p>Drag and drop your file here</p>
                <p>Supported files: .DXF, .DWG, .PDF</p>
              </div>
            </div>
            <input
              type="file"
              hidden
              id="browse"
              onChange={handleFileChange}
              accept=".dxf,.dwg,.pdf"
            />
            <label htmlFor="browse" className="browse-btn">
              Browse file
            </label>
            {file && (
              <div className="file-list">
                <div className="file-list__container">
                  <div className="file-item">
                    <div className="file-info">
                      <p>{file.name}</p>
                    </div>
                    <div className="file-actions">
                      <MdClear onClick={handleRemoveFile} />
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>
          {file && (
            <button className="submit-btn" onClick={handleSubmit}>
              Submit File
            </button>
          )}
        </>
      ) : ( // Show success message after submission
        <div className="success-message">
          <p>File submitted successfully!</p>
          {jsonResponse && ( // Conditionally render the JSON response
            <div className="json-response">
              <h3>Features Detected: {jsonResponse.length}</h3>
              <table>
                <thead>
                  <tr>
                    <th>Group</th>
                    <th>Perimeter Over 20</th>
                    <th>Multiple Radius</th>
                    <th>Kiss Cut</th>
                    <th>Border</th>
                  </tr>
                </thead>
                <tbody>
                  {DisplayData}
                </tbody>
                </table>
            </div>
          )}
          <button className="back-btn" onClick={backToUpload}>
            Go Back
          </button>
        </div>
      )}
    </section>
  );
};

// Export the DragNdrop component
export default DragNdrop;