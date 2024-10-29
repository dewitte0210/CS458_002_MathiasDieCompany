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
  onSubmit: () => void;
}

const DragDropZone: React.FC<DragDropZoneProps> = ({
  file, allowedFileExtensions, setFile, onFilesSelected, onSubmit
}) => {
  const handleFileDrop = (newFile: File) => {
    const fileExtension = newFile.name.split('.').pop()?.toLowerCase();
    if (fileExtension && allowedFileExtensions.includes(`.${fileExtension}`)) {
      setFile(newFile);
      onFilesSelected && onFilesSelected([newFile]);
    } else {
      alert('Invalid file type. Please upload a PDF, DWG, or DXF file.');
    }
  };

  const handleDrop = (event: React.DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    const droppedFiles = event.dataTransfer.files;
    if (droppedFiles.length > 0) handleFileDrop(droppedFiles[0]);
  };

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFiles = event.target.files;
    if (selectedFiles && selectedFiles.length > 0) handleFileDrop(selectedFiles[0]);
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
        <FileDisplay file={file} onRemoveFile={() => setFile(null)} />
      )}
      {file && (
        <button className="animated-button" onClick={onSubmit}>
          Submit File
        </button>
      )}
    </div>
  );
};

export default DragDropZone;
