import * as React from 'react';
import { AiOutlineCloudUpload } from "react-icons/ai";
import { MdClear } from "react-icons/md";
import "./drag-drop.css";
import { useState } from 'react';



/*
  Defines the shape of the props that the DragNdrop component accepts.
*/
interface DragNdropProps {
  onFilesSelected?: (files: File[]) => void;
  width: number;
  height: number;
}

/*
  Main component that handles the drag and drop functionality.
*/
const DragNdrop: React.FC<DragNdropProps> = ({
  onFilesSelected,
  width,
  height,
}) => {
  //state hooks
  const [file, setFile] = useState<File | null>(null); // Only allow one file
  const [submitted, setSubmitted] = useState(false);  // New state to track submission
  const allowedFileExtensions = ['.pdf', '.dwg', '.dxf'];
  
  const handleFileDrop = (newFile: File) => {
    const fileExtension = newFile.name.split('.').pop();
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
      const fileExtension = newFile.name.split('.').pop()?.toLowerCase(); // Get the file extension
      if (fileExtension && allowedFileExtensions.includes(`.${fileExtension}`)) {
        setFile(newFile);
        if (onFilesSelected) {
          onFilesSelected([newFile]); // Pass the file to the parent component
        }
      } else {
        alert('Invalid file type. Please upload a PDF, DWG, or DXF file.'); // Alert for invalid file type
      }
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
    const selectedFiles = event.target.files; // get the files that were selected
    if (selectedFiles && selectedFiles.length > 0) {
      const newFile = selectedFiles[0]; // Only pick the first file
      const fileExtension = newFile.name.split('.').pop()?.toLowerCase(); // Get the file extension
      if (fileExtension && allowedFileExtensions.includes(`.${fileExtension}`)) {
        setFile(newFile);
        if (onFilesSelected) {
          onFilesSelected([newFile]); // Pass the file to the parent component
        }
      } else {
        alert('Invalid file type. Please upload a PDF, DWG, or DXF file.'); // Alert for invalid file type
      }
    }
  };
  

  /*
    Event handler for when the user clicks the submit file button.
  */
  // const handleSubmit = () => {
  //   if (file) {
  //     uploadFile(file)
  //     .then(() => {
  //       setSubmitted(true);  // Update the state to indicate successful submission
  //       if (onFilesSelected) {
  //         onFilesSelected([file]);
  //       }
  //     })
  //     .catch((error: any) => {
  //       console.error(error); // Log the error to the console
  //       alert('An error occurred while submitting the file. Please try again.'); // Alert the user about the error
  //     });
  //   }
  // };
  const handleSubmit = async () => {
    console.log(file);
    if (!file) {
      console.error("No file selected");
      return;
    }

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
    
        console.log("File uploaded successfully:", res);
      } catch (error) {
        console.error("Error uploading file:", error);
        alert('An error occurred while submitting the file. Please try again.');
      }
  }
  /*
    Event handler for when the user clicks the back button after submission.
  */
  const backToUpload = () => {
    setSubmitted(false);
    setFile(null); // Clear the file after submission
  };
  return (
    <section className="drag-drop" style={{ width: width, height: height }}>
      {!submitted ? (  // Conditional rendering based on submission state
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
      ) : (  // Show success message after submission
        <div className="success-message">
          <p>File submitted successfully!</p>
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
