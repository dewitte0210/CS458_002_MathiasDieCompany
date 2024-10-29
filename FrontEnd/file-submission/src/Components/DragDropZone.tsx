import * as React from 'react';
import { AiOutlineCloudUpload } from "react-icons/ai";
import FileDisplay from './FileDisplay';

/*
  Handles the drag-and-drop area and file selection.
*/
interface DragDropZoneProps {
  file: File | null;
  allowedFileExtensions: string[];
  setFile: (file: File | null) => void;
  onFilesSelected?: (files: File[]) => void;
}

const DragDropZone: React.FC<DragDropZoneProps> = ({
  file, allowedFileExtensions, setFile, onFilesSelected
}) => {
  const handleFileDrop = (newFile: File) => {
    const fileExtension = newFile.name.split('.').pop()?.toLowerCase();
    if (fileExtension && allowedFileExtensions.includes(`.${fileExtension}`)) {
      setFile(newFile);
      onFilesSelected && onFilesSelected([newFile]); // Pass the file to the parent component
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
    if (droppedFiles.length > 0) handleFileDrop(droppedFiles[0]); // Only pick the first file
  };

  /*
    Event handler for when the user selects a file using the file input.
  */
  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFiles = event.target.files;
    if (selectedFiles && selectedFiles.length > 0) handleFileDrop(selectedFiles[0]);
  };

  /*
    Event handler for when the user clicks the remove file button.
  */
  const handleRemoveFile = () => {
    setFile(null); // Clear the selected file
    const fileInput = document.getElementById("browse") as HTMLInputElement; // Get the file input element
    if (fileInput) {
      fileInput.value = ""; // Reset the input value to allow re-upload
    }
  };

  return (
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
      <input type="file" hidden id="browse" onChange={handleFileChange} accept=".dxf,.dwg,.pdf" />
      <label htmlFor="browse" className="browse-btn">Browse file</label>
      {file && (
        <FileDisplay file={file} onRemoveFile={handleRemoveFile} />
      )}
    </div>
  );
};

export default DragDropZone;
